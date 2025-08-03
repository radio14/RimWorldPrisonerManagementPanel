using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Utils
{
    public static class BodyPartUtils
    {
        // 不可移除的部位
        public static readonly List<string> UnremovableParts = new List<string>()
        {
            // 躯干
            "Torso",
            // 颈部
            "Neck",
            // 肋骨
            "Ribcage",
            // 胸骨
            "Sternum",
        };

        public static IEnumerable<BodyPartRecord> GetAllHumanBodyParts()
        {
            BodyDef humanoidBody = BodyDefOf.Human;
            foreach (var part in humanoidBody.AllParts)
            {
                // Log.Message($"part {part.def.defName} {part.Label}");
                yield return part;
            }
        }

        public static bool IsUnremovable(BodyPartRecord part)
        {
            return part == part.body.corePart;
        }

        private static IEnumerable<BodyPartRecord> GetAllPartsRecursive(BodyDef bodyDef)
        {
            foreach (var part in bodyDef.AllParts)
            {
                yield return part;
            }
        }

        private static IEnumerable<BodyPartRecord> GetChildPartsRecursive(BodyPartRecord part)
        {
            foreach (var child in part.parts)
            {
                yield return child;

                foreach (var grandChild in GetChildPartsRecursive(child))
                {
                    yield return grandChild;
                }
            }
        }
    }
}