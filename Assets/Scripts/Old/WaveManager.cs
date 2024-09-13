using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{


    [Header("Spawn Manager Settings")]
    public bool _spawner;
    public List<GameObject> enemyPrefabs;
    public List<GameObject> powerUpPrefabs;
    [Range (0, 100)] public float _spawnRange;

    [Header("References")]
    private GameManager gameManager;


    [Header("Just For The Inspector")]
    public int _waveNumber;
    public int _enemyCount;


    void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    void Update()
    {
        WaveController();
    }



    void WaveController() {

        if (_spawner) {
            _enemyCount =  GameObject.FindGameObjectsWithTag("Enemy").Length;
            if ((_enemyCount == 0) && (enemyPrefabs.Count > 0)) { 

                _waveNumber++;
                SpawnEnemies(_waveNumber); 
                SpawnPowerUps(_waveNumber);
                Debug.Log("Wave " + _waveNumber);
            }
        }

        

    }

    void SpawnEnemies(int _enemiesToSpawn) {

        if (enemyPrefabs.Count > 0) {

            for (int i = 0; i < _enemiesToSpawn; i++) {

                int randomEnemy = Random.Range(0, enemyPrefabs.Count);
                Instantiate(enemyPrefabs[randomEnemy], GenerateRandSpawnPosition(), enemyPrefabs[randomEnemy].transform.rotation);
            }
        }
    }

    void SpawnPowerUps(int _powerUpToSpawn) {

        if (powerUpPrefabs.Count > 0) {

            for (int i = 0; i < _powerUpToSpawn; i++) {

                int randomPowerUp = Random.Range(0, powerUpPrefabs.Count);
                Instantiate(powerUpPrefabs[randomPowerUp], GenerateRandSpawnPosition(), powerUpPrefabs[randomPowerUp].transform.rotation);

            }
        }
    }


    Vector3 GenerateRandSpawnPosition() {

        float randomX = Random.Range(-_spawnRange, _spawnRange);
        float randomZ = Random.Range(-_spawnRange, _spawnRange);
        Vector3 randomSpawnPos = new(randomX, 5f, randomZ);

        return randomSpawnPos;

    }



}
