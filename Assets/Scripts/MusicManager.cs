using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The music is brought with you from scene to scene. This class, a parent of the music, provides a singleton access point 
/// to all the music. This means that music can fade in and out between scenes, etc.
/// 
/// Source: Originally made by me for "Engine Room", extended and modified for "Final Winter", then dialed back
/// for the hexagon tests by cutting unneeded functionality.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public MusicLoop[] music = new MusicLoop[0];
    private float[] fadeLengths;
    private float[] fadeStartTimes;
    private float[] fadeVolumes;
    private float[] fadeStartVolumes;

    internal static float globalMusicVolume = .3f;
    internal static float intenseMusicVolume = 1;
    internal static float intenseMusicFadeTime = 2f;

    internal int mapMusic = -1;
    internal int IntenseMusic = -1;

    public const int AIR = 0;
    public const int AIR_C = 1;

    private float musicVolume = 1f;

    void Start()
    {
        fadeLengths = new float[music.Length];
        fadeStartTimes = new float[music.Length];
        fadeVolumes = new float[music.Length];
        fadeStartVolumes = new float[music.Length];
        for (int x = 0; x < music.Length; x++)
        {
            Debug.Log("Audio: " + music[x].gameObject.name);
            music[x].audioSource.volume = 0;
            fadeLengths[x] = 0;
        }
    }

    /// <summary>
    /// Sets up the Music Manager's singleton design pattern - only one instance of
    /// the manager is allowed to exist and is referenced by the variable "instance"
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    internal void SetRelaxedMusic(int mapMusic)
    {
        this.mapMusic = mapMusic;
    }

    internal void SetIntenseMusic(int combatMusic)
    {
        this.IntenseMusic = combatMusic;
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < music.Length; x++)
        {
            if (fadeLengths[x] > 0)
            {
                fadeLengths[x] -= Time.deltaTime;
                music[x].audioSource.volume = Mathf.Lerp(fadeStartVolumes[x], fadeVolumes[x], 1 - (fadeLengths[x] / fadeStartTimes[x]))*music[x].additionalBalance*musicVolume;
                if (music[x].audioSource.volume <= 0)
                {
                    music[x].audioSource.Stop();
                }
                else
                {
                    if (!music[x].audioSource.isPlaying)
                    {
                        music[x].StartIntro();
                    }
                }
            }
        }
    }

    public void StopAllMusic()
    {
        for (int x = 0; x < music.Length; x++)
        {
            music[x].audioSource.Stop();
            fadeLengths[x] = 0;
            music[x].audioSource.volume = 0;
        }
    }

    public void ChangeMusicVolume(float newMusicVolume)
    {
        for (int x = 0; x < music.Length; x++)
        {
            if (music[x].audioSource.volume > 0)
            {
                music[x].audioSource.volume = music[x].audioSource.volume / musicVolume * newMusicVolume;
            }
        }
        musicVolume = newMusicVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public void FadeMusic(int num, float time, float volume)
    {
        if (num == -1)
        {
            return;
        }
        fadeLengths[num] = time;
        fadeVolumes[num] = volume;
        fadeStartTimes[num] = time;
        fadeStartVolumes[num] = music[num].audioSource.volume;
    }

    public void FadeOutMusic(int num, float time)
    {
        if (num == -1)
        {
            return;
        }
        if (num == -2)
        {
            for (int x = 0; x < music.Length; x++)
            {
                FadeMusic(x, time, 0);
            }
            return;
        }
        FadeMusic(num, time, 0);
    }

    public void TurnOnIntenseMusic()
    {
        FadeMusic(IntenseMusic, intenseMusicFadeTime, intenseMusicVolume);
    }

    public void TurnOffIntenseMusic()
    {
        FadeMusic(IntenseMusic, intenseMusicFadeTime, .000001f);
    }
}
