using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SimpleMatch3.Gameplay
{
    public class Token : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
    {
        public delegate void OnTokenAction(Token currentToken);
        public static OnTokenAction OnTokenSelected;
        public static OnTokenAction OnTokenLerpFinish;
        public static OnTokenAction OnTokenFinishRemoveAnimation;
        public static int id;

        public enum TOKEN_TYPES
        {
            EMPTY,
            SPIRIT,
            ICE,
            FOREST,
            FLASH,
            FIRE,
            EARTH,
            DEMON,
            INSECT,
            AIR,
            ALL_TYPES
        }

        [Header("Main Config"), Space]
        public TOKEN_TYPES tokenType;
        public Sprite icon;
        public int points;
        public float moveSpeed;

        [Header("Color Config"), Space]
        public Color selectedColor;
        public Color normalColor;

        [Header("Public Data"), Space]
        public Vector2 oldPosition;
        public Vector2 newPosition;
        public Vector2 gridIndex;
        private Image iconImage;
        private bool startLerp;
        private float lerpValue;
        private Animator animator;

        // Start is called before the first frame update
        void Awake()
        {
            animator = GetComponent<Animator>();
            iconImage = GetComponent<Image>();
            id++;
            name = "Token " + id;
        }

        // Update is called once per frame
        void Update()
        {
            if (startLerp)
            {
                lerpValue += Time.deltaTime * moveSpeed;

                transform.position = Vector2.Lerp(oldPosition, newPosition, lerpValue);

                if (lerpValue >= 1)
                {
                    lerpValue = 0;
                    startLerp = false;
                    if (OnTokenLerpFinish != null)
                    {
                        OnTokenLerpFinish(this);
                    }
                }
            }
        }

        public void SetData(TOKEN_TYPES type, Sprite newIcon, int newPoints)
        {
            tokenType = type;
            icon = newIcon;
            points = newPoints;
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

        public void StartLerp()
        {
            startLerp = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (OnTokenSelected != null)
            {
                OnTokenSelected(this);
            }
        }

        public void ExecuteAnimation(string animationName)
        {
            animator.Play(animationName);
        }

        public void OnFinishedRemoveAnimation()
        {
            if (OnTokenFinishRemoveAnimation != null)
            {
                OnTokenFinishRemoveAnimation(this);
            }
        }

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (Input.GetMouseButton(0))
            {
                if (OnTokenSelected != null)
                {
                    OnTokenSelected(this);
                }
            }
        }
    }
}
    
