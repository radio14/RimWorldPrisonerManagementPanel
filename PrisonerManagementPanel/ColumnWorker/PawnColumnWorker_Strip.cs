using System;
using Verse;
using UnityEngine;
using RimWorld;
using Verse.Sound;
using Verse.Steam;

namespace PrisonerManagementPanel.ColumnWorker;

public class PawnColumnWorker_Strip : PawnColumnWorker
{
    public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 100);

    protected override void HeaderClicked(Rect headerRect, PawnTable table)
    {
        // 检查是否按下了Shift键
        if (Event.current.shift)
        {
            if (Event.current.button == 0)
            {
                // Shift + 左键 - 为所有囚犯添加剥光任务
                foreach (Pawn pawn in table.PawnsListForReading)
                {
                    if (pawn.MapHeld != null && pawn.Spawned && pawn.IsPrisoner)
                    {
                        if (!HasStripJob(pawn))
                        {
                            pawn.MapHeld.designationManager.AddDesignation(
                                new Designation(pawn, DesignationDefOf.Strip));
                        }
                    }
                }

                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
            else if (Event.current.button == 1)
            {
                // Shift + 右键 - 移除所有囚犯的剥光任务
                foreach (Pawn pawn in table.PawnsListForReading)
                {
                    if (pawn.MapHeld != null && pawn.Spawned && pawn.IsPrisoner)
                    {
                        if (HasStripJob(pawn))
                        {
                            pawn.MapHeld.designationManager.RemoveDesignation(
                                pawn.MapHeld.designationManager.DesignationOn(pawn, DesignationDefOf.Strip));
                        }
                    }
                }

                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
        }
        else
        {
            base.HeaderClicked(headerRect, table);
        }
    }

    public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
    {
        if (pawn == null) return;

        this.DoAssignStripButtons(rect, pawn);
    }

    private void DoAssignStripButtons(Rect rect, Pawn pawn)
    {
        Rect rowRect = rect.ContractedBy(2f);
        if (Widgets.ButtonText(rowRect, GetStripButtonText(pawn)))
        {
            if (pawn.MapHeld != null && pawn.Spawned)
            {
                // 检查是否已经有剥光标记
                if (HasStripJob(pawn))
                {
                    pawn.MapHeld.designationManager.RemoveDesignation(
                        pawn.MapHeld.designationManager.DesignationOn(pawn, DesignationDefOf.Strip));
                }
                else
                {
                    pawn.MapHeld.designationManager.AddDesignation(new Designation(pawn, DesignationDefOf.Strip));
                }

                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
        }
    }

    private Boolean HasStripJob(Pawn pawn)
    {
        return pawn.MapHeld.designationManager.DesignationOn(pawn, DesignationDefOf.Strip) != null;
    }

    private string GetStripButtonText(Pawn pawn)
    {
        if (HasStripJob(pawn))
        {
            return "ColumnStrip_Doing".Translate();
        }

        return "DesignatorStrip".Translate();
    }

    protected override string GetHeaderTip(PawnTable table)
    {
        string headerTip = base.GetHeaderTip(table);
        if (!SteamDeck.IsSteamDeckInNonKeyboardMode)
            headerTip = (string)(headerTip + ("\n" + "ColumnStripShiftClickTip".Translate()));
        return headerTip;
    }

    public override int Compare(Pawn a, Pawn b)
    {
        if (!a.IsPrisoner && !b.IsPrisoner)
            return 0;
        if (!a.IsPrisoner)
            return -1;
        if (!b.IsPrisoner)
            return 1;

        int numA = HasStripJob(a) ? 1 : 0;
        int numB = HasStripJob(b) ? 1 : 0;

        return numA.CompareTo(numB);
    }
}