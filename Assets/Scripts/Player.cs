using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private GameObject bullet;
    [SerializeField] public int health = 100;
    [SerializeField] public int attackDamage = 5;
    [SerializeField] private TextMeshProUGUI healthText;
    private WaitForSeconds attackDelayWFS;
    private Rigidbody2D body;
    private float horizontal;
    private float vertical;
    private const float MoveLimiter = 0.7f;


    private void Start () {
        body = GetComponent<Rigidbody2D>();
        attackDelayWFS = new WaitForSeconds(attackDelay);
        healthText.text = $"Player: {health} HP";
    }

    private void Update() {
        horizontal = Input.GetAxisRaw("Horizontal"); 
        vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetMouseButton(0)) {
            AttemptAttack();
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
        Vector2 lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x);
        GameObject fired = Instantiate(bullet, transform.position, Quaternion.identity);
        fired.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, lookAngle * Mathf.Rad2Deg));
        fired.GetComponent<Rigidbody2D>().velocity = new(
            Mathf.Cos(lookAngle) * projectileSpeed,
            Mathf.Sin(lookAngle) * projectileSpeed
        );
        fired.GetComponent<Bullet>().damage = attackDamage;
        fired.GetComponent<SpriteRenderer>().color = Colors.blue;
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

    private void ChangeHealthBy(int amount) {
        health -= amount;
        healthText.text = $"Player: {health} HP";
    }
}
