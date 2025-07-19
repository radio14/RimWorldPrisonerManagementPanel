using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

public static class BodyPartUtils
{
    public static IEnumerable<BodyPartRecord> GetAllAmputatableParts()
    {
        BodyDef humanoidBody = BodyDefOf.Human;
        foreach (var part in humanoidBody.AllParts)
        {
            yield return part;
        }
    }

    private static IEnumerable<BodyPartRecord> GetAllPartsRecursive(BodyDef bodyDef)
    {
        foreach (var part in bodyDef.AllParts)
        {
            yield return part;

            // foreach (var child in GetChildPartsRecursive(part))
            // {
            //     yield return child;
            // }
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