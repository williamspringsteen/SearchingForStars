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
        //ResetLeaderboardLook();
        //App42Log.SetDebug(true);
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
        //Will likely have to build all leaderboard boxes and stuff first, before GetTopNRankers
        //TODO: Change these numbers to be screen independent
        GUI.Box(new Rect(450, 40, 250, 200), mBoxText);
        GUI.Label(new Rect(470, 50, 200, 200), mColumnName);
        GUI.Label(new Rect(470, 70, 200, 200), mSuccess);
        GUI.Label(new Rect(470, 70, 200, 200), mPlayerRank);
        GUI.Label(new Rect(540, 70, 200, 200), mPlayerName);
        GUI.Label(new Rect(620, 70, 200, 200), mPlayerScore);

        if (mCanSubmit)
        {
            GUI.Label(new Rect(20, 40, 200, 20), "Username");
            user = GUI.TextField(new Rect(100, 40, 200, 20), user);
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
            // handle here , If Game Name Does Not Exist.
        }
        else if (appErrorCode == 3013)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "Scores For The Game," + nxtLine +
                    "With The Name (" + mConstants.gameName + ")" + nxtLine +
                    " Does Not Exist.";
            // handle here , if no scores found for the given gameName.
        }
        else if (appErrorCode == 1401)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "Client Is Not authorized" + nxtLine +
                    "Please Verify Your" + nxtLine +
                    "API_KEY & SECRET_KEY" + nxtLine +
                    "From AppHq.";
            // handle here for Client is not authorized
        }
        else if (appErrorCode == 1500)
        {
            mBoxText = "Exception Occurred :" + nxtLine +
                "WE ARE SORRY !!" + nxtLine +
                    "But Somthing Went Wrong.";
            // handle here for Internal Server Error
        }
        else {
            mBoxText = "Exception Occurred :" + exception;
        }
        Debug.Log("Message : " + e);
    }
}