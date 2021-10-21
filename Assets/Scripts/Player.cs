using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private int unlockedSpells = 1;
    [SerializeField] private List<GameObject> spellIndicatorList;
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] public int health = 100;
    [SerializeField] public int attackDamage = 10;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject roundBullet;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject lStrikeBorder;
    [SerializeField] private GameObject lStrikeCenter;
    [SerializeField] private GameObject rockRiseCenter;
    [SerializeField] private GameObject waterPullBorder;
    [SerializeField] private GameObject iceShield;
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
    [SerializeField] private float plantRootDuration = 2f;
    [SerializeField] private bool plantRootUsed = false;
    [SerializeField] private float plantRootCooldown = 1f;
    [SerializeField] private int cSpellDamage = 5;
    [SerializeField] private int cSpellSpeed = 15;
    [SerializeField] private int cSpellAcc = 5;
    [SerializeField] private int cSpellRadius = 5;
    [SerializeField] private bool cSpellUsed = false;
    [SerializeField] private float cSpellCooldown = 1f;
    [SerializeField] private float iceShieldCooldown = 1f;
    [SerializeField] private float iceShieldDuration = 1f;
    [SerializeField] private bool iceShieldUsed = true;
    [SerializeField] private int iceTombDamage = 25;
    [SerializeField] private float iceTombRootDuration = 1f;
    [SerializeField] private float iceTombCooldown = 1f;
    [SerializeField] private bool iceTombUsed = false;
    private readonly Dictionary<string, Vector2> scales = new() {
        { "small", new Vector2(0.5f, 0.5f) },
        { "medium", new Vector2(1f, 1f) },
        { "large", new Vector2(1.7f, 1.7f) }
    };
    private Rigidbody2D body;
    private float horizontal;
    private float vertical;
    private const float MoveLimiter = 0.7f;
    private delegate void Spell(int spellIndex);
    [SerializeField] private List<Spell> spellList;
    [SerializeField] private List<float> cooldownList;

    private void Start () {
        body = GetComponent<Rigidbody2D>();
        spellList = new List<Spell> { LightningStrike, WaterPull, IceShield, PlantRoot, Fireball, GenericShotgun, ConvergingSpell, RockRise, IceTomb };
        cooldownList = new List<float> { attackDelay, lightningCooldown, waterPullCooldown, iceShieldCooldown, plantRootCooldown, fireballCooldown, gShotgunCooldown, cSpellCooldown, rockRiseCooldown, iceTombCooldown };
        healthText.text = $"Player: {health} HP";
        StartCoroutine(EnableFirstSpells());
    }
    
    private IEnumerator EnableFirstSpells() {
        yield return new WaitForSeconds(0.1f);
        spellIndicatorList[0].GetComponent<ItemFrame>().dropper.SetActive(false);
        spellIndicatorList[0].GetComponent<ItemFrame>().cover.SetActive(false);
        spellIndicatorList[0].GetComponent<ItemFrame>().spellIcon.SetActive(true);
        spellIndicatorList[1].GetComponent<ItemFrame>().dropper.SetActive(false);
        spellIndicatorList[1].GetComponent<ItemFrame>().cover.SetActive(false);
        spellIndicatorList[1].GetComponent<ItemFrame>().spellIcon.SetActive(true);
    }

    private void Update() {
        horizontal = Input.GetAxisRaw("Horizontal"); 
        vertical = Input.GetAxisRaw("Vertical");
        GetComponent<SpriteRenderer>().color = Colors.pastelGreen;
        if (Input.GetMouseButton(0)) { AttemptAttack(); }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { if(unlockedSpells >= 1) { spellList[0](0); } } 
        if (Input.GetKeyDown(KeyCode.Alpha2)) { if(unlockedSpells >= 2) { spellList[1](1); } }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { if(unlockedSpells >= 3) { spellList[2](2); } }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { if(unlockedSpells >= 4) { spellList[3](3); } }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { if(unlockedSpells >= 5) { spellList[4](4); } }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { if(unlockedSpells >= 6) { spellList[5](5); } }
        if (Input.GetKeyDown(KeyCode.Q)) {  if(unlockedSpells >= 7) { spellList[6](6); } }
        if (Input.GetKeyDown(KeyCode.E)) {  if(unlockedSpells >= 8) { spellList[7](7); } }
        if (Input.GetKeyDown(KeyCode.R)) {  if(unlockedSpells >= 9) { spellList[8](8); } }
        // flamethrower, forcepush, ice volley, spin attack, wind slash, boomerang, bounding box
        if (Input.GetKeyDown(KeyCode.Space)) {
            UnlockNextSpell();
        }
    }
    
    private void PutSpellOnCooldown(int i) {
        spellIndicatorList[i].GetComponent<ItemFrame>().PutOnCooldownFor(cooldownList[i]);
    }
    
    private void UnlockNextSpell() {
        unlockedSpells++;
        spellIndicatorList[unlockedSpells].GetComponent<ItemFrame>().Unlock();
    }
    
    private void LightningStrike(int spellIndex) { StartCoroutine(LightningStrikeCoro(spellIndex)); }
    // area marked, aoe strike down after a delay
    private IEnumerator LightningStrikeCoro(int spellIndex) {
        if (!lightningUsed) {
            PutSpellOnCooldown(spellIndex + 1);
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
    private void Fireball(int spellIndex) {
        if (!fireballUsed) {
            PutSpellOnCooldown(spellIndex + 1);
            fireballUsed = true;
            StartCoroutine(CountdownCooldownFor("fireball"));
            FireProjectile(
                fireballSpeed, 
                fireballDamage, 
                0f, 
                GetAngleToCursor(transform.position), 
                scales["large"], 
                Bullet.Behavior.Linger, 
                Colors.orange, 
                transform.position
            );
        }
    }
    
    // simple shotgun
    private void GenericShotgun(int spellIndex) {
        if (!gShotgunUsed) {
            PutSpellOnCooldown(spellIndex + 1);
            gShotgunUsed = true;
            StartCoroutine(CountdownCooldownFor("gShotgun"));
            Shotgun(
                gShotgunCount, 
                gShotgunSpeed, 
                gShotgunDamage, 
                0f, 
                GetAngleToCursor(transform.position), 
                scales["medium"], 
                gShotgunSpread, 
                Bullet.Behavior.Break, 
                Colors.white, 
                transform.position
            );
        }
    }

    private void PlantRoot(int spellIndex) {
        if (!plantRootUsed) {
            PutSpellOnCooldown(spellIndex + 1);
            plantRootUsed = true;
            StartCoroutine(CountdownCooldownFor("plantRoot"));
            Bullet b = FireProjectile(
                plantRootSpeed, 
                plantRootDamage, 
                0f, 
                GetAngleToCursor(
                transform.position), 
                scales["small"], 
                Bullet.Behavior.Break, 
                Colors.green, 
                transform.position
            );
            b.rootDuration = plantRootDuration;
        }
    }

    private void WaterPull(int spellIndex) { StartCoroutine(WaterPullCoro(spellIndex)); }
    private IEnumerator WaterPullCoro(int spellIndex) { 
        if (!waterPullUsed) {
            PutSpellOnCooldown(spellIndex + 1);
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            waterPullUsed = true;
            StartCoroutine(CountdownCooldownFor("waterPull"));
            GameObject b = Instantiate(waterPullBorder, drop, Quaternion.identity);
            b.transform.localScale = new Vector2(6f, 6f);
            Bullet c = FireProjectile(
                0, waterPullDamage, 0, 0, 
                new Vector2(5, 5), 
                Bullet.Behavior.Linger, 
                Colors.invisible, 
                drop
            );
            c.rootDuration = 1f;
            yield return new WaitForSeconds(0.05f);
            Destroy(c.gameObject);
            float initial = c.gameObject.transform.localScale.x;
            for (int i = 0; i < (waterPullDelay*20); i++) {
                b.gameObject.transform.localScale -= new Vector3(initial/(waterPullDelay*20), initial/(waterPullDelay*20), 0f);
                yield return new WaitForSeconds(0.05f);
            }
            Destroy(b);
        }
    }

    private void RockRise(int spellIndex) { StartCoroutine(RockRiseCoro(spellIndex)); }
    private IEnumerator RockRiseCoro(int spellIndex) { 
        if (!rockRiseUsed) {
            PutSpellOnCooldown(spellIndex + 1);
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rockRiseUsed = true;
            StartCoroutine(CountdownCooldownFor("rockRise"));
            GameObject b = Instantiate(rockRiseCenter, drop, Quaternion.identity);
            rockRiseCenter.transform.localScale = new Vector2(0f, 0f);
            for (int i = 0; i < (rockRiseDelay*5); i++) {
                b.transform.localScale += new Vector3(0.8f, 0.8f, 0f);
                yield return new WaitForSeconds(0.1f);
            }
            Bullet c = FireProjectile(
                0, rockRiseDamage, 0, 0, 
                new Vector2(10f, 10f), 
                Bullet.Behavior.Linger, 
                Colors.invisible, 
                drop,
                roundBullet
            );
            yield return new WaitForSeconds(0.5f);
            Destroy(c.gameObject);
            Destroy(b);
        }
    }

    private void ConvergingSpell(int spellIndex) { 
        if (!cSpellUsed)  {
            PutSpellOnCooldown(spellIndex + 1);
            cSpellUsed = true;
            StartCoroutine(CountdownCooldownFor("cSpell"));
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, -Mathf.PI/2, 
                scales["small"],
                Bullet.Behavior.Break, 
                Colors.white, 
                drop + new Vector2(0, cSpellRadius)
            );
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, Mathf.PI/2, 
                scales["small"],
                Bullet.Behavior.Break, 
                Colors.white, 
                drop + new Vector2(0, -cSpellRadius)
            );
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, Mathf.PI, 
                scales["small"],
                Bullet.Behavior.Break, 
                Colors.white, 
                drop + new Vector2(cSpellRadius, 0)
            );
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, 0, 
                scales["small"],
                Bullet.Behavior.Break, 
                Colors.white, 
                drop + new Vector2(-cSpellRadius, 0)
            );
        }
    }
    
    private void IceShield(int spellIndex) { StartCoroutine(IceShieldCoro(spellIndex)); }
    private IEnumerator IceShieldCoro(int spellIndex) {
        if (!iceShieldUsed) {
            PutSpellOnCooldown(spellIndex + 1);
            iceShieldUsed = true;
            StartCoroutine(CountdownCooldownFor("iceShield"));
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject g = Instantiate(iceShield, drop, Quaternion.identity);
            float theta = GetAngleToCursor(transform.position) + Mathf.PI/2;
            g.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
            yield return new WaitForSeconds(iceShieldDuration);
            Destroy(g);
        }
    }
    
    private void IceTomb(int spellIndex) { StartCoroutine(IceTombCoro(spellIndex)); }
    private IEnumerator IceTombCoro(int spellIndex) { 
        if (!iceTombUsed)  {
            PutSpellOnCooldown(spellIndex + 1);
            iceTombUsed = true;
            StartCoroutine(CountdownCooldownFor("iceTomb"));
            Bullet b = FireProjectile(
                0, iceTombDamage, 0, 0, 
                new Vector2(9f, 9f),
                Bullet.Behavior.Linger, 
                Colors.iceBlue, 
                transform.position,
                roundBullet
            );
            b.rootDuration = iceTombRootDuration;
            yield return new WaitForSeconds(iceTombRootDuration);
            Destroy(b.gameObject);
        }
    }
    
    private void Shotgun(int count, int spd, int projectileDamage, float acceleration, float theta, Vector2 scale, int spread, Bullet.Behavior behavior,  Color color, Vector3 pos) {
        for (int i = 0; i < count; i++) {
            FireProjectile(
                spd, 
                projectileDamage, 
                acceleration, 
                theta + ((-count/2 + i) * spread) * Mathf.Deg2Rad, 
                scales["medium"], 
                behavior, 
                color, 
                pos
            );
        }
    }

    private IEnumerator CountdownCooldownFor(string spellName) {
        yield return new WaitForSeconds(0.15f);
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
            case "iceShield": 
                yield return new WaitForSeconds(iceShieldCooldown);
                iceShieldUsed = false;
                break;
            case "iceTomb": 
                yield return new WaitForSeconds(iceTombCooldown);
                iceTombUsed = false;
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
        FireProjectile(
            projectileSpeed, 
            attackDamage, 
            0f, 
            GetAngleToCursor(transform.position), 
            scales["medium"], 
            Bullet.Behavior.Break, 
            Colors.blue, 
            transform.position
        );
    }
    
    private float GetAngleToCursor(Vector3 pos) { 
        Vector2 lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - pos;
        return Mathf.Atan2(lookDirection.y, lookDirection.x);
    }

    private Bullet FireProjectile(float spd, int dmg, float acc, float theta, Vector2 scale, Bullet.Behavior behavior, Color color, Vector3 pos, GameObject bulletType = null) {
        GameObject fired = Instantiate(bulletType == null ? bullet : roundBullet, pos, Quaternion.identity);
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
        healthText.text = $"{health} HP";
    }
}
