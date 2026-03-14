using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PrisonerManagementPanel.Utils;

public class Pmp_TabDrawer
{
    private const float MaxTabWidth = 200f;
    public const float TabHeight = 32f;
    public const float TabHoriztonalOverlap = 10f;
    private static readonly List<TabRecord> tmpTabs = new List<TabRecord>();
    private static readonly Color SelectedColor = new ColorInt(135, 135, 135).ToColor;
    private static readonly Color UnselectedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    public static TabRecord DrawTabs<T>(Rect baseRect, List<T> tabs, int rows, float? maxTabWidth) where T : TabRecord
    {
        if (rows <= 1)
            return (TabRecord)TabDrawer.DrawTabs<T>(baseRect, tabs);
        int num1 = Mathf.FloorToInt((float)tabs.Count / (float)rows);
        int num2 = 0;
        TabRecord tabRecord1 = (TabRecord)null;
        Rect rect = baseRect;
        baseRect.yMin -= (float)(rows - 1) * 31f;
        Widgets.DrawMenuSection(baseRect with { yMax = rect.y });
        for (int index1 = 0; index1 < rows; ++index1)
        {
            int num3 = index1 == 0 ? tabs.Count - (rows - 1) * num1 : num1;
            Pmp_TabDrawer.tmpTabs.Clear();
            for (int index2 = num2; index2 < num2 + num3; ++index2)
                Pmp_TabDrawer.tmpTabs.Add((TabRecord)tabs[index2]);
            TabRecord tabRecord2 = Pmp_TabDrawer.DrawTabs<TabRecord>(baseRect, Pmp_TabDrawer.tmpTabs,
                maxTabWidth ?? baseRect.width);
            if (tabRecord2 != null)
                tabRecord1 = tabRecord2;
            baseRect.yMin += 31f;
            num2 += num3;
        }

        Pmp_TabDrawer.tmpTabs.Clear();
        return tabRecord1;
    }

    public static float GetOverflowTabHeight<T>(
        Rect baseRect,
        List<T> tabs,
        float minTabWidth,
        float maxTabWidth)
        where T : TabRecord
    {
        int num = Mathf.CeilToInt((float)tabs.Count * minTabWidth / baseRect.width);
        return num <= 1 ? 32f : 32f * (float)num - (float)num;
    }

    public static TabRecord DrawTabsOverflow<T>(
        Rect baseRect,
        List<T> tabs,
        float minTabWidth,
        float maxTabWidth)
        where T : TabRecord
    {
        int numRows = Mathf.CeilToInt((float)tabs.Count * minTabWidth / baseRect.width);
        if (numRows <= 1)
        {
            baseRect.y += 32f;
            T obj = DrawTabs<T>(baseRect, tabs, maxTabWidth);
            baseRect.yMax = baseRect.y;
            return (TabRecord)obj;
        }

        // 更均匀地分配标签到各行
        int totalTabs = tabs.Count;
        int currentIndex = 0;
        TabRecord selectedTab = null;

        for (int row = 0; row < numRows; row++)
        {
            // 计算当前行应该显示的标签数
            int tabsPerRow = Mathf.CeilToInt((float)(totalTabs - currentIndex) / (numRows - row));
            int currentRowTabs = Mathf.Min(tabsPerRow, totalTabs - currentIndex);

            if (currentRowTabs <= 0) break;

            // 创建当前行的标签列表
            tmpTabs.Clear();
            for (int i = 0; i < currentRowTabs && currentIndex < totalTabs; i++)
            {
                tmpTabs.Add((TabRecord)tabs[currentIndex]);
                currentIndex++;
            }

            // 绘制当前行
            Rect rowRect = new Rect(baseRect.x, baseRect.y + (row + 1) * 31f, baseRect.width, 32f);
            TabRecord rowResult = DrawTabs<TabRecord>(rowRect, tmpTabs, baseRect.width);
            if (rowResult != null)
                selectedTab = rowResult;
        }

        tmpTabs.Clear();
        return selectedTab;
    }

    public static TTabRecord DrawTabs<TTabRecord>(
        Rect baseRect,
        List<TTabRecord> tabs,
        float maxTabWidth = 200f)
        where TTabRecord : TabRecord
    {
        TTabRecord tabRecord1 = default(TTabRecord);
        TTabRecord tabRecord2 = tabs.Find((Predicate<TTabRecord>)(t => t.Selected));
        float tabWidth = (baseRect.width + (float)(tabs.Count - 1) * 10f) / (float)tabs.Count;
        if ((double)tabWidth > (double)maxTabWidth)
            tabWidth = maxTabWidth;
        Rect rect1 = new Rect(baseRect);
        rect1.y -= 32f;
        rect1.height = 9999f;
        Widgets.BeginGroup(rect1);
        Text.Anchor = TextAnchor.MiddleCenter;
        Text.Font = GameFont.Small;
        Func<TTabRecord, Rect> func = (Func<TTabRecord, Rect>)(tab =>
            new Rect((float)tabs.IndexOf(tab) * (tabWidth - 10f), 1f, tabWidth, 32f));
        List<TTabRecord> source = tabs.ListFullCopy<TTabRecord>();
        if ((object)tabRecord2 != null)
        {
            source.Remove(tabRecord2);
            source.Add(tabRecord2);
        }

        TabRecord tabRecord3 = (TabRecord)null;
        List<TTabRecord> tabRecordList = source.ListFullCopy<TTabRecord>();
        tabRecordList.Reverse();
        for (int index = 0; index < tabRecordList.Count; ++index)
        {
            TTabRecord tabRecord4 = tabRecordList[index];
            Rect rect2 = func(tabRecord4);
            if (tabRecord3 == null && Mouse.IsOver(rect2))
                tabRecord3 = (TabRecord)tabRecord4;
            MouseoverSounds.DoRegion(rect2, SoundDefOf.Mouseover_Tab);
            if (Mouse.IsOver(rect2) && !tabRecord4.GetTip().NullOrEmpty())
                TooltipHandler.TipRegion(rect2, (TipSignal)tabRecord4.GetTip());
            if (Widgets.ButtonInvisible(rect2))
                tabRecord1 = tabRecord4;
        }

        Color lastColor = GUI.color;
        foreach (TTabRecord tabRecord5 in source)
        {
            Rect rect3 = func(tabRecord5);
            if (tabRecord5.Selected)
            {
                Widgets.DrawMenuSection(rect3);
                GUI.color = SelectedColor;
                Widgets.DrawBox(rect3, 1);
                GUI.color = lastColor;
            }
            else
            {
                GUI.color = UnselectedColor;
                GUI.DrawTexture(rect3, (Texture) BaseContent.WhiteTex);
                GUI.color = lastColor;
            }
            Rect labelRect = rect3;
            labelRect.x += 10f;
            labelRect.width -= 20f;
            Widgets.Label(labelRect, tabRecord5.label);
        }

        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.EndGroup();
        if ((object)tabRecord1 != null && (object)tabRecord1 != (object)tabRecord2)
        {
            SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
            if (tabRecord1.clickedAction != null)
                tabRecord1.clickedAction();
        }

        return tabRecord1;
    }
}