using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveManager
{
    public Transform m_DiscardPile;

    private int m_PawnIndex = -1;
    private int m_TargetIndex = -1;
    private int m_ValidIndex = -1;
    private bool m_HasMoved;
    private int m_PlayerId;
    private bool m_FirstMove;
    private Color m_PlayerColor;
    private Color m_EnemyColor;
    private Color m_TileColor;
    private GameObject[,] m_BoardState = new GameObject[30, 2];
    private List<GameObject> m_Discard = new List<GameObject>();

    public void Setup()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        for (int i = 0; i < tiles.Length; i++)
        {
            m_BoardState[i, 0] = tiles[i];
        }

        GameObject[] pawns = GameObject.FindGameObjectsWithTag("Pawn");
        for (int i = 0; i < m_BoardState.GetLength(0); i++)
        {
            for (int j = 0; j < pawns.Length; j++)
            {
                if (m_BoardState[i, 0].transform.position == pawns[j].transform.position)
                {
                    m_BoardState[i, 1] = pawns[j];
                }
            }
        }

        m_FirstMove = true;
    }

    public void Select(Color t_PlayerColor, int t_ThrowTotal)
    {
        m_PlayerColor = t_PlayerColor;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject selection = hit.collider.transform.parent.gameObject;
            if (selection.CompareTag("Pawn"))
            {
                if (selection.GetComponentInChildren<MeshRenderer>().material.color == t_PlayerColor)
                {
                    m_PawnIndex = GetIndex(selection);
                    if (m_FirstMove)
                    {
                        if (m_PawnIndex != 9)
                        {
                            return;
                        }
                        else
                        {
                            m_FirstMove = false;
                            DisplayValidMoves(t_ThrowTotal);
                        }
                    }
                    else
                    {
                        if (m_PawnIndex == 29)
                        {
                            DiscardPawn();
                            return;
                        }
                        else
                        {
                            DisplayValidMoves(t_ThrowTotal);
                        }
                    }
                }
                else
                {
                    //TODO: HANDLE SELECTION OF ENEMY PAWN
                }
            }
            else if (m_PawnIndex >= 0 && selection.CompareTag("Tile"))
            {
                m_TargetIndex = GetIndex(selection);
                if (m_TargetIndex == m_ValidIndex)
                {
                    m_HasMoved = Moved();
                }
                else
                {
                    m_TargetIndex = -1;
                }
            }
        }
    }

    private int GetIndex(GameObject t_Selection)
    {
        for (int i = 0; i < m_BoardState.Length; i++)
        {
            if (m_BoardState[i, 0].transform.position == t_Selection.transform.position)
            {
                return i;
            }
        }
        return -1;
    }

    private void DisplayValidMoves(int t_ThrowTotal)
    {
        m_ValidIndex = CalculateValidMove(t_ThrowTotal);
        if (m_ValidIndex == -1)
        {
            m_ValidIndex = CalculateValidMove(-t_ThrowTotal);
        }

        if (m_ValidIndex == -1)
        {
            m_HasMoved = true;
        }
        else
        {
            Highlight();
        }
    }

    private void Highlight()
    {
        m_BoardState[m_PawnIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        if (m_ValidIndex != 30)
        {
            m_TileColor = m_BoardState[m_ValidIndex, 0].GetComponentInChildren<MeshRenderer>().material.color;
            m_BoardState[m_ValidIndex, 0].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            if (m_BoardState[m_ValidIndex, 1] != null)
            {
                m_EnemyColor = m_BoardState[m_ValidIndex, 1].GetComponentInChildren<MeshRenderer>().material.color;
                m_BoardState[m_ValidIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            }
        }
    }

    private void ClearHighlight()
    {
        m_BoardState[m_PawnIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = m_PlayerColor;
        m_BoardState[m_ValidIndex, 0].GetComponentInChildren<MeshRenderer>().material.color = m_TileColor;
        if (m_BoardState[m_ValidIndex, 1] != null)
        {
            m_BoardState[m_ValidIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = m_EnemyColor;
        }
    }

    private int CalculateValidMove(int t_ThrowTotal)
    {
        //End condition
        if (m_PawnIndex == 29)
        {
            if (Exitable())
            {
                return 30;
            }
            else
            {
                return -1;
            }
        }

        if (m_PawnIndex + t_ThrowTotal < 0)
        {
            return -1;
        }

        int moveIndex = (m_PawnIndex + t_ThrowTotal >= 29) ? 29 : m_PawnIndex + t_ThrowTotal;

        //Blockade condition
        if (moveIndex - m_PawnIndex > 3)
        {
            if (Blockaded(moveIndex) == true)
            {
                return -1;
            }
        }

        //Occupied condition
        if (m_BoardState[moveIndex, 1] != null)
        {
            //Enemy condition
            if (m_BoardState[moveIndex, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor)
            {
                if (Attackable(moveIndex))
                {
                    return moveIndex;
                }
                else
                {
                    return -1;
                }
            }
            //Friendly condition
            else
            {
                return -1;
            }
        }
        //Unoccupied condition
        else
        {
            return moveIndex;
        }
    }

    private bool Exitable()
    {
        for (int i = 0; i < 9; i++)
        {
            if (m_BoardState[i,1] != null)
            {
                if (m_BoardState[i,1].GetComponentInChildren<MeshRenderer>().material.color == m_PlayerColor)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool Blockaded(int moveIndex)
    {
        for (int i = m_PawnIndex + 1; i + 2 < moveIndex; i++)
        {
            if (Math.Floor(i / 10f) != Math.Floor(i + 1 / 10f) || Math.Floor(i / 10f) != Math.Floor(i + 2 / 10f))
            {
                return false;
            }
            if ((m_BoardState[i, 1] != null && m_BoardState[i, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor) &&
                (m_BoardState[i + 1, 1] != null && m_BoardState[i + 1, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor) &&
                (m_BoardState[i + 2, 1] != null && m_BoardState[i + 2, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor))
            {
                return true;
            }
        }
        return false;
    }

    private bool Attackable(int moveIndex)
    {
        if (moveIndex == 14 || moveIndex == 25 || moveIndex == 27 || moveIndex == 28)
        {
            return false;
        }

        if (moveIndex == 29)
        {
            if (m_BoardState[moveIndex - 1, 1] != null && m_BoardState[moveIndex - 1, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        if ((m_BoardState[moveIndex + 1,1] != null && m_BoardState[moveIndex + 1, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor) || 
                (m_BoardState[moveIndex - 1, 1] != null && m_BoardState[moveIndex - 1, 1].GetComponentInChildren<MeshRenderer>().material.color != m_PlayerColor))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DiscardPawn()
    {
        m_BoardState[m_PawnIndex, 1].transform.position = m_DiscardPile.transform.position;
        m_Discard.Add(m_BoardState[m_PawnIndex, 1]);
        m_BoardState[m_PawnIndex, 1] = null;
        m_HasMoved = true;
    }

    private bool Moved()
    {
        ClearHighlight();
        if (m_TargetIndex == 26)
        {
            if (m_BoardState[14, 1] != null)
            {
                for (int i = 13; i > 0; i--)
                {
                    if (m_BoardState[i, 1] == null)
                    {
                        m_BoardState[m_PawnIndex, 1].transform.position = m_BoardState[i, 0].transform.position;
                        m_BoardState[i, 1] = m_BoardState[m_PawnIndex, 1];
                        m_BoardState[m_PawnIndex, 1] = null;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                m_BoardState[m_PawnIndex, 1].transform.position = m_BoardState[14, 0].transform.position;
                m_BoardState[14, 1] = m_BoardState[m_PawnIndex, 1];
                m_BoardState[m_PawnIndex, 1] = null;
                return true;
            }
        }

        if (m_BoardState[m_TargetIndex, 1] != null)
        {
            m_BoardState[m_PawnIndex, 1].transform.position = m_BoardState[m_TargetIndex, 0].transform.position;
            m_BoardState[m_TargetIndex, 1].transform.position = m_BoardState[m_PawnIndex, 0].transform.position;
            GameObject temp = m_BoardState[m_TargetIndex, 1];
            m_BoardState[m_TargetIndex, 1] = m_BoardState[m_PawnIndex, 1];
            m_BoardState[m_PawnIndex, 1] = temp;
            return true;
        }
        else
        {
            m_BoardState[m_PawnIndex, 1].transform.position = m_BoardState[m_TargetIndex, 0].transform.position;
            m_BoardState[m_TargetIndex, 1] = m_BoardState[m_PawnIndex, 1];
            m_BoardState[m_PawnIndex, 1] = null;
            return true;
        }
    }

    public bool HasWon(Color t_PlayerColor)
    {
        int count = 0;
        foreach (GameObject obj in m_Discard)
        {
            if (obj.GetComponentInChildren<MeshRenderer>().material.color == t_PlayerColor)
            {
                count++;
            }
        }

        if (count == 5)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SelectionComplete()
    {
        return m_HasMoved;
    }

    public void Reset()
    {
        m_PawnIndex = -1;
        m_TargetIndex = -1;
        m_ValidIndex = -1;
        m_HasMoved = false;
    }
}
