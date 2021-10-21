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
    [SerializeField] private TextMeshProUGUI waveText;
    private void Start() {
        curWave = 0;
        spawnPoints = new Dictionary<PointNames, Vector2>() {
            { PointNames.Center, new Vector2(0, 0) },
            { PointNames.TopRight, new Vector2(35, 18) },
            { PointNames.TopLeft, new Vector2(-35, 18) },
            { PointNames.BottomLeft, new Vector2(-35, -18) },
            { PointNames.BottomRight, new Vector2(35, -18) },
        };
        spawnPointList = new List<PointNames> { PointNames.Center, PointNames.TopRight, PointNames.TopLeft, PointNames.BottomLeft, PointNames.BottomRight };
        StartCoroutine(NextWave());
    }
    
    
    public IEnumerator NextWave() {
        curWave++;
        Enemy e;
        switch (curWave) {
            case 1: 
                liveEnemies = 5;
                for (int i = 0; i < 5; i++) {
                    e = SpawnEnemyAt(spawnPoints[PointNames.Center] + new Vector2(Random.Range(-4f, 4f), Random.Range(-4f, 4f)), Enemy.EnemyType.Regular);
                    SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
                    sr.color = Colors.pastelRed;
                    Color temp = Colors.pastelRed;
                    temp.a = 0f;
                    sr.color = temp;
                    yield return new WaitForSeconds(1f);
                }
                yield return new WaitForSeconds(5f);
                // for (int i = 0; i < 5; i++) {
                //     e = SpawnEnemyAt(spawnPoints[PointNames.Center] + new Vector2(Random.Range(-4f, 4f), Random.Range(-4f, 4f)), Enemy.EnemyType.FleeingTracker);
                //     SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
                //     sr.color = Colors.pastelRed;
                //     Color temp = Colors.pastelRed;
                //     temp.a = 0f;
                //     sr.color = temp;
                //     yield return new WaitForSeconds(1f);
                // }
                
                break;
            case 2:
                liveEnemies = 1;
                e = SpawnEnemyAt(spawnPoints[PointNames.Center], Enemy.EnemyType.BossOne);
                e.GetComponent<SpriteRenderer>().color = Colors.orange;
                break;
            default: Debug.LogError($"this wave ({curWave}) was not accounted for!"); break;
        }
        waveText.text = $"Wave {curWave}";
    }
    
    public Enemy SpawnEnemyAt(Vector2 loc, Enemy.EnemyType enemyType) {
        GameObject g = Instantiate(enemyPrefab, loc, Quaternion.identity);
        Enemy e = g.GetComponent<Enemy>();
        e.enemyType = enemyType;
        e.freezeMovement = true;
        StartCoroutine(FadeIn(g.GetComponent<SpriteRenderer>(), e));
        return e;
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
        liveEnemies--;
        Destroy(g);
        if (liveEnemies <= 0) {
            StartCoroutine(NextWave());
        }
    }
}
