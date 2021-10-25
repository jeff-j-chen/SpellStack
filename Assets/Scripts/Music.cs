using System;
using System.Collections;
using UnityEngine;

public class Music : MonoBehaviour {
    [SerializeField] private AudioClip[] musicPieces;
    [SerializeField] private string[] musicPieceNames;
    public AudioSource audioSource;
    private Scripts scripts;
    
    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        for (int i = 0; i < musicPieces.Length; i++) {
            musicPieceNames[i] = musicPieces[i].name;
        }
        scripts = FindObjectOfType<Scripts>();
        audioSource.volume = 0.5f;
    }
    
    private void Start() {
        FadeVolumeIn();
        PlayMusic("Karl");
    }

    public void PlayMusic(string pieceName) {
        audioSource.clip = musicPieces[Array.IndexOf(musicPieceNames, pieceName)];
        audioSource.Play();
    }

    public void FadeVolumeIn() {
        StartCoroutine(FadeVolumeInCoro()); 
    }
    
    public void FadeVolumeOut() { 
        StartCoroutine(FadeVolumeOutCoro()); 
    }

    private IEnumerator FadeVolumeInCoro() { 
        for (int i = 0; i < 5; i++) {
            yield return scripts.delays[0.05f];
            audioSource.volume += 0.1f;
        }
        audioSource.volume = 0.5f;
    }

    private IEnumerator FadeVolumeOutCoro() { 
        for (int i = 0; i < 5; i++) {
            yield return scripts.delays[0.05f];
            audioSource.volume -= 0.1f;
        }
        audioSource.volume = 0;
    }
}