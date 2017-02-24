using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State { Paused, Playing }

    [SerializeField]
    private GameObject [] SpawnEnemies;

    [SerializeField]
    private GameObject[] SpawnColls;

    [SerializeField]
    private GameObject[] SpawnPows;

    [SerializeField]
    private Player PlayerPrefab;

    [SerializeField]
    private Arena Arena;

    [SerializeField]
    private float TimeBetweenEnemySpawns;

    [SerializeField]
    private float TimeBetweenCollSpawns;

    [SerializeField]
    private float TimeBetweenPowSpawns;

    [SerializeField]
    private int MaxPowerups;
    
    //TODO: Split up Collectible and Powerups (Since there is only one collectible, and powerups will disappear, and act a lot differently, so it makes sense)
    private List<GameObject> mEnemies;
    private List<GameObject> mColls;
    private List<GameObject> mPows;
    private Player mPlayer;
    private State mState;
    private float mNextEnemySpawn;
    private float mNextCollSpawn;
    private float mNextPowSpawn;
    private int mNumPowerups;

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

            mNextCollSpawn -= Time.deltaTime;

            if (mNextCollSpawn <= 0.0f)
            {
                if (mColls == null)
                {
                    mColls = new List<GameObject>();
                }

                int indexToSpawnColl = Random.Range(0, SpawnColls.Length);
                GameObject spawnColl = SpawnColls[indexToSpawnColl];
                GameObject spawnedCollInstance = Instantiate(spawnColl);
                spawnedCollInstance.transform.parent = transform;
                mColls.Add(spawnedCollInstance);
                mNextCollSpawn = TimeBetweenCollSpawns;
            }
            //TODO: If maximum number of powerups reached, delete a random old one, and then make new one.
            //TODO_maybe: Potentially, only have a maximum of one of each powerup - but this doesn't really matter too much, it's just a feature that could easily be put in.
            mNextPowSpawn -= Time.deltaTime;

            if (mNextPowSpawn <= 0.0f)
            {
                if (mPows == null)
                {
                    mPows = new List<GameObject>();
                }

                if (mNumPowerups >= MaxPowerups)
                {
                    int indexToDeletePow = Random.Range(0, mPows.Count - 1);
                    GameObject powToDelete = mPows[indexToDeletePow];
                    mPows.Remove(powToDelete);
                    Destroy(powToDelete);
                }

                int indexToSpawnPow = Random.Range(0, SpawnPows.Length);
                GameObject spawnPow = SpawnPows[indexToSpawnPow];
                GameObject spawnedPowInstance = Instantiate(spawnPow);
                spawnedPowInstance.transform.parent = transform;
                mPows.Add(spawnedPowInstance);
                mNextPowSpawn = TimeBetweenPowSpawns;
                mNumPowerups++;
            }

            if (mPlayer.UsingBomb() && mEnemies != null)
            {
                List<int> enemyIndicesToRemove = new List<int>();

                float bombDistance = mPlayer.GetBombRadius();

                for (int count = 0; count < mEnemies.Count; ++count)
                {
                    Vector3 difference = mPlayer.transform.position - mEnemies[count].transform.position;
                    
                    if (difference.magnitude <= bombDistance)
                    {
                        enemyIndicesToRemove.Add(count);
                    }
                }

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

        if (mColls != null)
        {
            for (int count = 0; count < mColls.Count; ++count)
            {
                Destroy(mColls[count]);
            }
            mColls.Clear();
        }

        if (mPows != null)
        {
            for (int count = 0; count < mPows.Count; ++count)
            {
                Destroy(mPows[count]);
            }
            mPows.Clear();
        }

        mNextEnemySpawn = TimeBetweenEnemySpawns;
        mNextCollSpawn = TimeBetweenCollSpawns;
        mNextPowSpawn = TimeBetweenPowSpawns;
        mNumPowerups = 0;
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

        if (mColls != null)
        {
            for (int count = 0; count < mColls.Count; ++count)
            {
                Destroy(mColls[count]);
            }
            mColls.Clear();
        }

        if (mPows != null)
        {
            for (int count = 0; count < mPows.Count; ++count)
            {
                Destroy(mPows[count]);
            }
            mPows.Clear();
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
