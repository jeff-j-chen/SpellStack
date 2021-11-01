using System;
using System.Collections;
using UnityEngine;

public class Music : MonoBehaviour {
    [SerializeField] private AudioClip[] musicPieces;
    [SerializeField] private string[] musicPieceNames;
    public AudioSource audioSource;
    private bool musicEnabled;
    private int soundLevel;
    
    private void Awake() {
        SetUpSingleton();
        musicEnabled = PlayerPrefs.GetString("music") == "true" ? true : false;
        soundLevel = PlayerPrefs.GetInt("soundLevel");
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = musicEnabled ? 0.05f : 0;
        musicPieceNames = new string[musicPieces.Length];
        for (int i = 0; i < musicPieces.Length; i++) {
            musicPieceNames[i] = musicPieces[i].name;
        }
        audioSource.volume = 0;
    }
    
    private void SetUpSingleton() {
        if (FindObjectsOfType(GetType()).Length > 1) {
            Destroy(gameObject);
        }
        else {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start() {
        GetComponent<AudioSource>().volume = PlayerPrefs.GetString("music", "true") == "true" ? 0.05f : 0;
        StartCoroutine(PlayMusic("Karl"));
    }
    
    public IEnumerator PlayMusic(string pieceName) {
        audioSource.clip = musicPieces[Array.IndexOf(musicPieceNames, pieceName)];
        while (true) {
            FadeVolumeIn();
            audioSource.Play();
            yield return new WaitForSeconds(200f);
            FadeVolumeOut();
            yield return new WaitForSeconds(10f);
        }
    }
    
    public void FadeVolumeIn() {
        StartCoroutine(FadeVolumeInCoro()); 
    }
    
    public void FadeVolumeOut() { 
        StartCoroutine(FadeVolumeOutCoro()); 
    }
    
    private IEnumerator FadeVolumeInCoro() { 
        if (PlayerPrefs.GetString("music") == "true") {
            for (int i = 0; i < 5; i++) {
                yield return new WaitForSeconds(0.05f);
                audioSource.volume += 0.01f;
            }
            audioSource.volume = 0.05f;
        }
        else { 
            audioSource.volume = 0;
        }
    }
    
    private IEnumerator FadeVolumeOutCoro() { 
        audioSource.volume = PlayerPrefs.GetString("music") == "true" ? 0.05f : 0;
        for (int i = 0; i < 5; i++) {
            yield return new WaitForSeconds(0.05f);
            audioSource.volume -= 0.01f;
        }
        audioSource.volume = 0;
    }
}