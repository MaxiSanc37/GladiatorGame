using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//credits to The Game Dev Cave on Youtube

public class WaveSystem : MonoBehaviour
{
    //class that holds the contents of the wave (the enemies)
    [System.Serializable]   
    public class WaveContent
    {
        //list of enemy game objects
        [SerializeField][NonReorderable]
        GameObject[] enemySpawner;

        //returns the list of enemies that will spawn
        public GameObject[] GetEnemySpawnList()
        {
            return enemySpawner;
        }
    }

    //List of waves with their respective number of enemies (in WaveContent class)
    [SerializeField][NonReorderable] WaveContent[] waves;
    int currentWave = 0;
    public float spawnRange = 200f;
    public List<GameObject> currEnemies;
    private bool waveCleared = false;

    public TMP_Text waveCounterText;

    public UpgradeManager upgradeManager;

    // Start is called before the first frame update
    void Start()
    {
        //set the wave text in the UI
        waveCounterText.text = $"Wave {currentWave + 1}/{waves.Length}";
        SpawnWave();
    }

    // Update is called once per frame
    void Update()
    {
        /*checks if the enemies killed are greater than or equal the number of
        enemies in the current wave*/
        if (!waveCleared && currEnemies.Count == 0)
        {
            waveCleared = true;
            StartCoroutine(HandleEndOfWave());
        }
    }

    //Gives a small delay for the enemy to die in order to show the upgrade menu.
    private IEnumerator HandleEndOfWave()
    {
        yield return new WaitForSeconds(1f);
        upgradeManager.ShowRandomUpgrades(3);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        currentWave++;
        SpawnWave();
        waveCleared = false;
    }

    //spawns a wave of enemies
    void SpawnWave()
    {
        //check that the current wave does not surpass the total waves
        if(currentWave <= waves.Length-1)
        {
            /*while the counter is less than the number of enemies that must be
            spawned for this wave*/
            for (int i = 0; i < waves[currentWave].GetEnemySpawnList().Length; i++)
            {
                //adjust wave counter text in the UI
                waveCounterText.text = $"Wave {currentWave + 1}/{waves.Length}";
                //instantiate an enemy in the current wave, calling the spawn position finder
                GameObject newSpawn = Instantiate(waves[currentWave].GetEnemySpawnList()[i], FindSpawnPos(), Quaternion.identity);
                //adds the newspawn enemy to the list of current enemies
                currEnemies.Add(newSpawn);
                //getting the Actor script element from the enemy
                Actor enemy = newSpawn.GetComponent<Actor>();
                enemy.SetSpawner(this);
            }
        }
        //if it surpasses the total waves, destroy the spawner
        else
        {
            Destroy(gameObject);
        }
    }

    //this func finds a spawning location
    Vector3 FindSpawnPos()
    {
        //the vector with the spawn position
        Vector3 spawnPos;

        //random x value
        float xPos = Random.Range(-spawnRange, spawnRange) + transform.position.x;
        //random y value
        float zPos = Random.Range(-spawnRange, spawnRange) + transform.position.z;
        //y value
        float yLoc = transform.position.y;

        //spawnPos vector with the previous determined values
        spawnPos = new Vector3(xPos, yLoc, zPos);

        //checks that the spawn location is suitable via a raycasts
        if (Physics.Raycast(spawnPos, Vector3.down, 5))
        {
            //return spawn position
            return spawnPos;
        }
        else
        {
            //call the function again to find a valid spawn position
            return FindSpawnPos();
        }
    }

    public int GetCurrentWaveIndex()
    {
        return currentWave;
    }
}
