using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMusicManager : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component

    void Start()
    {
        // Start playing the music when the game starts
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
}
