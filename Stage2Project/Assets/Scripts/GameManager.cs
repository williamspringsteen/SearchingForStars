﻿using System.Collections;
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

    [SerializeField]
    private Leaderboard Leaderboard;
    
    private List<GameObject> mEnemies;
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

    void Awake()
    {
        mPlayer = Instantiate(PlayerPrefab);
        mPlayer.transform.parent = transform;

        mLeaderboard = Instantiate(Leaderboard);
        mLeaderboard.enabled = false;
        mTimeLeftToSubmit = 0.0f;

        ScreenManager.OnNewGame += ScreenManager_OnNewGame;
        ScreenManager.OnExitGame += ScreenManager_OnExitGame;
        ScreenManager.OnViewLeaderboard += ScreenManager_OnViewLeaderboard;
        //ScreenManager.OnViewInstructions += ScreenManager_OnViewInstructions;
        //SceenManager.OnChangeSettings += ScreenManager_OnChangeSettings;
        ScreenManager.OnMainMenu += ScreenManager_OnMainMenu;
        ScreenManager.OnSubmitAndMainMenu += ScreenManager_OnSubmitAndMainMenu;
    }

    void Start()
    {
        Arena.Calculate();
        mPlayer.enabled = false;
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

            if (mPlayer.UsingBomb() && mEnemies != null)
            {
                List<int> enemyIndicesToRemove = new List<int>();

                float bombDistance = mPlayer.GetBombRadius();

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
        mPlayer.ResetPlayer(true, false);
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

        mPlayer.ResetPlayer(false, true);

        mLeaderboard.EnableSubmission();
        mLeaderboard.enabled = true;
    }

    /*private void ViewInstructions()
    {

    }*/

    private void ViewLeaderboard()
    {
        //EndGame();
        mLeaderboard.DisableSubmission();
        mLeaderboard.enabled = true;
    }

    /*private void ChangeSettings()
    {

    }*/

    private void SubmitAndMainMenu()
    {
        //TODO: CHANGE ALL MAIN MENU STUFF TO SUBMIT
        mLeaderboard.SaveScore(mPlayer.GetScore());

        /* Wait a couple seconds before going back to main menu, so that it 
         * has time to submit the score and the user can see the leaderboard 
         * update briefly. */
        mTimeLeftToSubmit = mTimeToSubmit;

        //Suppress the OnGUI stuff, but don't turn .enabled off yet
    }

    private void MainMenu()
    {
        //TODO: CHANGE ALL MAIN MENU STUFF TO SUBMIT
        //mLeaderboard.SaveScore(mPlayer.GetScore());

        /* Wait a couple seconds before going back to main menu, so that it 
         * has time to submit the score and the user can see the leaderboard 
         * update briefly. */
        //mTimeLeftToSubmit = mTimeToSubmit;

        //Suppress the OnGUI stuff, but don't turn .enabled off yet
    }

    private void ScreenManager_OnNewGame()
    {
        BeginNewGame();
    }

    private void ScreenManager_OnExitGame()
    {
        EndGame();
    }

    /*private void ScreenManager_OnViewInstructions()
    {
        ViewInstructions();
    }*/

    private void ScreenManager_OnViewLeaderboard()
    {
        ViewLeaderboard();
    }

    /*private void ScreenManager_OnChangeSettings()
    {
        ChangeSettings();
    }*/

    private void ScreenManager_OnMainMenu()
    {
        MainMenu();
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
