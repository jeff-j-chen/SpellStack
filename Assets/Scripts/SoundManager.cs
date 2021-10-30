using System;
using UnityEngine;
public class SoundManager : MonoBehaviour {
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private string[] audioClipNames;
    [SerializeField] private AudioClip[] musicPieces;
    [SerializeField] private string[] musicPieceNames;
    private AudioSource audioSource;
    
    private void Start() {
        audioSource = GetComponent<AudioSource>();
        audioClipNames = new string[audioClips.Length];
        for (int i = 0; i < audioClips.Length; i++) {
            audioClipNames[i] = audioClips[i].name;
        }
    }
    
    public void PlayClip(string clipName) {
        print($"received request to play {clipName}");
        audioSource.PlayOneShot(audioClips[Array.IndexOf(audioClipNames, clipName)]);
    }
    
    public void PlayClip(int clipIndex) {
        audioSource.PlayOneShot(audioClips[clipIndex]);
    }
}