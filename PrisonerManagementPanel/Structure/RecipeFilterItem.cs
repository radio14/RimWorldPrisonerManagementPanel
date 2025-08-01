using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PrisonerManagementPanel.Structure;

public class RecipeFilterItem : IExposable
{
    private RecipeDef _recipe;
    // private List<BodyPartRecord> _selectedParts;
    private List<BodyPartDef> _selectedPartDefs;
    
    public RecipeDef Recipe
    {
        get => _recipe;
        set => _recipe = value;
    }
    
    // public List<BodyPartRecord> SelectedParts
    // {
    //     get => _selectedParts;
    //     set => _selectedParts = value;
    // }

    public List<BodyPartDef> SelectedPartDefs
    {
        get => _selectedPartDefs;
        set => _selectedPartDefs = value;
    }
    
    public List<BodyPartRecord> SelectedParts
    {
        get
        {
            if (_selectedPartDefs == null || _selectedPartDefs.Count == 0)
                return new List<BodyPartRecord>();

            // 将 BodyPartDef 转换为 BodyPartRecord
            return _selectedPartDefs
                .Where(def => def != null)
                .Select(def => BodyPartUtils.GetAllHumanBodyParts()
                    .FirstOrDefault(record => record.def == def))
                .Where(record => record != null)
                .ToList();
        }
        set
        {
            if (value == null)
            {
                _selectedPartDefs = new List<BodyPartDef>();
            }
            else
            {
                _selectedPartDefs = value.Select(record => record.def).ToList();
            }
        }
    }
    
    public bool IsBodyPartOperation => Recipe.defName == "RemoveBodyPart";

    public void ExposeData()
    {
        Scribe_Defs.Look(ref _recipe, "Recipe");
        Scribe_Collections.Look(ref _selectedPartDefs, "SelectedPartDefs", LookMode.Def);
    }
    
    // public void ExposeData()
    // {
    //     Scribe_Defs.Look(ref _recipe, "Recipe");
    //     // Scribe_Collections.Look(ref _selectedParts, "SelectedParts", LookMode.Reference);
    // }
}