using System;
using Verse;
using System.Reflection;
using HarmonyLib;
using PrisonerManagementPanel.Patch;
using RimWorld;

namespace PrisonerManagementPanel.Utils;

public static class HarmonyUtils
{
    // 检测 Harmony 是否存在
    public static bool HarmonyDetected()
    {
        try
        {
            Type harmonyType = Type.GetType("HarmonyLib.Harmony, 0Harmony");
            bool has = harmonyType != null;
            if (!has)
            {
                Log.Warning("[PrisonerManagementPanel] Harmony not detected, skipping patch");
            }

            return has;
        }
        catch
        {
            Log.Warning("[PrisonerManagementPanel] Harmony not detected, skipping patch");
            return false;
        }
    }

    /**
     * 为了兼容 以前没有打过 Harmony 的用户 暂时使用
     * @deprecated
     */
    public static void TryRegisterPatch()
    {
        try
        {
            // 直接创建 Harmony 实例并应用补丁
            var harmony = new Harmony("radio14.PrisonerManagementPanel");
            harmony.PatchAll();
        }
        catch (Exception ex)
        {
            Log.Warning($"[PrisonerManagementPanel] Harmony patch failed: {ex}");
        }
    }
}