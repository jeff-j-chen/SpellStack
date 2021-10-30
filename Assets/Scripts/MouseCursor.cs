using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour {
    private SpriteRenderer sr;
    private Color og;
    void Start() {
        Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
        sr = GetComponent<SpriteRenderer>();
        og = sr.color;
        StartCoroutine(RotateCursor());
    }

    void Update() {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private IEnumerator RotateCursor() {
        while (true) {
            transform.Rotate(0, 0, -9);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public IEnumerator AttackAnimation(Color c) {
        for (int i = 1; i < 6; i++) {
            transform.localScale -= new Vector3(0.05f, 0.05f, 0);
            sr.color = Color.Lerp(og, c, i / 5f);
            yield return new WaitForSeconds(0.01f);
        }
        for (int i = 1; i < 6; i++) {
            transform.localScale += new Vector3(0.05f, 0.05f, 0);
            sr.color = Color.Lerp(c, og, i / 5f);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
