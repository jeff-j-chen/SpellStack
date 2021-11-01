using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private int unlockedSpells = 0;
    [SerializeField] private List<GameObject> spellIndicatorList;
    [SerializeField] private List<Sprite> staffSprites;
    [SerializeField] private int curStaff = 0;
    [SerializeField] private List<float> attackSpeeds = new() { 0.4f, 0.2f, 0.4f, 0.4f };
    [SerializeField] private List<int> attackDmgs = new() { 5, 5, 5, 5 };
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] public int startHealth = 200;
    [SerializeField] public int health = 200;
    [SerializeField] public int regenAmount = 3;
    [SerializeField] public int bossRegenAmount = 50;
    [SerializeField] private GameObject playerHP;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject fireball;
    [SerializeField] private GameObject lstrikeGO;
    [SerializeField] private GameObject plantRootGO;
    [SerializeField] private GameObject cspellGO;
    [SerializeField] private GameObject shotgunGO;
    [SerializeField] private GameObject icetombGO;
    [SerializeField] private GameObject staff0shoot;
    [SerializeField] private GameObject staff1shoot;
    [SerializeField] private GameObject staff2shoot;
    [SerializeField] private GameObject staff3shoot;
    [SerializeField] private GameObject roundBullet;
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
    [SerializeField] private float waterPullDelay = 2f;
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
    [SerializeField] private float attackSpeedChange = 0.05f;
    [SerializeField] private float attackSpeedCooldown = 7f;
    [SerializeField] private float attackSpeedDuration = 3f;
    [SerializeField] private bool attackSpeedUsed = false;
    [SerializeField] private bool attackSpeedActive = false;
    [SerializeField] private Color[] staffColors = new Color[4];
    private readonly Dictionary<string, Vector2> scales = new() {
        { "small", new Vector2(0.5f, 0.5f) },
        { "medium", new Vector2(1f, 1f) },
        { "large", new Vector2(1.7f, 1.7f) }
    };
    private Rigidbody2D body;
    private float horizontal;
    private float vertical;
    private const float MoveLimiter = 0.7f;
    public bool lockActions = false;
    private SpriteRenderer sr;
    private bool isInvincible = false;
    
    private delegate void Spell(int spellIndex);
    [SerializeField] private List<Spell> spellList;
    [SerializeField] private List<float> cooldownList;
    [SerializeField] private SpriteRenderer redVignette;
    [SerializeField] private float maxOpacity = 1f;
    private ItemFrame basicAttackItemFrame;
    private Coroutine shootingCoro;
    private SoundManager soundManager;
    private WaveManager waveManager;
    private CameraShake cameraShake;
    private MouseCursor mouseCursor;

    private void Start () {
        lockActions = false;
        isInvincible = false;
        body = GetComponent<Rigidbody2D>();
        soundManager = FindObjectOfType<SoundManager>();
        waveManager = FindObjectOfType<WaveManager>();
        cameraShake = FindObjectOfType<CameraShake>();
        mouseCursor = FindObjectOfType<MouseCursor>();
        sr = GetComponent<SpriteRenderer>();
        spellList = new List<Spell> { Fireball, WaterPull, IceShield, LightningStrike, AttackSpeed,GenericShotgun, PlantRoot, ConvergingSpell, IceTomb, RockRise };
        cooldownList = new List<float> { 0, fireballCooldown, waterPullCooldown, iceShieldCooldown, lightningCooldown, attackSpeedCooldown, gShotgunCooldown, plantRootCooldown, cSpellCooldown,   iceTombCooldown, rockRiseCooldown };
        StartCoroutine(EnableFirstSpells());
        basicAttackItemFrame = spellIndicatorList[0].GetComponent<ItemFrame>();
        StartCoroutine(HealthRegen());
    }

    private void ScaleHPBar() {
        playerHP.transform.localScale = new Vector2(57.25f * ((float)health/startHealth), 1.25f);
        Color temp = redVignette.color;
        temp.a = maxOpacity * Mathf.Pow(1-(float)health/startHealth,2);
        redVignette.color = temp;
    }
    
    private IEnumerator HealthRegen() {
        while (true) {
            if (health <= startHealth - regenAmount && waveManager.transform.childCount >= 3) { health += regenAmount; }
            ScaleHPBar();
            yield return new WaitForSeconds(1f);
        }
    }

    public void HealAfterBoss() {
        health = Mathf.Clamp(health + bossRegenAmount, 0, startHealth);
        ScaleHPBar();
    }
    
    private IEnumerator EnableFirstSpells() {
        yield return new WaitForSeconds(0.1f);
        basicAttackItemFrame.dropper.SetActive(false);
        basicAttackItemFrame.cover.SetActive(false);
        basicAttackItemFrame.spellIcon.SetActive(true);
        basicAttackItemFrame.spellIcon.GetComponent<SpriteRenderer>().sprite = staffSprites[curStaff];
    }

    private void Update() {
        if (!lockActions) {
            horizontal = Input.GetAxisRaw("Horizontal"); 
            vertical = Input.GetAxisRaw("Vertical");
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
            if (Input.GetKeyDown(KeyCode.T)) {  if(unlockedSpells >= 10) { spellList[9](9); } }
            if (Input.GetKeyDown(KeyCode.J)) { UnlockNextStaff(); }
            if (Input.GetKeyDown(KeyCode.K)) { UnlockNextSpell(); }
            if (Input.GetKeyDown(KeyCode.L)) { HealAfterBoss(); }
        }
    }
    
    private void PutSpellOnCooldown(int i) {
        spellIndicatorList[i].GetComponent<ItemFrame>().PutOnCooldownFor(cooldownList[i]);
    }
    
    public void UnlockNextSpell() {
        unlockedSpells++;
        spellIndicatorList[unlockedSpells].GetComponent<ItemFrame>().Unlock();
    }
    
    public void UnlockNextStaff() {
        curStaff++;
        try { basicAttackItemFrame.spellIcon.GetComponent<SpriteRenderer>().sprite = staffSprites[curStaff]; } catch {}
        if (shootingCoro != null) { StopCoroutine(shootingCoro); }
        canAttack = true;
        AttemptAttack();
    }
    
    private void LightningStrike(int spellIndex) { StartCoroutine(LightningStrikeCoro(spellIndex)); }
    // area marked, aoe strike down after a delay
    private IEnumerator LightningStrikeCoro(int spellIndex) {
        if (!lightningUsed) {
            soundManager.PlayClip("lightningbuildup");
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
            GameObject l = Instantiate(lstrikeGO, drop + new Vector2(0f, 1.5f), Quaternion.identity);
            l.GetComponent<Bullet>().damage = lightningDamage;
            l.GetComponent<Bullet>().behavior = Bullet.Behavior.Linger;
            soundManager.PlayClip("lightning");
            cameraShake.Shake();
            yield return new WaitForSeconds(0.3f);
            Destroy(c);
            Destroy(l);
            Destroy(b);
        }
    }
    
    // slow moving piercing projectile
    private void Fireball(int spellIndex) {
        if (!fireballUsed) {
            soundManager.PlayClip("fireball");
            PutSpellOnCooldown(spellIndex + 1);
            fireballUsed = true;
            StartCoroutine(CountdownCooldownFor("fireball"));
            Bullet b = FireProjectile(
                fireballSpeed, 
                fireballDamage, 
                0f, 
                GetAngleToCursor(transform.position), 
                new Vector2(0, 0), 
                Bullet.Behavior.Linger, 
                Color.white, 
                transform.position,
                fireball
            );
            b.isFireball = true;
        }
    }
    
    // simple shotgun
    private void GenericShotgun(int spellIndex) {
        if (!gShotgunUsed) {
            soundManager.PlayClip("shotgun");
            PutSpellOnCooldown(spellIndex + 1);
            gShotgunUsed = true;
            StartCoroutine(CountdownCooldownFor("gShotgun"));
            Shotgun(
                gShotgunCount, 
                gShotgunSpeed, 
                gShotgunDamage, 
                0f, 
                GetAngleToCursor(transform.position), 
                new Vector2(0, 0), 
                gShotgunSpread, 
                Bullet.Behavior.Break, 
                Color.white, 
                transform.position,
                shotgunGO
            );
        }
    }

    private void PlantRoot(int spellIndex) {
        if (!plantRootUsed) {
            soundManager.PlayClip("plantroot");
            PutSpellOnCooldown(spellIndex + 1);
            plantRootUsed = true;
            StartCoroutine(CountdownCooldownFor("plantRoot"));
            Bullet b = FireProjectile(
                plantRootSpeed, 
                plantRootDamage, 
                0f, 
                GetAngleToCursor(
                transform.position), 
                new Vector2(0, 0), 
                Bullet.Behavior.Break, 
                Color.white, 
                transform.position,
                plantRootGO
            );
            b.rootDuration = plantRootDuration;
        }
    }

    private void WaterPull(int spellIndex) { StartCoroutine(WaterPullCoro(spellIndex)); }
    private IEnumerator WaterPullCoro(int spellIndex) { 
        if (!waterPullUsed) {
            soundManager.PlayClip("waterpull");
            PutSpellOnCooldown(spellIndex + 1);
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            waterPullUsed = true;
            StartCoroutine(CountdownCooldownFor("waterPull"));
            GameObject b = Instantiate(waterPullBorder, drop, Quaternion.identity);
            b.transform.localScale = new Vector2(7.5f, 7.5f);
            Bullet c = FireProjectile(
                0, waterPullDamage, 0, 0, 
                new Vector2(7.5f, 7.5f), 
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
                b.gameObject.transform.Rotate(0, 0, 18);
                yield return new WaitForSeconds(0.05f);
            }
            Destroy(b);
        }
    }

    private void RockRise(int spellIndex) { StartCoroutine(RockRiseCoro(spellIndex)); }
    private IEnumerator RockRiseCoro(int spellIndex) { 
        if (!rockRiseUsed) {
            print("INCREASE ROCKRISE COOLDOWN!");
            soundManager.PlayClip("rockrise");
            PutSpellOnCooldown(spellIndex + 1);
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rockRiseUsed = true;
            StartCoroutine(CountdownCooldownFor("rockRise"));
            GameObject b = Instantiate(rockRiseCenter, drop, Quaternion.identity);
            cameraShake.Shake();
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
            yield return new WaitForSeconds(3f);
            if (c != null) { Destroy(c.gameObject); }
            Destroy(b);
        }
    }

    private void ConvergingSpell(int spellIndex) { 
        if (!cSpellUsed)  {
            soundManager.PlayClip("cspell");
            PutSpellOnCooldown(spellIndex + 1);
            cSpellUsed = true;
            StartCoroutine(CountdownCooldownFor("cSpell"));
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, -Mathf.PI/2, 
                new Vector2(0, 0),
                Bullet.Behavior.Break, 
                Color.white, 
                drop + new Vector2(0, cSpellRadius),
                cspellGO
            );
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, Mathf.PI/2, 
                new Vector2(0, 0),
                Bullet.Behavior.Break, 
                Color.white, 
                drop + new Vector2(0, -cSpellRadius),
                cspellGO
            );
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, Mathf.PI, 
                new Vector2(0, 0),
                Bullet.Behavior.Break, 
                Color.white, 
                drop + new Vector2(cSpellRadius, 0),
                cspellGO
            );
            FireProjectile(
                cSpellSpeed, cSpellDamage, cSpellAcc, 0, 
                new Vector2(0, 0),
                Bullet.Behavior.Break, 
                Color.white, 
                drop + new Vector2(-cSpellRadius, 0),
                cspellGO
            );
        }
    }
    
    private void IceShield(int spellIndex) {
        if (!iceShieldUsed) {
            soundManager.PlayClip("icesound");
            PutSpellOnCooldown(spellIndex + 1);
            iceShieldUsed = true;
            StartCoroutine(CountdownCooldownFor("iceShield"));
            Vector2 drop = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject g = Instantiate(iceShield, drop, Quaternion.identity);
            float theta = GetAngleToCursor(transform.position) + Mathf.PI/2;
            g.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, theta * Mathf.Rad2Deg));
            StartCoroutine(FadeIceWall(g));
            
        }
    }
    
    private IEnumerator FadeIceWall(GameObject g) {
        SpriteRenderer iceSR = g.GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(iceShieldDuration-2f);
        Color temp = sr.color;
        temp.a = 1f;
        iceSR.color = temp;
        for (int i = 0; i < 10; i++) {
            temp.a -= 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a += 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a -= 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.025f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a += 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.025f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a -= 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.0125f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a += 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.0125f);
        }
        for (int i = 0; i < 10; i++) {
            temp.a -= 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.0125f/2);
        }
        for (int i = 0; i < 10; i++) {
            temp.a += 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.0125f/2);
        }
        for (int i = 0; i < 10; i++) {
            temp.a -= 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.0125f/4);
        }
        for (int i = 0; i < 10; i++) {
            temp.a += 0.05f;
            iceSR.color = temp;
            yield return new WaitForSeconds(0.0125f/4);
        }
        yield return new WaitForSeconds(0.0125f/8);
        Destroy(g);
    }
    
    private void IceTomb(int spellIndex) { StartCoroutine(IceTombCoro(spellIndex)); }
    private IEnumerator IceTombCoro(int spellIndex) { 
        if (!iceTombUsed)  {
            soundManager.PlayClip("icesound");
            PutSpellOnCooldown(spellIndex + 1);
            iceTombUsed = true;
            StartCoroutine(CountdownCooldownFor("iceTomb"));
            Bullet b = FireProjectile(
                0, iceTombDamage, 0, 0, 
                new Vector2(0, 0),
                Bullet.Behavior.Linger, 
                Colors.partial, 
                transform.position,
                icetombGO
            );
            b.rootDuration = iceTombRootDuration;
            yield return new WaitForSeconds(iceTombRootDuration);
            Destroy(b.gameObject);
        }
    }
    
    private void AttackSpeed(int spellIndex) { StartCoroutine(AttackSpeedCoro(spellIndex)); }
    private IEnumerator AttackSpeedCoro(int spellIndex) { 
        if (!attackSpeedUsed)  {
            PutSpellOnCooldown(spellIndex + 1);
            attackSpeedUsed = true;
            attackSpeedActive = true;
            StartCoroutine(CountdownCooldownFor("attackSpeed"));
            yield return new WaitForSeconds(attackSpeedDuration);
            attackSpeedActive = false;
        }
    }
    
    private void Shotgun(int count, int spd, int projectileDamage, float acceleration, float theta, Vector2 scale, int spread, Bullet.Behavior behavior,  Color color, Vector3 pos, GameObject go) {
        for (int i = 0; i < count; i++) {
            FireProjectile(
                spd, 
                projectileDamage, 
                acceleration, 
                theta + ((-count/2 + i) * spread) * Mathf.Deg2Rad, 
                scale, 
                behavior, 
                color, 
                pos,
                go
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
            case "attackSpeed": 
                yield return new WaitForSeconds(attackSpeedCooldown);
                attackSpeedUsed = false;
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
        if (!lockActions) { AttemptAttack(); }
    }

    private void AttemptAttack() {
        if (canAttack) {
            canAttack = false;
            shootingCoro = StartCoroutine(RefreshAttack());
        }
        else { return; }
        soundManager.PlayClip("shoot");
        StartCoroutine(mouseCursor.AttackAnimation(staffColors[curStaff]));
        float offsetAmount;
        Vector3 position;
        float theta;
        float rise;
        float run;
        Vector2 perpendicular;
        switch (curStaff) {
            case 0:
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[0], 
                    0f, 
                    GetAngleToCursor(transform.position), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white,
                    transform.position,
                    staff0shoot
                );
                break;
            case 1:
                offsetAmount = 0.5f;
                position = transform.position;
                theta = GetAngleToCursor(position);
                perpendicular = new(Mathf.Sin(theta)*offsetAmount, -Mathf.Cos(theta)*offsetAmount);
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[1], 
                    0f, 
                    GetAngleToCursor((Vector2)position - perpendicular), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    (Vector2)position + perpendicular,
                    staff1shoot
                );
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[1], 
                    0f, 
                    GetAngleToCursor((Vector2)position + perpendicular), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    (Vector2)position - perpendicular,
                    staff1shoot
                );
                break;
            case 2:
                offsetAmount = 1f;
                position = transform.position;
                theta = GetAngleToCursor(position);
                rise = Mathf.Sin(theta);
                run = Mathf.Cos(theta);
                perpendicular = new(rise*offsetAmount, -run*offsetAmount);
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[2], 
                    0f, 
                    GetAngleToCursor((Vector2)position), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    position,
                    staff2shoot
                );
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[2], 
                    0f, 
                    GetAngleToCursor((Vector2)position - perpendicular), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    (Vector2)position + perpendicular,
                    staff2shoot
                );
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[2], 
                    0f, 
                    GetAngleToCursor((Vector2)position + perpendicular), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    (Vector2)position - perpendicular,
                    staff2shoot
                );
                break;
            case 3:
                offsetAmount = 1f;
                position = transform.position;
                theta = GetAngleToCursor(position);
                rise = Mathf.Sin(theta);
                run = Mathf.Cos(theta);
                perpendicular = new(rise*offsetAmount, -run*offsetAmount);
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[3], 
                    0f, 
                    GetAngleToCursor((Vector2)position), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    position,
                    staff3shoot
                );
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[3], 
                    0f, 
                    GetAngleToCursor((Vector2)position - perpendicular), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    (Vector2)position + perpendicular,
                    staff3shoot
                );
                FireProjectile(
                    projectileSpeed, 
                    attackDmgs[3], 
                    0f, 
                    GetAngleToCursor((Vector2)position + perpendicular), 
                    new Vector2(0, 0), 
                    Bullet.Behavior.Break, 
                    Color.white, 
                    (Vector2)position - perpendicular,
                    staff3shoot
                );
                break;
            default:
                print("invalid staff detected!"); 
                break;
        }
    }
    
    private float GetAngleToCursor(Vector3 pos) { 
        Vector2 lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - pos;
        return Mathf.Atan2(lookDirection.y, lookDirection.x);
    }

    private Bullet FireProjectile(float spd, int dmg, float acc, float theta, Vector2 scale, Bullet.Behavior behavior, Color color, Vector3 pos, GameObject bulletType = null) {
        GameObject fired = Instantiate(bulletType == null ? bullet : bulletType, pos, Quaternion.identity);
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
        if (scale.x != 0) {fired.transform.localScale = scale;}
        return b;
    }

    private IEnumerator RefreshAttack() {
        if (basicAttackItemFrame != null && basicAttackItemFrame.dropper != null) {
            basicAttackItemFrame.dropper.SetActive(true);
            basicAttackItemFrame.cover.SetActive(true);
            float curSpeed = attackSpeedActive ? attackSpeedChange : attackSpeeds[curStaff];
            for (int i = 0; i < curSpeed*9; i++) {
                basicAttackItemFrame.dropper.transform.localPosition = new Vector2(0, -i*1.4f/(curSpeed * 10));
                yield return new WaitForSeconds(0.1f);
            }
            basicAttackItemFrame.dropper.SetActive(false);
            basicAttackItemFrame.cover.SetActive(false);
            yield return new WaitForSeconds(0.01f);
            canAttack = true;
        }
    }

    public void ChangeHealthBy(int amount) {
        if (!isInvincible) {
            soundManager.PlayClip("phurt");
            health -= amount;
            if (health <= 0) { 
                PlayerPrefs.SetInt("deaths", PlayerPrefs.GetInt("deaths") + 1);
                soundManager.PlayClip("pdeath"); 
                lockActions = true;
                waveManager.PlayerDies();
                Destroy(gameObject);
            }
            else {
                StartCoroutine(GivePlayerFeedback());
            }
        }
        ScaleHPBar();
    }
    
    private IEnumerator GivePlayerFeedback() {
        Color temp = sr.color;
        temp.a = 1;
        sr.color = temp;
        isInvincible = true;
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
        isInvincible = false;
        temp.a = 1;
        sr.color = temp;
    }
}
