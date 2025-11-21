using PrisonerManagementPanel.Surgery;
using Verse;

namespace PrisonerManagementPanel;

public class PrisonerManagementPanelSettings : ModSettings
{
    // 汲血默认设置
    public bool defaultBloodfeed = false;
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref defaultBloodfeed, "defaultBloodfeed", false);
    }
}