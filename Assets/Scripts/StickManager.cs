using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StickManager : MonoBehaviour
{
    #region Attributes

    private static int _numSticks = 4;

    [SerializeField] private GameObject _stickPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private GameObject[] _instances;

    public GameObject[] instances
    {
        get { return _instances; }
        set { _instances = value; }
    }

    #endregion

    #region Throwing Logic

    public void Throw()
    {
        for (int i = 0; i < _instances.Length; i++)
        {
            Rigidbody rBody = _instances[i].GetComponent<Rigidbody>();
            rBody.useGravity = true;
            rBody.AddForce(0, 400, 0);
            rBody.AddTorque(UnityEngine.Random.Range(0f, 500f), UnityEngine.Random.Range(0f, 500f), UnityEngine.Random.Range(0f, 500f));
        }
    }

    public int Score()
    {
        int total = 0;
        for (int i = 0; i < _instances.Length; i++)
        {
            if (Vector3.Dot(_instances[i].transform.up, Vector3.up) <= 0.0f)
            {
                total++;
            }
        }

        return (total == 0) ? 6 : total;
    }

    public bool IsSleeping()
    {
        for (int i = 0; i < _instances.Length; ++i)
        {
            if (_instances[i].GetComponent<Rigidbody>().IsSleeping() == false)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Setup

    public void SpawnSticks()
    {
        for (int i = 0; i < _instances.Length; i++)
        {
            _instances[i] = Instantiate(
                _stickPrefab,
                _spawnPoints[i].position,
                _spawnPoints[i].rotation
            );

            Rigidbody rBody = _instances[i].GetComponent<Rigidbody>();
            rBody.useGravity = false;
            rBody.maxAngularVelocity = 14.0f;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < _instances.Length; ++i)
        {
            _instances[i].GetComponent<Rigidbody>().useGravity = false;
            _instances[i].transform.position = _spawnPoints[i].position;
            _instances[i].transform.rotation = _spawnPoints[i].rotation;
        }
    }

    #endregion

    #region Monobehaviour Functions

    void Start()
    {
        _instances = new GameObject[_numSticks];

        SpawnSticks();
    }

    #endregion
}
