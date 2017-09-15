using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State { Paused, Playing }

    [SerializeField]
    private GameObject [] SpawnEnemies;

    [SerializeField]
    private GameObject [] SpawnBosses;

    [SerializeField]
    private GameObject[] SpawnColls;

    [SerializeField]
    private GameObject[] SpawnPows;

    [SerializeField]
    private Player[] PlayerPrefab;

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

    [SerializeField]
    private Leaderboard Leaderboard;
    
    private List<GameObject> mEnemies;
    private List<GameObject> mBosses;
    private List<GameObject> mColls;
    private List<GameObject> mPows;
    private Player mPlayer;
    private State mState;
    private float mNextEnemySpawn;
    private float mNextCollSpawn;
    private float mNextPowSpawn;
    private int mNumPowerups;
    private Leaderboard mLeaderboard;
    private float mTimeToSubmit = 0.5f;
    private float mTimeLeftToSubmit;
    private bool mIsPlayerInstantiated;
    private int mBossesSpawned;

    void Awake()
    {
        mLeaderboard = Instantiate(Leaderboard);
        mLeaderboard.enabled = false;
        mTimeLeftToSubmit = 0.0f;
        mIsPlayerInstantiated = false;
        mBossesSpawned = 0;

        ScreenManager.OnExitGame += ScreenManager_OnExitGame;
        ScreenManager.OnViewLeaderboard += ScreenManager_OnViewLeaderboard;
        ScreenManager.OnSubmitAndMainMenu += ScreenManager_OnSubmitAndMainMenu;
        ScreenManager.OnChooseShip1 += ScreenManager_OnChooseShip1;
        ScreenManager.OnChooseShip2 += ScreenManager_OnChooseShip2;
        ScreenManager.OnChooseShip3 += ScreenManager_OnChooseShip3;
        ScreenManager.OnChooseShip4 += ScreenManager_OnChooseShip4;
    }

    void Start()
    {
        Arena.Calculate();
        mPlayer = null;
        mState = State.Paused;
    }

    void Update()
    {
        if (mTimeLeftToSubmit > 0.0f)
        {
            mTimeLeftToSubmit -= Time.deltaTime;

            if (mTimeLeftToSubmit <= 0.0f)
            {
                mLeaderboard.enabled = false;
                mLeaderboard.ResetLeaderboardLook();
            }
        }

        if(mState == State.Playing && !mPlayer.IsDead())
        {

            if (mPlayer.GetScore() >= ((mBossesSpawned + 1) * 500))
            {
                if (mBosses == null)
                {
                    mBosses = new List<GameObject>();
                }

                int indexToSpawnBoss;
                if (mBossesSpawned >= SpawnBosses.Length)
                {
                    indexToSpawnBoss = SpawnBosses.Length - 1;
                }
                else
                {
                    indexToSpawnBoss = mBossesSpawned;
                }

                print("Spawning Boss Number " + indexToSpawnBoss);
                GameObject spawnBoss = SpawnBosses[indexToSpawnBoss];
                GameObject spawnedBossInstance = Instantiate(spawnBoss);
                spawnedBossInstance.transform.parent = transform;
                mBosses.Add(spawnedBossInstance);
                mBossesSpawned++;
            }

            /* Stop new enemies spawning while repellent powerup is in effect.
             * It is multiplied by 1.01f so that a new enemy doesn't just 
             * instantly spawn after powerup expires. */
            if (mPlayer.HasJustGotRepellentPowerup())
            {
                mNextEnemySpawn += mPlayer.GetRepellentPowerupTime() * 1.01f;
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

            mNextPowSpawn -= Time.deltaTime;

            if (mNextPowSpawn <= 0.0f)
            {
                if (mPows == null)
                {
                    mPows = new List<GameObject>();
                }

                /* If there is already the maximum number of powerups on the 
                 * field, a random old one will be removed, to make way for a 
                 * new one (in a different position). */
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

            if (mPlayer.UsingBomb() && ((mEnemies != null) || (mBosses != null)))
            {
                List<int> enemyIndicesToRemove = new List<int>();
                List<int> bossIndicesToRemove = new List<int>();

                float bombDistance = mPlayer.GetBombRadius();

                if (mEnemies != null)
                {

                    for (int count = 0; count < mEnemies.Count; ++count)
                    {
                        Vector3 difference = mPlayer.GetCenter() - mEnemies[count].transform.position;

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

                }

                if (mBosses != null)
                {

                    for (int count = 0; count < mBosses.Count; ++count)
                    {
                        Vector3 difference = mPlayer.GetCenter() - mBosses[count].transform.position;

                        if (difference.magnitude <= bombDistance)
                        {
                            bossIndicesToRemove.Add(count);
                        }
                    }

                    for (int i = 0; i < bossIndicesToRemove.Count; ++i)
                    {
                        int index = bossIndicesToRemove[i];
                        GameObject boss = mBosses[index - i];
                        mBosses.Remove(boss);
                        FlockWithGroup flockComp = boss.GetComponent<FlockWithGroup>();
                        if (flockComp != null)
                        {
                            flockComp.UpdateBuddyList();
                        }
                        Destroy(boss);
                    }

                }

                mPlayer.UsedBomb();
            }
        }
    }

    private void BeginNewGame(int playerPrefabIndex)
    {
        if (mIsPlayerInstantiated)
        {
            Destroy(mPlayer);
            mPlayer = null;
        }

        mPlayer = Instantiate(PlayerPrefab[playerPrefabIndex]);
        mPlayer.transform.parent = transform;
        mIsPlayerInstantiated = true;

        if (mEnemies != null)
        {
            for (int count = 0; count < mEnemies.Count; ++count)
            {
                Destroy(mEnemies[count]);
            }
            mEnemies.Clear();
        }

        if (mBosses != null)
        {
            for (int count = 0; count < mBosses.Count; ++count)
            {
                Destroy(mBosses[count]);
            }
            mBosses.Clear();
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
        mPlayer.ResetPlayer(true, false);
        mPlayer.enabled = true;
        mState = State.Playing;
        mBossesSpawned = 0;
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

        if (mBosses != null)
        {
            for (int count = 0; count < mBosses.Count; ++count)
            {
                Destroy(mBosses[count]);
            }
            mBosses.Clear();
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

        mPlayer.ResetPlayer(false, true);

        mLeaderboard.EnableSubmission();
        mLeaderboard.enabled = true;
    }

    /*private void ViewInstructions()
    {

    }*/

    private void ViewLeaderboard()
    {
        mLeaderboard.DisableSubmission();
        mLeaderboard.enabled = true;
    }

    /*private void ChangeSettings()
    {

    }*/

    private void SubmitAndMainMenu()
    {
        if (mPlayer != null)
        {
            mLeaderboard.SaveScore(mPlayer.GetScore());
        }

        /* Wait a couple seconds before going back to main menu, so that it 
         * has time to submit the score and the user can see the leaderboard 
         * update briefly. */
        mTimeLeftToSubmit = mTimeToSubmit;
    }

    private void ScreenManager_OnChooseShip1()
    {
        BeginNewGame(0);
    }

    private void ScreenManager_OnChooseShip2()
    {
        BeginNewGame(1);
    }
    private void ScreenManager_OnChooseShip3()
    {
        BeginNewGame(2);
    }

    private void ScreenManager_OnChooseShip4()
    {
        BeginNewGame(3);
    }

    private void ScreenManager_OnExitGame()
    {
        EndGame();
    }

    private void ScreenManager_OnViewLeaderboard()
    {
        ViewLeaderboard();
    }

    private void ScreenManager_OnSubmitAndMainMenu()
    {
        SubmitAndMainMenu();
    }

    internal void DelayEnemies(float delayTime)
    {
        mNextEnemySpawn += delayTime;
    }
}
