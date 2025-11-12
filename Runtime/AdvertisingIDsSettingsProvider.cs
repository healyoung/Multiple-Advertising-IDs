using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using HealYoung;
using UnityEditor;
using UnityEngine;

namespace ShanHai
{
    public static class MultipleAdIds
    {
        private static readonly string GlobalSettingsPath = $"AdvertisingIDsSettings";
        private static AdvertisingIDsSettings _iDs;
        public static readonly LoginDayCounter DayCounter = new LoginDayCounter();

        private static List<string> _todayBannerIds = new List<string>();
        private static List<string> _todayInterstitialIds = new List<string>();

        private static IBindableProperty<int> _intervalBanner;
        private static IBindableProperty<int> _intervalInterstitial;
        public static IBindableProperty<int> BannerTimer;
        public static IBindableProperty<int> InterstitialTimer;
        private static IBindableProperty<int> _bannerGroup;
        private static IBindableProperty<int> _interstitialGroup;
        private static IBindableProperty<int> _bannerCurrentGroup;
        private static IBindableProperty<int> _interstitialCurrentGroup;

        private static float _lastShowTime = 0f;

        public static AdvertisingIDsSettings AdvertisingIDs
        {
            get
            {
#if UNITY_EDITOR
                if (_iDs == null)
                    _iDs = GetSingletonAssetsByResources<AdvertisingIDsSettings>(GlobalSettingsPath);
                return _iDs;
#else
                return _iDs;
#endif
            }
        }

        // 包装类用于 JSON 解析
        [Serializable]
        private class StringListWrapper
        {
            public List<string> list;
        }

        #region 初始化

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void LoadAdvertisingIDsSetting()
        {
            _iDs = GetSingletonAssetsByResources<AdvertisingIDsSettings>(GlobalSettingsPath);
            if (_iDs == null)
            {
                Debug.LogError("AdvertisingIDsSettings not found in Resources!");
                return;
            }

            // 初始化日期系统
            DayCounter.LastLoginTime = new StorageProperty<int>("LoginDayCounter_LastLoginTime", 0);
            DayCounter.LoginDay = new StorageProperty<int>("LoginDayCounter_Day", 0);
            DayCounter.UpdateLastLoginTime();

            // 初始化参数
            _intervalInterstitial = new StorageProperty<int>("Interstitial_IntervalM", 3);
            _intervalBanner = new StorageProperty<int>("Banner_IntervalN", 3);

            _interstitialGroup = new StorageProperty<int>("Interstitial_InterstitialGroup", 2);
            _bannerGroup = new StorageProperty<int>("Banner_BannerGroup", 2);

            InterstitialTimer = new StorageProperty<int>($"Interstitial_{DayCounter.LoginDay.Value}_IntervalMTimer", 0);
            BannerTimer = new StorageProperty<int>($"Banner_{DayCounter.LoginDay.Value}_IntervalNTimer", 0);

            _interstitialCurrentGroup = new StorageProperty<int>($"Interstitial_{DayCounter.LoginDay.Value}_IntervalGroupTimer", 0);
            _bannerCurrentGroup = new StorageProperty<int>($"Banner_{DayCounter.LoginDay.Value}_IntervalGroupTimer", 0);

            ReadyTodayIds(AdvertingType.Banner);
            ReadyTodayIds(AdvertingType.Interstitial);
        }

        #endregion

        #region 准备广告ID

        private static void ReadyTodayIds(AdvertingType advertingType)
        {
            if (_iDs == null) return;

            var allIds = advertingType == AdvertingType.Banner ? _iDs.bannerIds : _iDs.interstitialIds;
            if (allIds == null || allIds.Count == 0)
            {
                Debug.LogWarning($"[{advertingType}] No ad IDs configured in AdvertisingIDsSettings.");
                return;
            }

            int group = advertingType == AdvertingType.Banner ? _bannerGroup.Value : _interstitialGroup.Value;
            group = Mathf.Max(1, group);

            int day = Mathf.Max(1, DayCounter.LoginDay.Value);
            int maxDay = Mathf.CeilToInt((float)allIds.Count / group);
            day = ((day - 1) % maxDay) + 1;

            int start = (day - 1) * group;
            int end = Mathf.Min(day * group, allIds.Count);

            List<string> result = new List<string>();
            for (int i = start; i < end; i++)
                result.Add(allIds[i]);

            if (advertingType == AdvertingType.Banner)
                _todayBannerIds = result;
            else
                _todayInterstitialIds = result;

            Debug.Log($"[{advertingType}] Ready IDs ({result.Count}) for Day {day}");
        }

        #endregion

        #region 核心逻辑

        public static void ShowAdSucceed(AdvertingType type)
        {
            // 防止 SDK 重复回调
            if (Time.realtimeSinceStartup - _lastShowTime < 0.1f) return;
            _lastShowTime = Time.realtimeSinceStartup;

            var timer = type == AdvertingType.Banner ? BannerTimer : InterstitialTimer;
            var interval = type == AdvertingType.Banner ? _intervalBanner : _intervalInterstitial;
            var groupTimer = type == AdvertingType.Banner ? _bannerCurrentGroup : _interstitialCurrentGroup;
            var ids = type == AdvertingType.Banner ? _todayBannerIds : _todayInterstitialIds;

            if (ids == null || ids.Count == 0)
            {
                Debug.LogWarning($"[{type}] No IDs available when showing ad success.");
                return;
            }

            timer.Value++;
            int intervalValue = Mathf.Max(1, interval.Value);

            if (timer.Value >= intervalValue)
            {
                timer.Value = 0;
                groupTimer.Value = (groupTimer.Value + 1) % ids.Count;
                Debug.Log($"[{type}] Switched to next ad ID index {groupTimer.Value}");
            }
        }

        public static string GetAdvertisingID(AdvertingType type)
        {
            var ids = type == AdvertingType.Banner ? _todayBannerIds : _todayInterstitialIds;
            var groupTimer = type == AdvertingType.Banner ? _bannerCurrentGroup : _interstitialCurrentGroup;

            if (ids == null || ids.Count == 0)
            {
                Debug.LogWarning($"[{type}] Request ID failed: no available ad IDs.");
                return string.Empty;
            }

            int index = Mathf.Clamp(groupTimer.Value, 0, ids.Count - 1);
            return ids[index];
        }

        #endregion

        #region 远程配置

        public static void SetRemoteData()
        {
            if (_iDs == null)
            {
                Debug.LogError("SetRemoteData failed: AdvertisingIDsSettings not loaded.");
                return;
            }

            _intervalInterstitial.Value = Mathf.Max(1, (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteInterstitialInterval).LongValue);
            _intervalBanner.Value = Mathf.Max(1, (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteBannerInterval).LongValue);

            _interstitialGroup.Value = Mathf.Max(1, (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteInterstitialGroup).LongValue);
            _bannerGroup.Value = Mathf.Max(1, (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteBannerGroup).LongValue);

            string bannerJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.allBannerIds).StringValue;
            string interstitialJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.allInterstitialIds).StringValue;

            if (!string.IsNullOrEmpty(bannerJson))
            {
                try
                {
                    var bannerWrapper = JsonUtility.FromJson<StringListWrapper>(bannerJson);
                    _iDs.bannerIds = bannerWrapper?.list ?? new List<string>();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Banner] JSON parse error: {e.Message}");
                    _iDs.bannerIds = new List<string>();
                }
            }

            if (!string.IsNullOrEmpty(interstitialJson))
            {
                try
                {
                    var interstitialWrapper = JsonUtility.FromJson<StringListWrapper>(interstitialJson);
                    _iDs.interstitialIds = interstitialWrapper?.list ?? new List<string>();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Interstitial] JSON parse error: {e.Message}");
                    _iDs.interstitialIds = new List<string>();
                }
            }
        }

        #endregion

        #region 工具方法

#if UNITY_EDITOR
        public static void SaveGlobeSetting()
        {
            EditorUtility.SetDirty(_iDs);
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
                Debug.LogError($"Could not find {assetType} asset, expected path: {assetsPath}");
                return null;
            }

            return customGlobalSettings;
        }

        private static void CollectAssets<T>(string assetType)
        {
#if UNITY_EDITOR
            string[] globalAssetPaths = AssetDatabase.FindAssets($"t:{assetType}");
            if (globalAssetPaths.Length > 1)
            {
                foreach (var assetPath in globalAssetPaths)
                    Debug.LogError($"Duplicate {assetType} found: {AssetDatabase.GUIDToAssetPath(assetPath)}");
                throw new Exception($"Multiple {assetType} assets found!");
            }
#endif
        }

        #endregion
    }
}