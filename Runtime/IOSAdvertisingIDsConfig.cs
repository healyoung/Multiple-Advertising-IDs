using UnityEngine;

namespace ShanHai
{
    /// <summary>
    /// iOS 端多广告 ID 配置文件。Create 菜单：Multiple/iOS AdvertisingIDsConfig。
    /// 本文件不含平台宏，任何平台下都可创建与编辑；资产放在宿主工程的 Resources 目录下，
    /// 由 MultipleAdIds 在 UNITY_IOS 构建中读取。
    /// @trace IOS-001
    /// </summary>
    [CreateAssetMenu(fileName = "IOSAdvertisingIDsConfig", menuName = "Multiple/iOS AdvertisingIDsConfig", order = 3)]
    public class IOSAdvertisingIDsConfig : AdvertisingIDsConfigBase
    {
    }
}
