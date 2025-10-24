using PrisonerManagementPanel.Utils;
using Verse;
using UnityEngine;
using RimWorld;

namespace PrisonerManagementPanel.ColumnWorker
{
    // 囚犯市场价值
    public class PawnColumnWorker_SlavePrice : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;

        public override int GetMinWidth(PawnTable table) 
        {
            return Mathf.Max(base.GetMinWidth(table), PawnColumnWorkerUtils.CalculateMinWidth("SlavePrice"));
        }

        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPrisoner)
            {
                return "";
            }

            return GetSlavePrice(pawn);
        }

        // 获取囚犯市场价值
        private string GetSlavePrice(Pawn pawn)
        {
            float statValue = pawn.GetStatValue(StatDefOf.MarketValue);

            return statValue.ToStringMoney();
        }

        public override int Compare(Pawn a, Pawn b)
        {
            if (!a.IsPrisoner && !b.IsPrisoner)
                return 0;
            if (!a.IsPrisoner)
                return -1;
            if (!b.IsPrisoner)
                return 1;

            float priceA = a.GetStatValue(StatDefOf.MarketValue);
            float priceB = b.GetStatValue(StatDefOf.MarketValue);

            return priceA.CompareTo(priceB);
        }
    }
}