using System;
using System.Collections.Generic;
using HealYoung;
using UnityEditor;
using WhiteEngine;
using UnityEngine;

public static class SettingsUtils
{
    private static readonly string GlobalSettingsPath = $"AdvertisingIDsSettings";
    private static AdvertisingIDsSettings _globalSettings;

    public static AdvertisingIDsSettings AdvertisingIDs
    {
        get
        {
#if UNITY_EDITOR
            if (_globalSettings == null)
            {
                _globalSettings = GetSingletonAssetsByResources<AdvertisingIDsSettings>(GlobalSettingsPath);
            }

            return _globalSettings;
#else
            return _globalSettings;
#endif
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void LoadAdvertisingIDsSetting()
    {
        _globalSettings = GetSingletonAssetsByResources<AdvertisingIDsSettings>(GlobalSettingsPath);
        _globalSettings.DayCounter.LastLoginTime = new StorageProperty<int>("LoginDayCounter_LastLoginTime", 0);
        _globalSettings.DayCounter.LoginDay = new StorageProperty<int>("LoginDayCounter_Day", 0);
        _globalSettings.DayCounter.UpdateLastLoginTime();

        _globalSettings.IntervalM = new StorageProperty<int>("Interstitial_IntervalM", 3);
        _globalSettings.IntervalN = new StorageProperty<int>("Banner_IntervalN", 3);

        _globalSettings.IntervalMTimer = new StorageProperty<int>($"Interstitial_{_globalSettings.DayCounter.LoginDay.Value}_IntervalMTimer", -1);
        _globalSettings.IntervalNTimer = new StorageProperty<int>($"Banner_{_globalSettings.DayCounter.LoginDay.Value}_IntervalNTimer", -1);
    }

    public static void SetRemoteData()
    {
        _globalSettings.IntervalM.Value = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_globalSettings.remoteInterstitialInterval).LongValue;
        _globalSettings.IntervalN.Value = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_globalSettings.remoteBannerInterval).LongValue;
        var bannerJson = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_globalSettings.allBannerIds).StringValue;
        var interstitialJson = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(_globalSettings.allInterstitialIds).StringValue;
        if (!string.IsNullOrEmpty(bannerJson))
        {
            _globalSettings.bannerIds = JsonUtility.FromJson<List<string>>(bannerJson);
        }

        if (!string.IsNullOrEmpty(interstitialJson))
        {
            _globalSettings.interstitialIds = JsonUtility.FromJson<List<string>>(interstitialJson);
        }
    }

#if UNITY_EDITOR
    public static void SaveGlobeSetting()
    {
        EditorUtility.SetDirty(_globalSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif


    private static T GetSingletonAssetsByResources<T>(string assetsPath) where T : ScriptableObject, new()
    {
        string assetType = typeof(T).Name;
        CollectAssets<T>(assetType);
        T customGlobalSettings = Resources.Load<T>(assetsPath);
        if (customGlobalSettings == null)
        {
            Debug.LogError($"Could not found {assetType} asset，so auto create:{assetsPath}.");
            return null;
        }

        return customGlobalSettings;
    }


    private static void CollectAssets<T>(string assetType)
    {
#if UNITY_EDITOR
        string[] globalAssetPaths = UnityEditor.AssetDatabase.FindAssets($"t:{assetType}");
        if (globalAssetPaths.Length > 1)
        {
            foreach (var assetPath in globalAssetPaths)
            {
                Debug.LogError($"Could not had Multiple {assetType}. Repeated Path: {UnityEditor.AssetDatabase.GUIDToAssetPath(assetPath)}");
            }

            throw new Exception($"Could not had Multiple {assetType}");
        }
#endif
    }

    /// <summary>
    /// 平台名字
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "IOS";
#else
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
                return "IOS";
            case RuntimePlatform.Android:
                return "Android";
            default:
                throw new System.NotSupportedException($"Platform '{Application.platform.ToString()}' is not supported.");
        }
#endif
    }
}