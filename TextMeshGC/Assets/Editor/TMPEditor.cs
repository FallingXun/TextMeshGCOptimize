using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TMPEditor : Editor
{
    public const string TMP_USE_POOL_TAG = "TMP_USE_POOL";

    [MenuItem("TMP工具/开启对象池模式")]
    public static void SetUsePool()
    {
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        if (symbols.Contains(TMP_USE_POOL_TAG) == false)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Format("{0};{1}", TMP_USE_POOL_TAG, symbols));
        }
    }

    [MenuItem("TMP工具/关闭对象池模式")]
    public static void SetNotUsePool()
    {

        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        if (symbols.Contains(TMP_USE_POOL_TAG))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols = symbols.Replace(TMP_USE_POOL_TAG, ""));
        }
    }
}
