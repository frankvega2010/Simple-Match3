using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour
{
    public GameManager gameManager;
    public Text turnsText;
    public Text pointsText;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.OnPlayerMatch += UpdateTurnsText;
        GameManager.OnPlayerScore += UpdatePointsText;
    }

    /*// Update is called once per frame
    void Update()
    {
        
    }*/

    private void UpdatePointsText(int points)
    {
        pointsText.text = "Points: " + '\n' + points;
    }

    private void UpdateTurnsText(int turns)
    {
        turnsText.text = "Turns Left: " + '\n' + turns;
    }

    private void OnDestroy()
    {
        GameManager.OnPlayerMatch -= UpdateTurnsText;
        GameManager.OnPlayerScore -= UpdatePointsText;
    }
}
