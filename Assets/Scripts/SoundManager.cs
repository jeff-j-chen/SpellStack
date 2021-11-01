using System;
using UnityEngine;
public class SoundManager : MonoBehaviour {
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private string[] audioClipNames;
    [SerializeField] private AudioClip[] musicPieces;
    [SerializeField] private string[] musicPieceNames;
    private AudioSource audioSource;
    private bool sfxEnabled;
    private int soundLevel;
    
    private void Start() {
        sfxEnabled = PlayerPrefs.GetString("sfx") == "true" ? true : false;
        soundLevel = PlayerPrefs.GetInt("soundLevel");
        audioSource = GetComponent<AudioSource>();
        audioSource.GetComponent<AudioSource>().volume = sfxEnabled ? soundLevel * 0.25f : 0;
        audioClipNames = new string[audioClips.Length];
        for (int i = 0; i < audioClips.Length; i++) {
            audioClipNames[i] = audioClips[i].name;
        }
    }
    
    public void PlayClip(string clipName) {
        if (clipName == "lightningbuildupSlowed") { print("slowed"); }
        audioSource.PlayOneShot(audioClips[Array.IndexOf(audioClipNames, clipName)]);
    }
    
    public void PlayClip(int clipIndex) {
        audioSource.PlayOneShot(audioClips[clipIndex]);
    }
}