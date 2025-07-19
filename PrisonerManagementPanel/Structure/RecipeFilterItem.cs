using System.Collections.Generic;
using Verse;

namespace PrisonerManagementPanel.Structure;

public class RecipeFilterItem
{
    public RecipeDef Recipe { get; set; }
    // public List<BodyPartDef> SelectedParts { get; set; } // 仅对移除身体部位有效
    public List<BodyPartRecord> SelectedParts { get; set; } // 仅对移除身体部位有效
    public bool IsExpanded;
    public bool IsBodyPartOperation => Recipe.defName == "RemoveBodyPart";
}