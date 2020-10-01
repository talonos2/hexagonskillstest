using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolygonUISlider : MonoBehaviour
{
    public int attributeNumber;
    public Color colorOfUnlockText;
    public string unlockName;

    private Image[] toMakeOpaque = new Image[3];
    private Slider slider;

    private float targetx;
    private float targeto;
    private float currento;
    private float sourcex;
    private float sourceo;

    RectTransform myTransform;

    void Start()
    {
        slider = GetComponent<Slider>();
        toMakeOpaque = GetComponentsInChildren<Image>();
        SetOpacity(0);
        slider.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public void SetLevel(float newValue)
    {
        slider.value = newValue;
    }

    public void SetOpacity(float newValue)
    {
        for (int x = 0; x < toMakeOpaque.Length; x++)
        {
            Color oldColor = toMakeOpaque[x].color;
            toMakeOpaque[x].color = new Color(oldColor.r, oldColor.g, oldColor.b, newValue);
        }
        currento = newValue;
    }

    public void UpdateAttribute(float newValue)
    {
        GameController.Instance.UpdateUserHex(attributeNumber, newValue);
    }

    internal void SetCurrentPositionAndOpacityAsAnchor()
    {
        sourcex = this.GetComponent<RectTransform>().localPosition.x;
        sourceo = currento;
    }

    internal void SetOffScreenAsTarget(int bar)
    {
        targetx = 0;
        targeto = 0;
    }

    internal void SetOnScreenAsTarget(int bar)
    {
        targetx = (bar * 12) - (attributeNumber * 24);
        targeto = 1;
        slider.gameObject.SetActive(true);
    }

    internal void Tween(float t)
    {
        myTransform = this.GetComponent<RectTransform>();

        myTransform.anchoredPosition = new Vector2 (Mathf.Lerp(sourcex, targetx, t), myTransform.localPosition.y);
        myTransform.ForceUpdateRectTransforms();

        SetOpacity(Mathf.Lerp(sourceo, targeto, t));
    }
}
