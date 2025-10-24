using PrisonerManagementPanel.Utils;
using Verse;

namespace PrisonerManagementPanel;

// Mod 入口主类
public class PrisonerManagementPanelMod : Mod
{
    public PrisonerManagementPanelMod(ModContentPack content) : base(content)
    {
        HarmonyUtils.TryRegisterPatch();
    }
}