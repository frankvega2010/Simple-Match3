using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Token : MonoBehaviour , IPointerClickHandler
{

    public delegate void OnTokenAction(Token currentToken);
    public static OnTokenAction OnTokenSelected;
    public static int id;

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
    public Color selectedColor;
    public Color normalColor;

    // Start is called before the first frame update
    void Awake()
    {
        iconImage = GetComponent<Image>();
        id++;
        name = "Token " + id;
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

    public void OnRemove()
    {
        iconImage.color = normalColor;
    }

    public void OnCancel()
    {
        iconImage.color = normalColor;
    }

    public void OnAdd()
    {
        iconImage.color = selectedColor;
    }

    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (OnTokenSelected != null)
        {
            OnTokenSelected(this);
        }

        Debug.Log("SELECTED");
    }
}
