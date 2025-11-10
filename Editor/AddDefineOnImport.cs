using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AddDefineOnImport
{
    private const string Define = "FIREBASE_REMOTE_CONFIG";

    static AddDefineOnImport()
    {
        AddDefine();
    }

    private static void AddDefine()
    {
        BuildTargetGroup buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

        // 读取当前的 Scripting Define Symbols
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);

        // 已经包含则不重复写入
        if (defines.Contains(Define)) return;

        if (!string.IsNullOrEmpty(defines))
            defines += ";" + Define;
        else
            defines = Define;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines);
        Debug.Log($"[MyPackage] Added define symbol: {Define}");
    }
}