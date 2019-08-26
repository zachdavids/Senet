using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class PlayerManager
{
    #region Editable Fields

    private int _playerNumber;

    public int playerNumber
    {
        get { return _playerNumber; }
        set { _playerNumber = value; }
    }

    public Color _playerColor;

    public Color playerColor
    {
        get { return _playerColor; }
        set { _playerColor = value; }
    }

    #endregion

    #region Setup

    private static int _numPawns = 5;

    [SerializeField] public Transform[] _spawnPoints = new Transform[_numPawns];
    [HideInInspector] public GameObject[] _instances = new GameObject[_numPawns];

    public void Setup()
    {
        for (int i = 0; i < _instances.Length; i++)
        {
            _instances[i].GetComponentInChildren<MeshRenderer>().material.color = playerColor;
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
}
