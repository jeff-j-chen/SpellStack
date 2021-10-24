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
    private Player player;
    private void Start() {
        player = FindObjectOfType<Player>();
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
    
    
    public IEnumerator NextWave() {
        curWave++;
        Enemy e;
        // if (curWave != 1) { yield return new WaitForSeconds(3f); } 
        print("add delay back in here later!");
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
                player.UnlockNextSpell();
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
                player.UnlockNextSpell();
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
                player.UnlockNextSpell();
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
                player.UnlockNextSpell();
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
                player.UnlockNextSpell();
                player.UnlockNextStaff();
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
                player.UnlockNextSpell();
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
                player.UnlockNextSpell();
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
                player.UnlockNextSpell();
                player.UnlockNextStaff();
                liveEnemies = 10;
                for (int i = 0; i < 5; i++) {
                    SummonEnemy(Enemy.EnemyType.MachineGunner, Colors.orange);
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
                player.UnlockNextSpell();
                liveEnemies = 1;
                SummonEnemy(Enemy.EnemyType.BossThree, Colors.red, forceCenterSpawn:true);
                break;
            default: Debug.LogError($"this wave ({curWave}) was not accounted for!"); break;
        }
        waveText.text = $"Wave {curWave}";
    }
    
    public void ScaleBossHP(float percentage) {
        bossBar.transform.localScale = new Vector2(30f * percentage, 1);
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
        StartCoroutine(FadeIn(g.GetComponent<SpriteRenderer>(), e));
        SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
        sr.color = color;
        Color temp = color;
        temp.a = 0f;
        sr.color = temp;
    }
    
    private IEnumerator FadeIn(SpriteRenderer sr, Enemy e) {
        yield return new WaitForSeconds(0.01f);
        Color temp = sr.color;
        temp.a = 0f;
        // sr.color = temp;
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
}
