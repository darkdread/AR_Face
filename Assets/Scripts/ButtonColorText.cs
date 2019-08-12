using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonColorText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public Color32 color;
    private Text text;

    private Color32 originalColor;

    private void Awake(){
        text = GetComponentInChildren<Text>();
        originalColor = text.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = color;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = originalColor;
    }
}