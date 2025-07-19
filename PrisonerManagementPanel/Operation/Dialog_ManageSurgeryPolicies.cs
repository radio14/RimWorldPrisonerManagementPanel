using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonerManagementPanel.Operation;

public class Dialog_ManageSurgeryPolicies(SurgeryPolicy policy) : Dialog_ManagePolicies<SurgeryPolicy>(policy)
{
    // private readonly ThingFilterUI.UIState recipeFilterState = new ThingFilterUI.UIState();
    private readonly RecipeFilterUI.UIState _recipeFilterState = new RecipeFilterUI.UIState();

    private static RecipeFilter _surgeryGlobalFilter;

    public static RecipeFilter SurgeryGlobalFilter
    {
        get
        {
            if (_surgeryGlobalFilter == null)
            {
                _surgeryGlobalFilter = new RecipeFilter();
                // foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where(r => r.IsSurgery))
                // {
                //     // surgeryGlobalFilter.SetAllow(recipe, true);
                //     surgeryGlobalFilter.AddRecipe(recipe);
                // }
            }
    
            return _surgeryGlobalFilter;
        }
    }

    protected override string TitleKey => "手术方案";

    protected override string TipKey => "创建和管理方案模板，以控制囚犯采取的手术清单。";

    public override Vector2 InitialSize => new Vector2(860f, 700f);

    public override void PreOpen()
    {
        base.PreOpen();
        this._recipeFilterState.quickSearch.Reset();
        // 同步策略数据
        SurgeryManager.SyncAllPolicies();
    }

    public override void PreClose()
    {
        base.PreClose();
        // 确保所有策略变更都同步到管理器
        SurgeryManager.RefreshDirtyPolicies();
    
        // 强制同步所有策略
        SurgeryManager.SyncAllPolicies();
    
        // 保存策略数据
        SurgeryManager.ExposeData();
    }

    protected override SurgeryPolicy CreateNewPolicy()
    {
        IEnumerable<BodyPartRecord> partRecords = BodyPartUtils.GetAllAmputatableParts();
        Log.Message($"[打印开始] -- {partRecords.Count()}");
        // 遍历身体部件
        // foreach (BodyPartRecord partRecord in partRecords)
        // {
        //     Log.Message(
        //         $"[BodyPartRecord] {partRecord.def.defName} - {partRecord.def.label} - {partRecord.def.LabelCap}");
        // }
        
        // int newId = GenerateUniquePolicyId();
        string newLabel = "新建手术方案"; // 默认标签
        // return new SurgeryPolicy(newId, newLabel, new List<RecipeDef>());
        return SurgeryManager.CreateSurgeryPolicy(newLabel);
    }
    
    protected override SurgeryPolicy GetDefaultPolicy()
    {
        return SurgeryManager.DefaultPlan;
    }

    protected override void SetDefaultPolicy(SurgeryPolicy policy)
    {
        SurgeryManager.SetDefaultSurgeryPolicy(policy);
    }

    protected override AcceptanceReport TryDeletePolicy(SurgeryPolicy policy)
    {
        return SurgeryManager.TryDeleteSurgeryPolicy(policy);
    }

    protected override List<SurgeryPolicy> GetPolicies()
    {
        return SurgeryManager.AllSurgeryPolicy;
    }

    protected override void DoContentsRect(Rect rect)
    {
        Rect left;
        Rect right;
        rect.SplitVerticallyWithMargin(out left, out right, 10f);
        RecipeFilterUI.DoSelectedRecipesList(left, this._recipeFilterState, this.SelectedPolicy.RecipeFilter);
        RecipeFilterUI.DoAllRecipesList(right, this._recipeFilterState, this.SelectedPolicy.RecipeFilter, this.SelectedPolicy);
        // RecipeFilterUI.DoRecipeFilterConfigWindow(rect, this._recipeFilterState, this.SelectedPolicy.RecipeFilter, Dialog_ManageSurgeryPolicies.SurgeryGlobalFilter, null, this.SelectedPolicy);
    }

    private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
    {
        yield return SpecialThingFilterDefOf.AllowFresh;
    }
}