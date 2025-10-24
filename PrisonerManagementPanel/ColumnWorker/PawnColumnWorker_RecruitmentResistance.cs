using PrisonerManagementPanel.Utils;
using Verse;
using UnityEngine;
using RimWorld;

namespace PrisonerManagementPanel.ColumnWorker
{
    // 囚犯剩余抵抗
    public class PawnColumnWorker_RecruitmentResistance : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;

        public override int GetMinWidth(PawnTable table) 
        {
            int width = (int)Text.CalcSize("NonRecruitable".Translate()).x;
            return Mathf.Max(base.GetMinWidth(table), PawnColumnWorkerUtils.CalculateMinWidth("RecruitmentResistance", width));
        }

        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPrisoner)
            {
                return "";
            }

            return GetRecruitmentResistance(pawn);
        }

        // 获取囚犯剩余抵抗
        private string GetRecruitmentResistance(Pawn pawn)
        {
            if (pawn.guest.Recruitable)
            {
                float num2 = (double)pawn.guest.resistance > 0.0
                    ? Mathf.Max(0.1f, pawn.guest.resistance)
                    : 0.0f;
                return num2.ToString("F1").Colorize(ColoredText.DateTimeColor);
            }
            else
            {
                return "NonRecruitable".Translate();
            }
        }

        // 排序方法
        public override int Compare(Pawn a, Pawn b)
        {
            if (!a.IsPrisoner && !b.IsPrisoner)
                return 0;
            if (!a.IsPrisoner)
                return -1;
            if (!b.IsPrisoner)
                return 1;

            float resistanceA = a.guest.Recruitable ? a.guest.resistance : float.MaxValue;
            float resistanceB = b.guest.Recruitable ? b.guest.resistance : float.MaxValue;

            return resistanceA.CompareTo(resistanceB);
        }
    }
}