using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Operation;

public class SurgeryPolicy : Policy
{
    public RecipeFilter RecipeFilter = new RecipeFilter();

    // 标记是否被修改过
    public bool IsDirty = false;

    protected override string LoadKey => "SurgeryPolicy";

    public SurgeryPolicy(int id, string label, List<RecipeDef> initialOperations)
        : base(id, label)
    {
        // operations = initialOperations ?? new List<RecipeDef>();
    }

    public override void CopyFrom(Policy other)
    {
        if (other is SurgeryPolicy otherPolicy)
        {
            this.RecipeFilter = otherPolicy.RecipeFilter;
            this.IsDirty = otherPolicy.IsDirty;
        }
    }

    // 重写 ExposeData 方法以支持更多字段的序列化
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref RecipeFilter, "filter");
        Scribe_Values.Look(ref IsDirty, "isDirty", false);
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    public void ClearDirty()
    {
        IsDirty = false;
    }
}