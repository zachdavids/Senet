using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StickManager
{
    public Transform[] m_SpawnPoints = new Transform[m_NumSticks];
    [HideInInspector] public GameObject[] m_Instances = new GameObject[m_NumSticks];

    private static int m_NumSticks = 4;

    public void Setup()
    {
        for (int i = 0; i < m_Instances.Length; i++)
        {
            Rigidbody rBody = m_Instances[i].GetComponent<Rigidbody>();
            rBody.useGravity = false;
            rBody.maxAngularVelocity = 14.0f;
        }
    }

    public void Throw()
    {
        for (int i = 0; i < m_Instances.Length; i++)
        {
            Rigidbody rBody = m_Instances[i].GetComponent<Rigidbody>();
            rBody.useGravity = true;
            rBody.AddForce(0, 400, 0);
            rBody.AddTorque(UnityEngine.Random.Range(0f, 500f), UnityEngine.Random.Range(0f, 500f), UnityEngine.Random.Range(0f, 500f));
        }
    }

    public int Score()
    {
        int total = 0;
        for (int i = 0; i < m_Instances.Length; i++)
        {
            if (Vector3.Dot(m_Instances[i].transform.up, Vector3.up) <= 0.0f)
            {
                total++;
            }
        }

        return (total == 0) ? 6 : total;
    }

    public bool IsSleeping()
    {
        for (int i = 0; i < m_Instances.Length; ++i)
        {
            if (m_Instances[i].GetComponent<Rigidbody>().IsSleeping() == false)
            {
                return false;
            }
        }

        return true;
    }

    public void Reset()
    {
        for (int i = 0; i < m_Instances.Length; ++i)
        {
            m_Instances[i].GetComponent<Rigidbody>().useGravity = false;
            m_Instances[i].transform.position = m_SpawnPoints[i].position;
            m_Instances[i].transform.rotation = m_SpawnPoints[i].rotation;
        }
    }
}
