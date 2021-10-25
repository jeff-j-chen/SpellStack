using UnityEngine;
using TMPro;

public class RetryButton : MonoBehaviour {
    private TextMeshProUGUI bannerText;
    
    private void Start() {
        bannerText = GetComponent<TextMeshProUGUI>();
        bannerText.color = Colors.textMain;
    }
    
    private void OnMouseEnter() {
        if (bannerText.text == "retry?") {
            bannerText.color = Colors.textHovered;
        }
    }
    
    private void OnMouseExit() {
        if (bannerText.text == "retry?") {
            bannerText.color = Colors.textMain;
        }
    }
    
    private void OnMouseDown() {
        if (bannerText.text == "retry?") {
            bannerText.color = Colors.textClicked;
        }
    }
    
    private void OnMouseUp() {
        if (bannerText.text == "retry?") {
            bannerText.color = Colors.textMain;
            FindObjectOfType<WaveManager>().AttemptRestart();
        }
    }
    
}
