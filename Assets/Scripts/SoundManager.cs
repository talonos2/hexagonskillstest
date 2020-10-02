using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public static AudioClip potBreakSound, rockAttackStrongSound, rockAttackWeakSound;
    static AudioSource audioSrc;

    public float soundEffectVolume = 1f;

    void Start()
    {
    }

    public void SetSFXVol(float amount)
    {
        soundEffectVolume = amount;
    }

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

    void Update()
    {

    }

    public void PlaySound(string clip, float vol)
    {
        float realVol = vol * soundEffectVolume;
        if (!audioSrc)
        {
            audioSrc = this.gameObject.AddComponent<AudioSource>();
        }
        audioSrc.PlayOneShot(Resources.Load<AudioClip>("Sounds/" + clip), realVol);
    }

    private AudioClip GetAudio(string v)
    {
            return Resources.Load<AudioClip>(v);
    }

    private void OnApplicationQuit()
    {
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
