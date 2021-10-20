using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private GameObject bullet;
    [SerializeField] public int health = 100;
    [SerializeField] public int attackDamage = 10;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject lStrikeBorder;
    [SerializeField] private GameObject lStrikeCenter;
    [SerializeField] private int lightningDamage = 25;
    [SerializeField] private float lightningDelay = 1.5f;
    [SerializeField] private float lightningCooldown = 1f;
    [SerializeField] private bool lightningUsed = false;
    [SerializeField] private int rockRiseDamage = 25;
    [SerializeField] private float rockRiseDelay = 1.5f;
    [SerializeField] private float rockRiseCooldown = 1f;
    [SerializeField] private bool rockRiseUsed = false;
    [SerializeField] private int waterPullDamage = 25;
    [SerializeField] private float waterPullDelay = 1.5f;
    [SerializeField] private float waterPullCooldown = 1f;
    [SerializeField] private bool waterPullUsed = false;
    [SerializeField] private int fireballDamage = 25;
    [SerializeField] private int fireballSpeed = 15;
    [SerializeField] private bool fireballUsed = false;
    [SerializeField] private float fireballCooldown = 1f;
    [SerializeField] private int gShotgunDamage = 5;
    [SerializeField] private int gShotgunSpeed = 25;
    [SerializeField] private int gShotgunSpread = 10;
    [SerializeField] private int gShotgunCount = 5;
    [SerializeField] private bool gShotgunUsed = false;
    [SerializeField] private float gShotgunCooldown = 1f;
    [SerializeField] private int plantRootDamage = 10;
    [SerializeField] private int plantRootSpeed = 15;
    [SerializeField] private float plantRootDuration = 1f;
    [SerializeField] private bool plantRootUsed = false;
    [SerializeField] private float plantRootCooldown = 1f;
    [SerializeField] private int cSpellDamage = 5;
    [SerializeField] private int cSpellSpeed = 15;
    [SerializeField] private int cSpellAcc = 5;
    [SerializeField] private int cSpellRadius = 5;
    [SerializeField] private bool cSpellUsed = false;
    [SerializeField] private float cSpellCooldown = 1f;
    private Dictionary<string, Vector2> scales = new() {
        { "small", new Vector2(0.5f, 0.5f) },
        { "medium", new Vector2(1f, 1f) },
        { "large", new Vector2(1.7f, 1.7f) }
    };
    private Rigidbody2D body;
    private float horizontal;
    private float vertical;
    private const float MoveLimiter = 0.7f;


    private void Start () {
        body = GetComponent<Rigidbody2D>();
        healthText.text = $"Player: {health} HP";
    }

    private void Update() {
        horizontal = Input.GetAxisRaw("Horizontal"); 
        vertical = Input.GetAxisRaw("Vertical");
        GetComponent<SpriteRenderer>().color = Colors.pastelGreen;
        if (Input.GetMouseButton(0)) { AttemptAttack(); }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {  StartCoroutine(LightningStrike()); } 
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Fireball(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { GenericShotgun(); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { PlantRoot(); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { WaterPull(); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { RockRise(); }
        if (Input.GetKeyDown(KeyCode.Q)) { ConvergingSpell(); }
        if (Input.GetKeyDown(KeyCode.E)) { Flamethrower(); }
        if (Input.GetKeyDown(KeyCode.R)) { Forcepush(); }
        if (Input.GetKeyDown(KeyCode.T)) { IceVolley(); }
    }
    
    // area marked, aoe strike down after a delay
    private IEnumerator LightningStrike() {
        if (!lightningUsed) {
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lightningUsed = true;
            StartCoroutine(CountdownCooldownFor("lightning"));
            GameObject b = Instantiate(lStrikeBorder, drop, Quaternion.identity);
            GameObject c = Instantiate(lStrikeCenter, drop, Quaternion.identity);
            float initial = c.transform.localScale.x;
            for (int i = 0; i < (lightningDelay*10); i++) {
                c.transform.localScale -= new Vector3(initial/(lightningDelay*10), initial/(lightningDelay*10), 0f);
                yield return new WaitForSeconds(0.1f);
            }
            GameObject l = Instantiate(bullet, drop, Quaternion.identity);
            l.transform.localScale = new Vector3(3f, 3f, 1f);
            l.GetComponent<Bullet>().damage = lightningDamage;
            l.GetComponent<Bullet>().behavior = Bullet.Behavior.Linger;
            l.GetComponent<SpriteRenderer>().color = Colors.yellow;
            yield return new WaitForSeconds(0.1f);
            Destroy(c);
            Destroy(l);
            Destroy(b);
        }
    }
    
    // slow moving piercing projectile
    private void Fireball() {
        if (!fireballUsed) {
            fireballUsed = true;
            StartCoroutine(CountdownCooldownFor("fireball"));
            FireProjectile(fireballSpeed, fireballDamage, 0f, scales["large"], Bullet.Behavior.Linger, Colors.orange);
        }
    }
    
    // simple shotgun
    private void GenericShotgun() {
        if (!gShotgunUsed) {
            gShotgunUsed = false;
            StartCoroutine(CountdownCooldownFor("gShotgun"));
            Shotgun(gShotgunCount, gShotgunDamage, gShotgunDamage, 0f, GetAngleToCursor(), scale["medium"], gshotGunSpread, Colors.white);
        }
    }

    private void PlantRoot() {
        if (!plantRootUsed) {
            plantRootUsed = true;
            StartCoroutine(CountdownCooldownFor("plantRoot"));
            Bullet b = FireProjectile(plantRootSpeed, plantRootDamage, 0f, scales["small"], Bullet.Behavior.Break, Colors.green);
            b.rootDuration = plantRootDuration;
        }
    }

    private void WaterPull() { 
        if (!waterPullUsed) {
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            waterPullUsed = true;
            StartCoroutine(CountdownCooldownFor("waterPull"));
            GameObject b = Instantiate(waterPullBorder, drop, Quaternion.identity);
            Bullet c = FireProjectile(0, 0, 0, new Vector2(5, 5), Bullet.Behavior.Linger, Colors.babyBlue);
            c.rootDuration = 1f;
            print("USE A UNITY MAGNET HERE!");
            float initial = c.gameObject.transform.localScale.x;
            for (int i = 0; i < (waterPullDelay*10); i++) {
                c.gameObject.transform.localScale -= new Vector3(initial/(waterPullDelay*10), initial/(waterPullDelay*10), 0f);
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.1f);
            Destroy(c);
            Destroy(b);
        }
    }

    private void RockRise() { 
        if (!rockRiseUsed) {
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rockRiseUsed = true;
            StartCoroutine(CountdownCooldownFor("rockRise"));
            GameObject b = Instantiate(rockRiseCenter, drop, Quaternion.identity);
            float initial = b.transform.localScale.x;
            for (int i = 0; i < (rockRiseDelay*10); i++) {
                b.transform.localScale += new Vector3(0.75f, 0.75f, 0f);
                yield return new WaitForSeconds(0.1f);
            }
            Bullet c = FireProjectile(0, 0, 0, new Vector2(4.25f, 4.25f), Bullet.Behavior.Linger, Colors.invisible);
            yield return new WaitForSeconds(0.1f);
            Destroy(c);
            Destroy(b);
        }
    }

    private void ConvergingSpell() { 
        if (!convergingSpellUsed)  {
            convergingSpellUsed = true;
            StartCoroutine(CountdownCooldownFor("cSpell"));

        }
    }

    private void Shotgun(int count, int projectileSpeed, int projectileDamage, float acceleration, float theta, Vector2 scale, int spread, Bullet.Behavior behavior, Color color) {
        for (int i = 0; i < count; i++) {
            FireProjectile(projectileSpeed, projectileDamage, acceleration, theta + ((-count/2 + i) * spread) * Mathf.Deg2Rad, scales["medium"], behavior, color);
        }
    }

    private IEnumerator CountdownCooldownFor(string spellName) {
        switch (spellName) {
            case "lightning": 
                yield return new WaitForSeconds(lightningCooldown);
                lightningUsed = false;
                break;
            case "fireball": 
                yield return new WaitForSeconds(fireballCooldown);
                fireballUsed = false;
                break;
            case "gShotgun": 
                yield return new WaitForSeconds(gShotgunCooldown);
                gShotgunUsed = false;
                break;
            case "plantRoot": 
                yield return new WaitForSeconds(plantRootCooldown);
                plantRootUsed = false;
                break;
            case "waterPull": 
                yield return new WaitForSeconds(waterPullCooldown);
                waterPullUsed = false;
                break;
            case "rockRise": 
                yield return new WaitForSeconds(rockRiseCooldown);
                rockRiseUsed = false;
                break;
            case "cSpell": 
                yield return new WaitForSeconds(cSpellCooldown);
                cSpellUsed = false;
                break;
            default: print($"invalid spell name ({spellName}) passed"); break;
        }
    }

    private void FixedUpdate() {
        if (horizontal != 0 && vertical != 0) {
            horizontal *= MoveLimiter;
            vertical *= MoveLimiter;
        } 

        body.velocity = new Vector2(horizontal * moveSpeed, vertical * moveSpeed);
    }

    private void OnMouseDown() {
        AttemptAttack();
    }

    private void AttemptAttack() {
        if (canAttack) {
            canAttack = false;
            StartCoroutine(RefreshAttack());
        }
        else { return; }
        FireProjectile(projectileSpeed, attackDamage, 0f, scales["medium"], Bullet.Behavior.Break, Colors.blue);
    }
    
    private float GetAngleToCursor(Vector3 pos=transform.position) { 
        Vector2 lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - pos;
        return Mathf.Atan2(lookDirection.y, lookDirection.x);
    }

    private Bullet FireProjectile(float spd, int dmg, float acc, Vector2 scale, Bullet.Behavior behavior, Color color, Vector3 pos=transform.position) {
        float theta = GetAngleToCursor();
        GameObject fired = Instantiate(bullet, pos, Quaternion.identity);
        fired.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
        fired.GetComponent<Rigidbody2D>().velocity = new Vector2(
            Mathf.Cos(theta) * spd,
            Mathf.Sin(theta) * spd
        );
        Bullet b = fired.GetComponent<Bullet>();
        b.damage = dmg;
        b.acceleration = acc;
        b.firedAngle = theta;
        b.behavior = behavior;
        fired.GetComponent<SpriteRenderer>().color = color;
        fired.transform.localScale = scale;
        return b;
    }

    private IEnumerator RefreshAttack() {
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }
        
    private void OnTriggerEnter2D(Collider2D other) {
        GameObject g = other.gameObject;
        switch (g.name) {
            case "enemy_bullet(Clone)":
                Destroy(g);
                ChangeHealthBy(g.GetComponent<Bullet>().damage);
                break;
            case "test":
                break;
        }
    }

    public void ChangeHealthBy(int amount) {
        health -= amount;
        healthText.text = $"Player: {health} HP";
    }
}
