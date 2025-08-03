using System;
using System.Reflection;
using HarmonyLib;
using PrisonerManagementPanel.Surgery;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Patch
{
    // [HarmonyPatch(typeof(Pawn_GuestTracker))]
    // [HarmonyPatch(nameof(Pawn_GuestTracker.SetGuestStatus))]
    // [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
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
                // 检查 Harmony 是否可用
                if (!HarmonyDetected())
                {
                    Log.Warning("[PrisonerManagement] Harmony not detected, skipping patch");
                    return;
                }

                Pawn pawnField = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (pawnField != null)
                {
                    PawnSurgeryPolicyStorage.Instance.ApplyDefaultSurgeryPolicy(pawnField);
                }
            }
        }

        // 检测 Harmony 是否存在
        private static bool HarmonyDetected()
        {
            try
            {
                Type harmonyType = Type.GetType("HarmonyLib.Harmony, 0Harmony");
                return harmonyType != null;
            }
            catch
            {
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
                Type harmonyType = Type.GetType("HarmonyLib.Harmony, 0Harmony");
                if (harmonyType == null)
                {
                    Log.Warning($"[PrisonerManagementPanel] Harmony not detected, skipping patch");
                    return;
                }

                object harmony = Activator.CreateInstance(harmonyType, "radio14.PrisonerManagementPanel");

                MethodBase targetMethod = typeof(Pawn_GuestTracker).GetMethod(
                    "SetGuestStatus",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(Faction), typeof(GuestStatus) },
                    null
                );

                if (targetMethod == null)
                {
                    Log.Warning("Target method SetGuestStatus not found");
                    return;
                }

                MethodInfo postfix = typeof(SetGuestStatus_Patch).GetMethod(
                    "Postfix",
                    BindingFlags.Static | BindingFlags.NonPublic
                );

                if (postfix == null)
                {
                    Log.Warning("Postfix method not found");
                    return;
                }

                MethodInfo patchProcessorMethod = harmonyType.GetMethod(
                    "CreateProcessor",
                    BindingFlags.Instance | BindingFlags.Public
                );

                if (patchProcessorMethod != null)
                {
                    object processor = patchProcessorMethod.Invoke(harmony, new object[] { targetMethod });

                    MethodInfo addPostfix = processor.GetType().GetMethod(
                        "AddPostfix",
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        new Type[] { typeof(MethodInfo) },
                        null
                    );

                    if (addPostfix != null)
                    {
                        addPostfix.Invoke(processor, new object[] { postfix });
                    }

                    MethodInfo patch = processor.GetType().GetMethod(
                        "Patch",
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        Type.EmptyTypes,
                        null
                    );

                    if (patch != null)
                    {
                        patch.Invoke(processor, null);
                    }
                }
                else
                {
                    Type harmonyMethodType = Type.GetType("HarmonyLib.HarmonyMethod, 0Harmony");
                    if (harmonyMethodType == null) return;

                    ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
                    if (ctor == null) return;

                    object harmonyMethod = ctor.Invoke(new object[] { postfix });

                    MethodInfo patchMethod = harmonyType.GetMethod(
                        "Patch",
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        new Type[] { typeof(MethodBase), typeof(HarmonyMethod), typeof(HarmonyMethod) },
                        null
                    );

                    if (patchMethod != null)
                    {
                        patchMethod.Invoke(harmony, new object[] { targetMethod, null, harmonyMethod });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[PrisonerManagementPanel] Harmony patch failed: {ex}");
            }
        }
    }
}