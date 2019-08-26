using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(StickManager))]
[RequireComponent(typeof(MoveManager))]
[RequireComponent(typeof(PlayerManager))]
public class GameManager : MonoBehaviour
{
    #region Attributes

    private static int _numPlayers = 2;
    private bool _gameOver;
    private float _delayTime = 1.5f;
    private WaitForSeconds _endDelay;
    private MoveManager _moveManager;
    private StickManager _stickManager;
    private PlayerManager[] _playerManagers;

    #endregion

    #region Game Logic

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (!_gameOver)
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
        for (int i = 0; i < _numPlayers; i++)
        {
            yield return StartCoroutine(TakeTurn(_playerManagers[i]));

            _gameOver = GetGameWinner();
            if (!_gameOver)
            {
                break;
            }
        }

        yield return null;
    }

    private IEnumerator RoundEnding()
    {
        Debug.Log("RoundEnding");
        yield return _endDelay;
    }

    private IEnumerator TakeTurn(PlayerManager t_Player)
    {
        Debug.Log("Player " + t_Player.playerNumber + " turn");
        bool newRoll = true;
        while (newRoll)
        {
            yield return StartCoroutine(Rolling());
            int score = _stickManager.Score();
            yield return StartCoroutine(Moving(t_Player.playerColor, score));
            newRoll = (score == 1 || score == 4 || score == 6) ? true : false;
            _stickManager.Reset();
            yield return null;
        }
        yield return _endDelay;
    }

    private IEnumerator Rolling()
    {
        Debug.Log("Rolling");
        yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
        _stickManager.Throw();
        yield return new WaitUntil(() => (_stickManager.IsSleeping()));
    }

    private IEnumerator Moving(Color t_PlayerColor, int t_ThrowTotal)
    {
        Debug.Log("Moving");
        while (!_moveManager.hasMoved)
        {
            yield return new WaitUntil(() => (Input.GetButtonDown("Fire1")));
            _moveManager.Select(t_PlayerColor, t_ThrowTotal);
        }
        _moveManager.Reset();
    }

    private bool GetGameWinner()
    {
        for (int i = 0; i < _numPlayers; i++)
        {
            if (_moveManager.HasWon(_playerManagers[i].playerColor) == true)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Monobehaviour Functions

    // Start is called before the first frame update
    void Start()
    {
        _stickManager = GetComponent<StickManager>();
        _playerManagers = GetComponents<PlayerManager>();
        _moveManager = GetComponent<MoveManager>();

        _moveManager.ConfigureBoard();

        _endDelay = new WaitForSeconds(_delayTime);
        StartCoroutine(GameLoop());
    }

    #endregion
}
