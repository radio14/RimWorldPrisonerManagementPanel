using System.Collections.Generic;
using System.Linq;
using Verse;
using PrisonerManagementPanel.Utils;

namespace PrisonerManagementPanel.Structure;

public class RecipeFilterItem : IExposable
{
    private RecipeDef _recipe;
    // private List<BodyPartRecord> _selectedParts;
    private List<BodyPartDef> _selectedPartDefs;
    private List<string> _selectedPartLabels;
    
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

            // 兼容旧版本没有_selectedPartLabels的情况
            if (_selectedPartLabels == null)
            {
                // 使用旧的匹配方式，只根据BodyPartDef匹配
                return _selectedPartDefs
                    .Where(def => def != null)
                    .Select(def => BodyPartUtils.GetAllHumanBodyParts()
                        .FirstOrDefault(record => record.def == def))
                    .Where(record => record != null)
                    .ToList();
            }

            // 将 BodyPartDef 和标签 转换为 BodyPartRecord
            var result = new List<BodyPartRecord>();
            for (int i = 0; i < _selectedPartDefs.Count; i++)
            {
                if (_selectedPartDefs[i] != null && i < _selectedPartLabels.Count)
                {
                    var partDef = _selectedPartDefs[i];
                    var partLabel = _selectedPartLabels[i];
                    
                    // 根据标签查找确切的身体部位记录
                    var partRecord = BodyPartUtils.GetAllHumanBodyParts()
                        .FirstOrDefault(record => record.def == partDef && record.LabelCap == partLabel);
                    
                    // 如果找不到确切匹配的部位，退回到只匹配BodyPartDef
                    if (partRecord == null)
                    {
                        partRecord = BodyPartUtils.GetAllHumanBodyParts()
                            .FirstOrDefault(record => record.def == partDef);
                    }
                    
                    if (partRecord != null)
                    {
                        result.Add(partRecord);
                    }
                }
            }
            
            return result;
        }
        set
        {
            if (value == null)
            {
                _selectedPartDefs = new List<BodyPartDef>();
                _selectedPartLabels = new List<string>();
            }
            else
            {
                _selectedPartDefs = value.Select(record => record.def).ToList();
                _selectedPartLabels = value.Select(record => record.LabelCap).ToList();
            }
        }
    }
    
    public bool IsBodyPartOperation => Recipe.defName == "RemoveBodyPart";

    public void ExposeData()
    {
        Scribe_Defs.Look(ref _recipe, "Recipe");
        Scribe_Collections.Look(ref _selectedPartDefs, "SelectedPartDefs", LookMode.Def);
        Scribe_Collections.Look(ref _selectedPartLabels, "SelectedPartLabels", LookMode.Value);
    }
    
    // public void ExposeData()
    // {
    //     Scribe_Defs.Look(ref _recipe, "Recipe");
    //     Scribe_Collections.Look(ref _selectedPartDefs, "SelectedPartDefs", LookMode.Def);
    // }
}