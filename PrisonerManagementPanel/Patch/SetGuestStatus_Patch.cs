using System;
using System.Reflection;
using HarmonyLib;
using PrisonerManagementPanel.Surgery;
using PrisonerManagementPanel.Utils;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Patch;

// [HarmonyPatch(typeof(Pawn_GuestTracker))]
// [HarmonyPatch(nameof(Pawn_GuestTracker.SetGuestStatus))]
// [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
[HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
public static class SetGuestStatus_Patch
{
    static void Postfix(
        Pawn_GuestTracker __instance,
        Faction newHost,
        GuestStatus guestStatus)
    {
        if (guestStatus == GuestStatus.Prisoner &&
            newHost != null &&
            newHost.IsPlayer)
        {
            if (!HarmonyUtils.HarmonyDetected())
            {
                return;
            }

            Pawn pawnField = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawnField != null)
            {
                PawnSurgeryPolicyStorage.Instance.ApplyDefaultSurgeryPolicy(pawnField);
            }
        }
    }
}