using System.Collections.Generic;
using UnityEngine;

namespace ShanHai
{
    /// <summary>
    /// 平台广告配置基类：每个平台一份完整配置，字段与 1.0.0 的 AdvertisingIDsSettings 对齐。
    /// 云控 Key、开关、间隔、分组、本地 ID 都在各自平台资产里独立配置，
    /// 因此 Android / iOS 可以使用完全不同的云控 ID 与参数。
    /// 本文件不含任何平台宏。
    /// @trace IOS-005 / IOS-008
    /// </summary>
    public abstract class AdvertisingIDsConfigBase : ScriptableObject
    {
        [Header("是否启用[横幅]广告")] public bool enableBanner = false;

        [Header("是否启用[插屏]广告")] public bool enableInterstitial = false;

        [Header("云控：横幅间隔 Key（间隔 N 次后切下一个 ID）")]
        public string remoteBannerInterval = "remote_banner_interval";

        [Header("本地：横幅间隔次数")] public int bannerInterval = 3;

        [Header("云控：插屏间隔 Key（间隔 N 次后切下一个 ID）")]
        public string remoteInterstitialInterval = "remote_interstitial_interval";

        [Header("本地：插屏间隔次数")] public int interstitialInterval = 3;

        [Header("云控：横幅分组 Key（每组 N 个 ID）")]
        public string remoteBannerGroup = "remote_banner_group";

        [Header("本地：横幅分组数")] public int bannerGroup = 2;

        [Header("云控：插屏分组 Key（每组 N 个 ID）")]
        public string remoteInterstitialGroup = "remote_interstitial_group";

        [Header("本地：插屏分组数")] public int interstitialGroup = 2;

        [Header("云控：所有插屏 ID 的 Key（JSON {\"list\":[...]}）")]
        public string allInterstitialIds = "all_interstitial_ids";

        [Header("云控：所有横幅 ID 的 Key（JSON {\"list\":[...]}）")]
        public string allBannerIds = "all_banner_ids";

        [Header("横幅广告 ID（一行一个，可任意追加）")]
        public List<string> bannerIds = new List<string>();

        [Header("插屏广告 ID（一行一个，可任意追加）")]
        public List<string> interstitialIds = new List<string>();
    }
}
