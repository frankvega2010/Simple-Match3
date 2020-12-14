using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int minimumMatch = 0;
    public BoardGrid grid = null;
    public List<Token> currentChain = new List<Token>();
    public bool inputEnabled = false;
    [SerializeField]
    private List<Token> movingTokens = new List<Token>();

    // Start is called before the first frame update
    void Start()
    {
        Token.OnTokenSelected += CheckTokenChain;
        Token.OnTokenLerpFinish += CheckInputAvailability;
        grid.GenerateGrid();
        AdjustGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("GenerateGrid"))
        {
            grid.GenerateGrid();
            AdjustGrid();
        }
    }

    public void CheckTokenChain(Token currentToken)
    {
        if(inputEnabled)
        {
            if (currentChain.Count <= 0)
            {
                currentChain.Add(currentToken);
                currentToken.OnAdd();
            }
            else
            {
                if (currentToken == currentChain[currentChain.Count - 1]) // Remove token from chain
                {
                    currentChain.Remove(currentToken);
                    currentToken.OnRemove();
                }
                else
                {
                    if (CheckAdjacentTokens(currentToken)) // Add new token
                    {
                        currentChain.Add(currentToken);
                        currentToken.OnAdd();
                    }
                    else
                    {
                        if (currentChain.Count >= minimumMatch) // Do match
                        {
                            foreach (Token t in currentChain)
                            {
                                Destroy(grid.GetCurrentTokens()[(int)t.gridIndex.x, (int)t.gridIndex.y].gameObject);
                                grid.GetCurrentTokens()[(int)t.gridIndex.x, (int)t.gridIndex.y] = null;
                            }

                            currentChain.Clear();
                            inputEnabled = false;
                            UpdateGrid();


                            //Give points
                        }
                        else // Cancel Chain
                        {
                            foreach (Token t in currentChain)
                            {
                                t.OnCancel();
                            }

                            currentChain.Clear();
                        }
                    }
                }
            }
        }
        
    }

    public bool CheckAdjacentTokens(Token currentToken)
    {
        int selectedColumn = 0;
        int selectedRow = 0;

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if(currentToken == grid.GetCurrentTokens()[c,r])
                {
                    selectedColumn = c;
                    selectedRow = r;
                    c = grid.GetUsedColumns();
                    r = grid.GetUsedRows();
                }
            }
        }

        List<Token> adjacentsTokens = new List<Token>();
        List<Token> availableTokens = new List<Token>();

        if (selectedRow + 1 < grid.GetUsedRows())
        {
            adjacentsTokens.Add(grid.GetCurrentTokens()[selectedColumn, selectedRow + 1]);
        }

        if (selectedRow - 1 >= 0)
        {
            adjacentsTokens.Add(grid.GetCurrentTokens()[selectedColumn, selectedRow - 1]);
        }

        if (selectedColumn + 1 < grid.GetUsedColumns())
        {
            adjacentsTokens.Add(grid.GetCurrentTokens()[selectedColumn + 1, selectedRow]);
        }

        if (selectedColumn - 1 >= 0)
        {
            adjacentsTokens.Add(grid.GetCurrentTokens()[selectedColumn - 1, selectedRow]);
        }

        foreach (Token adjtoken in adjacentsTokens)
        {
            foreach (Token t in currentChain)
            {
                if(adjtoken == t)
                {
                    availableTokens.Add(adjtoken);
                }
            }
        }

        bool boolResult = false;

        foreach (Token t in availableTokens)
        {
            if(currentToken.tokenType == t.tokenType)
            {
                if (currentChain[currentChain.Count - 1] == t)
                {
                    boolResult = true;
                }
            }
        }

        adjacentsTokens.Clear();
        availableTokens.Clear();

        return boolResult;
    }

    public void AdjustGrid()
    {
        int timesGO = 0;
        bool finishRepeatedTiles = false;

        while (!finishRepeatedTiles)
        {
            int amountFinished = 0;

            if (CheckRepeatedColorsHorizontal())
            {
                amountFinished++;
            }

            if (CheckRepeatedColorsVertical())
            {
                amountFinished++;
            }

            if (amountFinished >= 2)
            {
                finishRepeatedTiles = true;
            }
            else
            {
                finishRepeatedTiles = false;
            }

            Debug.Log("Amount FINISHED : " + amountFinished);
            timesGO++;
        }

        Debug.Log("Times WENT : " + timesGO);
    }

    public void UpdateGrid()
    {
        int newTokenRow = -1;
        bool foundEmptySpace = false;
        Token[,] currentTokens = grid.GetCurrentTokens();
        bool movableTokensAvailable = true;

        while(movableTokensAvailable)
        {
            movableTokensAvailable = false;

            for (int r = 0; r < grid.GetUsedRows(); r++)
            {
                for (int c = 0; c < grid.GetUsedColumns(); c++)
                {
                    if (currentTokens[c, r] != null)
                    {
                        foundEmptySpace = false;

                        for (int i = r + 1; i < grid.GetUsedRows(); i++)
                        {
                            if (i < grid.GetUsedRows() && i >= 0)
                            {
                                if (currentTokens[c, i] == null)
                                {
                                    newTokenRow = i;
                                    foundEmptySpace = true;
                                    movableTokensAvailable = true;
                                }
                                else
                                {
                                    i = grid.GetUsedRows();
                                }
                            }
                            else
                            {
                                i = grid.GetUsedRows();
                            }

                        }

                        if (foundEmptySpace)
                        {
                            currentTokens[c, newTokenRow] = currentTokens[c, r];
                            currentTokens[c, r] = null;

                            currentTokens[c, newTokenRow].oldPosition = new Vector2(grid.initialTransform.position.x, grid.initialTransform.position.y)
                                + new Vector2(c * (currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.width + grid.tileSpacing),
                                               r * (-currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.height - grid.tileSpacing));

                            currentTokens[c, newTokenRow].newPosition = new Vector2(grid.initialTransform.position.x, grid.initialTransform.position.y)
                                + new Vector2(c * (currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.width + grid.tileSpacing),
                                     newTokenRow * (-currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.height - grid.tileSpacing));

                            currentTokens[c, newTokenRow].gridIndex = new Vector2(c, newTokenRow);
                            currentTokens[c, newTokenRow].StartLerp();
                            movingTokens.Add(currentTokens[c, newTokenRow]);
                        }

                    }
                }

            }
        }
    }

    public void CheckInputAvailability(Token currentToken)
    {
        movingTokens.Remove(currentToken);

        if(movingTokens.Count <= 0)
        {
            inputEnabled = true;
        }
    }

   /* public bool CheckForAnyEmptySpace()
    {
        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (grid.GetCurrentTokens()[c, r] == null)
                {
                    bool isKnown = false;
                    Vector2 space = new Vector2(c, r);

                    foreach (Vector2 knownEmptySpace in knownEmptySpaces)
                    {
                        if(space == knownEmptySpace)
                        {
                            isKnown = true;
                        }
                    }

                    if(!isKnown)
                    {
                        return true;
                    }
                    
                }
            }
        }

        return false;
    }*/


    public void SetRandomData(int column, int row)
    {
        TokenInfo newInfo = grid.tokenPresets[Random.Range(0, grid.tokenPresets.Length)];
        grid.GetCurrentTokens()[column, row].SetData(newInfo.tokenType, newInfo.icon);
    }

    public bool CheckRepeatedColorsVertical()
    {
        bool isDone = true;
        int findSameColor = 0;

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (r != 0 && r < grid.GetUsedRows() - 1)
                {
                    findSameColor++;

                    int automaticMatch = 0;
                    if(minimumMatch % 2 == 0)
                    {
                        automaticMatch = minimumMatch - 1;
                    }
                    else
                    {
                        automaticMatch = minimumMatch;
                    }

                    for (int i = 1; i <= automaticMatch-1; i++)
                    {
                        if (r - i >= 0)
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c, r - i].tokenType)
                            {
                                findSameColor++;
                            }
                        }

                        if (r + i < grid.GetUsedRows())
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c, r + i].tokenType)
                            {
                                findSameColor++;
                            }
                        }
                    }

                    if (findSameColor >= automaticMatch)
                    {
                        SetRandomData(c, r);

                        for (int i = 1; i <= automaticMatch-1; i++)
                        {
                            if (r - i >= 0)
                            {
                                SetRandomData(c, r - i);
                            }

                            if (r + i < grid.GetUsedRows())
                            {
                                SetRandomData(c, r + i);
                            }

                        }
                        
                        isDone = false;
                    }

                    findSameColor = 0;
                }
            }
        }

        return isDone;
    }

    public bool CheckRepeatedColorsHorizontal()
    {
        bool isDone = true;
        int findSameColor = 0;

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (c != 0 && c < grid.GetUsedColumns() - 1)
                {
                    findSameColor++;

                    int automaticMatch = 0;
                    if (minimumMatch % 2 == 0)
                    {
                        automaticMatch = minimumMatch - 1;
                    }
                    else
                    {
                        automaticMatch = minimumMatch;
                    }

                    for (int i = 1; i <= automaticMatch-1; i++)
                    {
                        if (c - i >= 0)
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c - i, r].tokenType)
                            {
                                findSameColor++;
                            }
                        }

                        if (c + i < grid.GetUsedColumns())
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c + i, r].tokenType)
                            {
                                findSameColor++;
                            }
                        }
                    }

                    if (findSameColor >= automaticMatch)
                    {
                        SetRandomData(c, r);

                        for (int i = 1; i <= automaticMatch-1; i++)
                        {
                            if (c - i >= 0)
                            {
                                SetRandomData(c - i, r);
                            }

                            if (c + i < grid.GetUsedColumns())
                            {
                                SetRandomData(c + i, r);
                            }

                        }

                        isDone = false;
                    }

                    findSameColor = 0;
                }
            }
        }

        return isDone;
    }

    private void OnDestroy()
    {
        Token.OnTokenSelected -= CheckTokenChain;
        Token.OnTokenLerpFinish -= CheckInputAvailability;
    }
}
