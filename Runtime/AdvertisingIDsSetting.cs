using System;
using System.Collections.Generic;
using HealYoung;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ShanHai
{
    [CreateAssetMenu(fileName = "AdvertisingIDsSettings", menuName = "Multiple/AdvertisingIDsSettings", order = 1)]
    public class AdvertisingIDsSettings : ScriptableObject
    {
        [Header("云控使用的每日[横幅]广告ID的间隔的Key值,(间隔N次后，切换下一个ID)")] [SerializeField]
        public string remoteBannerInterval = "remote_banner_interval";

        [SerializeField] public int bannerInterval = 3;

        [Header("云控使用的每日[插屏]广告ID的间隔的Key值， (间隔N次后，切换下一个ID)")] [SerializeField]
        public string remoteInterstitialInterval = "remote_interstitial_interval";

        [SerializeField] public int interstitialInterval = 3;

        [Header("云控使用的每日[横幅]广告组数量的Key值，(每个广告组包含N个广告ID)")] [SerializeField]
        public string remoteBannerGroup = "remote_banner_group";

        [SerializeField] public int bannerGroup = 2;

        [Header("云控使用的每日[插屏]广告组数量的Key值，(每个广告组包含N个广告ID)")] [SerializeField]
        public string remoteInterstitialGroup = "remote_interstitial_group";

        [SerializeField] public int interstitialGroup = 2;

        [Header("云控使用的所有[插屏]广告ID的Key值，(所有的插屏广告ID，逗号分隔)")] [SerializeField]
        public string allInterstitialIds = "all_interstitial_ids";

        [Header("云控使用的所有[横幅]广告ID的Key值，(所有的横幅广告ID，逗号分隔)")] [SerializeField]
        public string allBannerIds = "all_banner_ids";

        [Header("横幅广告ID")] [SerializeField] public List<string> bannerIds = new List<string>();

        [Header("插屏广告ID")] [SerializeField] public List<string> interstitialIds = new List<string>();
    }

    public enum AdvertingType
    {
        Banner,
        Interstitial
    }
}