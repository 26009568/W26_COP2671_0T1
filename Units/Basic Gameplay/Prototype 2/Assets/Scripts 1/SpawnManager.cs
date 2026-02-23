using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] animalPrefabs;
private float spawnRangeX = 35.0f;
private float spawnPosZ = 20f;
private float startDelay = 20f;
private float spawnInterval = 1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         InvokeRepeating("SpawnRandomAnimal", startDelay, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
     
    }
    void SpawnRandomAnimal()
    {
         int animalIndex = Random.Range(0, animalPrefabs.Length);

            Instantiate(animalPrefabs[animalIndex], new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 0, spawnPosZ), animalPrefabs[animalIndex].transform.rotation);

    }
}
