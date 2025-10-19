using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Structure;
using PrisonerManagementPanel.Utils;
using Verse;

namespace PrisonerManagementPanel.Surgery
{
    public class RecipeFilter : IExposable
    {
        private List<RecipeFilterItem> _allowedItems = new List<RecipeFilterItem>();

        // Policy 模式 [全部覆盖(删除 Pawn 当前所有 Bill，重新添加 Policy 中的 Recipe) | 追加(在 Bill 列表末尾添加 Policy 中的 Recipe) | 部分覆盖(只添加 Policy 中存在但 Pawn 当前没有的 Recipe)]
        public SurgeryApplyMode ApplyMode = SurgeryApplyMode.ReplaceAll;
        // Policy 种族
        public ThingDef Race;

        public RecipeFilter()
        {
            // 初始化种族为人类
            Race = RaceUtils.DefaultRace();
        }

        public RecipeFilter(List<RecipeFilterItem> allowedItems)
        {
            _allowedItems = allowedItems;
            ApplyMode = SurgeryApplyMode.ReplaceAll;
            // 初始化种族为人类
            Race = RaceUtils.DefaultRace();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref ApplyMode, "ApplyMode", SurgeryApplyMode.ReplaceAll);
            Scribe_Defs.Look(ref Race, "Race");

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // 保存前清理null项
                _allowedItems = _allowedItems.Where(item => item != null && item.Recipe != null).ToList();
                Scribe_Collections.Look(ref _allowedItems, "AllowedItems", LookMode.Deep);
            }
            else
            {
                Scribe_Collections.Look(ref _allowedItems, "AllowedItems", LookMode.Deep);
                if (_allowedItems == null)
                {
                    _allowedItems = new List<RecipeFilterItem>();
                }
                else
                {
                    // 加载后清理null项
                    _allowedItems = _allowedItems.Where(item => item != null && item.Recipe != null).ToList();
                }

                if (Race == null)
                {
                    Race = RaceUtils.DefaultRace();
                }
                
                // 检查种族是否存在，如果不存在则重置为人类
                if (Race != null && !RaceUtils.GetAllRaces().Contains(Race))
                {
                    Race = RaceUtils.DefaultRace();
                    _allowedItems = new List<RecipeFilterItem>();
                }
            }
        }

        public void AddItem(RecipeFilterItem item)
        {
            // 对于"移除身体部位"操作，确保至少选择一个具体部位
            if (item.Recipe.defName == "RemoveBodyPart")
            {
                _allowedItems.Add(item);
                Log.Message($"添加的任务 移除身体部位：{item.Recipe.defName} - {item.Recipe.label}");
            }
            else
            {
                // 普通配方直接添加
                _allowedItems.Add(item);
                Log.Message($"添加的任务：{item.Recipe.defName} - {item.Recipe.label}");
            }
        }

        public void RemoveItem(RecipeFilterItem item)
        {
            if (item == null) return;
            _allowedItems.Remove(item);
        }

        public void ClearItems()
        {
            _allowedItems.Clear();
        }

        public List<RecipeFilterItem> AllowedItems => _allowedItems;
    }
}