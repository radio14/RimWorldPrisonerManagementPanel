using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Utils;
using Verse;
using UnityEngine;
using RimWorld;

namespace PrisonerManagementPanel.ColumnWorker
{
    // 囚犯互动
    public class PawnColumnWorker_PrisonerInteraction : PawnColumnWorker
    {
        public override int GetMinWidth(PawnTable table) 
        {
            return Mathf.Max(base.GetMinWidth(table), PawnColumnWorkerUtils.CalculateMinWidth("PrisonerInteraction"));
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.guest == null) return;

            // 获取当前互动模式
            PrisonerInteractionModeDef currentMode = pawn.guest.ExclusiveInteractionMode;

            // 获取所有可用的排他性互动模式
            List<PrisonerInteractionModeDef> modes = DefDatabase<PrisonerInteractionModeDef>.AllDefsListForReading
                .Where(d => !d.isNonExclusiveInteraction && CanUse(pawn, d))
                .OrderBy(d => d.listOrder)
                .ToList();

            // 如果没有可选项则不绘制
            if (modes.Count == 0) return;

            string label = currentMode?.LabelCap ?? "None".Translate();

            Rect contractedRect = rect.ContractedBy(0.0f, 2f);
            Widgets.Dropdown<Pawn, PrisonerInteractionModeDef>(
                contractedRect,
                pawn,
                GetCurrentMode,
                p => GenerateMenu(p, modes),
                label,
                dragLabel: label,
                paintable: true
            );
        }

        private PrisonerInteractionModeDef GetCurrentMode(Pawn pawn)
        {
            return pawn.guest.ExclusiveInteractionMode;
        }

        private IEnumerable<Widgets.DropdownMenuElement<PrisonerInteractionModeDef>> GenerateMenu(Pawn pawn,
            List<PrisonerInteractionModeDef> modes)
        {
            foreach (PrisonerInteractionModeDef mode in modes)
            {
                yield return new Widgets.DropdownMenuElement<PrisonerInteractionModeDef>()
                {
                    option = new FloatMenuOption(mode.LabelCap, () =>
                    {
                        pawn.guest.SetExclusiveInteraction(mode);
                        InteractionModeChanged(mode, pawn);
                    }),
                    payload = mode
                };
            }
        }

        private bool CanUse(Pawn pawn, PrisonerInteractionModeDef mode)
        {
            return (pawn.guest.Recruitable || !mode.hideIfNotRecruitable) &&
                   (!pawn.IsWildMan() || mode.allowOnWildMan) &&
                   (!mode.hideIfNoBloodfeeders || pawn.MapHeld == null || ColonyHasAnyBloodfeeder(pawn.MapHeld)) &&
                   (!mode.hideOnHemogenicPawns || !ModsConfig.BiotechActive || pawn.genes == null ||
                    !pawn.genes.HasActiveGene(GeneDefOf.Hemogenic)) &&
                   (mode.allowInClassicIdeoMode || !Find.IdeoManager.classicMode) &&
                   (!ModsConfig.AnomalyActive ||
                    (!mode.hideIfNotStudiableAsPrisoner || PawnColumnWorker_PrisonerInteraction.IsStudiable(pawn)) &&
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

        // 互动模式切换
        private void InteractionModeChanged(PrisonerInteractionModeDef newMode, Pawn pawn)
        {
            if (newMode == PrisonerInteractionModeDefOf.Enslave && pawn.MapHeld != null &&
                !ColonyHasAnyWardenCapableOfEnslavement(pawn.MapHeld))
                Messages.Message("MessageNoWardenCapableOfEnslavement".Translate(), pawn,
                    MessageTypeDefOf.CautionInput);

            if (newMode == PrisonerInteractionModeDefOf.Convert && pawn.guest.ideoForConversion == null)
                pawn.guest.ideoForConversion = Faction.OfPlayer.ideos.PrimaryIdeo;

            if (newMode == PrisonerInteractionModeDefOf.Execution && pawn.MapHeld != null &&
                !ColonyHasAnyWardenCapableOfViolence(pawn.MapHeld))
                Messages.Message("MessageCantDoExecutionBecauseNoWardenCapableOfViolence".Translate(), pawn,
                    MessageTypeDefOf.CautionInput);
        }

        private bool ColonyHasAnyBloodfeeder(Map map)
        {
            if (!ModsConfig.BiotechActive) return false;
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
                if (p.IsBloodfeeder())
                    return true;
            return false;
        }

        private bool ColonyHasAnyWardenCapableOfEnslavement(Map map)
        {
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
                if (p.workSettings.WorkIsActive(WorkTypeDefOf.Warden) &&
                    new HistoryEvent(HistoryEventDefOf.EnslavedPrisoner, p.Named(HistoryEventArgsNames.Doer))
                        .DoerWillingToDo())
                    return true;
            return false;
        }

        private bool ColonyHasAnyWardenCapableOfViolence(Map map)
        {
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
                if (p.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && !p.WorkTagIsDisabled(WorkTags.Violent))
                    return true;
            return false;
        }
    }
}