using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int minimumMatch;
    public BoardGrid grid;
    public List<Token> currentChain;

    // Start is called before the first frame update
    void Start()
    {
        Token.OnTokenSelected += CheckTokenChain;
    }

   /* // Update is called once per frame
    void Update()
    {
        
    }*/

    public void CheckTokenChain(Token currentToken)
    {
        if(currentChain.Count <= 0)
        {
            currentChain.Add(currentToken);
            currentToken.OnAdd();
        }
        else
        {
            if(currentToken == currentChain[currentChain.Count-1]) // Remove token from chain
            {
                currentChain.Remove(currentToken);
                currentToken.OnRemove();
            }
            else
            {
                if(CheckAdjacentTokens(currentToken)) // Add new token
                {
                    currentChain.Add(currentToken);
                    currentToken.OnAdd();
                }
                else
                {
                    if(currentChain.Count >= minimumMatch) // Do match
                    {
                        foreach (Token t in currentChain)
                        {
                            t.tokenType = Token.TOKEN_TYPES.EMPTY;
                            t.iconImage.sprite = null;
                        }

                        currentChain.Clear();

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

    private void OnDestroy()
    {
        Token.OnTokenSelected -= CheckTokenChain;
    }
}
