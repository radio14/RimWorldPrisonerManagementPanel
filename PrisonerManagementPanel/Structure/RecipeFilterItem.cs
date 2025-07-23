using System.Collections.Generic;
using Verse;

namespace PrisonerManagementPanel.Structure;

public class RecipeFilterItem : IExposable
{
    private RecipeDef _recipe;
    private List<BodyPartRecord> _selectedParts;

    public RecipeDef Recipe
    {
        get => _recipe;
        set => _recipe = value;
    }

    public List<BodyPartRecord> SelectedParts
    {
        get => _selectedParts;
        set => _selectedParts = value;
    }

    public bool IsBodyPartOperation => Recipe.defName == "RemoveBodyPart";
    
    public void ExposeData()
    {
        Scribe_Defs.Look(ref _recipe, "Recipe");
        Scribe_Collections.Look(ref _selectedParts, "SelectedParts", LookMode.Reference);
    }
}