using System.Collections.Generic;
using UnityEngine;

namespace ShanHai
{
    /// <summary>
    /// 平台广告 ID 配置基类：只承载 ID 列表，不含任何平台宏。
    /// 派生类可在 Unity 中通过 Create 菜单创建资产，资产放在宿主工程任意 Resources 目录下即可。
    /// 平台差异只出现在 MultipleAdIds.LoadPlatformIds 的读取分支，不出现在这里。
    /// @trace IOS-005
    /// </summary>
    public abstract class AdvertisingIDsConfigBase : ScriptableObject
    {
        [Header("横幅广告 ID（一行一个，可任意追加）")]
        public List<string> bannerIds = new List<string>();

        [Header("插屏广告 ID（一行一个，可任意追加）")]
        public List<string> interstitialIds = new List<string>();
    }
}
