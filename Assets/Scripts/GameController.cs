using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController i;

    //Copied and adapted from other singleton code I've used in personal projects.
    public static GameController Instance
    {
        get
        {
            if (i)
            {
                return i;
            }
            else
            {
                GameController sought_controller = (GameController)FindObjectOfType(typeof(GameController));
                if (sought_controller)
                {
                    i = sought_controller;
                }
                else
                {
                    GameObject go = new GameObject();
                    i = go.AddComponent<GameController>();
                }
                DontDestroyOnLoad(i);
                return i;
            }
        }
    }

    //Designer tweakable consts. (See HexagonMaker for full explaination.)
    public float initialTweenDuration = 1;
    public float tweenEasingIntensity = 2;
    public float hexSeperation = 2.5f;

    //Public variables
    public HexagonMaker referenceHex;
    public HexagonMaker userHex;
    public PolygonUISlider[] attributeSliders = new PolygonUISlider[10];
    public TextMeshProUGUI matchText;
    public TextMeshProUGUI unlockText;

    private int levelNumber = 0;
    private float remainingTween;
    private bool moveHexes;
    private Material referenceMaterial;
    private Material userMaterial;
    private float[] referenceAttributes = new float[10];
    private float[] userAttributes = new float[10];
    private float matchBarTargetOpacity;
    private float matchBarCurrentOpacity;
    private bool matchBarIsFading;
    private bool barsAreMoving;
    private float barTweenPosition;
    private bool unlockTextIsFading;
    private float unlockTextCurrentOpacity;
    private bool matchBarAppeared = false;

    void Start()
    {
        remainingTween = initialTweenDuration;

        //Because these assume that the meches have already been initialized, we must
        //remember to ensure GameController's start method is called *after* the hexagon's
        //using Unity's script execution ordering menu.
        referenceMaterial = referenceHex.GetComponent<MeshRenderer>().sharedMaterial;
        userMaterial = userHex.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void Update()
    {
        if (moveHexes)
        {
            remainingTween -= Time.deltaTime;
            if (remainingTween > 0)
            {
                float t = remainingTween / initialTweenDuration;
                t = Mathf.Pow(t, tweenEasingIntensity);
                //Could just multiply hex seperation by 1-t, but I usually use lerp even in simple lerps to keep myself in the habit.
                float seperation = Mathf.Lerp(0, hexSeperation, 1-t);
                userHex.transform.position = new Vector3(seperation, 0, 0);
                referenceHex.transform.position = new Vector3(-seperation, 0, 0);
            }
            else
            {
                userHex.transform.position = new Vector3(hexSeperation, 0, 0);
                referenceHex.transform.position = new Vector3(-hexSeperation, 0, 0);
                moveHexes = false;
            }
        }

        if (matchBarIsFading)
        {
            matchBarCurrentOpacity = Mathf.Lerp(matchBarCurrentOpacity, matchBarTargetOpacity, .003f);
            if (Math.Abs(matchBarCurrentOpacity- matchBarTargetOpacity)<.01)
            {
                matchBarCurrentOpacity = matchBarTargetOpacity;
                matchBarIsFading = false;
            }
            matchText.color = new Color(1, 1, 1, matchBarCurrentOpacity);
        }
    }

    public void LateUpdate()
    {
        if (barsAreMoving)
        {
            barTweenPosition = Mathf.Lerp(barTweenPosition, 1, .006f);
            if (barTweenPosition > .99)
            {
                barTweenPosition = 1;
                barsAreMoving = false;
            }
            for (int x = 0; x < 10; x++)
            {
                attributeSliders[x].Tween(barTweenPosition);
            }
        }
    }

    /// <summary>
    /// User double-clicked the polygon: submit and score.
    /// </summary>
    public void Submit()
    {
        switch (levelNumber)
        {
            case 0:
                DoFirstTweening();
                RandomizeReferenceHexagon(1);
                DisplayBar(0);
                SetBarsToRedTrainingAmount();
                break;
            case 1:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(2);
                DisplayBar(1);
                SetBarsToGreenTrainingAmount();
                break;
            case 2:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(2);
                SetBarsToZero();
                BeginHexSpin();
                break;
            case 3:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(2);
                SetBarsToZero();
                StartTimer();
                break;
            case 4:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(2);
                SetBarsToZero();
                ResetTimer();
                break;
            case 5:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(3);
                DisplayBar(2);
                SetBarsToZero();
                ResetTimer();
                break;
            case 6:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(4);
                DisplayBar(3);
                BeginHexFacing();
                SetBarsToZero();
                ResetTimer();
                break;
            case 7:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(5);
                DisplayBar(4);
                SetBarsToZero();
                ResetTimer();
                break;
            case 8:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(6);
                DisplayBar(5);
                SetBarsToZero();
                ResetTimer();
                break;
            case 9:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(6);
                DisplayBar(5);
                SetBarsToZero();
                ResetTimer();
                break;
        }
    }

    private void BeginHexFacing()
    {
        throw new NotImplementedException();
    }

    private void BeginHexSpin()
    {
        throw new NotImplementedException();
    }

    private void ResetTimer()
    {
        throw new NotImplementedException();
    }

    private void StartTimer()
    {
        throw new NotImplementedException();
    }

    private void HideMatchBar()
    {
        matchBarTargetOpacity = 0;
        matchBarIsFading = true;
        matchBarAppeared = false;
    }

    private void SetBarsToZero()
    {
        for (int x = 0; x < 10; x++)
        {
            attributeSliders[0].SetLevel(0);
        }
    }

    private void ScoreUserHexagon()
    {
        throw new NotImplementedException();
    }

    //We want to make sure the user drags the bar an appropriate amount in the first few levels, so we ensure the slider starts on
    //the "wrong" side of the 50% mark so they must drag it over half a bar before it's correct. This aids in user onboarding.
    private void SetBarsToRedTrainingAmount()
    {
        SetBarsToZero();
        attributeSliders[0].SetLevel(referenceAttributes[0] > .5f ? 0: 1);
        attributeSliders[1].SetLevel(.25f);
    }

    private void SetBarsToGreenTrainingAmount()
    {
        SetBarsToZero();
        attributeSliders[0].SetLevel(referenceAttributes[0] > .5f ? 0 : 1);
        attributeSliders[1].SetLevel(referenceAttributes[1] > .5f ? 0 : 1);
    }

    private void DisplayBar(int bar)
    {
        for (int x = 0; x < 10; x++)
        {
            attributeSliders[x].SetCurrentPositionAndOpacityAsAnchor();
            attributeSliders[x].SetOffScreenAsTarget(bar);
        }

        for (int x = 0; x <= bar; x++)
        {
            attributeSliders[x].SetOnScreenAsTarget(bar);
        }

        barTweenPosition = 0;
        barsAreMoving = true;
        unlockText.color = attributeSliders[bar].colorOfUnlockText;
        unlockText.text = attributeSliders[bar].unlockName + " bar unlocked!";
        unlockTextIsFading = true;
        unlockTextCurrentOpacity = 1;
    }

    private void DisplayMatchBar()
    {
        matchBarTargetOpacity = 1;
        matchBarIsFading = true;
        matchBarAppeared = true;
    }

    //Here is where we determine what random color the hexagon is.

    //Note that the color is random as early as the first time you double click it, but
    //the rules constraining on *how* random it is change the further you get in the game.
    private void RandomizeReferenceHexagon(int numberOfAttributesRandomized)
    {
        //Base case: Only redness is randomized.
        referenceAttributes[0] = UnityEngine.Random.Range(.01f, .99f);
        referenceAttributes[1] = .25f; //Greenness
        referenceAttributes[2] = 0f;   //Blueness
        referenceAttributes[3] = 0f;   //Metalicity or smoothness, not sure which yet.
        referenceAttributes[4] = 0f;   //Etc.

        //Randomize additional attributes.
        for (int x = 2; x < numberOfAttributesRandomized; x++)
        if (numberOfAttributesRandomized >= x)
        {
            referenceAttributes[x-1] = UnityEngine.Random.Range(.01f, .99f);
        }

        referenceMaterial.SetColor("tex_col", new Color(referenceAttributes[0], referenceAttributes[1], referenceAttributes[2]));
    }

    private void DoFirstTweening()
    {
        userHex.gameObject.SetActive(true);
        moveHexes = true;
    }

    public void UpdateUserHex(int attribute, float value)
    {
        if (levelNumber == 0&&!matchBarAppeared&&(value!=0&&value!=1&&value!=.25f))
        {
            DisplayMatchBar();
        }
        if (levelNumber == 2&&matchBarAppeared && (value != 0 && value != 1 && value != .25f))
        {
            HideMatchBar();
        }
        userAttributes[attribute] = value;

        userMaterial.SetColor("tex_col", new Color(userAttributes[0], userAttributes[1], userAttributes[2]));

        if (matchBarCurrentOpacity > .01f)
        {
            matchText.text = "Match: " + (int)(GetMatchAmount(levelNumber) * 100f);
        }
    }

    private float GetMatchAmount(int levelNumber)
    {
        throw new NotImplementedException();
    }
}
