using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.GeneExtraction;
using PrisonerManagementPanel.Surgery;
using PrisonerManagementPanel.Utils;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;

namespace PrisonerManagementPanel.ColumnWorker;

// 基因提取
public class PawnColumnWorker_GeneExtraction : PawnColumnWorker
{
    public override int GetMinWidth(PawnTable table) 
    {
        return Mathf.Max(base.GetMinWidth(table), PawnColumnWorkerUtils.CalculateMinWidth("GeneExtraction_Policy"));
    }

    public override void DoHeader(Rect rect, PawnTable table)
    {
        base.DoHeader(rect, table);
        MouseoverSounds.DoRegion(rect);

        float minWidth = 120f;
        string buttonText = "GeneExtraction_Policy".Translate();
        Vector2 textSize = Text.CalcSize(buttonText);

        float buttonWidth = Mathf.Max(minWidth, textSize.x + 20f);
        float buttonHeight = 32f;

        Rect buttonRect = new Rect(
            rect.x + (rect.width - buttonWidth) / 2f,
            rect.y + (rect.height - 65f),
            buttonWidth,
            buttonHeight
        );

        if (Widgets.ButtonText(buttonRect, buttonText))
        {
            // GetGene();
            Find.WindowStack.Add(new Dialog_ManageGeneExtract());
        }
    }

    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
        if (pawn != null && pawn.IsPrisoner)
        {
            int text = GetGeneCount(pawn);
            Rect textRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(textRect, text.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        if (Mouse.IsOver(rect) && pawn.genes != null)
        {
            List<GeneDef> selectedGenes = new List<GeneDef>();
            if (GeneExtractionStorage.Instance != null)
            {
                selectedGenes = GeneExtractionStorage.Instance.GetSelectedGenes();
            }

            List<GeneDef> commonGenes = GeneUtils.GetCommonGenes(pawn, selectedGenes);

            if (commonGenes.Any())
            {
                string tooltipText = "";
                // 检查是否开启了基因提取
                if (GeneExtractionStorage.Instance != null && !GeneExtractionStorage.Instance.IsOpen())
                {
                    tooltipText = "GeneExtraction_NoPolicy".Translate() + "\n";
                }
                
                // 是否在基因重构
                if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogermReplicating))
                {
                    string label = HediffDefOf.XenogermReplicating.label;
                    tooltipText = label + "\n\n";
                }

                tooltipText += "GeneExtraction_MatchingGene".Translate() + ":\n" +
                               string.Join("\n", commonGenes.Select(g => g.LabelCap));
                TooltipHandler.TipRegion(rect, tooltipText);
            }
            else
            {
                TooltipHandler.TipRegion(rect, "GeneExtraction_NoMatchingGene".Translate());
            }
        }
    }

    // 获取囚犯可提取基因数量
    private int GetGeneCount(Pawn pawn)
    {
        if (!ModsConfig.BiotechActive || pawn.genes == null)
        {
            return 0;
        }

        List<GeneDef> selectedGenes = new List<GeneDef>();
        if (GeneExtractionStorage.Instance != null)
        {
            selectedGenes = GeneExtractionStorage.Instance.GetSelectedGenes();
        }

        List<GeneDef> commonGenes = GeneUtils.GetCommonGenes(pawn, selectedGenes);
        return commonGenes.Count;
    }
    
    public override int Compare(Pawn a, Pawn b)
    {
        if (!a.IsPrisoner && !b.IsPrisoner)
            return 0;
        if (!a.IsPrisoner)
            return -1;
        if (!b.IsPrisoner)
            return 1;
        
        int geneCountA = GetGeneCount(a);
        int geneCountB = GetGeneCount(b);
    
        return geneCountB.CompareTo(geneCountA);
    }

    // private void GetGene()
    // {
    //     // 检查生物技术mod是否启用
    //     if (!ModsConfig.BiotechActive)
    //     {
    //         return;
    //     }
    //
    //     Map currentMap = Find.CurrentMap;
    //
    //     List<Pawn> list = currentMap.mapPawns.PrisonersOfColony;
    //
    //     foreach (Pawn pawn in list)
    //     {
    //         // 检查pawn是否有基因信息
    //         if (pawn.genes == null)
    //         {
    //             return;
    //         }
    //
    //         Log.Message($"人: {pawn.LabelCap}--{pawn.genes.GenesListForReading.Count.ToString()}");
    //
    //         List<Gene> genes = pawn.genes.GenesListForReading
    //             .Where(gene => gene.def.passOnDirectly && gene.def.biostatArc == 0).ToList();
    //         // 被筛选的
    //         List<Gene> genes2 = pawn.genes.GenesListForReading
    //             .Where(gene => !(gene.def.passOnDirectly && gene.def.biostatArc == 0)).ToList();
    //
    //         // foreach (Gene gene in genes)
    //         // {
    //         //     Log.Message($"基因过滤后: {gene.LabelCap}--{gene.def.passOnDirectly}--{gene.def.biostatArc}");
    //         // }
    //         //
    //         // foreach (Gene gene in genes2)
    //         // {
    //         //     Log.Message($"过滤: {gene.LabelCap}--{gene.def.passOnDirectly}--{gene.def.biostatArc}");
    //         // }
    //     }
    // }
}