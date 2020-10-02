using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicLoop : MonoBehaviour
{
    public AudioClip intro;
    public AudioClip loop;
    public float overrideIntroOffset;
    public float additionalBalance = 1.00f;

    public AudioSource audioSource;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartIntro()
    {
        StartCoroutine(BeginLoop());
    }

    IEnumerator BeginLoop()
    {
        if (intro != null)
        {
            audioSource.clip = intro;
            audioSource.Play();
            while (audioSource.time<(audioSource.clip.length-Time.deltaTime))
            {
                yield return null;
            }
        }
        audioSource.loop = true;
        audioSource.clip = loop;
        audioSource.Play();
    }
}