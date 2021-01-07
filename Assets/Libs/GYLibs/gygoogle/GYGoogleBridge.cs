using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GYLib;

#if ENABLE_GPS
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi.SavedGame;
#endif

public class GYGoogleBridge : MonoSingleton<GYGoogleBridge>
{
    public bool isGooglePlayInited = false;
#if UNITY_ANDROID

    public readonly GYGoogleSaved save = new GYGoogleSaved();

    // Start is called before the first frame update
    void Start()
    {

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            // enables saving game progress.
            .EnableSavedGames()
            // requests a server auth code be generated so it can be passed to an
            //  associated back end server application and exchanged for an OAuth token.
            .RequestServerAuthCode(false)
            // requests an ID token be generated.  This OAuth token can be used to
            //  identify the player to other services such as Firebase.
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
    }


    public void TryMuteLogin()
    {
        // authenticate user:
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.NoPrompt, (result) => {
            // handle results
            if (result == SignInStatus.Success)
            {
                isGooglePlayInited = true;
            }
        });
    }

    public void TryManualLogin()
    {
        // authenticate user:
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.NoPrompt, (result) => {
            // handle results
            if (result == SignInStatus.Success)
            {
                isGooglePlayInited = true;
            }
        });
    }
#endif
}
#endif