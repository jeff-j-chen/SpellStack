using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField] public int health;
    [SerializeField] public int enemyId;
    [SerializeField] private GameObject bullet;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float chaseSpeed = 6f;
    private enum MovementType { Standing , RunningAtPlayer};
    [SerializeField] private MovementType curMovementType = MovementType.Standing;
    private Player player;
    private delegate IEnumerator AttackPattern();
    private Coroutine currentAttackPattern;
    private Dictionary<int, Dictionary<int, AttackPattern>> patterns;
    private Rigidbody2D r;
    
    private void Start() {
        player = FindObjectOfType<Player>();
        r = GetComponent<Rigidbody2D>();
        health = enemyId switch {
            0 => 200,
            _ => 100
        };
        healthText.text = $"Boss: {health} HP";
        patterns = new Dictionary<int, Dictionary<int, AttackPattern>> {
            {
                0, new() {
                    { 200, SingleNonTracking },
                    { 175, NonTrackingAndTracking },
                    { 125, TrackingShotgun },
                    { 75, RunAtPlayer },
                }
            }
        };
        StartCoroutine(BeginPhaseAfterDelay(1f, patterns[enemyId][health]));
    }
    
    // TODO: bullet size
    // then enemy will be done, and begin work on major part (spells)

    private void Update() {
        if (curMovementType == MovementType.RunningAtPlayer) {
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
            r.velocity = new Vector2(
                Mathf.Cos(theta) * chaseSpeed,
                Mathf.Sin(theta) * chaseSpeed
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        GameObject g = other.gameObject;
        switch (g.name) {
            case "player_bullet(Clone)":
                Destroy(g);
                ChangeHealthBy(g.GetComponent<Bullet>().damage);
                break;
            case "test":
                break;
        }
    }

    private void ChangeHealthBy(int amount) {
        foreach (int threshold in patterns[0].Keys) {
            if (health > threshold && health - amount <= threshold) {
                if (currentAttackPattern != null) { StopCoroutine(currentAttackPattern); }
                StartCoroutine(BeginPhaseAfterDelay(0.5f, patterns[enemyId][threshold]));
            }
        }
        health -= amount;
        healthText.text = $"Boss: {health} HP";
    }

    private void FireProjectile(int projectileSpeed, int projectileDamage, float acceleration, float theta, Color color) {
        GameObject fired = Instantiate(bullet, transform.position, Quaternion.identity);
        fired.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
        fired.GetComponent<Rigidbody2D>().velocity = new(
            Mathf.Cos(theta) * projectileSpeed,
            Mathf.Sin(theta) * projectileSpeed
        );
        Bullet b = fired.GetComponent<Bullet>();
        b.damage = projectileDamage;
        b.acceleration = acceleration;
        b.firedAngle = theta;
        fired.GetComponent<SpriteRenderer>().color = color;
    }

    private IEnumerator SingleNonTracking() {
        while (true) {
            yield return new WaitForSeconds(0.4f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 5, 0f, theta, Colors.red);
        }
    }
    
    private IEnumerator NonTrackingAndTracking() {
        while (true) {
            yield return new WaitForSeconds(0.4f);
            Vector2 lookDirection = (Vector2)player.transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 5, 0f, theta, Colors.red);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 5, 3f, theta, Colors.yellow);
        }
    }

    private IEnumerator TrackingShotgun() {
        while (true) {
            Vector2 lookDirection;
            float theta;
            for (int i = 0; i < 3; i++) {
                yield return new WaitForSeconds(0.3f);
                lookDirection = player.transform.position - transform.position;
                theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
                FireProjectile(15, 5, 7f, theta, Colors.yellow);
            }
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 20, 5, 0f, theta, 12, Colors.green);
        }
    }
    
    private IEnumerator RunAtPlayer() {
        curMovementType = MovementType.RunningAtPlayer;
        yield return new WaitForSeconds(0.3f);
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 12, 5, 4f, theta, 15, Colors.green);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 5, 0f, theta, Colors.yellow);
        }
    }

    private void Shotgun(int count, int projectileSpeed, int projectileDamage, float acceleration, float theta, int spread, Color color) {
        for (int i = 0; i < count; i++) {
            FireProjectile(projectileSpeed, projectileDamage, acceleration, theta + ((-count/2 + i) * spread) * Mathf.Deg2Rad, color);
        }
    }

    private IEnumerator BeginPhaseAfterDelay(float delay, AttackPattern pattern) {
        yield return new WaitForSeconds(delay);
        currentAttackPattern = StartCoroutine(pattern());
    }
}
