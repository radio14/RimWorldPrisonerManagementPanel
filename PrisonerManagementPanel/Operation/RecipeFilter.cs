using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Structure;
using Verse;

namespace PrisonerManagementPanel.Operation
{
    public class RecipeFilter : IExposable
    {
        private List<RecipeFilterItem> _allowedItems = new List<RecipeFilterItem>();

        // Policy 模式 [全部覆盖(删除 Pawn 当前所有 Bill，重新添加 Policy 中的 Recipe) | 追加(在 Bill 列表末尾添加 Policy 中的 Recipe) | 部分覆盖(只添加 Policy 中存在但 Pawn 当前没有的 Recipe)]
        public SurgeryApplyMode ApplyMode = SurgeryApplyMode.ReplaceAll;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _allowedItems, "allowedItems", LookMode.Deep);
            Scribe_Values.Look(ref ApplyMode, "applyMode");
            // List<string> recipeNames = new List<string>();
            // List<string> bodyPartNames = new List<string>(_allowedBodyParts);
            //
            // if (Scribe.mode == LoadSaveMode.Saving)
            // {
            //     recipeNames.AddRange(_allowedRecipes.Select(r => r.Recipe.defName));
            //     bodyPartNames.AddRange(_allowedBodyParts);
            // }

            // Scribe_Collections.Look(ref recipeNames, "allowedRecipeNames", LookMode.Value);
            // Scribe_Collections.Look(ref bodyPartNames, "allowedBodyPartNames", LookMode.Value);

            // if (Scribe.mode == LoadSaveMode.LoadingVars)
            // {
            //     _allowedRecipes.Clear();
            //     foreach (string name in recipeNames)
            //     {
            //         RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(name);
            //         if (recipe != null)
            //         {
            //             _allowedRecipes.Add(recipe);
            //         }
            //     }
            //     _allowedBodyParts = new HashSet<string>(bodyPartNames);
            // }
        }

        public void AddItem(RecipeFilterItem item)
        {
            // 对于"移除身体部位"操作，确保至少选择一个具体部位
            if (item.Recipe.defName == "RemoveBodyPart")
            {
                // 查找是否已存在相同配方的项
                // var existingItem = _allowedItems.FirstOrDefault(i => 
                //     i.Recipe == item.Recipe && 
                //     i.SelectedParts.SequenceEqual(item.SelectedParts));
                //
                // if (existingItem == null)
                // {
                //     _allowedItems.Add(item);
                //     Log.Message($"添加的任务：{item.Recipe.defName} - {item.Recipe.label}");
                // }
                _allowedItems.Add(item);
                Log.Message($"添加的任务：{item.Recipe.defName} - {item.Recipe.label}");
            }
            else
            {
                // 普通配方直接添加
                _allowedItems.Add(item);
                Log.Message($"添加的任务：{item.Recipe.defName} - {item.Recipe.label}");
            }
        }

        // // public void AddItem(RecipeDef recipe)
        // {
        //     Log.Message($"添加的任务：{recipe.defName} - {recipe.label}");
        //     var item = new RecipeFilterItem { 
        //         Recipe = recipe,
        //         SelectedParts = recipe.defName == "RemoveBodyPart" 
        //             ? new List<BodyPartDef>() 
        //             : null
        //     };
        //     _allowedItems.Add(item);
        // }

        public void RemoveItem(RecipeFilterItem item)
        {
            if (item == null) return;

            int index = _allowedItems.FindIndex(i =>
                i.Recipe == item.Recipe &&
                (i.SelectedParts == null || i.SelectedParts.SequenceEqual(item.SelectedParts)));

            if (index >= 0)
            {
                _allowedItems.RemoveAt(index);
            }
        }

        public IEnumerable<RecipeFilterItem> AllowedItems => _allowedItems;
    }
}