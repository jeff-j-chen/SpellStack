using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField] public int health;
    public enum EnemyType { Regular, BossOne, BossTwo, Shotgun };
    [SerializeField] public EnemyType enemyType;
    [SerializeField] private GameObject bullet;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private int curPhase = 0;
    [SerializeField] public bool freezeMovement = false;
    [SerializeField] public bool isInvincible = false;
    private enum MovementType { Standing , RunningAtPlayer };
    private Dictionary<string, Vector2> scales = new() {
        { "small", new Vector2(0.5f, 0.5f) },
        { "medium", new Vector2(1f, 1f) },
        { "large", new Vector2(1.7f, 1.7f) }
    };
    private Dictionary<EnemyType, int> healthDict;
    [SerializeField] private MovementType curMovementType = MovementType.Standing;
    private Player player;
    private delegate IEnumerator AttackPattern();
    private Coroutine currentAttackPattern;
    private Dictionary<EnemyType, Dictionary<float, AttackPattern>> patterns;
    private Rigidbody2D r;
    private SpriteRenderer sr;
    private readonly List<float> phaseValues = new();
    private WaveManager waveManager;
    
    private void Start() {
        player = FindObjectOfType<Player>();
        r = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        waveManager = FindObjectOfType<WaveManager>();
        curPhase = 0;
        healthDict = new Dictionary<EnemyType, int>() {
            { EnemyType.Regular, 100 }, 
            { EnemyType.BossOne, 1000 }, 
            { EnemyType.BossTwo, 1500 }, 
            { EnemyType.Shotgun, 150 }, 
        };
        health = healthDict[enemyType];
        transform.localScale = enemyType switch {
            EnemyType.Regular => new Vector3(1.25f, 1.25f),
            EnemyType.BossOne => new Vector3(2, 2f),
            EnemyType.BossTwo => new Vector3(2, 2f),
            _ => new Vector3(1f, 1f),
        };
        healthText.text = $"Boss: {health} HP";
        patterns = new Dictionary<EnemyType, Dictionary<float, AttackPattern>> {
            { EnemyType.Regular, new() {
                  { 1f, SingleNonTrackingWithChase },
            }},
            { EnemyType.BossOne, new() {
                { 1f, SingleNonTrackingStationary },
                { 0.6f, NonTrackingAndTracking },
                { 0.3f, TrackingShotgun }
            }}
        };
        foreach (float percentage in patterns[enemyType].Keys) {
            phaseValues.Add(percentage);
        }
        isInvincible = true;
        StartCoroutine(BeginPhaseAfterDelay(1f, patterns[enemyType][1f], true));
    }
    

    private void FixedUpdate() {
        if (curMovementType == MovementType.RunningAtPlayer && !freezeMovement) {
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
            r.velocity = new Vector2(
                Mathf.Cos(theta) * chaseSpeed,
                Mathf.Sin(theta) * chaseSpeed
            );
        }
    }

    public void ChangeHealthBy(int amount) {
        if (!isInvincible) {
            foreach (float percentage in patterns[enemyType].Keys) {
                int threshold = (int)Mathf.Round(percentage * healthDict[enemyType]);
                if (health > threshold && health - amount <= threshold) {
                    curPhase++;
                    StartCoroutine(BeginPhaseAfterDelay(0.5f, patterns[enemyType][phaseValues[curPhase]], true));
                }
            }
            health -= amount;
            print($"enemy damaged by {amount}, health is now {health}");
            if (health <= 0) {
                waveManager.DecrementCount(gameObject);
            }
        }
    }

    private void FireProjectile(int projectileSpeed, int projectileDamage, float acceleration, float theta, Vector2 scale, Bullet.Behavior behavior, Color color) {
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
        b.behavior = behavior;
        fired.GetComponent<SpriteRenderer>().color = color;
        fired.transform.localScale = scale;
    }

    private IEnumerator SingleNonTrackingStationary() {
        curMovementType = MovementType.Standing;
        while (true) {
            yield return new WaitForSeconds(0.4f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 0f, theta, scales["medium"], Bullet.Behavior.Break, Colors.red);
        }
    }
    
    private IEnumerator SingleNonTrackingWithChase() {
        curMovementType = MovementType.RunningAtPlayer;
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 0f, theta, scales["medium"], Bullet.Behavior.Break, Colors.red);
        }
    }
    
    private IEnumerator NonTrackingAndTracking() {
        curMovementType = MovementType.Standing;
        while (true) {
            yield return new WaitForSeconds(0.4f);
            Vector2 lookDirection = (Vector2)player.transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 5, 0f, theta, scales["large"], Bullet.Behavior.Break, Colors.red);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 3f, theta, scales["medium"], Bullet.Behavior.Break, Colors.yellow);
        }
    }

    private IEnumerator TrackingShotgun() {
        curMovementType = MovementType.Standing;
        while (true) {
            Vector2 lookDirection;
            float theta;
            for (int i = 0; i < 3; i++) {
                yield return new WaitForSeconds(0.3f);
                lookDirection = player.transform.position - transform.position;
                theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
                FireProjectile(15, 5, 7f, theta, scales["medium"], Bullet.Behavior.Break, Colors.yellow);
            }
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 20, 5, 0f, theta, scales["medium"], 12, Bullet.Behavior.Break, Colors.green);
        }
    }
    
    private IEnumerator RunAtPlayer() {
        curMovementType = MovementType.RunningAtPlayer;
        yield return new WaitForSeconds(0.3f);
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 12, 5, 4f, theta, scales["medium"], 15, Bullet.Behavior.Break, Colors.green);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 5, 0f, theta, scales["large"], Bullet.Behavior.Break, Colors.yellow);
        }
    }

    private void Shotgun(int count, int projectileSpeed, int projectileDamage, float acceleration, float theta, Vector2 scale, int spread, Bullet.Behavior behavior, Color color) {
        for (int i = 0; i < count; i++) {
            FireProjectile(projectileSpeed, projectileDamage, acceleration, theta + ((-count/2 + i) * spread) * Mathf.Deg2Rad, scales["medium"], behavior, color);
        }
    }

    private IEnumerator BeginPhaseAfterDelay(float delay, AttackPattern pattern, bool freezeMovementDuring) {
        if (currentAttackPattern != null) { StopCoroutine(currentAttackPattern); }
        if (freezeMovementDuring) {
            freezeMovement = true;
            yield return new WaitForSeconds(delay);
            Color temp = sr.color;
            temp.a = 1f;
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(delay / 30f);
                temp.a -= 0.05f;
                sr.color = temp;
            }
            yield return new WaitForSeconds(10 * delay / 30f);
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(delay / 30f);
                temp.a += 0.05f;
                sr.color = temp;
            }
        }
        else {
            yield return new WaitForSeconds(delay);
            yield return new WaitForSeconds(delay);
        }
        freezeMovement = false;
        isInvincible = false;
        currentAttackPattern = StartCoroutine(pattern());
    }
}
