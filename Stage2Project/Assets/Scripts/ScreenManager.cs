using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public delegate void GameEvent();
    public static event GameEvent OnNewGame;
    public static event GameEvent OnExitGame;
    //public static event GameEvent OnViewInstructions;
    public static event GameEvent OnViewLeaderboard;
    //public static event GameEvent OnChangeSettings;
    public static event GameEvent OnMainMenu;

    public enum Screens { TitleScreen, GameScreen, InstructionsScreen, LeaderboardScreen, SettingsScreen, NumScreens }

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
    }

    public void StartGame()
    {
        if(OnNewGame != null)
        {
            OnNewGame();
        }

        TransitionTo(Screens.GameScreen);
    }

    public void EndGame()
    {
        if (OnExitGame != null)
        {
            OnExitGame();
        }

        TransitionTo(Screens.TitleScreen);
    }

    public void ViewInstructions()
    {
        /*if (OnViewInstructions != null)
        {
            OnViewInstructions();
        }*/

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

    public void ChangeSettings()
    {
        /*if (OnChangeSettings != null)
        {
            OnChangeSettings();
        }*/

        TransitionTo(Screens.SettingsScreen);
    }

    public void MainMenu()
    {
        if (OnMainMenu != null)
        {
            OnMainMenu();
        }

        TransitionTo(Screens.TitleScreen);
    }

    private void TransitionTo(Screens screen)
    {
        mScreens[(int)mCurrentScreen].enabled = false;
        mScreens[(int)screen].enabled = true;
        mCurrentScreen = screen;
    }
}
