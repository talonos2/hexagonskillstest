using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public float timeThatUnlockTextLasts = 1.5f;
    public float timeBeforeUnlockTextFades = 1f;
    public float timeThatUnlockPulseLasts = .4f;
    public float sizeOfPulse = 1.5f;
    public float rotationSpeed = 1;
    public float additionalRotationSpeedPerLevel = 1;
    public float timeBetweenScoreShownAndScored = 1;
    public float maxTimeForScoreToBeScored = 2;

    //Public variables
    public HexagonMaker referenceHex;
    public HexagonMaker userHex;
    public PolygonUISlider[] attributeSliders = new PolygonUISlider[10];
    public TextMeshProUGUI matchText;
    public TextMeshProUGUI unlockText;
    public TextMeshProUGUI unlockPulseText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hintText;
    public MeshRenderer backgroundRenderer;
    public ParticleSystem matchParticles;
    public ParticleSystem failParticles;
    public Light rotatingLight;
    public Light ambientLight;
    public Image menuButton;
    public TextMeshProUGUI menuButtonText;
    public GameObject MenuImage;
    public GameObject BlockAllOtherStuff;

    private int levelNumber = 0;
    private float remainingTween;
    private bool moveHexes;
    private Material referenceMaterial;
    private Material userMaterial;
    private Material skyMaterial;
    private readonly float[] referenceAttributes = new float[10];
    private readonly float[] userAttributes = new float[10];
    private readonly int[] slidersByLevelNumber = new int[] { 1, 1, 2, 2, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6, 6};
    private float matchBarTargetOpacity;
    private float matchBarCurrentOpacity;
    private bool matchBarIsFading;
    private bool barsAreMoving;
    private float barTweenPosition;
    private float timeSinceUnlockTextAppeared = 0;
    private bool unlockTextIsFading;
    private bool matchBarAppeared = false;
    private bool shownHint2;
    private bool matchParticlesPlayed;
    private bool hexSpin = false;
    private float rotationAmount = 0;
    private bool timerOn = false;
    private float timeLeft = 0;
    private bool timerFinishedFadingIn = false;
    private bool scoreFinishedFadingIn = false;
    private bool scoreStartedFadingIn = false;
    private float pointsCurrentOpacity = 0;
    private float timerCurrentOpacity = 0;
    private int totalScore = 0;
    private float timeSinceLastPointScored = 0;
    private int unscoredPoints = 0;
    private bool hexFacing = false;
    private float inwardFacingDirection = 0;
    private float timeSinceScoringAppeared = 0;
    private float skyCurrentOpacity = 1;
    private bool fadeSky = false;
    private bool fadeSkyComplete = false;
    private float fadeMenuCurrentOpacity = 0;
    private bool fadeMenuButton = false;
    private bool fadeMenuButtonComplete = false;
    private float hintTextCurrentOpacity = 0;
    private bool fadeHintTextComplete = true;


    void Start()
    {
        remainingTween = initialTweenDuration;

        //Because these assume that the meches have already been initialized, we must
        //remember to ensure GameController's start method is called *after* the hexagon's
        //using Unity's script execution ordering menu.
        referenceMaterial = referenceHex.GetComponent<MeshRenderer>().sharedMaterial;
        userMaterial = userHex.GetComponent<MeshRenderer>().sharedMaterial;

        skyMaterial = new Material(backgroundRenderer.sharedMaterial);
        backgroundRenderer.sharedMaterial = skyMaterial;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (MenuImage.active)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
            return;
        }
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
            matchText.color = new Color(matchText.color.r, matchText.color.g, matchText.color.b, matchBarCurrentOpacity);
        }

        if (unlockTextIsFading)
        {
            timeSinceUnlockTextAppeared += Time.deltaTime;
            if (timeSinceUnlockTextAppeared>timeThatUnlockTextLasts)
            {
                unlockTextIsFading = false;
                unlockText.color = Color.clear;
                unlockPulseText.color = Color.clear;
            }
            else if (timeSinceUnlockTextAppeared > timeBeforeUnlockTextFades)
            {
                unlockPulseText.color = Color.clear;
                float t = (timeSinceUnlockTextAppeared - timeBeforeUnlockTextFades) / (timeThatUnlockTextLasts - timeBeforeUnlockTextFades);
                unlockText.color = new Color(unlockText.color.r, unlockText.color.g, unlockText.color.b, Mathf.Lerp(1, 0, t));
            }
            else
            {
                float t = timeSinceUnlockTextAppeared / (timeThatUnlockPulseLasts);
                float scale = Mathf.Lerp(1, sizeOfPulse, t);
                unlockPulseText.transform.localScale = new Vector3(scale, scale, 1);
                unlockPulseText.color = new Color(unlockText.color.r, unlockText.color.g, unlockText.color.b, Mathf.Lerp(1, 0, t));
            }
        }

        if (hexSpin)
        {
            rotationAmount += Time.deltaTime;
            if (hexFacing)
            {
                inwardFacingDirection = Mathf.Lerp(inwardFacingDirection, 20, .001f);
                rotatingLight.intensity = inwardFacingDirection * 2;
                ambientLight.intensity = 1 - (inwardFacingDirection / 50);
            }
            referenceHex.transform.rotation = Quaternion.Euler(0, inwardFacingDirection, rotationAmount);
            userHex.transform.rotation = Quaternion.Euler(0, -inwardFacingDirection, -rotationAmount);

            rotatingLight.transform.position = new Vector3(Mathf.Sin(rotationAmount/10)*4, Mathf.Cos(rotationAmount / 10) * 4, -10);
        }

        if (scoreStartedFadingIn&&!scoreFinishedFadingIn)
        {
            pointsCurrentOpacity = Mathf.Lerp(pointsCurrentOpacity, 1, .003f);
            if (pointsCurrentOpacity > .99f)
            {
                pointsCurrentOpacity = 1;
                scoreFinishedFadingIn = true;
            }
            scoreText.color = new Color(1, 1, 1, pointsCurrentOpacity);
        }

        if (timerOn && !timerFinishedFadingIn)
        {
            timerCurrentOpacity = Mathf.Lerp(timerCurrentOpacity, 1, .003f);
            if (timerCurrentOpacity > .99f)
            {
                timerCurrentOpacity = 1;
                timerFinishedFadingIn = true;
            }
            timerText.color = new Color(1, 1, 1, timerCurrentOpacity);
        }

        if (fadeSky && !fadeSkyComplete)
        {
            skyCurrentOpacity = Mathf.Lerp(skyCurrentOpacity, 0, .001f);
            if (skyCurrentOpacity < .01f)
            {
                skyCurrentOpacity = 0;
                fadeSkyComplete = true;
            }
            skyMaterial.SetFloat("tex_trans", 1-skyCurrentOpacity);
        }

        if (fadeMenuButton && !fadeMenuButtonComplete)
        {
            fadeMenuCurrentOpacity = Mathf.Lerp(fadeMenuCurrentOpacity, 1, .003f);
            if (fadeMenuCurrentOpacity > .99f)
            {
                fadeMenuCurrentOpacity = 1;
                fadeMenuButtonComplete = true;
            }
            menuButton.color = new Color(menuButton.color.r, menuButton.color.g, menuButton.color.b, fadeMenuCurrentOpacity);
            menuButtonText.color = new Color(menuButtonText.color.r, menuButtonText.color.g, menuButtonText.color.b, fadeMenuCurrentOpacity);
        }

        if (!fadeHintTextComplete)
        {
            hintTextCurrentOpacity = Mathf.Lerp(hintTextCurrentOpacity, 0, .001f);
            if (hintTextCurrentOpacity < .01f)
            {
                hintTextCurrentOpacity = 0;
                fadeHintTextComplete = true;
            }
            hintText.color = new Color(1, 1, 1, hintTextCurrentOpacity);
        }

        if (timerOn)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                Submit();
                failParticles.Play();
            }

            timerText.text = ""+(int)(timeLeft + 1f);
        }

        if (unscoredPoints > 0)
        {
            timeSinceScoringAppeared += Time.deltaTime;
            if (timeSinceScoringAppeared>timeBetweenScoreShownAndScored)
            {
                timeSinceLastPointScored += Time.deltaTime;
                while (timeSinceLastPointScored > (maxTimeForScoreToBeScored/160)) // 160 is most possible points in a round.
                {
                    timeSinceLastPointScored -= (maxTimeForScoreToBeScored / 160);
                    unscoredPoints--;
                    scoreText.text = "Score: " + (totalScore - unscoredPoints);
                    if (unscoredPoints <= 0)
                    {
                        break; //Prevents double-scoring the last point if there's a slow framerate.
                    }
                }
                if (unscoredPoints <= 0)
                {
                    HideMatchBar();
                }
            }
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
        if (levelNumber < 3)
        {
            float match = GetMatchAmount(slidersByLevelNumber[levelNumber]);
            {
                if (match < .95f)
                {
                    hintText.color = Color.white;
                    hintTextCurrentOpacity = 1;
                    fadeHintTextComplete = false;
                    return;
                }
            }
        }
        switch (levelNumber)
        {
            case 0:
                DoFirstTweening();
                RandomizeReferenceHexagon(1);
                DisplayBar(0);
                SetBarsToRedTrainingAmount();
                break;
            case 1:
                matchParticlesPlayed = false;
                RandomizeReferenceHexagon(2);
                DisplayBar(1);
                SetBarsToGreenTrainingAmount();
                break;
            case 2:
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
                BasicLevel(2);
                fadeMenuButton = true;
                break;
            case 5:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(3);
                DisplayBar(2);
                SetBarsToZero();
                ResetTimer();
                break;
            case 6:
                BasicLevel(3);
                fadeSky = true;
                break;
            case 7:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(4);
                DisplayBar(3);
                BeginHexFacing();
                SetBarsToZero();
                ResetTimer();
                break;
            case 8:
                BasicLevel(4);
                break;
            case 9:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(5);
                DisplayBar(4);
                SetBarsToZero();
                ResetTimer();
                break;
            case 10:
                BasicLevel(5);
                break;
            case 11:
                ScoreUserHexagon();
                RandomizeReferenceHexagon(6);
                DisplayBar(5);
                SetBarsToZero();
                ResetTimer();
                break;
            case 12:
                BasicLevel(6);
                break;
            case 13:
                ScoreUserHexagon();
                unlockText.text = "GAME OVER. Final Score: " + totalScore;
                unlockText.color = Color.white;
                hintText.text = "(You can keep messing with the sliders or\n" +
                                    "hit testing the polygon if you want.)";
                hintText.color = Color.white;
                timerOn = false;
                timerText.gameObject.SetActive(false);
                break;
            default:
                RandomizeReferenceHexagon(6);
                DisplayMatchBar();
                break;
        }
        levelNumber++;
        if (levelNumber > 4)
        {
            rotationSpeed += additionalRotationSpeedPerLevel;
        }
    }

    private void BasicLevel(int sliders)
    {
        ScoreUserHexagon();
        RandomizeReferenceHexagon(sliders);
        SetBarsToZero();
        ResetTimer();
    }

    private void BeginHexFacing()
    {
        hexFacing = true;
    }

    private void BeginHexSpin()
    {
        hexSpin = true;
    }

    private void ResetTimer()
    {
        timeLeft = 30;
    }

    private void StartTimer()
    {
        timeLeft = 30;
        timerOn = true;
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
            attributeSliders[x].SetLevel(0);
        }
    }

    private void ScoreUserHexagon()
    {
        float match = GetMatchAmount(levelNumber);
        int score = Mathf.RoundToInt(match * 100f);
        if (timerOn)
        {
            score += Mathf.RoundToInt(timeLeft * 2);
            matchText.text = Mathf.RoundToInt(match * 100f) + "% Match + " + Mathf.RoundToInt(timeLeft * 2) + " Time: " + score + " points!";
        }
        else
        {
            matchText.text = Mathf.RoundToInt(match * 100f) + "% Match: " + score + " points!";
        }

        totalScore += score;
        unscoredPoints += score;

        if (!scoreStartedFadingIn)
        {
            scoreStartedFadingIn = true;
        }
        matchBarTargetOpacity = 0;
        matchBarCurrentOpacity = 1;
        matchText.color = new Color(.3f, 1, .5f, 1);
        timeSinceScoringAppeared = 0;
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
        unlockPulseText.color = attributeSliders[bar].colorOfUnlockText;
        unlockText.text = attributeSliders[bar].unlockName + " bar unlocked!";
        unlockPulseText.text = attributeSliders[bar].unlockName + " bar unlocked!";
        unlockTextIsFading = true;
        timeSinceUnlockTextAppeared = 0;
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
        referenceAttributes[0] = UnityEngine.Random.Range(.1f, .9f);
        referenceAttributes[1] = .25f; //Greenness
        referenceAttributes[2] = 0f;   //Blueness
        referenceAttributes[3] = 0f;   //Metalicity or smoothness, not sure which yet.
        referenceAttributes[4] = 0f;   //Etc.

        //Randomize additional attributes.
        for (int x = 2; x <= numberOfAttributesRandomized; x++)
        if (numberOfAttributesRandomized >= x)
        {
            referenceAttributes[x-1] = UnityEngine.Random.Range(.1f, .9f);
        }

        referenceMaterial.SetColor("tex_col", new Color(referenceAttributes[0], referenceAttributes[1], referenceAttributes[2]));
        referenceMaterial.SetFloat("tex_metalness", referenceAttributes[3]);
        referenceMaterial.SetFloat("tex_trans", referenceAttributes[4]);
        referenceMaterial.SetFloat("tex_rock", referenceAttributes[5]);
    }

    private void DoFirstTweening()
    {
        userHex.gameObject.SetActive(true);
        moveHexes = true;
    }

    public void UpdateUserHex(int attribute, float value)
    {
        if (levelNumber == 1&&!matchBarAppeared&&(value!=0&&value!=1&&value!=.25f))
        {
            DisplayMatchBar();
        }
        if (levelNumber == 3&&matchBarAppeared && (value != 0 && value != 1 && value != .25f))
        {
            HideMatchBar();
        }
        userAttributes[attribute] = value;

        userMaterial.SetColor("tex_col", new Color(userAttributes[0], userAttributes[1], userAttributes[2]));
        userMaterial.SetFloat("tex_metalness", userAttributes[3]);
        userMaterial.SetFloat("tex_trans", userAttributes[4]);
        userMaterial.SetFloat("tex_rock", userAttributes[5]);

        if (matchBarCurrentOpacity > .01f&&levelNumber < 3)
        {
            float match = GetMatchAmount(levelNumber);
            matchText.text = "Match: " + Mathf.RoundToInt(match * 100f)+"%";
            if (match > .95f)
            {
                matchText.color = new Color(.3f, 1, .5f, matchText.color.a);
                if (!shownHint2)
                {
                    referenceHex.hintParticleSystem.Play();
                    shownHint2 = true;
                }
                if (!matchParticlesPlayed)
                {
                    matchParticles.Play();
                }
            }
            else
            {
                matchText.color = new Color(1, 1, 1, matchText.color.a);
            }
        }
    }

    private float GetMatchAmount(int levelNumber)
    {
        int numberOfSliders = slidersByLevelNumber[levelNumber];
        float closeness = 0;
        for (int x = 0; x < numberOfSliders; x++)
        {
            float thisCloseness = 0;
            if (userAttributes[x]>referenceAttributes[x])
            {
                thisCloseness = 1-((userAttributes[x]- referenceAttributes[x]) / (1 - referenceAttributes[x]));
                //Hypothetical: 1-((      .6         -          .4           ) / (1 -        .4             ))
                //              1-((.6-.4)/(1-.4)) = 1-(.2/.6) = 1-.33 = .67, which is because right .6 is two thirds of the way from 1 to .4
            }
            else
            {
                thisCloseness = userAttributes[x] / referenceAttributes[x];
                //Hypothetical:       .3          /         .4           = .75, which is because right .3 is 34ths of the way from 0 to .4
            }
            closeness += thisCloseness / numberOfSliders; //Average them all.
        }
        return closeness;
    }

    public void ShowMenu()
    {
        Time.timeScale = .00001f;
        MenuImage.SetActive(true);
        BlockAllOtherStuff.SetActive(true);
    }

    public void HideMenu()
    {
        Time.timeScale = 1;
        MenuImage.SetActive(false);
        BlockAllOtherStuff.SetActive(false);
    }

    public void ResideHexes(float number)
    {
        int realNumber = Mathf.RoundToInt(number);
        userHex.MakeARegularPolygon(realNumber);
        referenceHex.MakeARegularPolygon(realNumber);
    }
}
