﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public int tileSpacing = 0;
    public int rows = 0;
    public int columns = 0;
    public Transform initialTransform;
    public GameObject board;
    public GameObject tokenPrefab;
    public TokenInfo[] tokenPresets;

    private Token[,] tokens;
    private GameObject[,] tokensGameObject;
    private int usedRows = 0;
    private int usedColumns = 0;

    // Start is called before the first frame update
    void Start()
    {
        //GenerateGrid();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    public void GenerateGrid()
    {
        if (tokensGameObject != null)
        {
            for (int r = 0; r < usedRows; r++)
            {
                for (int c = 0; c < usedColumns; c++)
                {
                    Destroy(tokensGameObject[c, r].gameObject);
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
                tokensGameObject[c, r] = Instantiate(tokenPrefab, board.transform, false);
                tokensGameObject[c, r].transform.position = initialTransform.position;
                tokensGameObject[c, r].SetActive(true);
                TokenInfo newInfo = tokenPresets[Random.Range(0, tokenPresets.Length)];
                tokens[c, r] = tokensGameObject[c, r].GetComponent<Token>();
                tokens[c, r].SetData(newInfo.tokenType, newInfo.icon, newInfo.points);
                tokens[c, r].gridIndex = new Vector2(c, r);
                float posX = c * (tokens[c, r].GetComponent<RectTransform>().rect.width + tileSpacing);
                float posY = r * (-tokens[c, r].GetComponent<RectTransform>().rect.height - tileSpacing);
                tokensGameObject[c, r].transform.position = new Vector2(initialTransform.position.x + posX, initialTransform.position.y + posY);
            }
        }
    }

    public void SpawnNewToken(int column, int row)
    {
        tokensGameObject[column, row] = Instantiate(tokenPrefab, board.transform, false);
        tokensGameObject[column, row].transform.position = initialTransform.position;
        tokensGameObject[column, row].transform.position += new Vector3(0, 300, 0);
        tokensGameObject[column, row].SetActive(true);
        TokenInfo newInfo = tokenPresets[Random.Range(0, tokenPresets.Length)];
        tokens[column, row] = tokensGameObject[column, row].GetComponent<Token>();
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
}
