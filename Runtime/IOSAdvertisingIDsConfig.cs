#if UNITY_IOS || UNITY_EDITOR
using System.Collections.Generic;

namespace ShanHai
{
    /// <summary>
    /// iOS 端多广告 ID 配置文件（随 UPM 包一起发布）。
    ///
    /// 添加 ID：直接在下面两个数组中逐行追加字符串，数量不限，无需改动其它代码。
    /// 顺序即「登录第 N 天 × 每组 M 个 ID」的分配顺序（见 MultipleAdIds.ReadyTodayIds）。
    /// 开关、间隔次数、分组数、云控 Key 等参数仍统一在 AdvertisingSettings.asset 中配置；
    /// Firebase 云控下发的 all_banner_ids / all_interstitial_ids 会覆盖本文件的默认值。
    ///
    /// 编译条件：仅 iOS 与编辑器编译，Android 真机包不包含本配置内容。
    /// @trace IOS-001
    /// </summary>
    internal static class IOSAdvertisingIDsConfig
    {
        /// <summary>iOS 横幅广告 ID，一行一个，可任意追加</summary>
        internal static readonly IReadOnlyList<string> BannerIds = new List<string>
        {
            // "ios_banner_0001",
            // "ios_banner_0002",
        };

        /// <summary>iOS 插屏广告 ID，一行一个，可任意追加</summary>
        internal static readonly IReadOnlyList<string> InterstitialIds = new List<string>
        {
            // "ios_interstitial_0001",
            // "ios_interstitial_0002",
        };
    }
}
#endif
