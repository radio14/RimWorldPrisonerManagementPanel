using Verse;

namespace PrisonerManagementPanel.Surgery;

public enum SurgeryApplyMode
{
    ReplaceAll,   // 全部更新
    Append,       // 追加
    PartialUpdate // 部分更新
}

public static class SurgeryApplyModeExtensions
{
    public static string GetLabel(this SurgeryApplyMode mode)
    {
        return mode switch
        {
            SurgeryApplyMode.ReplaceAll => "SurgeryApplyMode_ReplaceAll_Label".Translate(),
            SurgeryApplyMode.Append => "SurgeryApplyMode_Append_Label".Translate(),
            SurgeryApplyMode.PartialUpdate => "SurgeryApplyMode_PartialUpdate_Label".Translate(),
            _ => mode.ToString().CapitalizeFirst()
        };
    }
    
    public static TipSignal GetTipSignal(this SurgeryApplyMode mode)
    {
        return mode switch
        {
            SurgeryApplyMode.ReplaceAll => "SurgeryApplyMode_ReplaceAll_Tip".Translate(),
            SurgeryApplyMode.Append => "SurgeryApplyMode_Append_Tip".Translate(),
            SurgeryApplyMode.PartialUpdate => "SurgeryApplyMode_PartialUpdate_Tip".Translate(),
            _ => "SurgeryApplyMode_Unknown_Tip".Translate()
        };
    }
}
