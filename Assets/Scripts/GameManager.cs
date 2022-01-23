using System;
using System.Collections;
using System.Collections.Generic;
using Duality;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : RuntimeSingleton<GameManager>
{
    [SerializeField] private string m_nextScene;
    [SerializeField] private Vector2Int m_stageDiamondPos;
    [SerializeField] private GameObject m_diamondPrefab;
    [SerializeField] private int m_diamondGravityDir;

    private bool levelLoading = false;

    public void CheckForDiamond(Vector2 pos)
    {
        if (MathUtility.FloorToInt(pos) == m_stageDiamondPos)
        {
            var obj = Instantiate(m_diamondPrefab, pos + new Vector2(0.5f, 0.5f), Quaternion.identity);
            var fire = obj.GetComponent<Fire>();
            fire.GravityDirection = m_diamondGravityDir;
            fire.GetComponent<Rigidbody2D>().velocity =
                fire.GetComponent<Diamond>().InitialVelocity * Vector2.down * m_diamondGravityDir;
        }
    }

    public void LoadNextLevel()
    {
        if(levelLoading)
            return;
        
        Debug.Log("TODO: Load Next Level");
        levelLoading = true;

        SceneManager.LoadScene(m_nextScene);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_stageDiamondPos.ToVector3(0) + new Vector3(0.5f, 0.5f, 0), 0.5f);
    }
}
