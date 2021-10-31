using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour {
    private Menu menu;
    private void Start() {
        menu = FindObjectOfType<Menu>();
    }

    private void OnMouseDown() {
        menu.ButtonClicked(gameObject.name);
    }
}
