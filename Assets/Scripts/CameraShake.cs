using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    private Transform t;
    private float startShakeDuration = 2.0f;
    private float curShakeDuration = 0f;
    private float shakeMagnitude = 0.7f;
    private float dampingSpeed = 0.1f;
    private Vector2 initialPosition = new Vector2(0, 0);

    private void Start() {
        t = transform.position;
    }

    private void FixedUpdate() {
        if (curShakeDuration > 0) {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            curShakeDuration -= dampingSpeed;
        }
        else {
            curShakeDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    public void Shake() {
        curShakeDuration = startShakeDuration;
    }
}
