using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PrisonerManagementPanel.Utils;

public static class RaceUtils
{
    // 获取游戏中所有可用的种族定义
    public static IEnumerable<ThingDef> GetAllRaces()
    {
        var thingDefs = DefDatabase<ThingDef>.AllDefs.Where(def => 
            def.race != null && 
            def.race.Humanlike &&
            !def.defName.StartsWith("Corpse_") &&
            def.race.body != null
        );
            
        return thingDefs;
    }
    
    // 根据Pawn获取其种族定义
    public static ThingDef GetRaceFromPawn(Pawn pawn)
    {
        return pawn?.def;
    }
    
    // 获取人类种族
    public static ThingDef DefaultRace()
    {
        // return ThingDefOf.Human;
        return DefDatabase<ThingDef>.GetNamedSilentFail("Human");
    }
    
    // 比较两个种族是否相同
    public static bool IsRaceMatch(ThingDef race1, ThingDef race2)
    {
        return race1 == race2;
    }
}