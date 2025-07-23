using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonerManagementPanel.Surgery;

[StaticConstructorOnStartup]
public class Dialog_ManageSurgeryPolicies(SurgeryPolicy policy) : Dialog_ManagePolicies<SurgeryPolicy>(policy)
{
    private static Texture2D InfoIcon;
    private readonly RecipeFilterUI.UIState _recipeFilterState = new RecipeFilterUI.UIState();

    private static RecipeFilter _surgeryGlobalFilter;

    public static RecipeFilter SurgeryGlobalFilter
    {
        get
        {
            if (_surgeryGlobalFilter == null)
            {
                _surgeryGlobalFilter = new RecipeFilter();
            }

            return _surgeryGlobalFilter;
        }
    }

    static Dialog_ManageSurgeryPolicies()
    {
        InfoIcon = TexButton.Info;
    }

    protected override string TitleKey => "SurgeryPolicy_TitleKey";

    protected override string TipKey => "SurgeryPolicy_TipKey";

    public override Vector2 InitialSize => new Vector2(860f, 700f);

    public override void DoWindowContents(Rect inRect)
    {
        // 先调用基类方法绘制窗口内容
        base.DoWindowContents(inRect);

        // 添加信息按钮在标题旁
        Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 32f);
        AddInfoButton(titleRect);
    }

    public override void PreOpen()
    {
        base.PreOpen();
        this._recipeFilterState.quickSearch.Reset();
    }

    public override void PreClose()
    {
        base.PreClose();
        // 确保所有策略变更都同步到管理器
        PawnSurgeryPolicyStorage.Instance.RefreshDirtyPolicies();
    }

    protected override SurgeryPolicy CreateNewPolicy()
    {
        return PawnSurgeryPolicyStorage.Instance.CreateSurgeryPolicy();
    }

    protected override SurgeryPolicy GetDefaultPolicy()
    {
        return PawnSurgeryPolicyStorage.Instance.GetDefaultPolicy();
    }

    protected override void SetDefaultPolicy(SurgeryPolicy policy)
    {
        PawnSurgeryPolicyStorage.Instance.SetDefaultSurgeryPolicy(policy);
    }

    protected override AcceptanceReport TryDeletePolicy(SurgeryPolicy policy)
    {
        return PawnSurgeryPolicyStorage.Instance.TryDeleteSurgeryPolicy(policy);
    }

    protected override List<SurgeryPolicy> GetPolicies()
    {
        return PawnSurgeryPolicyStorage.Instance.GetAllSurgeryPolicy();
    }

    protected override void DoContentsRect(Rect rect)
    {
        if (PawnSurgeryPolicyStorage.Instance.IsNonePolicy(this.SelectedPolicy))
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 30f),
                "SurgeryPolicy_NonePolicy_Info".Translate());
            return;
        }
        
        if (PawnSurgeryPolicyStorage.Instance.IsClearPolicy(this.SelectedPolicy))
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, 30f),
                "SurgeryPolicy_ClearPolicy_Info".Translate());
            return;
        }

        // 显示 ApplyMode 设置区域
        Rect modeRect = new Rect(rect.x, rect.y, rect.width, 60f);
        DoApplyModeSelection(modeRect, SelectedPolicy);
        rect.y += 60f;
        rect.height -= 60f;

        Rect left;
        Rect right;
        rect.SplitVerticallyWithMargin(out left, out right, 10f);
        RecipeFilterUI.DoSelectedRecipesList(left, this._recipeFilterState, this.SelectedPolicy.RecipeFilter);
        RecipeFilterUI.DoAllRecipesList(right, this._recipeFilterState, this.SelectedPolicy.RecipeFilter,
            this.SelectedPolicy);
    }

    private void DoApplyModeSelection(Rect rect, SurgeryPolicy policy)
    {
        Rect titleRect = rect.TopPart(0.4f);
        Widgets.Label(titleRect, "SurgeryApplyMode_ApplyModeSelection_Title".Translate());

        Rect buttonsRect = rect.BottomPart(0.6f);
        float buttonWidth = (buttonsRect.width / Enum.GetValues(typeof(SurgeryApplyMode)).Length) - 12f;

        Listing_Standard listing = new Listing_Standard();
        listing.ColumnWidth = buttonWidth;
        listing.Begin(buttonsRect);

        foreach (SurgeryApplyMode mode in Enum.GetValues(typeof(SurgeryApplyMode)))
        {
            string label = mode.GetLabel();
            bool selected = policy.ApplyMode == mode;

            listing.Gap(4f);

            Rect optionRect = listing.GetRect(Text.LineHeight);

            TooltipHandler.TipRegion(optionRect, mode.GetTipSignal());

            if (Widgets.RadioButtonLabeled(optionRect, label, selected))
            {
                policy.ApplyMode = mode;
            }
        }

        listing.End();
    }

    private void AddInfoButton(Rect titleRect)
    {
        // 获取标题文本的实际宽度
        string titleText = TitleKey.Translate();
        Vector2 titleSize = Text.CalcSize(titleText);

        // 计算按钮位置 (紧邻标题文本右侧)
        Rect infoButtonRect = new Rect(
            titleRect.x + titleSize.x + 5f + 48f, // 标题文本右侧5px + 24自身宽度
            titleRect.y + (titleRect.height - 24f) / 2f, // 垂直居中
            24f,
            24f
        );

        // 确保按钮不会超出窗口边界
        if (infoButtonRect.xMax > titleRect.xMax)
        {
            infoButtonRect.x = titleRect.xMax - 24f;
        }

        // 绘制信息按钮
        if (Widgets.ButtonImage(infoButtonRect, InfoIcon))
        {
            // 打开提示弹窗
            Find.WindowStack.Add(new Dialog_MessageBox(
                "SurgeryPolicy_Info".Translate(),
                "OK".Translate(),
                null,
                null,
                null,
                TitleKey.Translate()
            ));
        }
    }
}