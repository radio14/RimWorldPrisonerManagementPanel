using Verse;
using RimWorld;

namespace PrisonerManagementPanel
{
    // 汲血
    public class PawnColumnWorker_Bloodfeed : PawnColumnWorker_Checkbox
    {
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
            SetAllowBloodfeed(pawn, value);
        }

        // 查询当前是否已经设置了提取任务
        public static bool HasInteractionWithHemogenFarm(Pawn pawn)
        {
            if (!ModsConfig.BiotechActive || pawn.BillStack == null)
                return false;
            return pawn.IsPrisoner &&
                   pawn.guest.HasInteractionWith(mode => mode == PrisonerInteractionModeDefOf.Bloodfeed);
        }


        // 设置是否允许提取
        public static void SetAllowBloodfeed(Pawn pawn, bool enabled)
        {
            if (!ModsConfig.BiotechActive || !pawn.IsPrisoner)
                return;
            pawn.guest.ToggleNonExclusiveInteraction(PrisonerInteractionModeDefOf.Bloodfeed, enabled);
        }
    }
}