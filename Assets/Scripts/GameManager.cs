using System;
using System.Collections;
using System.Collections.Generic;
using Duality;
using SardineFish.Utils;
using UnityEngine;

public class GameManager : RuntimeSingleton<GameManager>
{
    [SerializeField] private int m_currentLevel;
    [SerializeField] private List<Vector2> m_stageDiamondPos;
    [SerializeField] private GameObject m_diamondPrefab;
    [SerializeField] private int m_diamondGravityDir;

    public void CheckForDiamond(Vector2 pos)
    {
        if (pos == m_stageDiamondPos[m_currentLevel])
        {
            var obj = Instantiate(m_diamondPrefab, pos + new Vector2(0.5f, 0.5f), Quaternion.identity);
            var fire = obj.GetComponent<Fire>();
            fire.GravityDirection = m_diamondGravityDir;
        }
    }

    public void LoadNextLevel()
    {
        Debug.Log("TODO: Load Next Level");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var pos     in m_stageDiamondPos)
        {
            Gizmos.DrawWireSphere(pos.ToVector3(0) + new Vector3(0.5f, 0.5f, 0), 0.5f);
        }
    }
}
