using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State { Paused, Playing }

    [SerializeField]
    private GameObject [] SpawnEnemies;

    [SerializeField]
    private GameObject[] SpawnCollPows;

    [SerializeField]
    private Player PlayerPrefab;

    [SerializeField]
    private Arena Arena;

    [SerializeField]
    private float TimeBetweenEnemySpawns;

    [SerializeField]
    private float TimeBetweenCollPowSpawns;
    
    //TODO: Split up Collectible and Powerups (Since there is only one collectible, and powerups will disappear, and act a lot differently, so it makes sense)
    private List<GameObject> mEnemies;
    private List<GameObject> mCollPows;
    private Player mPlayer;
    private State mState;
    private float mNextEnemySpawn;
    private float mNextCollPowSpawn;

    void Awake()
    {
        mPlayer = Instantiate(PlayerPrefab);
        mPlayer.transform.parent = transform;

        ScreenManager.OnNewGame += ScreenManager_OnNewGame;
        ScreenManager.OnExitGame += ScreenManager_OnExitGame;
    }

    void Start()
    {
        Arena.Calculate();
        mPlayer.enabled = false;
        mState = State.Paused;
    }

    void Update()
    {
        if(mState == State.Playing)
        {
            //Stop new enemies spawning while repellent powerup is in effect.
            //It is multiplied by 1.1f so that a new enemy doesn't just instantly spawn after powerup expires.
            if (mPlayer.HasJustGotRepelPowerup())
            {
                mNextEnemySpawn += mPlayer.GetPowerupTime() * 1.1f;
            }
            mNextEnemySpawn -= Time.deltaTime;
            if( mNextEnemySpawn <= 0.0f )
            {
                if (mEnemies == null)
                {
                    mEnemies = new List<GameObject>();
                }

                int indexToSpawnEnemy = Random.Range(0, SpawnEnemies.Length);
                GameObject spawnEnemy = SpawnEnemies[indexToSpawnEnemy];
                GameObject spawnedEnemyInstance = Instantiate(spawnEnemy);
                spawnedEnemyInstance.transform.parent = transform;
                mEnemies.Add(spawnedEnemyInstance);
                mNextEnemySpawn = TimeBetweenEnemySpawns;
            }

            mNextCollPowSpawn -= Time.deltaTime;
            if (mNextCollPowSpawn <= 0.0f)
            {
                if (mCollPows == null)
                {
                    mCollPows = new List<GameObject>();
                }

                int indexToSpawnCollPow = Random.Range(0, SpawnCollPows.Length);
                GameObject spawnCollPow = SpawnCollPows[indexToSpawnCollPow];
                GameObject spawnedCollPowInstance = Instantiate(spawnCollPow);
                spawnedCollPowInstance.transform.parent = transform;
                mCollPows.Add(spawnedCollPowInstance);
                mNextCollPowSpawn = TimeBetweenCollPowSpawns;
            }

            /*
            //Player has a gotbomb bool - GotBomb() is public/internal
            if (player.UsingBomb() && mEnemies != null) 
            {
                List<int> enemyIndicesToRemove = new List<int>();

                distance = player.GetBombRadius();

                for each enemy in mEnemies 
                {
                    vector3 difference = player.transform.position - enemy.transform.position;
    
                    if (difference.magnitude < distance) {
                        enemyIndicesToRemove.Add(i); //i is for loop iteration variable
                    }
                }

                for each index in enemyIndicesToRemove
                {
                    Enemy enemy = mEnemies[index];
                    mEnemies.Remvove(enemy);
                    Destroy(enemy);
                }

                player.UsedBomb(); //Sets a bool to false - this function us internal
            }
            */

        if (mPlayer.UsingBomb() && mEnemies != null)
            {
                List<int> enemyIndicesToRemove = new List<int>();

                float bombDistance = mPlayer.GetBombRadius();

                for (int count = 0; count < mEnemies.Count; ++count)
                {
                    Vector3 difference = mPlayer.transform.position - mEnemies[count].transform.position;
                    if (mEnemies[count].GetComponent<GroupTag>().Affiliation == GroupTag.Group.Two)
                    {
                        print("Enemy of type one is " + difference.magnitude + " away from player");
                    }
                    else if (mEnemies[count].GetComponent<GroupTag>().Affiliation == GroupTag.Group.One)
                    {
                        print("Enemy of type two is " + difference.magnitude + " away from player");
                    }
                    else
                    {
                        print("This shouldn't happen.");
                    }
                    
                    if (difference.magnitude <= bombDistance)
                    {
                        enemyIndicesToRemove.Add(count);
                    }
                }

                print("FOUND " + enemyIndicesToRemove.Count + " ENEMIES TO REMOVE");

                for (int i = 0; i < enemyIndicesToRemove.Count; ++i)
                {
                    int index = enemyIndicesToRemove[i];
                    GameObject enemy = mEnemies[index - i];
                    mEnemies.Remove(enemy);
                    FlockWithGroup flockComp = enemy.GetComponent<FlockWithGroup>();
                    if (flockComp != null)
                    {
                        flockComp.UpdateBuddyList();
                    }
                    Destroy(enemy);
                }

                mPlayer.UsedBomb();
            }
        }
    }

    private void BeginNewGame()
    {
        if (mEnemies != null)
        {
            for (int count = 0; count < mEnemies.Count; ++count)
            {
                Destroy(mEnemies[count]);
            }
            mEnemies.Clear();
        }

        if (mCollPows != null)
        {
            for (int count = 0; count < mCollPows.Count; ++count)
            {
                Destroy(mCollPows[count]);
            }
            mCollPows.Clear();
        }

        mNextEnemySpawn = TimeBetweenEnemySpawns;
        mNextCollPowSpawn = TimeBetweenCollPowSpawns;
        mPlayer.ResetPlayer();
        mPlayer.enabled = true;
        mState = State.Playing;
    }

    private void EndGame()
    {
        mPlayer.enabled = false;
        mState = State.Paused;

        if (mEnemies != null)
        {
            for (int count = 0; count < mEnemies.Count; ++count)
            {
                Destroy(mEnemies[count]);
            }
            mEnemies.Clear();
        }

        if (mCollPows != null)
        {
            for (int count = 0; count < mCollPows.Count; ++count)
            {
                Destroy(mCollPows[count]);
            }
            mCollPows.Clear();
        }

        mPlayer.ResetPlayer();
    }

    private void ScreenManager_OnNewGame()
    {
        BeginNewGame();
    }

    private void ScreenManager_OnExitGame()
    {
        EndGame();
    }

    internal void DelayEnemies(float delayTime)
    {
        mNextEnemySpawn += delayTime;
    }
}
