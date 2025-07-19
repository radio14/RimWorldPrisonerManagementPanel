using PrisonerManagementPanel.Operation;
using Verse;

namespace PrisonerManagementPanel;

public class PrisonerManagementPanelSettings : ModSettings
{
    public override void ExposeData()
    {
        base.ExposeData();

        // 暴露手术策略和绑定关系
        SurgeryManager.ExposeData();
    }
}