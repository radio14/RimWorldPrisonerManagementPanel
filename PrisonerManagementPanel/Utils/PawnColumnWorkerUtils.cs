using UnityEngine;
using Verse;

namespace PrisonerManagementPanel.Utils;

public static class PawnColumnWorkerUtils
{
    /// <summary>
    /// 计算列的最小宽度，基于文本内容和边距
    /// </summary>
    /// <param name="textKey">翻译键</param>
    /// <param name="minWidth">最小宽度</param>
    /// <param name="padding">左右边距总和</param>
    /// <returns>计算后的最小宽度</returns>
    public static int CalculateMinWidth(string textKey, int minWidth = 30, float padding = 20f)
    {
        string text = textKey.Translate();
        Vector2 textSize = Text.CalcSize(text);
        int requiredWidth = Mathf.CeilToInt(textSize.x + padding);
        return Mathf.Max(minWidth, requiredWidth);
    }
}