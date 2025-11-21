using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using HealYoung;
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
        private static IBindableProperty<int> _bannerGroup;
        private static IBindableProperty<int> _interstitialGroup;
        private static IBindableProperty<int> _bannerCurrentGroup;
        private static IBindableProperty<int> _interstitialCurrentGroup;
        public static IBindableProperty<int> BannerTimer;
        public static IBindableProperty<int> InterstitialTimer;

        public static AdvertisingIDsSettings AdvertisingIDs
        {
            get
            {
#if UNITY_EDITOR
                if (_iDs == null)
                {
                    _iDs = GetSingletonAssetsByResources<AdvertisingIDsSettings>(GlobalSettingsPath);
                }
                return _iDs;
#else
                return _iDs;
#endif
            }
        }

        /// <summary>
        /// 准备今天的 ID
        /// </summary>
        private static void ReadyTodayIds(AdvertingType type)
        {
            if (_iDs == null) return;

            var allIds = type == AdvertingType.Banner ? _iDs.bannerIds : _iDs.interstitialIds;
            if (allIds == null || allIds.Count == 0) return;

            int group = type == AdvertingType.Banner ? _bannerGroup.Value : _interstitialGroup.Value;
            group = Mathf.Max(1, group);

            int maxDay = Mathf.Max(1, allIds.Count / group);
            int day = Mathf.Clamp(DayCounter.LoginDay.Value, 1, maxDay); // 超出天数使用最后一天

            int start = (day - 1) * group;
            int end = Mathf.Min(day * group, allIds.Count);

            List<string> result = new List<string>();
            for (int i = start; i < end; i++)
            {
                result.Add(allIds[i]);
            }

            if (type == AdvertingType.Banner)
                _todayBannerIds = result;
            else
                _todayInterstitialIds = result;
        }

        /// <summary>
        /// 获取广告 ID
        /// </summary>
        public static string GetAdvertisingID(AdvertingType type)
        {
            var ids = type == AdvertingType.Banner ? _todayBannerIds : _todayInterstitialIds;
            var currentGroup = type == AdvertingType.Banner ? _bannerCurrentGroup : _interstitialCurrentGroup;

            if (ids == null || ids.Count == 0) return string.Empty;

            int index = Mathf.Clamp(currentGroup.Value, 0, ids.Count - 1);
            return ids[index];
        }

        /// <summary>
        /// 广告展示成功
        /// </summary>
        public static void ShowAdSucceed(AdvertingType type)
        {
            var timer = type == AdvertingType.Banner ? BannerTimer : InterstitialTimer;
            var interval = type == AdvertingType.Banner ? _intervalBanner : _intervalInterstitial;
            var currentGroup = type == AdvertingType.Banner ? _bannerCurrentGroup : _interstitialCurrentGroup;
            var ids = type == AdvertingType.Banner ? _todayBannerIds : _todayInterstitialIds;

            timer.Value++;

            if (timer.Value >= interval.Value)
            {
                timer.Value = 0;

                if (ids != null && ids.Count > 0)
                {
                    currentGroup.Value++;
                    // 超过组数时固定使用最后一个
                    if (currentGroup.Value >= ids.Count)
                        currentGroup.Value = ids.Count - 1;
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void LoadAdvertisingIDsSetting()
        {
            _iDs = GetSingletonAssetsByResources<AdvertisingIDsSettings>(GlobalSettingsPath);

            DayCounter.LastLoginTime = new StorageProperty<int>("LoginDayCounter_LastLoginTime", 0);
            DayCounter.LoginDay = new StorageProperty<int>("LoginDayCounter_Day", 0);
            DayCounter.UpdateLastLoginTime();

            _intervalInterstitial = new StorageProperty<int>("Interstitial_IntervalM", _iDs.interstitialInterval);
            _intervalBanner = new StorageProperty<int>("Banner_IntervalN", _iDs.bannerInterval);

            _interstitialGroup = new StorageProperty<int>("Interstitial_InterstitialGroup", _iDs.interstitialGroup);
            _bannerGroup = new StorageProperty<int>("Banner_BannerGroup", _iDs.bannerGroup);

            InterstitialTimer = new StorageProperty<int>($"Interstitial_{DayCounter.LoginDay.Value}_IntervalMTimer", 0);
            BannerTimer = new StorageProperty<int>($"Banner_{DayCounter.LoginDay.Value}_IntervalNTimer", 0);

            _interstitialCurrentGroup = new StorageProperty<int>($"Interstitial_{DayCounter.LoginDay.Value}_IntervalGroupTimer", 0);
            _bannerCurrentGroup = new StorageProperty<int>($"Banner_{DayCounter.LoginDay.Value}_IntervalGroupTimer", 0);

            ReadyTodayIds(AdvertingType.Banner);
            ReadyTodayIds(AdvertingType.Interstitial);
        }

        public static void SetRemoteData()
        {
            _intervalInterstitial.Value = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteInterstitialInterval).LongValue;
            _intervalBanner.Value = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteBannerInterval).LongValue;

            _interstitialGroup.Value = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteInterstitialGroup).LongValue;
            _bannerGroup.Value = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteBannerGroup).LongValue;

            var bannerJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.allBannerIds).StringValue;
            var interstitialJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.allInterstitialIds).StringValue;

            if (!string.IsNullOrEmpty(bannerJson))
                _iDs.bannerIds = JsonUtility.FromJson<StringWper>(bannerJson).list;

            if (!string.IsNullOrEmpty(interstitialJson))
                _iDs.interstitialIds = JsonUtility.FromJson<StringWper>(interstitialJson).list;
        }

        #region Helper

        private static T GetSingletonAssetsByResources<T>(string assetsPath) where T : ScriptableObject, new()
        {
            string assetType = typeof(T).Name;
            CollectAssets<T>(assetType);

            T customGlobalSettings = Resources.Load<T>(assetsPath);
            if (customGlobalSettings == null)
            {
                Debug.LogError($"Could not found {assetType} asset at {assetsPath}");
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
                    Debug.LogError($"Multiple {assetType} assets found: {UnityEditor.AssetDatabase.GUIDToAssetPath(assetPath)}");
                }

                throw new Exception($"Multiple {assetType} assets found");
            }
#endif
        }

        #endregion
    }
}
