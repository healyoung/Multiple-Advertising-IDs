using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using HealYoung;
using UnityEngine;

namespace ShanHai
{
    public static class MultipleAdIds
    {
        public const string GlobalSettingsPath = "AdvertisingSettings";

        /// <summary>
        /// 当前编译目标应使用的配置文件，由平台宏在编译期决定（配置文件本身不含平台宏）。
        /// @trace IOS-002
        /// </summary>
#if UNITY_IOS
        public const string ActiveIdSource = "IOSAdvertisingIDsConfig.asset (iOS)";
#elif UNITY_ANDROID
        public const string ActiveIdSource = "AndroidAdvertisingIDsConfig.asset (Android)";
#else
        public const string ActiveIdSource = "AndroidAdvertisingIDsConfig.asset (Default)";
#endif

        /// <summary>运行时实际生效的配置资产名；未创建平台配置时显示回退来源。</summary>
        public static string ActiveConfigName { get; private set; } = "not loaded";

        private static AdvertisingIDsSettings _iDs;
        public static readonly LoginDayCounter DayCounter = new LoginDayCounter();

        private static List<string> _todayBannerIds = new List<string>();
        private static List<string> _todayInterstitialIds = new List<string>();

        // 平台相关的全量 ID 缓存：来自当前平台对应的配置资产，未创建时回退 AdvertisingSettings.asset
        private static List<string> _allBannerIds;
        private static List<string> _allInterstitialIds;

        private static IBindableProperty<int> _intervalBanner;
        private static IBindableProperty<int> _intervalInterstitial;
        private static IBindableProperty<int> _bannerCurrentGroup;
        private static IBindableProperty<int> _interstitialCurrentGroup;
        public static IBindableProperty<int> BannerTimer;
        public static IBindableProperty<int> InterstitialTimer;
        private static int _bannerGroup;
        private static int _interstitialGroup;

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

        public static void OnNewDay()
        {
            InterstitialTimer = new StorageProperty<int>($"Interstitial_{DayCounter.LoginDay.Value}_IntervalMTimer_V1.0.0", 0);
            BannerTimer = new StorageProperty<int>($"Banner_{DayCounter.LoginDay.Value}_IntervalNTimer_V1.0.0", 0);
            _interstitialCurrentGroup = new StorageProperty<int>($"Interstitial_{DayCounter.LoginDay.Value}_IntervalGroupTimer_V1.0.0", 0);
            _bannerCurrentGroup = new StorageProperty<int>($"Banner_{DayCounter.LoginDay.Value}_IntervalGroupTimer_V1.0.0", 0);
            ReadyTodayIds(AdvertingType.Banner);
            ReadyTodayIds(AdvertingType.Interstitial);
        }

        /// <summary>
        /// 准备今天的 ID
        /// </summary>
        public static void ReadyTodayIds(AdvertingType type)
        {
            if (_allBannerIds == null || _allInterstitialIds == null)
                LoadPlatformIds();

            var allIds = type == AdvertingType.Banner ? _allBannerIds : _allInterstitialIds;
            int group = type == AdvertingType.Banner ? _bannerGroup : _interstitialGroup;
            group = Mathf.Max(1, group);

            if (allIds == null || allIds.Count == 0)
            {
                if (type == AdvertingType.Banner) _todayBannerIds = new List<string>();
                else _todayInterstitialIds = new List<string>();
                return;
            }

            int maxDay = Mathf.Max(1, Mathf.CeilToInt(allIds.Count / (float)group));
            int day = Mathf.Clamp(DayCounter.LoginDay.Value, 1, maxDay);

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
            if (_iDs == null)
            {
                Debug.LogError("MultipleAdIds: AdvertisingIDsSettings asset is missing, skip initialization.");
                return;
            }

            // 平台宏切换：iOS 读 iOS 配置资产，Android/其它读 Android 配置资产
            LoadPlatformIds();

            DayCounter.LastLoginTime = new StorageProperty<int>("LoginDayCounter_LastLoginTime_V1.0.0", 0);
            DayCounter.LoginDay = new StorageProperty<int>("LoginDayCounter_Day_V1.0.0", 0);
            DayCounter.UpdateLastLoginTime();

            _intervalInterstitial = new StorageProperty<int>("Interstitial_IntervalM_V1.0.0", _iDs.interstitialInterval);
            _intervalBanner = new StorageProperty<int>("Banner_IntervalN_V1.0.0", _iDs.bannerInterval);

            _interstitialGroup =  _iDs.interstitialGroup;
            _bannerGroup = _iDs.bannerGroup;
            OnNewDay();
        }

        public static void SetRemoteData()
        {
            try
            {
                if (_iDs == null)
                {
                    Debug.LogWarning("MultipleAdIds: settings asset is missing, skip remote data.");
                    return;
                }

                if (_allBannerIds == null || _allInterstitialIds == null)
                    LoadPlatformIds();

                if (_iDs.enableInterstitial)
                {
                    var interIntervalVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteInterstitialInterval).LongValue;
                    var interGroupVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteInterstitialGroup).LongValue;

                    if (interIntervalVal > 0)
                        _intervalInterstitial.Value = (int)interIntervalVal;
                    else
                        _intervalInterstitial.Value = Mathf.Max(1, _iDs.interstitialInterval);

                    if (interGroupVal > 0)
                        _interstitialGroup = (int)interGroupVal;
                    else
                        _interstitialGroup = Mathf.Max(1, _iDs.interstitialGroup);

                    var interstitialJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.allInterstitialIds).StringValue;
                    Debug.Log(interstitialJson);
                    if (!string.IsNullOrEmpty(interstitialJson))
                    {
                        try
                        {
                            var parsed = JsonUtility.FromJson<StringWper>(interstitialJson);
                            if (parsed != null && parsed.list != null && parsed.list.Count > 0)
                                _allInterstitialIds = new List<string>(parsed.list);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                if (_iDs.enableBanner)
                {
                    var bannerIntervalVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteBannerInterval).LongValue;
                    var bannerGroupVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.remoteBannerGroup).LongValue;

                    if (bannerIntervalVal > 0)
                        _intervalBanner.Value = (int)bannerIntervalVal;
                    else
                        _intervalBanner.Value = Mathf.Max(1, _iDs.bannerInterval);

                    if (bannerGroupVal > 0)
                        _bannerGroup = (int)bannerGroupVal;
                    else
                        _bannerGroup = Mathf.Max(1, _iDs.bannerGroup);

                    var bannerJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_iDs.allBannerIds).StringValue;
                    Debug.Log(bannerJson);
                    if (!string.IsNullOrEmpty(bannerJson))
                    {
                        try
                        {
                            var parsed = JsonUtility.FromJson<StringWper>(bannerJson);
                            if (parsed != null && parsed.list != null && parsed.list.Count > 0)
                                _allBannerIds = new List<string>(parsed.list);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                ReadyTodayIds(AdvertingType.Banner);
                ReadyTodayIds(AdvertingType.Interstitial);
                if (_bannerCurrentGroup != null && _todayBannerIds != null && _todayBannerIds.Count > 0)
                {
                    if (_bannerCurrentGroup.Value >= _todayBannerIds.Count)
                        _bannerCurrentGroup.Value = _todayBannerIds.Count - 1;
                }
                if (_interstitialCurrentGroup != null && _todayInterstitialIds != null && _todayInterstitialIds.Count > 0)
                {
                    if (_interstitialCurrentGroup.Value >= _todayInterstitialIds.Count)
                        _interstitialCurrentGroup.Value = _todayInterstitialIds.Count - 1;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
                throw;
            }
        }

        #region Helper

        /// <summary>
        /// 按平台宏读取对应的配置资产（配置文件本身不区分平台）。
        /// iOS：IOSAdvertisingIDsConfig.asset；Android / 其它：AndroidAdvertisingIDsConfig.asset。
        /// 未创建平台配置资产时回退到 AdvertisingSettings.asset，保持 1.0.1 及以前的工程可用。
        /// 云控只覆盖运行时缓存，不改写任何 ScriptableObject 资产。
        /// @trace IOS-003
        /// </summary>
        private static void LoadPlatformIds()
        {
#if UNITY_IOS
            var config = FindPlatformConfig<IOSAdvertisingIDsConfig>();
#else
            var config = FindPlatformConfig<AndroidAdvertisingIDsConfig>();
#endif
            if (config != null)
            {
                ActiveConfigName = config.name;
                _allBannerIds = CopyIds(config.bannerIds);
                _allInterstitialIds = CopyIds(config.interstitialIds);
                return;
            }

            ActiveConfigName = GlobalSettingsPath + ".asset (fallback)";
            _allBannerIds = _iDs == null ? new List<string>() : CopyIds(_iDs.bannerIds);
            _allInterstitialIds = _iDs == null ? new List<string>() : CopyIds(_iDs.interstitialIds);
        }

        /// <summary>
        /// 在宿主工程所有 Resources 目录下查找指定平台的配置资产，不依赖固定文件名。
        /// @trace IOS-004
        /// </summary>
        private static T FindPlatformConfig<T>() where T : AdvertisingIDsConfigBase
        {
            var all = Resources.LoadAll<T>("");
            if (all.Length > 1)
                Debug.LogWarning($"MultipleAdIds: multiple {typeof(T).Name} assets found under Resources, using the first one.");
            return all.Length > 0 ? all[0] : null;
        }

        private static List<string> CopyIds(IReadOnlyList<string> source)
        {
            return source == null ? new List<string>() : new List<string>(source);
        }

        private static T GetSingletonAssetsByResources<T>(string assetsPath) where T : ScriptableObject, new()
        {
            string assetType = typeof(T).Name;
            CollectAssets(assetType);

            T customGlobalSettings = Resources.Load<T>(assetsPath);
            if (customGlobalSettings == null)
            {
                Debug.LogError($"Could not found {assetType} asset at {assetsPath}");
                return null;
            }

            return customGlobalSettings;
        }

        private static void CollectAssets(string assetType)
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
