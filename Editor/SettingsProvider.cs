using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using ShanHai;

namespace ShanHai.Editor
{
    public class AdvertisingIDsSettingsProvider : SettingsProvider
    {
        private static readonly string SettingsPath = $"Assets/Resources/{MultipleAdIds.GlobalSettingsPath}.asset";
        private static readonly string HeaderName = $"AdvertisingIDs/{MultipleAdIds.GlobalSettingsPath}";
        private const string ResourcesFolder = "Assets/Resources";
        private SerializedObject _customSettings;

        private static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(MultipleAdIds.AdvertisingIDs);
        }

        private static void EnsureResourcesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
                AssetDatabase.CreateFolder("Assets", "Resources");
        }

        /// <summary>
        /// 创建或定位平台配置文件。资产落在宿主工程 Resources 下，可编辑、可版本控制，
        /// 不依赖只读的 UPM 包目录；创建代码本身不含任何平台宏。
        /// @trace IOS-007
        /// </summary>
        private static void CreateOrPingPlatformConfig<T>(string fileName) where T : ScriptableObject
        {
            EnsureResourcesFolder();
            string path = $"{ResourcesFolder}/{fileName}.asset";

            var config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        private static bool IsSettingsAvailable()
        {
            EnsureResourcesFolder();
            if (!File.Exists(SettingsPath))
            {
                var settings = ScriptableObject.CreateInstance<AdvertisingIDsSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return true;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            _customSettings = GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            using var changeCheckScope = new EditorGUI.ChangeCheckScope();
            EditorGUILayout.HelpBox(
                $"编译目标应使用的配置：{MultipleAdIds.ActiveIdSource}\n" +
                $"运行时实际生效：{MultipleAdIds.ActiveConfigName}\n" +
                "Android / iOS 两份配置各自包含完整参数：开关、云控 Key（间隔 / 分组 / 所有 ID）、" +
                "本地间隔与分组、以及各自的广告 ID 列表。用下方按钮创建或选中资产后，在 Inspector 里分别填写，" +
                "两端云控 ID 互不影响。本面板的 AdvertisingSettings.asset 仅在未创建平台配置时作为回退。",
                MessageType.Info);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("创建/定位 Android 配置"))
                CreateOrPingPlatformConfig<AndroidAdvertisingIDsConfig>("AndroidAdvertisingIDsConfig");
            if (GUILayout.Button("创建/定位 iOS 配置"))
                CreateOrPingPlatformConfig<IOSAdvertisingIDsConfig>("IOSAdvertisingIDsConfig");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_customSettings.FindProperty("enableBanner"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("enableInterstitial"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("remoteInterstitialInterval"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("interstitialInterval"));
            EditorGUILayout.Space(30);
            EditorGUILayout.PropertyField(_customSettings.FindProperty("remoteBannerInterval"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("bannerInterval"));
            EditorGUILayout.Space(30);
            EditorGUILayout.PropertyField(_customSettings.FindProperty("remoteInterstitialGroup"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("interstitialGroup"));
            EditorGUILayout.Space(30);
            EditorGUILayout.PropertyField(_customSettings.FindProperty("remoteBannerGroup"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("bannerGroup"));
            EditorGUILayout.Space(30);
            EditorGUILayout.PropertyField(_customSettings.FindProperty("allInterstitialIds"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("interstitialIds"));
            EditorGUILayout.Space(30);
            EditorGUILayout.PropertyField(_customSettings.FindProperty("allBannerIds"));
            EditorGUILayout.PropertyField(_customSettings.FindProperty("bannerIds"));

            EditorGUILayout.Space(20);
            if (!changeCheckScope.changed) return;
            _customSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        public AdvertisingIDsSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new AdvertisingIDsSettingsProvider(HeaderName, SettingsScope.Project)
                {
                    keywords = GetSearchKeywordsFromGUIContentProperties<AdvertisingIDsSettings>()
                };
                return provider;
            }

            Debug.LogError($"Open AdvertisingIDs Settings error,Please Create AdvertisingIDs AdvertisingIDsSettings.assets File in Path Assets/Resources/");

            return null;
        }
    }
}
