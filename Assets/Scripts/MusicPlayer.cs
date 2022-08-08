using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] clips;
    private AudioSource audioSource;
    private int clipNo = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
    }

    private AudioClip GetNextClip()
    {
        
        clipNo++;
        if (clipNo == clips.Length)
        {
            clipNo = 0;
        }
        return clips[clipNo];

    }
    public void PlayNextClip()
    {
        audioSource.clip = GetNextClip();
        audioSource.Play();
    }

    public void StopPlaying()
    {
        audioSource.Stop();
    }
}
