using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour {
    private enum PointNames { Center, TopLeft, TopRight, BottomLeft, BottomRight };
    private Dictionary<PointNames, Vector2> spawnPoints;
    private List<PointNames> spawnPointList;
    [SerializeField] private int curWave = 0;
    [SerializeField] private int liveEnemies = 0;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bossBar;
    [SerializeField] private GameObject bossBarBg;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject lStrikeBorder;
    [SerializeField] private GameObject lStrikeCenter;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float rand = 4;
    [SerializeField] private GameObject cardTilemap;
    [SerializeField] private GameObject behindBanner;
    [SerializeField] private List<Enemy> spawnedEnemies = new();
    [SerializeField] private GameObject lightningGO;
    [SerializeField] private GameObject tutorialBanner;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private bool showTutorial;
    [SerializeField] public List<GameObject> bullets = new();
    [SerializeField] private bool lockWave = false;
    private Player player;
    private SoundManager soundManager;
    private Coroutine bossMinions;
    private PauseMenu pauseMenu;

    private void Start() {
        showTutorial = PlayerPrefs.GetString("tutorial", "true") == "true" ? true : false;
        player = FindObjectOfType<Player>();
        pauseMenu = FindObjectOfType<PauseMenu>();
        soundManager = FindObjectOfType<SoundManager>();
        Color temp = behindBanner.GetComponent<SpriteRenderer>().color;
        temp.a = 0f;
        behindBanner.GetComponent<SpriteRenderer>().color = temp;
        spawnPoints = new Dictionary<PointNames, Vector2>() {
            { PointNames.Center, new Vector2(0, 2.5f) },
            { PointNames.TopRight, new Vector2(33, 16) },
            { PointNames.TopLeft, new Vector2(-33, 16) },
            { PointNames.BottomLeft, new Vector2(-33, -10.5f) },
            { PointNames.BottomRight, new Vector2(33, -10.5f) },
        };
        spawnPointList = new List<PointNames> { PointNames.Center, PointNames.TopRight, PointNames.TopLeft, PointNames.BottomLeft, PointNames.BottomRight };
        StartCoroutine(NextWave());
    }
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            showTutorial = false;
            PlayerPrefs.SetString("tutorial", "false");
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            
            if (FindObjectOfType<Player>() == null) { print("quit!");Application.Quit();}
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            showTutorial = true;
            PlayerPrefs.SetString("tutorial", "true");
        }
    }

    private IEnumerator SlideCardDown() {
        // card: 25.73313 -> 0
        // text: 456 -> 0
        if (curWave != 1) { waveText.text = "new spell unlocked!"; }
        else { 
            waveText.text = "get ready...";
        }
        for (int i = 1; i < 41; i++) {
            cardTilemap.transform.localPosition = new Vector2(0, 25.733f - i * 25.733f/40);
            waveText.transform.localPosition = new Vector2(0, 456.0f - i * (456.0f-42.67f)/40);
            yield return new WaitForSeconds(0.025f);
        }
        if (curWave == 1) {
            soundManager.PlayClip("unlocked");
            yield return new WaitForSeconds(2f);
            if(!PlayerPrefs.HasKey("deaths")) PlayerPrefs.SetInt("deaths",1);
            waveText.text = "attempt " + PlayerPrefs.GetInt("deaths"); 
        }
        soundManager.PlayClip("unlocked");
        if (curWave != 1) {player.UnlockNextSpell();}
        yield return new WaitForSeconds(1.5f); 
        if (curWave is 6 or 9 or 11) {
            soundManager.PlayClip("unlocked");
            player.UnlockNextStaff();
            waveText.text = "new staff unlocked!";
            yield return new WaitForSeconds(1.5f);
        }
        soundManager.PlayClip("unlocked");
        waveText.text = $"-wave {curWave}/11-";
        yield return new WaitForSeconds(1.5f);
        for (int i = 1; i < 41; i++) {
            cardTilemap.transform.localPosition = new Vector2(0, i * 25.733f/40);
            waveText.transform.localPosition = new Vector2(0, 42.67f + i * (456.0f-42.67f)/40);
            yield return new WaitForSeconds(0.025f);
        }
    }

    private void TutorialText(string text, float waitTime) { StartCoroutine(TutorialTextCoro(text, waitTime)); }

    private IEnumerator TutorialTextCoro(string text, float waitTime) {
        tutorialText.text = text;
        // text: 421 -> 309.72
        // card: 25.6 -> 19.02
        for (int i = 0; i < 20; i++) {
            tutorialBanner.transform.localPosition -= new Vector3(0, (25.6f-19.02f)/20);
            tutorialText.transform.localPosition -= new Vector3(0, (421f-309.62f)/20);
            yield return new WaitForSeconds(0.025f);
        }
        yield return new WaitForSeconds(waitTime);
        for (int i = 0; i < 20; i++) {
            tutorialBanner.transform.localPosition += new Vector3(0, (25.6f-19.02f)/20);
            tutorialText.transform.localPosition += new Vector3(0, (421f-309.62f)/20);
            yield return new WaitForSeconds(0.025f);
        }
    }
    
    private IEnumerator NextWave() {
        if (!lockWave) {
            lockWave = true;
            curWave++;
            for (int i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }
            foreach (GameObject b in bullets) {
                if (b != null) { Destroy(b); }
            }
            if (bossMinions != null) { StopCoroutine(bossMinions); }
            bullets.Clear();
            bossBar.SetActive(false);
            bossBarBg.SetActive(false);
            waveText.fontSize = curWave == 12 ? 64 : 128;
            switch (curWave) {
                // regular: yellow, simpletracker: green, shotgun: babyblue, machinegun: orange, fatshot: purple, wavy:green
                case 1:
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(7.5f);
                    if (showTutorial) {
                        TutorialText("Use <WASD> or <Arrow Keys> to move and <Left Click> to shoot.\nPress <P> to pause the game.", 6f);
                        yield return new WaitForSeconds(7.7f);
                        if (showTutorial) {
                            TutorialText("Coming up is a basic enemy - very simple to dispatch with a basic attack.", 5f);
                            yield return new WaitForSeconds(5.5f);
                        }
                    }
                    liveEnemies = 1;
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, forceCenterSpawn:true, reduceHealth:true);
                    break;
                case 2:
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    if (showTutorial) {
                        TutorialText("With more enemies, spells become important.\nPress <1> to incinerate your enemies with a fireball!", 5f);
                        yield return new WaitForSeconds(5.5f);
                    }
                    liveEnemies = 4;
                    for (int i = 0; i < 1; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, forceCenterSpawn: true);
                    }
                    yield return new WaitForSeconds(5f);
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, forceCenterSpawn: true);
                    }
                    break;
                case 3:
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    if (showTutorial) {
                        TutorialText("More spells means you can combo them together for more power.\nUse <1> and <2> on the same spot to pull enemies and guarantee your fireball to hit!", 5f);
                        yield return new WaitForSeconds(5.5f);
                    }
                    liveEnemies = 10;
                    for (int i = 0; i < 2; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(3f);
                    for (int i = 0; i < 4; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(2f);
                    for (int i = 0; i < 2; i++) {
                        SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(2f);
                    for (int i = 0; i < 2; i++) {
                        SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                        yield return new WaitForSeconds(0.8f);
                    }
                    break;
                case 4:
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    if (showTutorial) {
                        TutorialText("You slowly regenerate health when there are 3 or more enemies alive.", 5f);
                        yield return new WaitForSeconds(5.5f);
                    }
                    liveEnemies = 7;
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                    yield return new WaitForSeconds(0.8f);
                    yield return new WaitForSeconds(6f);
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                    yield return new WaitForSeconds(0.8f);
                    break;
                case 5: 
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    if (showTutorial) {
                        TutorialText("First bossfight incoming - good luck!", 3f);
                        yield return new WaitForSeconds(3.5f);
                    }
                    ScaleBossHP(1);
                    bossBar.SetActive(true);
                    bossBarBg.SetActive(true);
                    liveEnemies = 1;
                    SummonEnemy(Enemy.EnemyType.BossOne, Colors.pastelRed, forceCenterSpawn:true);
                    bossMinions = StartCoroutine(SpawnMinionsFor(Enemy.EnemyType.BossOne));
                    break;
                case 6:
                    player.HealAfterBoss();
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f+1.5f);
                    if (showTutorial) {
                        TutorialText("After defeating a boss, your staff is upgraded and you are partially healed.", 3f);
                        yield return new WaitForSeconds(3.5f);
                    }
                    yield return new WaitForSeconds(3.5f);
                    liveEnemies = 24;
                    for (int i = 0; i < 5; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(5f);
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                        yield return new WaitForSeconds(0.8f);
                    }
                    for (int i = 0; i < 5; i++) {
                        SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(5f);
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                        yield return new WaitForSeconds(0.8f);
                    }
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                        yield return new WaitForSeconds(0.8f);
                    }
                    for (int i = 0; i < 5; i++) {
                        SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                        yield return new WaitForSeconds(0.8f);
                    }
                    break;
                case 7:
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    liveEnemies = 21;
                    for (int i = 0; i < 4; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(5f);
                    for (int i = 0; i < 5; i++) {
                        SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                        yield return new WaitForSeconds(0.8f);
                    }
                    SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                    yield return new WaitForSeconds(5f);
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(5f);
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                        yield return new WaitForSeconds(0.8f);
                    }
                    for (int i = 0; i < 6; i++) {
                        SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                        yield return new WaitForSeconds(0.8f);
                    }
                    break;
                case 8:                 
                    ScaleBossHP(1);
                    bossBar.SetActive(true);
                    bossBarBg.SetActive(true);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    liveEnemies = 1;
                    SummonEnemy(Enemy.EnemyType.BossTwo, Colors.orange, forceCenterSpawn:true);
                    bossMinions = StartCoroutine(SpawnMinionsFor(Enemy.EnemyType.BossTwo));
                    break;
                case 9:
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    player.HealAfterBoss();
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f+1.5f);
                    liveEnemies = 22;
                    SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                    SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                    yield return new WaitForSeconds(7f);
                    for (int i = 0; i < 4; i++) {
                        SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                        yield return new WaitForSeconds(0.8f);
                        yield return new WaitForSeconds(3f);
                    }
                    break;
                case 10:
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f);
                    ScaleBossHP(1);
                    bossBar.SetActive(true);
                    bossBarBg.SetActive(true);
                    liveEnemies = 1;
                    SummonEnemy(Enemy.EnemyType.BossThree, Colors.red, forceCenterSpawn:true);
                    bossMinions = StartCoroutine(SpawnMinionsFor(Enemy.EnemyType.BossThree));
                    break;
                case 11:
                    player.HealAfterBoss();
                    bossBar.SetActive(false);
                    bossBarBg.SetActive(false);
                    StartCoroutine(SlideCardDown());
                    yield return new WaitForSeconds(5f+1.5f);        
                    TutorialText("Good luck :)", 3f);
                    yield return new WaitForSeconds(3f);  
                    liveEnemies = 80;
                    for (int i = 0; i < 5; i++) {
                        SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                        SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                        SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                        SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                        SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                        SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                        yield return new WaitForSeconds(0.8f);
                        SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                        SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                        yield return new WaitForSeconds(12f);
                    }
                    break;
                case 12: 
                    StartCoroutine(HideGame(true));
                    soundManager.PlayClip("unlocked");
                    break;
                default: Debug.LogError($"this wave ({curWave}) was not accounted for!"); break;
            }
            yield return new WaitForSeconds(2f);
            lockWave = false;
        }
        else {
            print("tried to advance waves while locked!");
        }
    }
    
    private IEnumerator SpawnMinionsFor(Enemy.EnemyType enemyType) {
        while (true) {
            switch (enemyType) {
                case Enemy.EnemyType.BossOne: {
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.aqua, true);
                    yield return new WaitForSeconds(0.8f);
                    yield return new WaitForSeconds(10f);
                    break;
                }
                case Enemy.EnemyType.BossTwo:
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta, true);
                    yield return new WaitForSeconds(0.8f);
                    yield return new WaitForSeconds(10f);
                    break;
                case Enemy.EnemyType.BossThree:
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen, true);
                    yield return new WaitForSeconds(0.8f);
                    yield return new WaitForSeconds(10f);
                    break;
                default: 
                    Debug.LogError($"shouldn't be spawning boss minions for {enemyType}");
                    break;
            }
            
        }
    }
    
    public void ScaleBossHP(float percentage) {
        bossBar.transform.localScale = new Vector2(28f * percentage, 1);
    }
    
    private void SummonEnemy(Enemy.EnemyType enemyType, Color color, bool isUseless=false, bool forceCenterSpawn=false, bool reduceHealth=false) {
        Vector3 pt = forceCenterSpawn 
            ? spawnPoints[PointNames.Center] 
            : spawnPoints[spawnPointList[Random.Range(0, spawnPoints.Count)]] + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        GameObject g = Instantiate(enemyPrefab, pt, Quaternion.identity);
        Enemy e = g.GetComponent<Enemy>();
        e.enemyType = enemyType;
        e.freezeMovement = true;
        e.uselessMinion = isUseless;
        e.transform.parent = transform;
        if (reduceHealth) { e.health = e.health / 2; }
        StartCoroutine(FadeIn(g.GetComponent<SpriteRenderer>(), e));
        SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
        sr.color = color;
        Color temp = color;
        temp.a = 0f;
        sr.color = temp;
        spawnedEnemies.Add(e);
    }
    
    private IEnumerator FadeIn(SpriteRenderer sr, Enemy e) {
        yield return new WaitForSeconds(0.01f);
        Color temp = sr.color;
        temp.a = 0f;
        sr.color = temp;
        for (int i = 0; i < 10; i++) {
            yield return new WaitForSeconds(0.1f);
            temp.a += 0.1f;
            sr.color = temp;
        }
        e.freezeMovement = false;
        // phase start handled in enemy start
    }
    
    public void DecrementCount(GameObject g) {
        Destroy(g);
        if (!g.GetComponent<Enemy>().uselessMinion) { 
            liveEnemies--; 
            if (liveEnemies <= 0) {
                StartCoroutine(NextWave());
            }
        }
    }
    
    public void SummonAtPos(Vector3 pos) {
        StartCoroutine(SummonAtPosCoro(pos));
    }
    
    private IEnumerator SummonAtPosCoro(Vector3 pos) {
        Vector2 drop = (Vector2)pos + new Vector2(Random.Range(-rand, rand), Random.Range(-rand, rand));
        GameObject b = Instantiate(lStrikeBorder, drop, Quaternion.identity);
        GameObject c = Instantiate(lStrikeCenter, drop, Quaternion.identity);
        float initial = c.transform.localScale.x;
        for (int i = 0; i < (2f*10); i++) {
            c.transform.localScale -= new Vector3(initial/(2f*10), initial/(2f*10), 0f);
            yield return new WaitForSeconds(0.1f);
        }
        GameObject l = Instantiate(lightningGO, drop + new Vector2(0, 1f), Quaternion.identity);
        l.transform.localScale = new Vector3(4f, 4f, 1f);
        l.GetComponent<Bullet>().damage = 30;
        l.GetComponent<Bullet>().behavior = Bullet.Behavior.Linger;
        l.GetComponent<SpriteRenderer>().color = Colors.elstrike;
        yield return new WaitForSeconds(0.3f);
        Destroy(c);
        Destroy(l);
        Destroy(b);
    }
    
    public void PlayerDies() {
        foreach (Enemy e in spawnedEnemies) {
            if (e != null) { Destroy(e.gameObject); }
        }
        StartCoroutine(HideGame(false));
    }

    private IEnumerator HideGame(bool wonGame) {
        SpriteRenderer sr = behindBanner.GetComponent<SpriteRenderer>();
        Color temp = sr.color;
        temp.a = 0;
        sr.color = temp;
        for (int i = 0; i < 40; i++) {
            yield return new WaitForSeconds(0.025f);
            temp.a += 0.025f;
            sr.color = temp;
        }
        if (wonGame) { 
            pauseMenu.Pause(false, curWave, PlayerPrefs.GetInt("deaths"));
        }
        else {
            pauseMenu.Pause(true, curWave);
        }
    }
    
    public void PlayFire() {
        soundManager.PlayClip("firehit");
    }
}
