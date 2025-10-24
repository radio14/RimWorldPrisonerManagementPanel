using HarmonyLib;
using PrisonerManagementPanel.GeneExtraction;
using PrisonerManagementPanel.Utils;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Patch;

[HarmonyPatch(typeof(Building_GeneExtractor), "Tick")]
public static class Building_GeneExtractor_Tick_Patch
{
    public static void Postfix(Building_GeneExtractor __instance)
    {
        if (!HarmonyUtils.HarmonyDetected())
        {
            return;
        }
        if (!__instance.Working && __instance.PowerOn && __instance.SelectedPawn == null)
        {
            GeneAllocator.ExecuteGeneExtraction();
        }
    }
}