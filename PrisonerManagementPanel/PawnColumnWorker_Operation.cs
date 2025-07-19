using System;
using PrisonerManagementPanel.Operation;
using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;

namespace PrisonerManagementPanel
{
    // 囚犯手术列表
    public class PawnColumnWorker_Operation : PawnColumnWorker
    {
        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 160);

        // 绘制单元格内容
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Rect rect1 = rect.ContractedBy(0.0f, 2f); // 留点边距

            Widgets.Dropdown<Pawn, SurgeryPolicy>(
                rect1,
                pawn,
                // Getter：获取当前pawn的operation plan
                (Func<Pawn, SurgeryPolicy>)(p => SurgeryManager.GetPawnSurgeryPolicy(p)),
                // 菜单生成器
                (Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<SurgeryPolicy>>>)(GenerateDropdownMenu),
                // 显示文本（截断处理）
                SurgeryManager.GetPawnSurgeryPolicy(pawn)?.label.Truncate(rect1.width) ?? "None".Translate(),
                // 拖拽标签（可选）
                dragLabel: SurgeryManager.GetPawnSurgeryPolicy(pawn)?.label ?? "None".Translate(),
                // 可绘画样式
                paintable: true
            );
        }

        private IEnumerable<Widgets.DropdownMenuElement<SurgeryPolicy>> GenerateDropdownMenu(Pawn pawn)
        {
            foreach (var plan in SurgeryManager.AllSurgeryPolicy)
            {
                SurgeryPolicy selectedPlan = plan; // 避免闭包捕获问题
                yield return new Widgets.DropdownMenuElement<SurgeryPolicy>
                {
                    option = new FloatMenuOption(
                        selectedPlan.label,
                        () => SurgeryManager.SetPawnSurgeryPolicy(pawn, selectedPlan)
                    ),
                    payload = selectedPlan
                };
            }

            // “无”选项
            yield return new Widgets.DropdownMenuElement<SurgeryPolicy>
            {
                option = new FloatMenuOption("None".Translate(), () => SurgeryManager.SetPawnSurgeryPolicy(pawn, null)),
                payload = null
            };

            // “编辑...” 选项
            yield return new Widgets.DropdownMenuElement<SurgeryPolicy>
            {
                option = new FloatMenuOption($"{"AssignTabEdit".Translate()}...",
                    () =>
                    {
                        Find.WindowStack.Add(
                            new Dialog_ManageSurgeryPolicies(SurgeryManager.GetPawnSurgeryPolicy(pawn)));
                    }),
                payload = null
            };
        }

        private string GetLabelFor(Pawn pawn)
        {
            // 根据pawn获取当前设置的Operation方案名称
            // 示例逻辑：你可以自己用Comp或静态字典存储
            return "Default"; // 示例
        }
    }
}