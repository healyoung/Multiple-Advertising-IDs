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
        private SerializedObject _customSettings;

        private static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(MultipleAdIds.AdvertisingIDs);
        }

        private static bool IsSettingsAvailable()
        {
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