using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using RimWorld;

namespace PrisonerManagementPanel
{
    public class MainTabWindow_PrisonersTab : MainTabWindow_PawnTable
    {
        protected override PawnTableDef PawnTableDef
        {
            get { return DefDatabase<PawnTableDef>.GetNamed("Prisoners"); }
        }

        protected override IEnumerable<Pawn> Pawns
        {
            get => GetAllPrisoners();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
        }

        private List<Pawn> GetAllPrisoners()
        {
            Map currentMap = Find.CurrentMap;

            return currentMap.mapPawns.PrisonersOfColony;
        }

        public override void PostOpen()
        {
            base.PostOpen();
        }
    }

    public class PawnTable_Prisoners(
        PawnTableDef def,
        Func<IEnumerable<Pawn>> pawnsGetter,
        int uiWidth,
        int uiHeight) : PawnTable(def, pawnsGetter, uiWidth, uiHeight)
    {
    }
}