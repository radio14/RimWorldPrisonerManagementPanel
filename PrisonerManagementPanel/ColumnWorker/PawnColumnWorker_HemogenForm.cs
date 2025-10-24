using System;
using System.Collections.Generic;
using PrisonerManagementPanel.Utils;
using Verse;
using RimWorld;
using UnityEngine;

namespace PrisonerManagementPanel.ColumnWorker
{
    // 收取血原质
    public class PawnColumnWorker_HemogenFarm : PawnColumnWorker_Checkbox
    {
        public override int GetMinWidth(PawnTable table) 
        {
            return Mathf.Max(base.GetMinWidth(table), PawnColumnWorkerUtils.CalculateMinWidth("HemogenFarm"));
        }
        
        // 是否显示复选框
        protected override bool HasCheckbox(Pawn pawn)
        {
            return pawn.IsPrisoner && ModsConfig.BiotechActive;
        }

        // 获取当前状态
        protected override bool GetValue(Pawn pawn)
        {
            return HasInteractionWithHemogenFarm(pawn);
        }

        // 点击复选框时调用
        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            SetAllowHemogenHarvest(pawn, value);
        }

        // 查询当前是否已经设置了提取任务
        public static bool HasInteractionWithHemogenFarm(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || pawn.BillStack == null)
                return false;
            return pawn.IsPrisoner &&
                   pawn.guest.HasInteractionWith(mode => mode == PrisonerInteractionModeDefOf.HemogenFarm);
        }


        // 设置是否允许提取
        public static void SetAllowHemogenHarvest(Pawn pawn, bool enabled)
        {
            if (!ModsConfig.BiotechActive || !pawn.IsPrisoner)
                return;
            pawn.guest.ToggleNonExclusiveInteraction(PrisonerInteractionModeDefOf.HemogenFarm, enabled);
            NonExclusiveInteractionToggled(pawn, enabled);
        }

        public static void NonExclusiveInteractionToggled(Pawn pawn, bool enabled)
        {
            BillStack billStack = pawn.BillStack;
            Bill bill1;
            if (billStack == null)
            {
                bill1 = (Bill)null;
            }
            else
            {
                List<Bill> bills = billStack.Bills;
                bill1 = bills != null
                    ? bills.FirstOrDefault<Bill>((Predicate<Bill>)(x => x.recipe == RecipeDefOf.ExtractHemogenPack))
                    : (Bill)null;
            }

            Bill bill2 = bill1;
            if (enabled)
            {
                if (bill2 != null || !SanguophageUtility.CanSafelyBeQueuedForHemogenExtraction(pawn))
                    return;
                HealthCardUtility.CreateSurgeryBill(pawn, RecipeDefOf.ExtractHemogenPack, (BodyPartRecord)null);
            }
            else
            {
                if (bill2 == null)
                    return;
                pawn.BillStack.Bills.Remove(bill2);
            }
        }
    }
}