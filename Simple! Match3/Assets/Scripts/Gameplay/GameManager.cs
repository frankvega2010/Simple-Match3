using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void OnGameManagerAction(int amount);
    public static OnGameManagerAction OnPlayerScore;
    public static OnGameManagerAction OnPlayerMatch;
    public static OnGameManagerAction OnGameFinished;
    public static OnGameManagerAction OnGameStart;

    [Header("Main Config"),Space]
    public BoardGrid grid = null;
    public GameObject singleUseAudioPrefab = null;
    public int minimumMatch = 0;
    public int maxTurns = 0;

    [Header("Audio Config"), Space]
    public AudioClip[] tokenSelectSound;
    public AudioClip tokenDeselectSound;
    public AudioClip[] comboSound;
    public AudioSource backgroundMusic;
    private AudioSource audioSource;

    [Header("Public Data"), Space]
    public int currentPoints = 0;
    public List<Token> currentChain = new List<Token>();
    public bool inputEnabled = false;
    [SerializeField]
    private List<Token> movingTokens = new List<Token>();
    private int turnsLeft = 0;
    [SerializeField]
    private bool isGameFinished = false;

    // Start is called before the first frame update
    void Start()
    {
        Token.OnTokenSelected += CheckTokenChain;
        Token.OnTokenLerpFinish += CheckInputAvailability;
        audioSource = GetComponent<AudioSource>();
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("GenerateGrid"))
        {
            Restart();
        }

        if(Input.GetMouseButtonUp(0))
        {
            DoMatch();
        }
    }

    public void Restart()
    {
        currentPoints = 0;
        turnsLeft = maxTurns;
        isGameFinished = false;

        if (OnPlayerMatch != null)
        {
            OnPlayerMatch(turnsLeft);
        }

        if (OnPlayerScore != null)
        {
            OnPlayerScore(currentPoints);
        }

        if(OnGameStart != null)
        {
            OnGameStart(0);
        }

        backgroundMusic.pitch = 1;
        currentChain.Clear();
        grid.GenerateGrid();
        AdjustGrid();
    }

    public void DoMatch()
    {
        if(!isGameFinished)
        {
            if (currentChain.Count >= minimumMatch) // Do match
            {
                if (turnsLeft > 0)
                {
                    int points = 0;

                    foreach (Token t in currentChain)
                    {
                        points += t.points;
                        Destroy(grid.GetCurrentTokens()[(int)t.gridIndex.x, (int)t.gridIndex.y].gameObject);
                        grid.GetCurrentTokens()[(int)t.gridIndex.x, (int)t.gridIndex.y] = null;
                    }

                    points *= currentChain.Count;

                    currentChain.Clear();
                    inputEnabled = false;
                    UpdateGrid();
                    RefillGrid();

                    //Give points * amount of tokens in chain

                    currentPoints += points;

                    if (OnPlayerScore != null)
                    {
                        OnPlayerScore(currentPoints);
                    }

                    turnsLeft--;

                    if(turnsLeft <= Mathf.Round(maxTurns * 0.25f))
                    {
                        backgroundMusic.pitch = 1.85f;
                    }

                    if (OnPlayerMatch != null)
                    {
                        OnPlayerMatch(turnsLeft);
                    }

                    PlayComboSound();

                }
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

    public void CheckTokenChain(Token currentToken)
    {
        if(inputEnabled && !isGameFinished)
        {
            bool tokenWasDeleted = false;

            if (currentChain.Count <= 0)
            {
                currentChain.Add(currentToken);
                currentToken.OnAdd();
                PlaySelectSound();
            }
            else
            {
                if (currentChain.Count - 2 >= 0)
                {
                    if (currentToken == currentChain[currentChain.Count - 2])
                    {
                        Token tokenToRemove = currentChain[currentChain.Count - 1];
                        currentChain.Remove(tokenToRemove);
                        tokenToRemove.OnRemove();
                        PlayDeselectSound();
                        tokenWasDeleted = true;
                    }
                }

                if(!tokenWasDeleted)
                {
                    if (!currentChain.Contains(currentToken))
                    {
                        if (CheckAdjacentTokens(currentToken)) // Add new token
                        {
                            currentChain.Add(currentToken);
                            currentToken.OnAdd();
                            PlaySelectSound();
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
                            grid.GetCurrentTokensGameObject()[c, newTokenRow] = grid.GetCurrentTokensGameObject()[c, r];
                            currentTokens[c, newTokenRow] = currentTokens[c, r];
                            currentTokens[c, r] = null;
                            grid.GetCurrentTokensGameObject()[c, r] = null;

                            currentTokens[c, newTokenRow].oldPosition = new Vector2(grid.initialTransform.position.x, grid.initialTransform.position.y)
                                + new Vector2(c * (currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.width + grid.tileSpacing),
                                               r * (-currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.height - grid.tileSpacing));

                            currentTokens[c, newTokenRow].newPosition = new Vector2(grid.initialTransform.position.x, grid.initialTransform.position.y)
                                + new Vector2(c * (currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.width + grid.tileSpacing),
                                     newTokenRow * (-currentTokens[c, newTokenRow].GetComponent<RectTransform>().rect.height - grid.tileSpacing));

                            currentTokens[c, newTokenRow].gridIndex = new Vector2(c, newTokenRow);
                            currentTokens[c, newTokenRow].StartLerp();

                            if(!movingTokens.Contains(currentTokens[c, newTokenRow]))
                            {
                                movingTokens.Add(currentTokens[c, newTokenRow]);
                            }
                            //movingTokens.Add(currentTokens[c, newTokenRow]);
                        }

                    }
                }

            }
        }
    }

    public void RefillGrid()
    {
        Token[,] currentTokens = grid.GetCurrentTokens();

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if(currentTokens[c,r] == null)
                {
                    grid.SpawnNewToken(c, r);

                    currentTokens[c, r].oldPosition = new Vector2(grid.initialTransform.position.x, grid.initialTransform.position.y)
                        + new Vector2(c * (currentTokens[c, r].GetComponent<RectTransform>().rect.width + grid.tileSpacing),
                                       0)
                        + new Vector2(0,+250);

                    currentTokens[c, r].newPosition = new Vector2(grid.initialTransform.position.x, grid.initialTransform.position.y)
                        + new Vector2(c * (currentTokens[c, r].GetComponent<RectTransform>().rect.width + grid.tileSpacing),
                             r * (-currentTokens[c, r].GetComponent<RectTransform>().rect.height - grid.tileSpacing));

                    if (!movingTokens.Contains(currentTokens[c, r]))
                    {
                        movingTokens.Add(currentTokens[c, r]);
                    }

                    //movingTokens.Add(currentTokens[c, r]);

                    currentTokens[c, r].StartLerp();
                }
            }
        }
    }

    public void CheckInputAvailability(Token currentToken)
    {
        if(movingTokens.Contains(currentToken))
        {
            movingTokens.Remove(currentToken);
        }

        if (movingTokens.Count <= 0)
        {
            if(!IsMatchAvailable())
            {
                inputEnabled = true;

                if (turnsLeft <= 0)
                {
                    // GAME FINISHED

                    isGameFinished = true;

                    if (OnGameFinished != null)
                    {
                        OnGameFinished(0);
                    }
                }
            }
        }
    }

    public bool IsMatchAvailable()
    {
        inputEnabled = false;
        bool foundMatch = false;

        if (!CheckMatchHorizontal())
        {
            foundMatch = true;
        }

        if (!CheckMatchVertical())
        {
            foundMatch = true;
        }

        /*if(foundMatch)
        {
            
        }*/

        UpdateGrid();
        RefillGrid();

        return foundMatch;
    }

    public void SetRandomData(int column, int row)
    {
        TokenInfo newInfo = grid.tokenPresets[Random.Range(0, grid.tokenPresets.Length)];
        grid.GetCurrentTokens()[column, row].SetData(newInfo.tokenType, newInfo.icon, newInfo.points);
    }

    public bool CheckRepeatedColorsVertical()
    {
        bool isDone = true;
        int findSameColor = 0;
        List<Vector2> tokenToRandomizeIndex = new List<Vector2>();

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (r != 0 && r < grid.GetUsedRows() - 1)
                {
                    findSameColor++;
                    tokenToRandomizeIndex.Add(new Vector2(c, r));

                    int automaticMinMatch = 0;
                    int automaticLoop = 0;
                    if(minimumMatch % 2 == 0)
                    {
                        automaticMinMatch = minimumMatch - 1;
                    }
                    else
                    {
                        automaticMinMatch = minimumMatch;
                    }

                    automaticLoop = grid.GetUsedRows() / 2;

                    for (int i = 1; i <= automaticLoop; i++)
                    {
                        if (r - i >= 0)
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c, r - i].tokenType)
                            {
                                findSameColor++;
                                tokenToRandomizeIndex.Add(new Vector2(c, r-i));
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }
                    }

                    for (int i = 1; i <= automaticLoop; i++)
                    {
                        if (r + i < grid.GetUsedRows())
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c, r + i].tokenType)
                            {
                                findSameColor++;
                                tokenToRandomizeIndex.Add(new Vector2(c, r + i));
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }
                    }

                    if (findSameColor >= automaticMinMatch)
                    {
                        foreach (Vector2 v in tokenToRandomizeIndex)
                        {
                            SetRandomData((int)v.x, (int)v.y);
                        }

                        isDone = false;
                    }

                    tokenToRandomizeIndex.Clear();
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
        List<Vector2> tokenToRandomizeIndex = new List<Vector2>();

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (c != 0 && c < grid.GetUsedColumns() - 1)
                {
                    findSameColor++;
                    tokenToRandomizeIndex.Add(new Vector2(c, r));

                    int automaticMinMatch = 0;
                    int automaticLoop = 0;
                    if (minimumMatch % 2 == 0)
                    {
                        automaticMinMatch = minimumMatch - 1;
                    }
                    else
                    {
                        automaticMinMatch = minimumMatch;
                    }

                    automaticLoop = grid.GetUsedColumns() / 2;


                    for (int i = 1; i <= automaticLoop; i++)
                    {
                        if (c - i >= 0)
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c - i, r].tokenType)
                            {
                                findSameColor++;
                                tokenToRandomizeIndex.Add(new Vector2(c-i, r));
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }
                    }
                        

                    for (int i = 1; i <= automaticLoop; i++)
                    {
                        if (c + i < grid.GetUsedColumns())
                        {
                            if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c + i, r].tokenType)
                            {
                                findSameColor++;
                                tokenToRandomizeIndex.Add(new Vector2(c + i, r));
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }
                    }

                    if (findSameColor >= automaticMinMatch)
                    {
                        foreach (Vector2 v in tokenToRandomizeIndex)
                        {
                            SetRandomData((int)v.x, (int)v.y);
                        }

                        isDone = false;
                    }

                    tokenToRandomizeIndex.Clear();
                    findSameColor = 0;
                }
            }
        }

        return isDone;
    }


    public bool CheckMatchHorizontal()
    {
        bool isDone = true;
        int findSameColor = 0;
        List<Vector2> tokenToDeleteIndex = new List<Vector2>();

        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (c != 0 && c < grid.GetUsedColumns() - 1)
                {
                    findSameColor++;
                    tokenToDeleteIndex.Add(new Vector2(c, r));

                    int automaticMinMatch = 0;
                    int automaticLoop = 0;
                    if (minimumMatch % 2 == 0)
                    {
                        automaticMinMatch = minimumMatch - 1;
                    }
                    else
                    {
                        automaticMinMatch = minimumMatch;
                    }

                    automaticLoop = grid.GetUsedColumns() / 2;
                    
                        
                    if (grid.GetCurrentTokens()[c, r] != null)
                    {
                        for (int i = 1; i <= automaticLoop; i++)
                        {
                            if (c - i >= 0 && grid.GetCurrentTokens()[c - i, r] != null)
                            {
                                if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c - i, r].tokenType)
                                {
                                    findSameColor++;
                                    tokenToDeleteIndex.Add(new Vector2(c - i, r));
                                }
                                else
                                {
                                    i = automaticLoop;
                                }
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }


                        for (int i = 1; i <= automaticLoop; i++)
                        {
                            if (c + i < grid.GetUsedColumns() && grid.GetCurrentTokens()[c + i, r] != null)
                            {
                                if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c + i, r].tokenType)
                                {
                                    findSameColor++;
                                    tokenToDeleteIndex.Add(new Vector2(c + i, r));
                                }
                                else
                                {
                                    i = automaticLoop;
                                }
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }
                    }
                        

                    if (findSameColor >= automaticMinMatch)
                    {
                        //GIVE POINTS

                        PlayComboSound();

                        int points = 0;

                        foreach (Vector2 v in tokenToDeleteIndex)
                        {
                            points += grid.GetCurrentTokens()[(int)v.x, (int)v.y].points;
                            Destroy(grid.GetCurrentTokens()[(int)v.x, (int)v.y].gameObject);
                            grid.GetCurrentTokens()[(int)v.x, (int)v.y] = null;
                        }

                        points *= findSameColor;

                        currentPoints += points;

                        if(OnPlayerScore != null)
                        {
                            OnPlayerScore(currentPoints);
                        }

                        isDone = false;
                    }

                    tokenToDeleteIndex.Clear();
                    findSameColor = 0;
                }
            }
        }

        return isDone;
    }

    public bool CheckMatchVertical()
    {
        bool isDone = true;
        int findSameColor = 0;
        List<Vector2> tokenToDeleteIndex = new List<Vector2>();


        for (int r = 0; r < grid.GetUsedRows(); r++)
        {
            for (int c = 0; c < grid.GetUsedColumns(); c++)
            {
                if (r != 0 && r < grid.GetUsedRows() - 1)
                {
                    findSameColor++;
                    tokenToDeleteIndex.Add(new Vector2(c, r));

                    int automaticMinMatch = 0;
                    int automaticLoop = 0;
                    if (minimumMatch % 2 == 0)
                    {
                        automaticMinMatch = minimumMatch - 1;
                    }
                    else
                    {
                        automaticMinMatch = minimumMatch;
                    }

                    automaticLoop = grid.GetUsedRows() / 2;

                    if (grid.GetCurrentTokens()[c, r] != null)
                    {
                        for (int i = 1; i <= automaticLoop; i++)
                        {
                            if (r - i >= 0 && grid.GetCurrentTokens()[c, r - i] != null)
                            {
                                if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c, r - i].tokenType)
                                {
                                    findSameColor++;
                                    tokenToDeleteIndex.Add(new Vector2(c, r - i));
                                }
                                else
                                {
                                    i = automaticLoop;
                                }
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }

                        for (int i = 1; i <= automaticLoop; i++)
                        {
                            if (r + i < grid.GetUsedRows() && grid.GetCurrentTokens()[c, r + i] != null)
                            {
                                if (grid.GetCurrentTokens()[c, r].tokenType == grid.GetCurrentTokens()[c, r + i].tokenType)
                                {
                                    findSameColor++;
                                    tokenToDeleteIndex.Add(new Vector2(c, r+i));
                                }
                                else
                                {
                                    i = automaticLoop;
                                }
                            }
                            else
                            {
                                i = automaticLoop;
                            }
                        }
                    }

                    

                    if (findSameColor >= automaticMinMatch)
                    {
                        //GIVE POINTS

                        PlayComboSound();

                        int points = 0;

                        foreach (Vector2 v in tokenToDeleteIndex)
                        {
                            points += grid.GetCurrentTokens()[(int)v.x, (int)v.y].points;
                            Destroy(grid.GetCurrentTokens()[(int)v.x, (int)v.y].gameObject);
                            grid.GetCurrentTokens()[(int)v.x, (int)v.y] = null;
                        }

                        points *= findSameColor;
                        currentPoints += points;

                        if (OnPlayerScore != null)
                        {
                            OnPlayerScore(currentPoints);
                        }

                        isDone = false;
                    }

                    tokenToDeleteIndex.Clear();
                    findSameColor = 0;
                }
            }
        }

        return isDone;
    }

    public void PlaySelectSound()
    {
        int randomIndex = Random.Range(0, tokenSelectSound.Length);

        if (audioSource)
        {
            audioSource.clip = tokenSelectSound[randomIndex];
            audioSource.Play();
        }
    }

    public void PlayDeselectSound()
    {
        if (audioSource)
        {
            audioSource.clip = tokenDeselectSound;
            audioSource.Play();
        }
    }

    public void PlayComboSound()
    {
        int randomIndex = Random.Range(0, comboSound.Length);

        GameObject newSound = Instantiate(singleUseAudioPrefab);
        newSound.SetActive(true);
        newSound.GetComponent<AudioSourceSingleTime>().SetUpAndPlayClip(comboSound[randomIndex]);
    }

    private void OnDestroy()
    {
        Token.OnTokenSelected -= CheckTokenChain;
        Token.OnTokenLerpFinish -= CheckInputAvailability;
    }
}
