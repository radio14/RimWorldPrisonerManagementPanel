using PrisonerManagementPanel.Utils;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace PrisonerManagementPanel;

// Mod 入口主类
public class PrisonerManagementPanelMod : Mod
{
    public static PrisonerManagementPanelSettings Settings;
    
    public PrisonerManagementPanelMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<PrisonerManagementPanelSettings>();
        HarmonyUtils.TryRegisterPatch();
    }
    
    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);
        
        listing.CheckboxLabeled("BloodfeedDefaultSetting_Label".Translate(), 
                               ref Settings.defaultBloodfeed, 
                               "BloodfeedDefaultSettingTooltip".Translate());
        listing.CheckboxLabeled("HemogenFarmDefaultSetting_Label".Translate(), 
                               ref Settings.defaultHemogenFarm, 
                               "HemogenFarmDefaultSettingTooltip".Translate());
        listing.Gap(12f);
        
        listing.End();
        Settings.Write();
    }

    public override string SettingsCategory()
    {
        return "PrisonerManagementPanel";
    }
}