using System;
using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Structure;
using PrisonerManagementPanel.Utils;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace PrisonerManagementPanel.Surgery
{
    public static class RecipeFilterUI
    {
        private static float viewHeight;
        private const float ExtraViewHeight = 90f;
        private const float RangeLabelTab = 10f;
        private const float RangeLabelHeight = 19f;
        private const float SliderHeight = 32f;
        private const float SliderTab = 20f;

        private static readonly string[] ApplyModeLabels = Enum.GetValues(typeof(SurgeryApplyMode))
            .Cast<SurgeryApplyMode>()
            .Select(m => m.ToString().CapitalizeFirst())
            .ToArray();

        public class UIState
        {
            public Vector2 scrollPosition;
            public Vector2 leftScrollPosition;
            public Vector2 rightScrollPosition;
            public QuickSearchWidget quickSearch = new QuickSearchWidget();
            public RecipeDef expandedRecipe;
        }

        // 已选择的配方
        public static void DoSelectedRecipesList(
            Rect rect,
            RecipeFilterUI.UIState state,
            RecipeFilter filter)
        {
            Widgets.DrawMenuSection(rect);

            // 标题
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 30f),
                "SurgeryPolicy_Selected".Translate());
            Text.Font = GameFont.Small;

            // 列表区域
            Rect listRect = new Rect(rect.x + 5f, rect.y + 35f, rect.width - 10f, rect.height - 40f);

            // 计算内容高度（只计算普通配方和具体部位操作）
            float contentHeight = filter.AllowedItems
                .Where(item => item.Recipe.defName != "RemoveBodyPart" || item.SelectedParts.Count > 0)
                .Sum(item => 28f);

            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, contentHeight);

            Widgets.BeginScrollView(listRect, ref state.leftScrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            // 创建要删除项的临时列表
            List<RecipeFilterItem> itemsToRemove = new List<RecipeFilterItem>();

            foreach (var item in filter.AllowedItems.ToList()) // 使用 ToList() 创建副本避免枚举时修改
            {
                // 添加null检查
                if (item == null || item.Recipe == null)
                {
                    // 如果配方项或配方为null，标记为待删除
                    itemsToRemove.Add(item);
                    continue;
                }
                
                // 跳过未选择具体部位的"移除身体部位"项
                if (item.Recipe.defName == "RemoveBodyPart" && item.SelectedParts.Count == 0)
                    continue;

                listing.Gap(4f);
                Rect rowRect = listing.GetRect(24f);

                string displayLabel = GetDisplayLabel(item);

                if (displayLabel != null)
                {
                    Widgets.Label(rowRect.LeftPart(0.6f), displayLabel);

                    // 删除按钮 - 只收集要删除的项，不立即删除
                    if (Widgets.ButtonText(rowRect.RightPart(0.2f), "×"))
                    {
                        itemsToRemove.Add(item);
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }
            }

            listing.End();
            Widgets.EndScrollView(); // 确保 EndScrollView 被调用

            // 在枚举结束后删除项
            foreach (var item in itemsToRemove)
            {
                filter.RemoveItem(item);
            }
        }

        // 全部
        public static void DoAllRecipesList(
            Rect rect,
            RecipeFilterUI.UIState state,
            RecipeFilter filter,
            SurgeryPolicy policy,
            RecipeFilter globalFilter = null,
            IEnumerable<RecipeDef> forceHiddenDefs = null)
        {
            Widgets.DrawMenuSection(rect);

            // 搜索框
            Rect searchRect = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 24f);
            state.quickSearch.OnGUI(searchRect);

            // 列表区域
            Rect listRect = new Rect(rect.x + 5f, rect.y + 30f, rect.width - 10f, rect.height - 35f);

            // 计算内容高度
            float contentHeight = CalculateAllRecipesHeight(state.quickSearch.filter, forceHiddenDefs);
            // float contentHeight = CalculateAllRecipesHeight(state.quickSearch.filter, forceHiddenDefs, state.expandedRecipe);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, contentHeight);

            Widgets.BeginScrollView(listRect, ref state.rightScrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            foreach (RecipeDef recipe in PawnSurgeryPolicyStorage.AllRecipes)
            {
                // 过滤隐藏项
                if (forceHiddenDefs != null && forceHiddenDefs.Contains(recipe))
                    continue;

                // 搜索过滤
                if (state.quickSearch.filter.Active && !state.quickSearch.filter.Matches(recipe.LabelCap))
                    continue;

                listing.Gap(4f);
                Rect rowRect = listing.GetRect(24f);

                // 显示配方名称
                Widgets.Label(rowRect.LeftPart(0.7f), recipe.LabelCap);
                if (recipe.defName == "RemoveBodyPart" ||
                    recipe.targetsBodyPart && recipe.appliedOnFixedBodyParts.Count != 0)
                {
                    // 特殊处理：点击时不添加，而是展开部位选择
                    if (Widgets.ButtonText(rowRect.RightPart(0.3f),
                            state.expandedRecipe == recipe
                                ? "SurgeryPolicy_List_Collapse".Translate()
                                : "SurgeryPolicy_List_Expand".Translate()))
                    {
                        state.expandedRecipe = (state.expandedRecipe == recipe) ? null : recipe;
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }

                    // 展开部位选择
                    if (state.expandedRecipe == recipe)
                    {
                        listing.Gap(4f);
                        DoBodyPartSelection(filter, listing, recipe, policy);
                    }
                }
                else
                {
                    // 普通配方直接添加
                    if (Widgets.ButtonText(rowRect.RightPart(0.3f), "+"))
                    {
                        filter.AddItem(new RecipeFilterItem { Recipe = recipe });
                        policy.MarkDirty();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }
            }

            listing.End();
            Widgets.EndScrollView();
        }
        
        // private static float CalculateAllRecipesHeight(
        //     QuickSearchFilter searchFilter,
        //     IEnumerable<RecipeDef> forceHiddenDefs,
        //     RecipeDef expandedRecipe) // 新增参数
        // {
        //     float height = 0f;
        //
        //     foreach (RecipeDef recipe in PawnSurgeryPolicyStorage.AllRecipes)
        //     {
        //         if (forceHiddenDefs != null && forceHiddenDefs.Contains(recipe))
        //             continue;
        //
        //         if (searchFilter.Active && !searchFilter.Matches(recipe.LabelCap))
        //             continue;
        //
        //         // 基础行高度
        //         height += 28f;
        //
        //         // 只有当前配方是展开的，并且有固定身体部位时才加上子项高度
        //         if (recipe == expandedRecipe && recipe.appliedOnFixedBodyParts.Count > 0)
        //         {
        //             height += 28f * recipe.appliedOnFixedBodyParts.Count;
        //         }
        //     }
        //
        //     return height;
        // }

        private static float CalculateAllRecipesHeight(
            QuickSearchFilter searchFilter,
            IEnumerable<RecipeDef> forceHiddenDefs)
        {
            float height = 0f;
        
            foreach (RecipeDef recipe in PawnSurgeryPolicyStorage.AllRecipes)
            {
                if (forceHiddenDefs != null && forceHiddenDefs.Contains(recipe))
                    continue;
        
                if (searchFilter.Active && !searchFilter.Matches(recipe.LabelCap))
                    continue;
        
                if (recipe.appliedOnFixedBodyParts.Count > 0)
                {
                    height += 28f * recipe.appliedOnFixedBodyParts.Count;
                }
        
                height += 28f; // 每行高度
            }
        
            return height;
        }

        private static void DoBodyPartSelection(RecipeFilter filter, Listing_Standard listing, RecipeDef recipe,
            SurgeryPolicy policy)
        {
            if (recipe.defName == "RemoveBodyPart")
            {
                foreach (BodyPartRecord part in BodyPartUtils.GetAllHumanBodyParts())
                {
                    // 过滤不可移除的部位
                    if (BodyPartUtils.IsUnremovable(part))
                    {
                        continue;
                    }
                    
                    listing.Gap(4f);
                    Rect partRect = listing.GetRect(24f);

                    // 部位名称
                    Widgets.Label(partRect.LeftPart(0.7f), part.LabelCap);

                    // 添加按钮
                    if (Widgets.ButtonText(partRect.RightPart(0.3f), "+"))
                    {
                        // 创建新的配方项（每个部位单独添加）
                        filter.AddItem(new RecipeFilterItem
                        {
                            Recipe = recipe,
                            SelectedParts = new List<BodyPartRecord> { part }
                        });
                        policy.MarkDirty();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }

                return;
            }

            foreach (BodyPartRecord part in BodyPartUtils.GetAllHumanBodyParts())
            {
                // 检查部位是否适用当前配方
                if (IsPartValidForRecipe(recipe, part))
                {
                    listing.Gap(4f);
                    Rect partRect = listing.GetRect(24f);

                    Widgets.Label(partRect.LeftPart(0.7f), part.LabelCap);

                    if (Widgets.ButtonText(partRect.RightPart(0.3f), "+"))
                    {
                        filter.AddItem(new RecipeFilterItem
                        {
                            Recipe = recipe,
                            SelectedParts = new List<BodyPartRecord> { part }
                        });
                        policy.MarkDirty();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                }
            }
        }

        // 检查身体部位是否适用于配方
        private static bool IsPartValidForRecipe(RecipeDef recipe, BodyPartRecord part)
        {
            // 1. 检查配方直接指定的部位
            if (recipe.appliedOnFixedBodyParts.Contains(part.def))
                return true;

            // 2. 检查部位分组
            foreach (BodyPartGroupDef group in recipe.appliedOnFixedBodyPartGroups)
            {
                if (part.groups.Contains(group))
                    return true;
            }

            return false;
        }

        // 名称显示
        public static string GetDisplayLabel(RecipeFilterItem item)
        {
            if (item == null || item.Recipe == null)
            {
                return null;
            }

            if (item.Recipe.targetsBodyPart && item.SelectedParts != null && item.SelectedParts.Count > 0)
            {
                var firstPart = item.SelectedParts.First();
                if (firstPart != null)
                {
                    return
                        $"{item.Recipe.LabelCap}({firstPart.customLabel ?? firstPart.def?.label ?? "Unknown"})";
                }
            }

            return item.Recipe.LabelCap;
        }
    }
}