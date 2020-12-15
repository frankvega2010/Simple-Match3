using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    public GameObject singleUseAudioPrefab = null;
    public List<AudioSourceSingleTime> audioSourcePool = new List<AudioSourceSingleTime>();

    // Start is called before the first frame update
    void Start()
    {
        AudioSourceSingleTime.OnAudioFinished += AddToPool;
    }

    public void PlaySound(AudioClip clip)
    {
        if(audioSourcePool.Count > 0)
        {
            AudioSourceSingleTime audioSource = audioSourcePool[audioSourcePool.Count - 1];
            audioSourcePool.Remove(audioSourcePool[audioSourcePool.Count - 1]);
            audioSource.gameObject.SetActive(true);
            audioSource.SetUpAndPlayClip(clip);
            audioSource.Invoke("DeActivate", clip.length);
        }
        else
        {
            GameObject newSound = Instantiate(singleUseAudioPrefab);
            AudioSourceSingleTime audioSource = newSound.GetComponent<AudioSourceSingleTime>();
            newSound.SetActive(true);
            audioSource.SetUpAndPlayClip(clip);
            audioSource.Invoke("DeActivate", clip.length);
        }
    }

    public void AddToPool(AudioSourceSingleTime currentAudioSource)
    {
        audioSourcePool.Add(currentAudioSource);
    }

    private void OnDestroy()
    {
        AudioSourceSingleTime.OnAudioFinished -= AddToPool;
    }
}
