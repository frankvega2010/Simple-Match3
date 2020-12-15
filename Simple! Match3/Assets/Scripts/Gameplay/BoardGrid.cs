using System.Collections.Generic;
using UnityEngine;

namespace SimpleMatch3.Gameplay
{
    public class BoardGrid : MonoBehaviour
    {
        public int tileSpacing = 0;
        public int rows = 0;
        public int columns = 0;
        public Transform initialTransform = null;
        public GameObject board = null;
        public GameObject tokenPrefab = null;
        public TokenInfo[] tokenPresets = null;

        public List<Token> tokensPool = new List<Token>();
        private Token[,] tokens = null;
        private GameObject[,] tokensGameObject = null;
        private int usedRows = 0;
        private int usedColumns = 0;

        // Start is called before the first frame update
        void Start()
        {
            Token.OnTokenFinishRemoveAnimation += DestroyToken;
        }

        public void GenerateGrid()
        {
            if (tokensGameObject != null)
            {
                for (int r = 0; r < usedRows; r++)
                {
                    for (int c = 0; c < usedColumns; c++)
                    {
                        DestroyToken(tokens[c, r]);
                        tokens[c, r] = null;
                    }
                }
            }

            tokens = new Token[columns, rows];
            tokensGameObject = new GameObject[columns, rows];
            usedRows = rows;
            usedColumns = columns;

            for (int r = 0; r < usedRows; r++)
            {
                for (int c = 0; c < usedColumns; c++)
                {
                    if (tokensPool.Count > 0)
                    {
                        tokensGameObject[c, r] = tokensPool[tokensPool.Count - 1].gameObject;
                        tokensPool.Remove(tokensPool[tokensPool.Count - 1]);
                    }
                    else
                    {
                        tokensGameObject[c, r] = Instantiate(tokenPrefab, board.transform, false);
                    }

                    tokensGameObject[c, r].transform.position = initialTransform.position;
                    tokensGameObject[c, r].SetActive(true);
                    TokenInfo newInfo = tokenPresets[Random.Range(0, tokenPresets.Length)];
                    tokens[c, r] = tokensGameObject[c, r].GetComponent<Token>();
                    tokens[c, r].ExecuteAnimation("TokenIdle");
                    tokens[c, r].SetData(newInfo.tokenType, newInfo.icon, newInfo.points);
                    tokens[c, r].gridIndex = new Vector2(c, r);
                    float posX = c * (tileSpacing);
                    float posY = r * (-tileSpacing);
                    tokensGameObject[c, r].transform.position = new Vector2(initialTransform.position.x + posX, initialTransform.position.y + posY);
                }
            }
        }

        public void SpawnNewToken(int column, int row)
        {
            if (tokensPool.Count > 0)
            {
                tokensGameObject[column, row] = tokensPool[tokensPool.Count - 1].gameObject;
                tokensPool.Remove(tokensPool[tokensPool.Count - 1]);
            }
            else
            {
                tokensGameObject[column, row] = Instantiate(tokenPrefab, board.transform, false);
            }

            tokensGameObject[column, row].transform.position = initialTransform.position;
            tokensGameObject[column, row].transform.position += new Vector3(0, 300, 0);
            tokensGameObject[column, row].SetActive(true);
            TokenInfo newInfo = tokenPresets[Random.Range(0, tokenPresets.Length)];
            tokens[column, row] = tokensGameObject[column, row].GetComponent<Token>();
            tokens[column, row].ExecuteAnimation("TokenIdle");
            tokens[column, row].SetData(newInfo.tokenType, newInfo.icon, newInfo.points);
            tokens[column, row].gridIndex = new Vector2(column, row);
        }

        public int GetUsedRows()
        {
            return usedRows;
        }

        public int GetUsedColumns()
        {
            return usedColumns;
        }

        public Token[,] GetCurrentTokens()
        {
            return tokens;
        }

        public GameObject[,] GetCurrentTokensGameObject()
        {
            return tokensGameObject;
        }

        public void DestroyToken(Token currentToken)
        {
            tokensPool.Add(currentToken);
            currentToken.OnCancel();
            currentToken.gameObject.SetActive(false);
        }

        public void DestroyTokenAnimated(Token currentToken)
        {
            //Execute Animation
            currentToken.ExecuteAnimation("TokenRemove");
        }

        private void OnDestroy()
        {
            Token.OnTokenFinishRemoveAnimation -= DestroyToken;
        }
    }
}