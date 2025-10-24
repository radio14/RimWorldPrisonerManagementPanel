using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PrisonerManagementPanel.Utils;

public static class GeneUtils
{
    // 判断Pawn是否拥有目标基因列表中的所有基因
    public static bool HasAllGenesFromList(Pawn pawn, List<GeneDef> targetGenes)
    {
        if (pawn?.genes?.GenesListForReading == null || targetGenes == null)
            return false;

        return targetGenes.All(geneDef => pawn.genes.GenesListForReading.Any(gene => gene.def == geneDef));
    }
    
    // 判断Pawn是否拥有目标基因列表中的任意一个基因
    public static bool HasAnyGeneFromList(Pawn pawn, List<GeneDef> targetGenes)
    {
        if (pawn?.genes?.GenesListForReading == null || targetGenes == null)
            return false;

        return targetGenes.Any(geneDef => pawn.genes.GenesListForReading.Any(gene => gene.def == geneDef));
    }
    
    // 返回Pawn和目标基因列表中都拥有的基因列表
    public static List<GeneDef> GetCommonGenes(Pawn pawn, List<GeneDef> targetGenes)
    {
        if (pawn?.genes?.GenesListForReading == null || targetGenes == null)
            return new List<GeneDef>();

        return pawn.genes.GenesListForReading.Where(gene => targetGenes.Any(geneDef => gene.def == geneDef)).Select(gene => gene.def).ToList();
    }
}