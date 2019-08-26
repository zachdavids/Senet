using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StickManager
{
    #region Setup 

    private static int _numSticks = 4;

    [SerializeField] public Transform[] _spawnPoints = new Transform[_numSticks];
    [HideInInspector] public GameObject[] _instances = new GameObject[_numSticks];

    public void Setup()
    {
        for (int i = 0; i < _instances.Length; i++)
        {
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
}
