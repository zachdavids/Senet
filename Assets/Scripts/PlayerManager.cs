using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerManager
{
    public Color m_PlayerColor;
    public Transform[] m_SpawnPoints = new Transform[m_NumPawns];
    [HideInInspector] public int m_PlayerNumber;
    [HideInInspector] public string m_ColoredPlayerText;
    [HideInInspector] public GameObject[] m_Instances = new GameObject[m_NumPawns];

    private static int m_NumPawns = 5;

    public void Setup()
    {
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

        for (int i = 0; i < m_Instances.Length; i++)
        {
            m_Instances[i].GetComponentInChildren<MeshRenderer>().material.color = m_PlayerColor;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < m_Instances.Length; ++i)
        {
            m_Instances[i].transform.position = m_SpawnPoints[i].position;
        }
    }
}
