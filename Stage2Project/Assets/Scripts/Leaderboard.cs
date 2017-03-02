using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using com.shephertz.app42.paas.sdk.csharp;
using com.shephertz.app42.paas.sdk.csharp.game;

/*Register with App42 platform appHq.shephertz.com/register.
 *Create an app once you are on Quickstart page after registration.
 */

/*Login with AppHQ Management Console from https://apphq.shephertz.com/register/app42Login
 *Go to "Gaming" from left tab, click on Games and select Game.
 *Create game with App42 by clicking on Add Game button in upper right corner in AppHQ Games tab.
 */

public class Leaderboard : MonoBehaviour, App42CallBack
{
    ScoreBoardService mScoreBoardService = null;
    Constants mConstants = new Constants();

    public string mBoxText;
    public string mColumnName = "Rank          " + "Name            " + "Score          ";
    public string mSuccess;
    public string mPlayerRank;
    public string mPlayerName;
    public string mPlayerScore;
    public string user;
    public int mScoresDisplayed = 10;
    public int mScore;
    public bool mLeaderboardAccess;
    public bool mLeaderboardAccessed;
    public bool mSave;
    public bool mSubmit;
    public bool mCanSubmit;

    void Awake()
    {
        ResetLeaderboardLook();
    }

    void Start()
    {
        App42API.Initialize(mConstants.apiKey, mConstants.secretKey);
        App42API.SetOfflineStorage(true, 20);
    }

    internal void SaveScore(int score)
    {
        mScore = score;
        mSave = true;
    }

    internal void ResetLeaderboardLook()
    {
        mBoxText = "";
        mSuccess = "";
        mPlayerRank = "";
        mPlayerName = "";
        mPlayerScore = "";
        user = "";
        mLeaderboardAccess = false;
        mLeaderboardAccessed = false;
        mSave = false;
        mSubmit = false;
        mCanSubmit = true;
    }

    internal void EnableSubmission()
    {
        mCanSubmit = true;
    }

    internal void DisableSubmission()
    {
        mCanSubmit = false;
    }

    void OnGUI()
    {
        float leaderboardWidth = 250;
        float leaderboardHeight = 200;
        float leaderboardX = (Screen.width / 2) - (leaderboardWidth / 2);
        float leaderboardY = (Screen.height / 2) - (leaderboardHeight / 2);

        GUI.Box(new Rect(leaderboardX, leaderboardY, leaderboardWidth, leaderboardHeight), mBoxText);
        GUI.Label(new Rect(leaderboardX + 20, leaderboardY + 10, leaderboardWidth - 50, leaderboardHeight), mColumnName);
        GUI.Label(new Rect(leaderboardX + 20, leaderboardY + 30, leaderboardWidth - 50, leaderboardHeight), mSuccess);
        GUI.Label(new Rect(leaderboardX + 20, leaderboardY + 30, leaderboardWidth - 50, leaderboardHeight), mPlayerRank);
        GUI.Label(new Rect(leaderboardX + 90, leaderboardY + 30, leaderboardWidth - 50, leaderboardHeight), mPlayerName);
        GUI.Label(new Rect(leaderboardX + 170, leaderboardY + 30, leaderboardWidth - 50, leaderboardHeight), mPlayerScore);

        if (mCanSubmit)
        {
            float usernameWidth = 200;
            float usernameHeight = 20;
            float usernameX = ((leaderboardX) / 2) - (usernameWidth / 2);
            float usernameY = leaderboardY;

            GUI.Label(new Rect(usernameX, usernameY, usernameWidth, usernameHeight), "Username");
            user = GUI.TextField(new Rect(usernameX + 80, usernameY, usernameWidth, usernameHeight), user);
        }

        if (mSave)
        {
            mSave = false;

            if (user != null && !user.Equals(""))
            {
                string username = user;
                mScoreBoardService = App42API.BuildScoreBoardService();
                mScoreBoardService.SaveUserScore(mConstants.gameName, username, mScore, this);

                mSubmit = true;
            }
        }

        //This is only accessed once - during OnGUI()'s first call.
        if (!mLeaderboardAccessed)
        {
            mScoreBoardService = App42API.BuildScoreBoardService();
            mScoreBoardService.GetTopNRankers(mConstants.gameName, mScoresDisplayed, this);
            mLeaderboardAccess = true;
        }
    }

    public void OnSuccess(object response)
    {
        var nxtLine = System.Environment.NewLine;

        if (response is App42OfflineResponse)
        {
            mPlayerRank = "";
            mPlayerName = "";
            mPlayerScore = "";
            mBoxText = "";
            mSuccess = "Network UnAvailable : " + nxtLine +
                "----------------------------------------" + nxtLine +
                    "Information is Stored in cache," + nxtLine +
                    "Will send to App42 when network is available.";
        }
        else
        {
            Game gameResponseObj = (Game)response;

            if (mSubmit)
            {
                mSubmit = false;
            }

            //Print the top rankers that we got in OnGUI()
            if (mLeaderboardAccess)
            {
                mLeaderboardAccess = false;

                IList<Game.Score> topRankersList = gameResponseObj.GetScoreList();

                if (topRankersList.Count > 0)
                {
                    for (int i = 0; i < gameResponseObj.GetScoreList().Count; i++)
                    {
                        string scorerName = gameResponseObj.GetScoreList()[i].GetUserName();
                        double scorerValue = gameResponseObj.GetScoreList()[i].GetValue();

                        mPlayerRank = mPlayerRank + (i + 1).ToString() + nxtLine;
                        mPlayerName = mPlayerName + scorerName + nxtLine;
                        mPlayerScore = mPlayerScore + scorerValue.ToString() + nxtLine;
                    }
                }

                mLeaderboardAccessed = true;
            }
        }
    }

    public void OnException(Exception e)
    {
        var nxtLine = System.Environment.NewLine;

        App42Exception exception = (App42Exception)e;
        int appErrorCode = exception.GetAppErrorCode();
        if (appErrorCode == 3002)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "Game With The Name (" + mConstants.gameName + ")" + nxtLine +
                    " Does Not Exist.";
        }
        else if (appErrorCode == 3013)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "Scores For The Game," + nxtLine +
                    "With The Name (" + mConstants.gameName + ")" + nxtLine +
                    " Does Not Exist.";
        }
        else if (appErrorCode == 1401)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "Client Is Not authorized" + nxtLine +
                    "Please Verify Your" + nxtLine +
                    "API_KEY & SECRET_KEY" + nxtLine +
                    "From AppHq.";
        }
        else if (appErrorCode == 1500)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "WE ARE SORRY !!" + nxtLine +
                    "But Somthing Went Wrong.";
        }
        else {
            mBoxText = "Exception Occurred :" + exception;
        }
        Debug.Log("Message : " + e);
    }
}