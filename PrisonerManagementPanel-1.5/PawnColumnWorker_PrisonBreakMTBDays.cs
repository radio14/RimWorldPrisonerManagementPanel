using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace PrisonerManagementPanel
{
    // 囚犯越狱间隔天数列
    public class PawnColumnWorker_PrisonBreakMTBDays : PawnColumnWorker_Text
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleCenter;

        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 80);

        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPrisoner)
            {
                return "";
            }

            return GetPrisonBreakMTBDays(pawn);
        }

        // 获取囚犯越狱间隔天数
        private string GetPrisonBreakMTBDays(Pawn pawn)
        {
            StringBuilder sb = new StringBuilder();
            int num1 = (int)PrisonBreakUtility.InitiatePrisonBreakMtbDays(pawn, sb, true);
            string label1;
            if (PrisonBreakUtility.IsPrisonBreaking(pawn))
            {
                label1 = (string)("CurrentlyPrisonBreaking".Translate());
            }
            else if (num1 < 0)
            {
                label1 = (string)("Never".Translate());
                Gene gene;
                if (PrisonBreakUtility.GenePreventsPrisonBreaking(pawn, out gene))
                {
                    sb.AppendLineIfNotEmpty();
                    sb.AppendTagged((TaggedString)"PrisonBreakingDisabledDueToGene".Translate(gene.def.Named("GENE"))
                        .Colorize(ColorLibrary.RedReadable));
                }
            }
            else
            {
                label1 = "PeriodDays".Translate((NamedArgument)num1).ToString().Colorize(ColoredText.DateTimeColor);
            }

            return label1;
        }
    }
}