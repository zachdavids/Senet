using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveManager
{
    #region Editable Attributes

    private bool _hasMoved;

    public bool hasMoved
    {
        get { return _hasMoved; }
    }

    private Transform _discard;

    public Transform discard
    {
        get { return _discard; }
        set { _discard = value; }
    }

    #endregion

    #region Setup

    private int _pawnIndex;
    private int _targetIndex;
    private int _validIndex;
    private bool _firstMove;
    private GameObject[,] _boardState;
    private List<GameObject> _discardPile;

    public void Setup()
    {
        _boardState = new GameObject[30, 2];
        _discardPile = new List<GameObject>();

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        for (int i = 0; i < tiles.Length; i++)
        {
            _boardState[i, 0] = tiles[i];
        }

        GameObject[] pawns = GameObject.FindGameObjectsWithTag("Pawn");
        for (int i = 0; i < _boardState.GetLength(0); i++)
        {
            for (int j = 0; j < pawns.Length; j++)
            {
                if (_boardState[i, 0].transform.position == pawns[j].transform.position)
                {
                    _boardState[i, 1] = pawns[j];
                }
            }
        }

        Reset();

        _firstMove = true;
    }

    public void Reset()
    {
        _pawnIndex = -1;
        _targetIndex = -1;
        _validIndex = -1;
        _hasMoved = false;
    }

    #endregion

    #region Move Logic

    private Color _playerColor;
    private Color _enemyColor;
    private Color _tileColor;

    public void Select(Color t_PlayerColor, int t_ThrowTotal)
    {
        _playerColor = t_PlayerColor;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject selection = hit.collider.transform.parent.gameObject;
            if (selection.CompareTag("Pawn"))
            {
                if (selection.GetComponentInChildren<MeshRenderer>().material.color == t_PlayerColor)
                {
                    _pawnIndex = GetIndex(selection);
                    if (_firstMove)
                    {
                        if (_pawnIndex != 9)
                        {
                            return;
                        }
                        else
                        {
                            _firstMove = false;
                            DisplayValidMoves(t_ThrowTotal);
                        }
                    }
                    else
                    {
                        if (_pawnIndex == 29)
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
            else if (_pawnIndex >= 0 && selection.CompareTag("Tile"))
            {
                _targetIndex = GetIndex(selection);
                if (_targetIndex == _validIndex)
                {
                    _hasMoved = Moved();
                }
                else
                {
                    _targetIndex = -1;
                }
            }
        }
    }

    private bool Moved()
    {
        ClearHighlight();
        if (_targetIndex == 26)
        {
            if (_boardState[14, 1] != null)
            {
                for (int i = 13; i > 0; i--)
                {
                    if (_boardState[i, 1] == null)
                    {
                        _boardState[_pawnIndex, 1].transform.position = _boardState[i, 0].transform.position;
                        _boardState[i, 1] = _boardState[_pawnIndex, 1];
                        _boardState[_pawnIndex, 1] = null;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                _boardState[_pawnIndex, 1].transform.position = _boardState[14, 0].transform.position;
                _boardState[14, 1] = _boardState[_pawnIndex, 1];
                _boardState[_pawnIndex, 1] = null;
                return true;
            }
        }

        if (_boardState[_targetIndex, 1] != null)
        {
            _boardState[_pawnIndex, 1].transform.position = _boardState[_targetIndex, 0].transform.position;
            _boardState[_targetIndex, 1].transform.position = _boardState[_pawnIndex, 0].transform.position;
            GameObject temp = _boardState[_targetIndex, 1];
            _boardState[_targetIndex, 1] = _boardState[_pawnIndex, 1];
            _boardState[_pawnIndex, 1] = temp;
            return true;
        }
        else
        {
            _boardState[_pawnIndex, 1].transform.position = _boardState[_targetIndex, 0].transform.position;
            _boardState[_targetIndex, 1] = _boardState[_pawnIndex, 1];
            _boardState[_pawnIndex, 1] = null;
            return true;
        }
    }

    private int GetIndex(GameObject t_Selection)
    {
        for (int i = 0; i < _boardState.Length; i++)
        {
            if (_boardState[i, 0].transform.position == t_Selection.transform.position)
            {
                return i;
            }
        }
        return -1;
    }

    private void DisplayValidMoves(int t_ThrowTotal)
    {
        _validIndex = CalculateValidMove(t_ThrowTotal);
        if (_validIndex == -1)
        {
            _validIndex = CalculateValidMove(-t_ThrowTotal);
        }

        if (_validIndex == -1)
        {
            _hasMoved = true;
        }
        else
        {
            Highlight();
        }
    }

    private int CalculateValidMove(int t_ThrowTotal)
    {
        //End condition
        if (_pawnIndex == 29)
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

        if (_pawnIndex + t_ThrowTotal < 0)
        {
            return -1;
        }

        int moveIndex = (_pawnIndex + t_ThrowTotal >= 29) ? 29 : _pawnIndex + t_ThrowTotal;

        //Blockade condition
        if (moveIndex - _pawnIndex > 3)
        {
            if (Blockaded(moveIndex) == true)
            {
                return -1;
            }
        }

        //Occupied condition
        if (_boardState[moveIndex, 1] != null)
        {
            //Enemy condition
            if (_boardState[moveIndex, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor)
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

    private void Highlight()
    {
        _boardState[_pawnIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        if (_validIndex != 30)
        {
            _tileColor = _boardState[_validIndex, 0].GetComponentInChildren<MeshRenderer>().material.color;
            _boardState[_validIndex, 0].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            if (_boardState[_validIndex, 1] != null)
            {
                _enemyColor = _boardState[_validIndex, 1].GetComponentInChildren<MeshRenderer>().material.color;
                _boardState[_validIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            }
        }
    }

    private void ClearHighlight()
    {
        _boardState[_pawnIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = _playerColor;
        _boardState[_validIndex, 0].GetComponentInChildren<MeshRenderer>().material.color = _tileColor;
        if (_boardState[_validIndex, 1] != null)
        {
            _boardState[_validIndex, 1].GetComponentInChildren<MeshRenderer>().material.color = _enemyColor;
        }
    }
    private void DiscardPawn()
    {
        _boardState[_pawnIndex, 1].transform.position = _discard.transform.position;
        _discardPile.Add(_boardState[_pawnIndex, 1]);
        _boardState[_pawnIndex, 1] = null;
        _hasMoved = true;
    }

    public bool HasWon(Color t_PlayerColor)
    {
        int count = 0;
        foreach (GameObject obj in _discardPile)
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

    #endregion

    #region Board Logic

    private bool Exitable()
    {
        for (int i = 0; i < 9; i++)
        {
            if (_boardState[i,1] != null)
            {
                if (_boardState[i,1].GetComponentInChildren<MeshRenderer>().material.color == _playerColor)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool Blockaded(int moveIndex)
    {
        for (int i = _pawnIndex + 1; i + 2 < moveIndex; i++)
        {
            if (Math.Floor(i / 10f) != Math.Floor(i + 1 / 10f) || Math.Floor(i / 10f) != Math.Floor(i + 2 / 10f))
            {
                return false;
            }
            if ((_boardState[i, 1] != null && _boardState[i, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor) &&
                (_boardState[i + 1, 1] != null && _boardState[i + 1, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor) &&
                (_boardState[i + 2, 1] != null && _boardState[i + 2, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor))
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
            if (_boardState[moveIndex - 1, 1] != null && _boardState[moveIndex - 1, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        if ((_boardState[moveIndex + 1,1] != null && _boardState[moveIndex + 1, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor) || 
                (_boardState[moveIndex - 1, 1] != null && _boardState[moveIndex - 1, 1].GetComponentInChildren<MeshRenderer>().material.color != _playerColor))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #endregion
}
