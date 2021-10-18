using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public int damage;

    public float acceleration = 0;
    public float firedAngle;
    private Rigidbody2D r;

    private void Start() {
        r = GetComponent<Rigidbody2D>();
        if (acceleration > 0) {
            StartCoroutine(RampUpSpeed());
        }
    }
    
    private IEnumerator RampUpSpeed() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            r.velocity += new Vector2(
                Mathf.Cos(firedAngle) * acceleration,
                Mathf.Sin(firedAngle) * acceleration
            );
        }
    }
}
