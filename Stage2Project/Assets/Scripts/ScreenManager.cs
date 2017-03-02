using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public delegate void GameEvent();
    public static event GameEvent OnExitGame;
    public static event GameEvent OnViewLeaderboard;
    public static event GameEvent OnChooseShip1;
    public static event GameEvent OnChooseShip2;
    public static event GameEvent OnChooseShip3;
    public static event GameEvent OnChooseShip4;
    public static event GameEvent OnSubmitAndMainMenu;

    private float mTimeToSubmit = 0.5f;
    private float mTimeLeftToSubmit;

    public enum Screens { TitleScreen, GameScreen, InstructionsScreen, LeaderboardScreen, ShipScreen, NumScreens }

    private Canvas [] mScreens;
    private Screens mCurrentScreen;

    void Awake()
    {
        mScreens = new Canvas[(int)Screens.NumScreens];
        Canvas[] screens = GetComponentsInChildren<Canvas>();
        for (int count = 0; count < screens.Length; ++count)
        {
            for (int slot = 0; slot < mScreens.Length; ++slot)
            {
                if (mScreens[slot] == null && ((Screens)slot).ToString() == screens[count].name)
                {
                    mScreens[slot] = screens[count];
                    break;
                }
            }
        }

        for (int screen = 1; screen < mScreens.Length; ++screen)
        {
            mScreens[screen].enabled = false;
        }

        mCurrentScreen = Screens.TitleScreen;

        mTimeLeftToSubmit = 0.0f;
    }

    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (mTimeLeftToSubmit > 0.0f)
        {
            mTimeLeftToSubmit -= Time.deltaTime;

            if (mTimeLeftToSubmit <= 0.0f)
            {
                TransitionTo(Screens.TitleScreen);
            }
        }
    }

    public void StartGame()
    {
        TransitionTo(Screens.ShipScreen);
    }

    public void EndGame()
    {
        if (OnExitGame != null)
        {
            OnExitGame();
        }

        TransitionTo(Screens.LeaderboardScreen);
    }

    public void ViewInstructions()
    {
        TransitionTo(Screens.InstructionsScreen);
    }

    public void ViewLeaderboard()
    {
        if (OnViewLeaderboard != null)
        {
            OnViewLeaderboard();
        }

        TransitionTo(Screens.LeaderboardScreen);
    }

    public void ChooseShip1()
    {
        if (OnChooseShip1 != null)
        {
            OnChooseShip1();
        }

        TransitionTo(Screens.GameScreen);
    }

    public void ChooseShip2()
    {
        if (OnChooseShip2 != null)
        {
            OnChooseShip2();
        }

        TransitionTo(Screens.GameScreen);
    }

    public void ChooseShip3()
    {
        if (OnChooseShip3 != null)
        {
            OnChooseShip3();
        }

        TransitionTo(Screens.GameScreen);
    }

    public void ChooseShip4()
    {
        if (OnChooseShip4 != null)
        {
            OnChooseShip4();
        }

        TransitionTo(Screens.GameScreen);
    }

    public void MainMenu()
    {
        TransitionTo(Screens.TitleScreen);
    }

    public void SubmitAndMainMenu()
    {
        if (OnSubmitAndMainMenu != null)
        {
            OnSubmitAndMainMenu();
        }

        mTimeLeftToSubmit = mTimeToSubmit;
    }

    /* If paused, then resume the game. Otherwise, pause the game. */
    public void PauseGame()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    private void TransitionTo(Screens screen)
    {
        mScreens[(int)mCurrentScreen].enabled = false;
        mScreens[(int)screen].enabled = true;
        mCurrentScreen = screen;
    }

}
