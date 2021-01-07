using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

#if ENABLE_GPS

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi.SavedGame;
#endif

public class GYGoogleSaved
{
    SaveGoogleGameProxy _saveProxy = new SaveGoogleGameProxy();
    public Texture2D savedImage;
    public byte[] lastSaveDatas;

    public void LoopCheckSaved()
    {
#if UNITY_ANDROID
        if (_saveProxy.isDataDirty && _saveProxy.bytesSave != null &&
            currentSave != null)
        {
            //TO DO 从PlayerData获取在线时长
            SaveImmediately();
        }
#endif
    }

    /// <summary>
    /// 立刻存档
    /// </summary>
    public void SaveImmediately()
    {
        //TO DO 从PlayerData获取在线时长
        TimeSpan ts = new TimeSpan();
        SaveGame(currentSave, _saveProxy.bytesSave, ts);
    }

#if UNITY_ANDROID
    public ISavedGameMetadata currentSave { private set; get; }
    /// <summary>
    /// 打开存档
    /// </summary>
    /// <param name="filename"></param>
    public void OpenSavedGame(string filename)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        savedGameClient.OpenWithManualConflictResolution(filename, DataSource.ReadCacheOrNetwork, true, OnConflictExec, OnSavedGameOpened);
    }

    /// <summary>
    /// 解决云端冲突
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="original"></param>
    /// <param name="originalData"></param>
    /// <param name="unmerged"></param>
    /// <param name="unmergedData"></param>
    private void OnConflictExec(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        var choosen = original.LastModifiedTimestamp >= unmerged.LastModifiedTimestamp ? original : unmerged;
        resolver.ChooseMetadata(choosen);

        currentSave = choosen;
    }

    /// <summary>
    /// 打开存档
    /// </summary>
    /// <param name="status"></param>
    /// <param name="game"></param>
    public void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.
            currentSave = game;
        }
        else
        {
            // handle error
        }
    }

    /// <summary>
    /// 立刻存档
    /// </summary>
    /// <param name="game"></param>
    /// <param name="savedData"></param>
    /// <param name="totalPlaytime"></param>
    private void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        _saveProxy.bytesSave = savedData;

        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder
            .WithUpdatedPlayedTime(totalPlaytime)
            .WithUpdatedDescription("Saved game at " + DateTime.Now);
        if (savedImage != null)
        {
            // This assumes that savedImage is an instance of Texture2D
            // and that you have already called a function equivalent to
            // getScreenshot() to set savedImage
            // NOTE: see sample definition of getScreenshot() method below
            byte[] pngData = savedImage.EncodeToPNG();
            builder = builder.WithUpdatedPngCoverImage(pngData);
        }
        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }

    /// <summary>
    /// 存档失败时调用
    /// </summary>
    public UnityAction<string, byte[]> OnSaveFailed;

    /// <summary>
    /// 存档完成
    /// </summary>
    /// <param name="status"></param>
    /// <param name="game"></param>
    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.
        }
        else
        {
            // handle error
            if (OnSaveFailed != null)
            {
                OnSaveFailed(status.ToString(), lastSaveDatas);
            }
        }
    }

    public Texture2D GetScreenshot()
    {
        // Create a 2D texture that is 1024x700 pixels from which the PNG will be
        // extracted
        Texture2D screenShot = new Texture2D(1024, 700);

        // Takes the screenshot from top left hand corner of screen and maps to top
        // left hand corner of screenShot texture
        screenShot.ReadPixels(
            new Rect(0, 0, Screen.width, (Screen.width / 1024) * 700), 0, 0);
        return screenShot;
    }

    /// <summary>
    /// 读取存档
    /// </summary>
    /// <param name="game"></param>
    public void LoadGameData(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    /// <summary>
    /// 加载存档成功
    /// </summary>
    public UnityAction<byte[]> OnLoadSaveCompleted;

    /// <summary>
    /// 加载存档失败时调用
    /// </summary>
    public UnityAction<string> OnLoadSaveFailed;
    private void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle processing the byte array data
            if (OnLoadSaveCompleted != null)
            {
                OnLoadSaveCompleted(data);
            }
        }
        else
        {
            // handle error
            if (OnLoadSaveFailed != null)
            {
                OnLoadSaveFailed(status.ToString());
            }
        }
    }
#endif
}
#endif