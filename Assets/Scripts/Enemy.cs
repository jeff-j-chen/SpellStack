using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField] public int health;
    public enum EnemyType { Regular, BossOne, BossTwo, Shotgun, FleeingTracker, MachineGunner, FatShot, LightningMage, WavyShooter, BossThree };
    [SerializeField] public EnemyType enemyType;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private int curPhase = 0;
    [SerializeField] public bool freezeMovement = false;
    [SerializeField] public bool isInvincible = false;
    [SerializeField] private int lightningCount = 100;
    private enum MovementType { Standing, RunningAtPlayer, RunningAwayFromPlayer, MaintainDistance };
    [SerializeField] private float distanceToMaintain = 50f;
    private Dictionary<EnemyType, int> healthDict;
    [SerializeField] private MovementType curMovementType = MovementType.Standing;
    [SerializeField] private GameObject regularBullet;
    [SerializeField] private GameObject sniperBullet;
    [SerializeField] private GameObject machinegunBullet;
    [SerializeField] private GameObject shotgunBullet;
    [SerializeField] private GameObject fatShotBullet;
    [SerializeField] private GameObject wavyBullet;

    private Player player;
    private delegate IEnumerator AttackPattern();
    private Coroutine currentAttackPattern;
    private Dictionary<EnemyType, Dictionary<float, AttackPattern>> patterns;
    private Rigidbody2D r;
    private SpriteRenderer sr;
    private readonly List<float> phaseValues = new();
    private WaveManager waveManager;
    public bool uselessMinion = false;
    private SoundManager soundManager;
    private CameraShake cameraShake;
    
    private void Start() {
        player = FindObjectOfType<Player>();
        r = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        waveManager = FindObjectOfType<WaveManager>();
        soundManager = FindObjectOfType<SoundManager>();
        cameraShake = FindObjectOfType<CameraShake>();
        curPhase = 0;
        healthDict = new Dictionary<EnemyType, int>() {
            { EnemyType.Regular, 50 },
            { EnemyType.FleeingTracker, 50 },
            { EnemyType.MachineGunner, 70 },
            { EnemyType.Shotgun, 80 }, 
            { EnemyType.LightningMage, 90 },
            { EnemyType.FatShot, 100 },
            { EnemyType.WavyShooter, 110 },
            { EnemyType.BossOne, 1000 },
            { EnemyType.BossTwo, 2000 },
            { EnemyType.BossThree, 3000 },
        };
        health = healthDict[enemyType];
        transform.localScale = enemyType switch {
            EnemyType.Regular => new Vector3(1.25f, 1.25f),
            EnemyType.Shotgun => new Vector3(1.25f, 1.25f),
            EnemyType.FleeingTracker => new Vector3(1.25f, 1.25f),
            EnemyType.MachineGunner => new Vector3(1.25f, 1.25f),
            EnemyType.FatShot => new Vector3(1.25f, 1.25f),
            EnemyType.LightningMage => new Vector3(1.25f, 1.25f),
            EnemyType.WavyShooter => new Vector3(1.25f, 1.25f),
            EnemyType.BossOne => new Vector3(2, 2f),
            EnemyType.BossTwo => new Vector3(2, 2f),
            EnemyType.BossThree => new Vector3(2, 2f),
            _ => new Vector3(1f, 1f),
        };
        patterns = new Dictionary<EnemyType, Dictionary<float, AttackPattern>> {
            { EnemyType.Regular, new() {
                  { 1f, SingleNonTrackingWithChase },
            }},
            { EnemyType.MachineGunner, new() {
                  { 1f, MachineGun },
            }},
            { EnemyType.FleeingTracker, new() {
                  { 1f, SingleTrackingWhileFleeing },
            }},
            { EnemyType.Shotgun, new() {
                  { 1f, ShotgunWithChase },
            }},
            { EnemyType.FatShot, new() {
                  { 1f, FatShotDistMaintain },
            }},
            { EnemyType.LightningMage, new() {
                  { 1f, LightningSummon },
            }},
            { EnemyType.WavyShooter, new() {
                  { 1f, WavyShots },
            }},
            { EnemyType.BossOne, new() {
                { 1f, SingleNonTrackingWithSlowChase },
                { 0.7f, TrackingShotgun },
                { 0.35f, NonTrackingAndTracking }
            }},
            { EnemyType.BossTwo, new() {
                { 1f, SpiralDoom }
            }},
            { EnemyType.BossThree, new() {
                { 1f, LightningChase },
                { 0.5f, LightningChaseWithWaves }
            }},
        };
        foreach (float percentage in patterns[enemyType].Keys) {
            phaseValues.Add(percentage);
        }
        isInvincible = true;
        StartCoroutine(BeginPhaseAfterDelay(1f, patterns[enemyType][1f], true));
    }
    

    private void FixedUpdate() {
        if (curMovementType != MovementType.Standing && !freezeMovement) {
            if (curMovementType == MovementType.RunningAtPlayer) {
                MoveToPlayer(false);
            }
            else if (curMovementType == MovementType.RunningAwayFromPlayer) {
                MoveToPlayer(true);
            }
            else if (curMovementType == MovementType.MaintainDistance) { 
                if (player != null) {
                    if (DistFormula(transform.position, player.transform.position) > distanceToMaintain) { 
                        MoveToPlayer(false);
                    }
                    else { 
                        MoveToPlayer(true);
                    }
                }
            }
        }
        else if (freezeMovement) {
            r.velocity = new Vector2(0, 0);
        }
    }

    private void MoveToPlayer(bool flip) {
        Vector2 lookDirection = player.transform.position - transform.position;
        float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
        if (!flip)  {
            r.velocity = new Vector2(
                Mathf.Cos(theta) * chaseSpeed,
                Mathf.Sin(theta) * chaseSpeed
            );
        }
        else { 
            r.velocity = new Vector2(
                Mathf.Cos(theta) * -chaseSpeed,
                Mathf.Sin(theta) * -chaseSpeed
            );
        }
    }

    public void ChangeHealthBy(int amount) {
        if (!isInvincible) {
            if (patterns == null) { return; }
            foreach (float percentage in patterns[enemyType].Keys) {
                int threshold = (int)Mathf.Round(percentage * healthDict[enemyType]);
                if (health > threshold && health - amount <= threshold) {
                    curPhase++;
                    StartCoroutine(BeginPhaseAfterDelay(0.5f, patterns[enemyType][phaseValues[curPhase]], true));
                }
            }
            health -= amount;
            if (health <= 0) { 
                waveManager.DecrementCount(gameObject); 
                soundManager.PlayClip("edeath");
            }
            else {
                soundManager.PlayClip("ehurt");
                StartCoroutine(GivePlayerFeedback());
            }
            if (enemyType is EnemyType.BossOne or EnemyType.BossTwo or EnemyType.BossThree) {
                waveManager.ScaleBossHP(health / (float)healthDict[enemyType]);
            }
        }
    }
    
    private IEnumerator GivePlayerFeedback() {
        Color temp = sr.color;
        temp.a = 1;
        sr.color = temp;
        for (int i = 0; i < 10; i++) {
            temp.a -= 0.05f;
            sr.color = temp;
            yield return new WaitForSeconds(0.006f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a += 0.05f;
            sr.color = temp;
            yield return new WaitForSeconds(0.006f);
        }
        temp.a = 1;
        sr.color = temp;
    }
    
    public void RootForDuration(float duration) {
        StartCoroutine(RootForDurationCoro(duration));
    }

    private IEnumerator RootForDurationCoro(float duration) { 
        freezeMovement = true;
        yield return new WaitForSeconds(duration);
        freezeMovement = false;
    }

    private void FireProjectile(int projectileSpeed, int projectileDamage, float acceleration, float theta, Vector2 scale, Bullet.Behavior behavior, Color color, bool isWavy=false, GameObject projectilePrefab=null) {
        if (projectilePrefab == null) {
            projectilePrefab = bullet;
        } 
        GameObject fired = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        fired.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
        fired.GetComponent<Rigidbody2D>().velocity = new(
            Mathf.Cos(theta) * projectileSpeed,
            Mathf.Sin(theta) * projectileSpeed
        );
        waveManager.bullets.Add(fired);
        Bullet b = fired.GetComponent<Bullet>();
        b.damage = projectileDamage;
        b.acceleration = acceleration;
        b.firedAngle = theta;
        b.behavior = behavior;
        // fired.GetComponent<SpriteRenderer>().color = color;
        if (scale.x != 0) {
            fired.transform.localScale = scale;
        }
        if (isWavy) {
            StartCoroutine(WaveBullet(fired));
        }
    }
    
    private IEnumerator WaveBullet(GameObject g) {
        Bullet b = g.GetComponent<Bullet>();
        float theta = b.firedAngle;
        Rigidbody2D br = b.GetComponent<Rigidbody2D>();
        float rise = Mathf.Sin(theta);
        float run = Mathf.Cos(theta);
        Vector2 perpendicular = new(rise, -run);
        while (true) {
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(0.04f);
                if (br == null) { yield break; }
                br.velocity += perpendicular;
            }
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(0.04f);
                if (br == null) { yield break; }
                br.velocity -= perpendicular;
            }
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(0.04f);
                if (br == null) { yield break; }
                br.velocity -= perpendicular;
            }
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < 10; i++) {
                yield return new WaitForSeconds(0.04f);
                if (br == null) { yield break; }
                br.velocity += perpendicular;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator SpiralDoom() {
        curMovementType = MovementType.Standing;
        float theta = 0;
        while (true) {
            yield return new WaitForSeconds(0.25f);
            theta += 13f * Mathf.Deg2Rad;
            Shotgun(6, 8, 5, 0f, theta, new Vector2(0, 0), 60, Bullet.Behavior.Break, Colors.magenta, projectilePrefab:machinegunBullet);
        }
    }
    
    private IEnumerator SingleNonTrackingWithChase() {
        curMovementType = MovementType.RunningAtPlayer;
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.red, projectilePrefab:regularBullet);
        }
    }
    
    private IEnumerator SingleNonTrackingWithSlowChase() {
        curMovementType = MovementType.RunningAtPlayer;
        chaseSpeed = 2f;
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.red, projectilePrefab:regularBullet);
        }
    }
    
    private IEnumerator WavyShots() {
        curMovementType = MovementType.MaintainDistance;
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(10, 20, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.blue, true, projectilePrefab:wavyBullet);
        }
    }
    
    private IEnumerator FatShotDistMaintain() {
        curMovementType = MovementType.MaintainDistance;
        while (true) {
            yield return new WaitForSeconds(2.5f);
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(10, 40, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.purple, projectilePrefab:fatShotBullet);
        }
    }
    
    private IEnumerator SingleTrackingWhileFleeing() {
        curMovementType = MovementType.RunningAwayFromPlayer;
        while (true) {
            yield return new WaitForSeconds(0.6f);
            Vector2 lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(10, 10, 1.5f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.babyBlue, projectilePrefab:sniperBullet);
        }
    }
    
    private IEnumerator NonTrackingAndTracking() {
        curMovementType = MovementType.MaintainDistance;
        while (true) {
            yield return new WaitForSeconds(0.5f);
            Vector2 lookDirection = player.transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 50, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.red, projectilePrefab:fatShotBullet);
            yield return new WaitForSeconds(0.3f);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(10, 10, 1f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.yellow, projectilePrefab:regularBullet);
        }
    }

    private IEnumerator TrackingShotgun() {
        curMovementType = MovementType.RunningAtPlayer;
        chaseSpeed = 3f;
        while (true) {
            Vector2 lookDirection;
            float theta;
            for (int i = 0; i < 3; i++) {
                yield return new WaitForSeconds(0.5f);
                lookDirection = player.transform.position - transform.position;
                theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
                FireProjectile(15, 5, 7f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.yellow, projectilePrefab:regularBullet);
            }
            yield return new WaitForSeconds(0.5f);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 20, 5, 0f, theta, new Vector2(0, 0), 12, Bullet.Behavior.Break, Colors.green, projectilePrefab:shotgunBullet);
        }
    }

    private IEnumerator ShotgunWithChase() {
        curMovementType = MovementType.RunningAtPlayer;
        while (true) {
            Vector2 lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 8, 3, 0f, theta, new Vector2(0, 0), 20, Bullet.Behavior.Break, Colors.green, projectilePrefab:shotgunBullet);
            yield return new WaitForSeconds(2.5f);
        }
    }

    private IEnumerator MachineGun() {
        curMovementType = MovementType.MaintainDistance;
        while (true) {
            Vector2 lookDirection = player.transform.position - transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.red, projectilePrefab:machinegunBullet);
            yield return new WaitForSeconds(0.5f);
            lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(15, 10, 0f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.yellow, projectilePrefab:machinegunBullet);
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private IEnumerator LightningSummon() {
        curMovementType = MovementType.MaintainDistance;
        while (true) {
            soundManager.PlayClip("lightningbuildupSlowed");
            waveManager.SummonAtPos(player.transform.position);
            yield return new WaitForSeconds(2f);
            soundManager.PlayClip("lightning");
            cameraShake.Shake();
        }
    }
    
    private IEnumerator LightningChase() {
        curMovementType = MovementType.RunningAtPlayer;
        while (true) {
            soundManager.PlayClip("lightningbuildupSlowed");
            for (int i = 0; i < lightningCount-20; i++) {
                waveManager.SummonAtPos(new Vector2(Random.Range(-33f, 33f), Random.Range(-15f, 18f)));
            }
            Vector2 lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 6, 5, 0f, theta, new Vector2(0, 0), 30, Bullet.Behavior.Break, Colors.green, projectilePrefab:shotgunBullet);
            yield return new WaitForSeconds(2f);
            soundManager.PlayClip("lightning");
            cameraShake.Shake();
        }
    }
    
    private IEnumerator LightningChaseWithWaves() {
        curMovementType = MovementType.RunningAtPlayer;
        while (true) {
            soundManager.PlayClip("lightningbuildupSlowed");
            for (int i = 0; i < lightningCount; i++) {
                waveManager.SummonAtPos(new Vector2(Random.Range(-33f, 33f), Random.Range(-15f, 18f)));
            }
            Vector2 lookDirection = (Vector2)player.transform.position + player.GetComponent<Rigidbody2D>().velocity - (Vector2)transform.position;
            float theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            Shotgun(5, 6, 5, 0f, theta, new Vector2(0, 0), 30, Bullet.Behavior.Break, Colors.green, true, projectilePrefab:shotgunBullet);
            lookDirection = player.transform.position;
            theta = Mathf.Atan2(lookDirection.y, lookDirection.x);
            FireProjectile(5, 30, 1f, theta, new Vector2(0, 0), Bullet.Behavior.Break, Colors.purple, projectilePrefab:fatShotBullet);
            yield return new WaitForSeconds(2f);
            soundManager.PlayClip("lightning");
        }
    }
    


    private void Shotgun(int count, int projectileSpeed, int projectileDamage, float acceleration, float theta, Vector2 scale, int spread, Bullet.Behavior behavior, Color color, bool isWavy=false, GameObject projectilePrefab=null) {
        for (int i = 0; i < count; i++) {
            FireProjectile(projectileSpeed, projectileDamage, acceleration, theta + ((-count/2 + i) * spread) * Mathf.Deg2Rad, scale, behavior, color, isWavy, projectilePrefab);
        }
    }

    private IEnumerator BeginPhaseAfterDelay(float delay, AttackPattern pattern, bool freezeMovementDuring) {
        if (currentAttackPattern != null) {
            StopCoroutine(currentAttackPattern);
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
                freezeMovement = false;
                isInvincible = false;
            }
            else {
                yield return new WaitForSeconds(delay);
                yield return new WaitForSeconds(delay);
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

    private float DistFormula(Vector3 pos1, Vector3 pos2) {
        return Mathf.Sqrt((pos1.x - pos2.x) * (pos1.x - pos2.x) + (pos1.y - pos2.y) * (pos1.y - pos2.y));
    }
}