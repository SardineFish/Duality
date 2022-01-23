using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Duality;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : RuntimeSingleton<GameManager>
{
    [SerializeField] private string m_currentScene;
    [SerializeField] private string m_nextScene;
    [SerializeField] private Vector2Int m_stageDiamondPos;
    [SerializeField] private Vector2Int m_DiamondSpawnPos;
    [SerializeField] private GameObject m_diamondPrefab;
    [SerializeField] private int m_diamondGravityDir;
    
    [SerializeField] private Animator m_levelTransition;
    [SerializeField] private float m_transitionDuration;

    private bool levelLoading = false;

    public void CheckForDiamond(Vector2 pos)
    {
        if (MathUtility.FloorToInt(pos) == m_stageDiamondPos)
        {
            var obj = Instantiate(m_diamondPrefab, m_DiamondSpawnPos + new Vector2(0.5f, 0.5f), Quaternion.identity);
            var fire = obj.GetComponent<Fire>();
            fire.GravityDirection = m_diamondGravityDir;
            fire.GetComponent<Rigidbody2D>().velocity =
                fire.GetComponent<Diamond>().InitialVelocity * Vector2.down * m_diamondGravityDir;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            ResetLevel();
        
    }

    public void LoadNextLevel()
    {
        if(levelLoading)
            return;
        
        // Debug.Log("TODO: Load Next Level");
        // levelLoading = true;
        //
        // StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
        
        CutScene.Instance.ChangeScene(m_nextScene);
    }

    public void ResetLevel()
    {
        CutScene.Instance.ChangeScene(m_currentScene);
    }

    public void ReloadCurrentLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        m_levelTransition.SetTrigger("Start");

        yield return new WaitForSeconds(m_transitionDuration);

        SceneManager.LoadScene(levelIndex);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_stageDiamondPos.ToVector3(0) + new Vector3(0.5f, 0.5f, 0), 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_DiamondSpawnPos.ToVector3() + new Vector3(.5f, .5f, 0), 0.5f);
    }
}
