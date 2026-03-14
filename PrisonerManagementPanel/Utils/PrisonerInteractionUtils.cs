using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Utils;

public static class PrisonerInteractionUtils
{
    public static bool ColonyHasAnyBloodfeeder(Map map)
    {
        if (!ModsConfig.BiotechActive) return false;
        foreach (var p in map.mapPawns.FreeColonistsSpawned)
            if (p.IsBloodfeeder())
                return true;
        return false;
    }

    public static bool CanUse(Pawn pawn, PrisonerInteractionModeDef mode)
    {
        return (pawn.guest.Recruitable || !mode.hideIfNotRecruitable) &&
               (!pawn.IsWildMan() || mode.allowOnWildMan) &&
               (!mode.hideIfNoBloodfeeders || pawn.MapHeld == null || ColonyHasAnyBloodfeeder(pawn.MapHeld)) &&
               (!mode.hideOnHemogenicPawns || !ModsConfig.BiotechActive || pawn.genes == null ||
                !pawn.genes.HasActiveGene(GeneDefOf.Hemogenic)) &&
               (mode.allowInClassicIdeoMode || !Find.IdeoManager.classicMode) &&
               (!ModsConfig.AnomalyActive ||
                (!mode.hideIfNotStudiableAsPrisoner || IsStudiable(pawn)) &&
                (!mode.hideIfGrayFleshNotAppeared || Find.Anomaly.hasSeenGrayFlesh));
    }
    
    private static bool IsStudiable(Pawn pawn)
    {
        if (!ModsConfig.AnomalyActive)
            return false;

        CompStudiable comp;
        return pawn.TryGetComp<CompStudiable>(out comp) &&
               comp.EverStudiable() &&
               pawn.kindDef.studiableAsPrisoner &&
               !pawn.everLostEgo;
    }
    
}