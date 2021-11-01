using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour {
    [SerializeField] private GameObject sfx;
    [SerializeField] private GameObject music;
    [SerializeField] private GameObject volume;
    [SerializeField] private GameObject retry;
    [SerializeField] private GameObject quit;
    [SerializeField] private GameObject backToMenu;
    [SerializeField] private GameObject pauseCover;
    [SerializeField] private GameObject pauseButtons;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI pressText;
    private Player player;

    private void Start() {
        player = FindObjectOfType<Player>();
        pauseCover.SetActive(false);
        pauseButtons.SetActive(false);
        sfx.SetActive(false);
        music.SetActive(false);
        volume.SetActive(false);
        retry.SetActive(false);
        quit.SetActive(false);
        backToMenu.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            if (pauseCover.activeSelf && FindObjectOfType<Player>() != null) {
                Resume();
            } else {
                Pause(false);
            }
        }
    }

    public void Resume() { 
        pauseCover.SetActive(false);
        pauseButtons.SetActive(false);
        sfx.SetActive(false);
        music.SetActive(false);
        volume.SetActive(false);
        retry.SetActive(false);
        quit.SetActive(false);
        backToMenu.SetActive(false);
        pauseText.text = "";
        pressText.text = "";
        Time.timeScale = 1;
        player.lockActions = false;
    }

    public void Pause(bool gameOver, int curWave=0, int attempt=0) { 
        pauseButtons.SetActive(true);
        pauseCover.SetActive(true);
        sfx.SetActive(true);
        music.SetActive(true);
        volume.SetActive(true);
        retry.SetActive(true);
        quit.SetActive(true);
        backToMenu.SetActive(true);
        if (gameOver) {
            pauseText.text = "defeat";
            pressText.text = $"lost on wave {curWave}/12";
        }
        else {
            if (curWave != 12) { 
                pauseText.text = "paused";
                pressText.text = "press <p> to resume";
            }
            else { 
                pauseText.text = "victory";
                pressText.text = $"won on attempt {attempt}";
            }
        }
        Time.timeScale = 0;
        player.lockActions = true;
    }
}
