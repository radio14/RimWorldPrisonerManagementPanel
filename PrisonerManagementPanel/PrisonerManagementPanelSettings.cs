using PrisonerManagementPanel.Surgery;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel;

public class PrisonerManagementPanelSettings : ModSettings
{
    // 汲血默认设置
    public bool defaultBloodfeed = false;
    
    // 收取血原质默认设置
    public bool defaultHemogenFarm = false;
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref defaultBloodfeed, "defaultBloodfeed", false);
        Scribe_Values.Look(ref defaultHemogenFarm, "defaultHemogenFarm", false);
    }
}