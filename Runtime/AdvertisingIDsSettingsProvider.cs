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
        private static ActiveAdConfig _config;
        public static readonly LoginDayCounter DayCounter = new LoginDayCounter();

        private static List<string> _todayBannerIds = new List<string>();
        private static List<string> _todayInterstitialIds = new List<string>();

        // 全量 ID 缓存：来自当前平台生效的配置，云控只覆盖缓存、不改写任何资产
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
            _iDs = LoadLegacySettings();
            ResolveConfig();
            if (_config == null)
            {
                Debug.LogError("MultipleAdIds: no advertising config found. Create a platform config (Create > Multiple) or Assets/Resources/AdvertisingSettings.asset.");
                return;
            }

            DayCounter.LastLoginTime = new StorageProperty<int>("LoginDayCounter_LastLoginTime_V1.0.0", 0);
            DayCounter.LoginDay = new StorageProperty<int>("LoginDayCounter_Day_V1.0.0", 0);
            DayCounter.UpdateLastLoginTime();

            _intervalInterstitial = new StorageProperty<int>("Interstitial_IntervalM_V1.0.0", _config.InterstitialInterval);
            _intervalBanner = new StorageProperty<int>("Banner_IntervalN_V1.0.0", _config.BannerInterval);

            _interstitialGroup = _config.InterstitialGroup;
            _bannerGroup = _config.BannerGroup;
            OnNewDay();
        }

        /// <summary>
        /// 读取 Firebase Remote Config 已激活的值并按「当前平台生效的配置」中的 Key 应用。
        /// 云控 Key、开关、间隔、分组都取自对应平台的配置资产，不再共用 AdvertisingSettings.asset。
        /// @trace IOS-010
        /// </summary>
        public static void SetRemoteData()
        {
            try
            {
                // 允许业务层在 Firebase 就绪后较早调用：确保初始化与平台配置都已解析
                if (DayCounter.LoginDay == null || _intervalBanner == null || _intervalInterstitial == null || _config == null)
                {
                    Debug.LogWarning("MultipleAdIds: SetRemoteData called before initialization, running initialization first.");
                    LoadAdvertisingIDsSetting();
                }

                if (_config == null)
                {
                    Debug.LogWarning("MultipleAdIds: no advertising config found, skip remote data.");
                    return;
                }

                if (_allBannerIds == null || _allInterstitialIds == null)
                    LoadPlatformIds();

                if (_config.EnableInterstitial)
                {
                    var interIntervalVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_config.RemoteInterstitialInterval).LongValue;
                    var interGroupVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_config.RemoteInterstitialGroup).LongValue;

                    if (interIntervalVal > 0)
                        _intervalInterstitial.Value = (int)interIntervalVal;
                    else
                        _intervalInterstitial.Value = Mathf.Max(1, _config.InterstitialInterval);

                    if (interGroupVal > 0)
                        _interstitialGroup = (int)interGroupVal;
                    else
                        _interstitialGroup = Mathf.Max(1, _config.InterstitialGroup);

                    var interstitialJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_config.AllInterstitialIds).StringValue;
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

                if (_config.EnableBanner)
                {
                    var bannerIntervalVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_config.RemoteBannerInterval).LongValue;
                    var bannerGroupVal = FirebaseRemoteConfig.DefaultInstance.GetValue(_config.RemoteBannerGroup).LongValue;

                    if (bannerIntervalVal > 0)
                        _intervalBanner.Value = (int)bannerIntervalVal;
                    else
                        _intervalBanner.Value = Mathf.Max(1, _config.BannerInterval);

                    if (bannerGroupVal > 0)
                        _bannerGroup = (int)bannerGroupVal;
                    else
                        _bannerGroup = Mathf.Max(1, _config.BannerGroup);

                    var bannerJson = FirebaseRemoteConfig.DefaultInstance.GetValue(_config.AllBannerIds).StringValue;
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
        /// 运行时配置快照：平台配置资产与旧 AdvertisingSettings.asset 在这里归一，
        /// 后续所有参数与云控读取都只依赖此快照。
        /// </summary>
        private sealed class ActiveAdConfig
        {
            public string SourceName;
            public bool EnableBanner;
            public bool EnableInterstitial;
            public int BannerInterval;
            public int InterstitialInterval;
            public int BannerGroup;
            public int InterstitialGroup;
            public string RemoteBannerInterval;
            public string RemoteInterstitialInterval;
            public string RemoteBannerGroup;
            public string RemoteInterstitialGroup;
            public string AllBannerIds;
            public string AllInterstitialIds;
            public List<string> BannerIds;
            public List<string> InterstitialIds;
        }

        /// <summary>
        /// 按平台宏解析生效配置：参数与云控 Key 全部来自当前平台的配置资产；
        /// 未创建平台资产时回退 1.0.0 的 AdvertisingSettings.asset，保持旧工程可用。
        /// @trace IOS-009
        /// </summary>
        private static void ResolveConfig()
        {
#if UNITY_IOS
            var platformConfig = FindPlatformConfig<IOSAdvertisingIDsConfig>();
#else
            var platformConfig = FindPlatformConfig<AndroidAdvertisingIDsConfig>();
#endif
            if (platformConfig != null)
                _config = FromPlatformConfig(platformConfig);
            else if (_iDs != null)
                _config = FromLegacySettings(_iDs);
            else
                _config = null;

            ActiveConfigName = _config == null ? "none" : _config.SourceName;
        }

        private static ActiveAdConfig FromPlatformConfig(AdvertisingIDsConfigBase c)
        {
            return new ActiveAdConfig
            {
                SourceName = c.name,
                EnableBanner = c.enableBanner,
                EnableInterstitial = c.enableInterstitial,
                BannerInterval = c.bannerInterval,
                InterstitialInterval = c.interstitialInterval,
                BannerGroup = c.bannerGroup,
                InterstitialGroup = c.interstitialGroup,
                RemoteBannerInterval = c.remoteBannerInterval,
                RemoteInterstitialInterval = c.remoteInterstitialInterval,
                RemoteBannerGroup = c.remoteBannerGroup,
                RemoteInterstitialGroup = c.remoteInterstitialGroup,
                AllBannerIds = c.allBannerIds,
                AllInterstitialIds = c.allInterstitialIds,
                BannerIds = CopyIds(c.bannerIds),
                InterstitialIds = CopyIds(c.interstitialIds)
            };
        }

        private static ActiveAdConfig FromLegacySettings(AdvertisingIDsSettings s)
        {
            return new ActiveAdConfig
            {
                SourceName = GlobalSettingsPath + ".asset (fallback)",
                EnableBanner = s.enableBanner,
                EnableInterstitial = s.enableInterstitial,
                BannerInterval = s.bannerInterval,
                InterstitialInterval = s.interstitialInterval,
                BannerGroup = s.bannerGroup,
                InterstitialGroup = s.interstitialGroup,
                RemoteBannerInterval = s.remoteBannerInterval,
                RemoteInterstitialInterval = s.remoteInterstitialInterval,
                RemoteBannerGroup = s.remoteBannerGroup,
                RemoteInterstitialGroup = s.remoteInterstitialGroup,
                AllBannerIds = s.allBannerIds,
                AllInterstitialIds = s.allInterstitialIds,
                BannerIds = CopyIds(s.bannerIds),
                InterstitialIds = CopyIds(s.interstitialIds)
            };
        }

        /// <summary>
        /// 把当前平台生效配置的全量 ID 载入运行时缓存。
        /// 云控只覆盖缓存，不改写任何 ScriptableObject 资产。
        /// @trace IOS-003
        /// </summary>
        private static void LoadPlatformIds()
        {
            if (_config == null) ResolveConfig();
            _allBannerIds = _config == null ? new List<string>() : CopyIds(_config.BannerIds);
            _allInterstitialIds = _config == null ? new List<string>() : CopyIds(_config.InterstitialIds);
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

        /// <summary>静默加载旧版设置：只有平台配置、没有旧资产时也属正常，不报错。</summary>
        private static AdvertisingIDsSettings LoadLegacySettings()
        {
            CollectAssets(typeof(AdvertisingIDsSettings).Name);
            return Resources.Load<AdvertisingIDsSettings>(GlobalSettingsPath);
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
