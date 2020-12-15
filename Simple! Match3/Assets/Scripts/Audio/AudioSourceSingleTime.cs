using UnityEngine;

namespace SimpleMatch3.Audio
{
    public class AudioSourceSingleTime : MonoBehaviour
    {
        public delegate void OnAudioSourceAction(AudioSourceSingleTime currentAudioSource);
        public static OnAudioSourceAction OnAudioFinished;

        public AudioSource audioSource;

        public void SetUpAndPlayClip(AudioClip audioClip)
        {
            if (audioSource)
            {
                audioSource.enabled = true;
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }

        public void DeActivate()
        {
            if (OnAudioFinished != null)
            {
                OnAudioFinished(this);
            }

            gameObject.SetActive(false);
        }
    }
}