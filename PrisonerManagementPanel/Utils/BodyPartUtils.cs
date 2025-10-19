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

        // 根据种族获取身体部位
        public static IEnumerable<BodyPartRecord> GetAllBodyPartsForRace(ThingDef race)
        {
            // 检查是否是有效的种族定义并且有身体定义
            if (race?.race?.body != null)
            {
                foreach (var part in race.race.body.AllParts)
                {
                    yield return part;
                }
            }
            else
            {
                // 如果无法获取种族的身体定义，回退到人类身体定义
                foreach (var part in GetAllHumanBodyParts())
                    yield return part;
            }
        }

        public static bool IsUnremovable(BodyPartRecord part)
        {
            // 检查part是否为null
            if (part == null)
                return true;
                
            return part == part.body?.corePart;
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
            // 检查part是否为null
            if (part == null)
                yield break;
                
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