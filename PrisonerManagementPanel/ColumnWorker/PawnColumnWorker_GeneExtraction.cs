using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.GeneExtraction;
using PrisonerManagementPanel.Surgery;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;

namespace PrisonerManagementPanel.ColumnWorker;

// 基因提取
public class PawnColumnWorker_GeneExtraction : PawnColumnWorker_Text
{
    protected override TextAnchor Anchor => TextAnchor.MiddleCenter;
    public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 80);

    public override void DoHeader(Rect rect, PawnTable table)
    {
        base.DoHeader(rect, table);
        MouseoverSounds.DoRegion(rect);
        if (!Widgets.ButtonText(new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f),
                (string)"基因提取设置"))
            return;
        GetGene();
        Find.WindowStack.Add((Window)new Dialog_ManageGeneExtract());
    }

    protected override string GetTextFor(Pawn pawn)
    {
        if (pawn == null || !pawn.IsPrisoner)
        {
            return "";
        }

        return GetGeneCount(pawn);
    }

    // 获取囚犯可提取基因数量
    private string GetGeneCount(Pawn pawn)
    {
        // 检查生物技术mod是否启用
        if (!ModsConfig.BiotechActive)
        {
            return "N/A";
        }

        // 检查pawn是否有基因信息
        if (pawn.genes == null)
        {
            return "0";
        }

        // 返回基因数量
        return pawn.genes.GenesListForReading.Where(gene => gene.def.passOnDirectly && gene.def.biostatArc == 0)
            .ToList().Count().ToString();
    }

    private void GetGene()
    {
        // 检查生物技术mod是否启用
        if (!ModsConfig.BiotechActive)
        {
            return;
        }

        Map currentMap = Find.CurrentMap;

        List<Pawn> list = currentMap.mapPawns.PrisonersOfColony;

        foreach (Pawn pawn in list)
        {
            // 检查pawn是否有基因信息
            if (pawn.genes == null)
            {
                return;
            }

            Log.Message($"人: {pawn.LabelCap}--{pawn.genes.GenesListForReading.Count.ToString()}");

            List<Gene> genes = pawn.genes.GenesListForReading
                .Where(gene => gene.def.passOnDirectly && gene.def.biostatArc == 0).ToList();
            // 被筛选的
            List<Gene> genes2 = pawn.genes.GenesListForReading
                .Where(gene => !(gene.def.passOnDirectly && gene.def.biostatArc == 0)).ToList();

            foreach (Gene gene in genes)
            {
                Log.Message($"基因过滤后: {gene.LabelCap}--{gene.def.passOnDirectly}--{gene.def.biostatArc}");
            }

            foreach (Gene gene in genes2)
            {
                Log.Message($"过滤: {gene.LabelCap}--{gene.def.passOnDirectly}--{gene.def.biostatArc}");
            }
        }
    }
}