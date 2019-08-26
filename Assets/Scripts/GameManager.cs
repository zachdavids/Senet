using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject m_PawnPrefab;
    public GameObject m_StickPrefab;
    public StickManager m_StickManager;
    public MoveManager m_MoveManager = new MoveManager();
    public PlayerManager[] m_Players = new PlayerManager[m_NumPlayers];

    private static int m_NumPlayers = 2;
    private float m_StartDelay = 1.5f;
    private float m_EndDelay = 1.5f;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private PlayerManager m_GameWinner;

    // Start is called before the first frame update
    void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        SpawnAllPawns();
        SpawnAllSticks();
        m_MoveManager.Setup();
        StartCoroutine(GameLoop());
    }

    private void SpawnAllPawns()
    {
        for (int i = 0; i < m_Players.Length; ++i)
        {
            for (int j = 0; j < m_Players[i].m_Instances.Length; ++j)
            {
                m_Players[i].m_Instances[j] = Instantiate(m_PawnPrefab, m_Players[i].m_SpawnPoints[j].position,
                    m_Players[i].m_SpawnPoints[j].rotation) as GameObject;
            }
            m_Players[i].m_PlayerNumber = i + 1;
            m_Players[i].Setup();
        }
    }

    private void SpawnAllSticks()
    {
        for (int i = 0; i < m_StickManager.m_Instances.Length; i++)
        {
            m_StickManager.m_Instances[i] = Instantiate(m_StickPrefab, m_StickManager.m_SpawnPoints[i].position,
                m_StickManager.m_SpawnPoints[i].rotation) as GameObject;
        }
        m_StickManager.Setup();
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting()
    {
        Debug.Log("RoundStarting");
        yield return null;
    }
 
    private IEnumerator RoundPlaying()
    {
        Debug.Log("RoundPlaying");
        for (int i = 0; i < m_Players.Length; i++)
        {
            yield return StartCoroutine(TakeTurn(m_Players[i]));

            m_GameWinner = GetGameWinner();
            if (m_GameWinner != null)
            {
                break;
            }
        }

        yield return null;
    }

    private IEnumerator RoundEnding()
    {
        Debug.Log("RoundEnding");
        yield return m_EndWait;
    }

    private IEnumerator TakeTurn(PlayerManager t_Player)
    {
        Debug.Log("Player " + t_Player.m_PlayerNumber + " turn");
        bool newRoll = true;
        while (newRoll)
        {
            yield return StartCoroutine(Rolling());
            int score = m_StickManager.Score();
            yield return StartCoroutine(Moving(t_Player.m_PlayerColor, score));
            newRoll = (score == 1 || score == 4 || score == 6) ? true : false;
            m_StickManager.Reset();
            yield return null;
        }
        yield return m_EndWait;
    }

    private IEnumerator Rolling()
    {
        Debug.Log("Rolling");
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        m_StickManager.Throw();
        yield return new WaitUntil(() => (m_StickManager.IsSleeping()));
    }

    private IEnumerator Moving(Color t_PlayerColor, int t_ThrowTotal)
    {
        Debug.Log("Moving");
        while (!m_MoveManager.SelectionComplete())
        {
            yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
            m_MoveManager.Select(t_PlayerColor, t_ThrowTotal);
        }
        m_MoveManager.Reset();
    }

    private PlayerManager GetGameWinner()
    {
        for (int i = 0; i < m_NumPlayers; i++)
        {
            if (m_MoveManager.HasWon(m_Players[i].m_PlayerColor) == true)
            {
                return m_Players[i];
            }
        }

        return null;
    }
}
