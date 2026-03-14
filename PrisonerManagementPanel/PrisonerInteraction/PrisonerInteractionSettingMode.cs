using Verse;

namespace PrisonerManagementPanel.PrisonerInteraction;

public enum PrisonerInteractionSettingMode
{
    EqualTreatment, // 一视同仁
    ByRace, // 分门别类
    ByGender, // 因性制宜
    ByRaceAndGender // 因族因性
}

public static class PrisonerInteractionSettingModeExtensions
{
    public static string GetLabel(this PrisonerInteractionSettingMode mode)
    {
        return mode switch
        {
            PrisonerInteractionSettingMode.EqualTreatment => "PrisonerInteraction_Mode_EqualTreatment".Translate(),
            PrisonerInteractionSettingMode.ByRace => "PrisonerInteraction_Mode_ByRace".Translate(),
            PrisonerInteractionSettingMode.ByGender => "PrisonerInteraction_Mode_ByGender".Translate(),
            PrisonerInteractionSettingMode.ByRaceAndGender => "PrisonerInteraction_Mode_ByRaceAndGender".Translate(),
            _ => mode.ToString().CapitalizeFirst()
        };
    }

    public static TipSignal GetTipSignal(this PrisonerInteractionSettingMode mode)
    {
        return mode switch
        {
            PrisonerInteractionSettingMode.EqualTreatment => "PrisonerInteraction_Mode_EqualTreatment_Tip".Translate(),
            PrisonerInteractionSettingMode.ByRace => "PrisonerInteraction_Mode_ByRace_Tip".Translate(),
            PrisonerInteractionSettingMode.ByGender => "PrisonerInteraction_Mode_ByGender_Tip".Translate(),
            PrisonerInteractionSettingMode.ByRaceAndGender => "PrisonerInteraction_Mode_ByRaceAndGender_Tip".Translate(),
            _ => "None".Translate()
        };
    }
}
