using PrisonerManagementPanel.Patch;
using Verse;

namespace PrisonerManagementPanel;

// Mod 入口主类
public class PrisonerManagementPanelMod : Mod
{
    public PrisonerManagementPanelMod(ModContentPack content) : base(content)
    {
        SetGuestStatus_Patch.TryRegisterPatch();
    }
}