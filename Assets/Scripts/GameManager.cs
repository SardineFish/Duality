using System.Collections;
using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;

public class GameManager : RuntimeSingleton<GameManager>
{
    [SerializeField] private int m_currentLevel;
    [SerializeField] private List<Vector2> m_stageDiamondPos;
    [SerializeField] private GameObject m_diamondPrefab;

    public void CheckForDiamond(Vector2 pos)
    {
        if (pos == m_stageDiamondPos[m_currentLevel])
            Instantiate(m_diamondPrefab, pos, Quaternion.identity);
    }

    public void LoadNextLevel()
    {
        Debug.Log("TODO: Load Next Level");
    }
}
