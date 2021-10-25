using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private Player player;
    private SoundManager soundManager;
    private void Start() {
        player = FindObjectOfType<Player>();
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
    
    private IEnumerator SlideCardDown() {
        // card: 25.73313 -> 0
        // text: 456 -> 0
        if (curWave != 1) { waveText.text = "new spell unlocked!"; }
        else { 
            if(!PlayerPrefs.HasKey("deaths")) PlayerPrefs.SetInt("deaths",1);
            waveText.text = "attempt " + PlayerPrefs.GetInt("deaths"); 
        }
        for (int i = 1; i < 41; i++) {
            cardTilemap.transform.localPosition = new Vector2(0, 25.733f - i * 25.733f/40);
            waveText.transform.localPosition = new Vector2(0, 456.0f - i * (456.0f-42.67f)/40);
            yield return new WaitForSeconds(0.025f);
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
        waveText.text = $"-wave {curWave}-";
        yield return new WaitForSeconds(1.5f);
        for (int i = 1; i < 41; i++) {
            cardTilemap.transform.localPosition = new Vector2(0, i * 25.733f/40);
            waveText.transform.localPosition = new Vector2(0, 42.67f + i * (456.0f-42.67f)/40);
            yield return new WaitForSeconds(0.025f);
        }
    }
    
    
    private IEnumerator NextWave() {
        curWave++;
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
        StartCoroutine(SlideCardDown());
        bossBar.SetActive(false);
        bossBarBg.SetActive(false);
        if (curWave is 6 or 9 or 11) { yield return new WaitForSeconds(4.5f); }
        else { yield return new WaitForSeconds(3f); }
        switch (curWave) {
            // regular: yellow, simpletracker: green, shotgun: babyblue, machinegun: orange, fatshot: purple
            case 1:
                print("basic enemy. pretty simple to dispatch with your basic attack");
                liveEnemies = 1;
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                yield return new WaitForSeconds(1.8f);
                break;
            case 2:
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                print("with more enemies, spells become more important. try using the lightning spell you just unlocked!");
                liveEnemies = 3;
                for (int i = 0; i < 3; i++) {
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(1.8f);
                }
                break;
            case 3:
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                print("you may have noticed that the lightning would be more useful if the enemy was dragged towards it... try comboing it with the whirlpool spell!");
                liveEnemies = 6;
                for (int i = 0; i < 4; i++) {
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(0.8f);
                }
                for (int i = 0; i < 2; i++) {
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.green);
                    yield return new WaitForSeconds(0.8f);
                }
                break;
            case 4:
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                liveEnemies = 9;
                for (int i = 0; i < 3; i++) {
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(0.8f);
                }
                for (int i = 0; i < 6; i++) {
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.green);
                    yield return new WaitForSeconds(0.8f);
                }
                break;
            case 5: 
                bossBar.SetActive(true);
                bossBarBg.SetActive(true);
                print("first bossfight, good luck!");
                liveEnemies = 1;
                SummonEnemy(Enemy.EnemyType.BossOne, Colors.pastelRed, forceCenterSpawn:true);
                while (true) {
                    for (int i = 0; i < 3; i++) {
                        SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, true);
                        yield return new WaitForSeconds(0.8f);
                    }
                    yield return new WaitForSeconds(20f);
                }
            case 6:
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                liveEnemies = 13;
                for (int i = 0; i < 5; i++) {
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(0.8f);
                }
                yield return new WaitForSeconds(5f);
                for (int i = 0; i < 3; i++) {
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                    yield return new WaitForSeconds(0.8f);
                }
                for (int i = 0; i < 2; i++) {
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                    yield return new WaitForSeconds(0.8f);
                }
                yield return new WaitForSeconds(5f);
                for (int i = 0; i < 3; i++) {
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.green);
                    yield return new WaitForSeconds(0.8f);
                }
                break;
            case 7:
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                liveEnemies = 14;
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
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(0.8f);
                }
                SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                break;
            case 8:                 
                bossBar.SetActive(true);
                bossBarBg.SetActive(true);
                liveEnemies = 1;
                SummonEnemy(Enemy.EnemyType.BossTwo, Colors.purple, forceCenterSpawn:true);
                while (true) {
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.green, true);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue, true);
                    yield return new WaitForSeconds(0.8f);
                    yield return new WaitForSeconds(15f);
                }
            case 9:
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                liveEnemies = 10;
                for (int i = 0; i < 5; i++) {
                    SummonEnemy(Enemy.EnemyType.WavyShooter, Colors.pastelGreen);
                    yield return new WaitForSeconds(0.8f);
                }
                yield return new WaitForSeconds(5f);
                for (int i = 0; i < 5; i++) {
                    SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                    yield return new WaitForSeconds(0.8f);
                }
                break;
            case 10:                 
                bossBar.SetActive(true);
                bossBarBg.SetActive(true);
                liveEnemies = 1;
                SummonEnemy(Enemy.EnemyType.BossThree, Colors.red, forceCenterSpawn:true);
                break;
            case 11:                 
                bossBar.SetActive(false);
                bossBarBg.SetActive(false);
                liveEnemies = 70;
                for (int i = 0; i < 5; i++) {
                    SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                    SummonEnemy(Enemy.EnemyType.LightningMage, Colors.red);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    SummonEnemy(Enemy.EnemyType.Regular, Colors.yellow);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.green);
                    SummonEnemy(Enemy.EnemyType.FleeingTracker, Colors.green);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                    SummonEnemy(Enemy.EnemyType.Shotgun, Colors.babyBlue);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
                    yield return new WaitForSeconds(0.8f);
                    SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                    SummonEnemy(Enemy.EnemyType.FatShot, Colors.magenta);
                    yield return new WaitForSeconds(10.8f);
                }
                break;
            default: Debug.LogError($"this wave ({curWave}) was not accounted for!"); break;
        }
    }
    
    public void ScaleBossHP(float percentage) {
        bossBar.transform.localScale = new Vector2(28f * percentage, 1);
    }
    
    private void SummonEnemy(Enemy.EnemyType enemyType, Color color, bool isUseless=false, bool forceCenterSpawn=false) {
        Vector3 pt = forceCenterSpawn 
            ? spawnPoints[PointNames.Center] 
            : spawnPoints[spawnPointList[Random.Range(0, spawnPoints.Count)]] + new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        GameObject g = Instantiate(enemyPrefab, pt, Quaternion.identity);
        Enemy e = g.GetComponent<Enemy>();
        e.enemyType = enemyType;
        e.freezeMovement = true;
        e.uselessMinion = isUseless;
        e.transform.parent = transform;
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
    
    public void PlayerDies() { StartCoroutine(PlayerDiesCoro()); }
    private IEnumerator PlayerDiesCoro() {
        SpriteRenderer sr = behindBanner.GetComponent<SpriteRenderer>();
        foreach (Enemy e in spawnedEnemies) {
            if (e != null) { Destroy(e.gameObject); }
        }
        StartCoroutine(PresentRetry());
        Color temp = sr.color;
        temp.a = 0;
        sr.color = temp;
        for (int i = 0; i < 40; i++) {
            yield return new WaitForSeconds(0.025f);
            temp.a += 0.025f;
            sr.color = temp;
        }
    }
    
    private IEnumerator PresentRetry() {
        // card: 25.73313 -> 0
        // text: 456 -> 0
        waveText.text = "retry?";
        for (int i = 1; i < 41; i++) {
            cardTilemap.transform.localPosition = new Vector2(0, 25.733f - i * 25.733f/40);
            waveText.transform.localPosition = new Vector2(0, 456.0f - i * (456.0f-42.67f)/40);
            yield return new WaitForSeconds(0.025f);
        }
    }
    
    private IEnumerator HideRetry() {
        // card: 25.73313 -> 0
        // text: 456 -> 0
        waveText.text = "retry?";
        for (int i = 1; i < 41; i++) {
            cardTilemap.transform.localPosition = new Vector2(0, i * 25.733f/40);
            waveText.transform.localPosition = new Vector2(0, 42.67f + i * (456.0f-42.67f)/40);
            yield return new WaitForSeconds(0.025f);
        }
    }
    
    public void AttemptRestart() {
        StartCoroutine(HideRetry());
        Initiate.Fade("Game", Color.black, 2.5f);
    }
    
}
