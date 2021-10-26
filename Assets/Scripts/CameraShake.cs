using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour {
    [SerializeField] private float startShakeDuration = 2.0f;
    [SerializeField] private float curShakeDuration = 0f;
    [SerializeField] private float shakeMagnitude = 0.7f;
    [SerializeField] private float dampingSpeed = 0.1f;
    private Vector3 initialPosition = new(0, 0, -10);

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
