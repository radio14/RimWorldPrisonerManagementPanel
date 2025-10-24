using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Utils;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.GeneExtraction;

public static class GeneAllocator
{
    // 执行基因提取任务
    public static void ExecuteGeneExtraction()
    {
        if (Find.CurrentMap == null)
        {
            return;
        }
        
        if (!GeneExtractionStorage.Instance.IsOpen() || !CanStart())
        {
            return;
        }

        // 查找当前地图中的基因提取器
        var geneExtractors = Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_GeneExtractor>()
            .ToList();

        // 查找可用的基因提取器（未在工作中 且 有电力 且 没有绑定人的）
        var availableExtractor = geneExtractors.FirstOrDefault(extractor =>
            !extractor.Working && extractor.PowerOn && extractor.SelectedPawn == null);

        if (availableExtractor != null)
        {
            var prisoners = Find.CurrentMap.mapPawns.PrisonersOfColony
                .Where(pawn => pawn.Spawned && pawn.IsPrisonerOfColony)
                .ToList();
            // 查找第一个符合条件的囚犯
            var prisoner = prisoners.FirstOrDefault(pawn =>
                (bool)availableExtractor.CanAcceptPawn(pawn) &&
                HasGene(pawn) &&
                !IsPawnSelectedByOtherExtractor(pawn, geneExtractors) &&
                !pawn.health.hediffSet.HasHediff(HediffDefOf.XenogermReplicating));

            if (prisoner != null)
            {
                // Messages.Message($"{prisoner.LabelCap} 等待提取基因", MessageTypeDefOf.CautionInput);
                availableExtractor.SelectedPawn = prisoner;
            }
        }
    }

    private static bool HasGene(Pawn pawn)
    {
        return GeneUtils.HasAnyGeneFromList(pawn, GeneExtractionStorage.Instance.GetSelectedGenes());
    }

    private static bool CanStart()
    {
        return GeneExtractionStorage.Instance.GetSelectedGenes().Count > 0 && 
               Find.CurrentMap.mapPawns.PrisonersOfColony.Any(pawn => pawn.Spawned && pawn.IsPrisonerOfColony);
    }

    private static bool IsPawnSelectedByOtherExtractor(Pawn pawn, IReadOnlyList<Building_GeneExtractor> extractors)
    {
        return extractors.Any(extractor => extractor.SelectedPawn == pawn);
    }
}