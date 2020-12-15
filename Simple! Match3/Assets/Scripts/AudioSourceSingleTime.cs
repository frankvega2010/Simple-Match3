using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceSingleTime : MonoBehaviour
{
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetUpAndPlayClip(AudioClip audioClip)
    {
        if (audioSource)
        {
            audioSource.enabled = true;
            audioSource.clip = audioClip;
            audioSource.Play();
            Destroy(gameObject, audioClip.length);
        }
    }
}
