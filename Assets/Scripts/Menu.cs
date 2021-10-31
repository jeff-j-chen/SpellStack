using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {
    [SerializeField] private Sprite check;
    [SerializeField] private Sprite x;
    [SerializeField] private Sprite[] volumeSprites;
    [SerializeField] private Sprite sfxEnabledSprite;
    [SerializeField] private Sprite sfxDisabledSprite;
    [SerializeField] private Sprite musicEnabledSprite;
    [SerializeField] private Sprite musicDisabledSprite;
    [SerializeField] private SpriteRenderer tutorialSR;
    [SerializeField] private SpriteRenderer musicSR;
    [SerializeField] private SpriteRenderer sfxSR;
    [SerializeField] private SpriteRenderer volumeSR;
    private bool tutorialEnabled;
    private bool sfxEnabled;
    private bool musicEnabled;
    private int soundLevel;
    private SoundManager soundManager;
    private Music music;

    private void Start() {
        soundManager = FindObjectOfType<SoundManager>();
        music = FindObjectOfType<Music>();
        
        tutorialEnabled = PlayerPrefs.GetString("tutorial", "true") == "true" ? true : false;
        sfxEnabled = PlayerPrefs.GetString("sfx", "true") == "true" ? true : false;
        musicEnabled = PlayerPrefs.GetString("music", "true") == "true" ? true : false;
        soundLevel = PlayerPrefs.GetInt("soundLevel", 3);
        tutorialSR.sprite = tutorialEnabled ? check : x;
        sfxSR.sprite = sfxEnabled ? sfxEnabledSprite : sfxDisabledSprite;
        musicSR.sprite = musicEnabled ? musicEnabledSprite : musicDisabledSprite;
        volumeSR.sprite = volumeSprites[soundLevel];
    }

    public void Continue() { 
        soundManager.PlayClip("click");
        Debug.LogError("You need to add a save system here!");
    }

    public void NewGame() { 
        soundManager.PlayClip("click");
        Initiate.Fade("Game", Color.black, 2.5f);
    }

    public void Keybindings() { 
        soundManager.PlayClip("click");
        Initiate.Fade("Keybindings", Color.black, 2.5f);
    }

    public void ButtonClicked(string name) { 
        switch (name) { 
            case "tutorial": 
                tutorialEnabled = !tutorialEnabled;
                PlayerPrefs.SetString("tutorial", tutorialEnabled ? "true" : "false");

                tutorialSR.sprite = tutorialEnabled ? check : x;
                break;
            case "sfx":
                sfxEnabled = !sfxEnabled;
                PlayerPrefs.SetString("sfx", sfxEnabled ? "true" : "false");
                sfxSR.sprite = sfxEnabled ? sfxEnabledSprite : sfxDisabledSprite;
                soundManager.GetComponent<AudioSource>().volume = sfxEnabled ? soundLevel * 0.25f : 0;
                break;
            case "music":
                musicEnabled = !musicEnabled;
                PlayerPrefs.SetString("music", musicEnabled ? "true" : "false");
                musicSR.sprite = musicEnabled ? musicEnabledSprite : musicDisabledSprite;
                music.GetComponent<AudioSource>().volume = musicEnabled ? 0.05f : 0;
                break;
            case "volume":
                soundLevel++;
                if (soundLevel >= 4) { soundLevel = 0; }
                PlayerPrefs.SetInt("soundLevel", soundLevel);
                volumeSR.sprite = volumeSprites[soundLevel];
                soundManager.GetComponent<AudioSource>().volume = soundLevel * 0.25f;
                music.GetComponent<AudioSource>().volume = soundLevel * 0.25f * 0.05f;
                break;
            case "quit":
                print("quit!");
                Application.Quit();
                break;

            default:
                print("bad button name!"); 
                break;
        }
        soundManager.PlayClip("click");
    }
}
