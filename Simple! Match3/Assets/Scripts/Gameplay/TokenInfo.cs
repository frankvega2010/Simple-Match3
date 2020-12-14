using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Token Data", menuName = "Frank/TokenData", order = 1)]
public class TokenInfo : ScriptableObject
{
    public Token.TOKEN_TYPES tokenType;
    public Sprite icon;
    public int points;
}
