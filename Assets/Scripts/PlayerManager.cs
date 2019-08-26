using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerManager : MonoBehaviour
{
    #region Attributes

    private static int _numPawns = 5;

    [SerializeField] private GameObject _pawnPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private Color _playerColor;
    public Color playerColor
    {
        get { return _playerColor; }
        set { _playerColor = value; }
    }

    [SerializeField] private int _playerNumber;

    public int playerNumber
    {
        get { return _playerNumber; }
        set { _playerNumber = value; }
    }

    private GameObject[] _instances;

    public GameObject[] instances
    {
        get { return _instances; }
        set { _instances = value; }
    }

    #endregion

    #region Setup

    private void SpawnPawns()
    {
        for (int i = 0; i < _numPawns; ++i)
        {
            _instances[i] = Instantiate(
                _pawnPrefab, 
                _spawnPoints[i].position,
                _spawnPoints[i].rotation
            );

            _instances[i].GetComponentInChildren<MeshRenderer>().material.color = _playerColor;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < _instances.Length; ++i)
        {
            _instances[i].transform.position = _spawnPoints[i].position;
        }
    }

    #endregion

    #region Monobehaviour Functions

    void Start()
    {
        _instances = new GameObject[_numPawns];

        SpawnPawns();
    }

    #endregion
}
