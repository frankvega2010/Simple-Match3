using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Token : MonoBehaviour
{
    public enum TOKEN_TYPES
    {
        EMPTY,
        RUBY,
        EMERALD,
        AZZURE,
        ALL_TYPES
    }

    public TOKEN_TYPES tokenType;
    public Sprite icon;
    public Image iconImage;

    // Start is called before the first frame update
    void Awake()
    {
        iconImage = GetComponent<Image>();
    }

    /*// Update is called once per frame
    void Update()
    {
        
    }*/

    public void SetData(TOKEN_TYPES type, Sprite newIcon)
    {
        tokenType = type;
        icon = newIcon;
        iconImage.sprite = icon;
    }
}
