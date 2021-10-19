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
    [SerializeField] private int fireballDamage = 25;
    [SerializeField] private int fireballSpeed = 15;
    [SerializeField] private bool fireballUsed = false;
    [SerializeField] private float fireballCooldown = 1f;
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            StartCoroutine(LightningStrike(drop));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            StartCoroutine(Fireball());
        }
    }
    
    private IEnumerator LightningStrike(Vector2 drop) {
        if (!lightningUsed) {
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
    
    private IEnumerator Fireball() {
        if (!fireballUsed) {
            fireballUsed = true;
            StartCoroutine(CountdownCooldownFor("fireball"));
            FireProjectile(fireballSpeed, fireballDamage, 0f, scales["large"], Bullet.Behavior.Linger, Colors.orange);
        }
        yield break;
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
            default: print("invalid spell type passed"); break;
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
    
    private void FireProjectile(float spd, int dmg, float acc, Vector2 scale, Bullet.Behavior behavior, Color color) {
        Vector2 lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x);
        GameObject fired = Instantiate(bullet, transform.position, Quaternion.identity);
        fired.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, lookAngle * Mathf.Rad2Deg));
        fired.GetComponent<Rigidbody2D>().velocity = new Vector2(
            Mathf.Cos(lookAngle) * spd,
            Mathf.Sin(lookAngle) * spd
        );
        Bullet b = fired.GetComponent<Bullet>();
        b.damage = dmg;
        b.acceleration = acc;
        b.firedAngle = lookAngle;
        b.behavior = behavior;
        fired.GetComponent<SpriteRenderer>().color = color;
        fired.transform.localScale = scale;
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
