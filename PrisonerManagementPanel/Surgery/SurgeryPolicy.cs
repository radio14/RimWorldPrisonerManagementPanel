using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Surgery;

public class SurgeryPolicy : Policy
{
    public RecipeFilter RecipeFilter = new RecipeFilter();
    // public SurgeryApplyMode ApplyMode = SurgeryApplyMode.ReplaceAll;
    // 标记是否被修改过
    public bool IsDirty = false;

    protected override string LoadKey => "SurgeryPolicy";

    public SurgeryPolicy()
    {
        this.RecipeFilter = new RecipeFilter();
    }
    
    public SurgeryPolicy(int id, string label)
        : base(id, label)
    {
        this.id = id;
        this.label = label;
        this.RecipeFilter = new RecipeFilter();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref RecipeFilter, "recipeFilter");
        // Scribe_Values.Look(ref ApplyMode, "applyMode", SurgeryApplyMode.ReplaceAll);
    }
    
    public override void CopyFrom(Policy other)
    {
        if (other is SurgeryPolicy otherPolicy)
        {
            this.RecipeFilter = otherPolicy.RecipeFilter;
            this.IsDirty = otherPolicy.IsDirty;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is SurgeryPolicy other)
        {
            return id == other.id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
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