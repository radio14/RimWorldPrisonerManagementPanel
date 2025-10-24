using System;
using PrisonerManagementPanel.Surgery;
using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;
using Verse.Sound;

namespace PrisonerManagementPanel.ColumnWorker
{
    // 囚犯手术列表
    public class PawnColumnWorker_Surgery : PawnColumnWorker
    {
        public override int GetMinWidth(PawnTable table) => Mathf.Max(base.GetMinWidth(table), 180);

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            MouseoverSounds.DoRegion(rect);
            if (!Widgets.ButtonText(new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f), (string) "ManageSurgeryPolicies".Translate()))
                return;
            Find.WindowStack.Add((Window) new Dialog_ManageSurgeryPolicies((SurgeryPolicy) null));
        }
        
        // 绘制单元格内容
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null) return;

            if (pawn.Dead || !pawn.RaceProps.Humanlike)
            {
                return;
            }

            this.DoAssignSurgeryButtons(rect, pawn);
        }

        private IEnumerable<Widgets.DropdownMenuElement<SurgeryPolicy>> GenerateDropdownMenu(Pawn pawn)
        {
            foreach (var policy in PawnSurgeryPolicyStorage.Instance.GetAllSurgeryPolicy())
            {
                if (policy == null) continue;

                // 强制使用全局 ClearPolicy 引用
                SurgeryPolicy actualPolicy = policy;
                if (PawnSurgeryPolicyStorage.Instance.IsClearPolicy(policy))
                {
                    actualPolicy = PawnSurgeryPolicyStorage.Instance.ClearPolicy;
                }

                yield return new Widgets.DropdownMenuElement<SurgeryPolicy>
                {
                    option = new FloatMenuOption(
                        actualPolicy.label,
                        () => PawnSurgeryPolicyStorage.Instance.SetPawnSurgeryPolicy(pawn, actualPolicy)
                    ),
                    payload = actualPolicy
                };
            }

            SurgeryPolicy policy2 = PawnSurgeryPolicyStorage.Instance.GetPawnSurgeryPolicy(pawn);
            // “编辑...” 选项
            yield return new Widgets.DropdownMenuElement<SurgeryPolicy>
            {
                option = new FloatMenuOption($"{"AssignTabEdit".Translate()}...",
                    () =>
                    {
                        Find.WindowStack.Add(
                            new Dialog_ManageSurgeryPolicies(PawnSurgeryPolicyStorage.Instance.IsNonePolicy(policy2)
                                ? PawnSurgeryPolicyStorage.Instance.NonePolicy
                                : policy2));
                    }),
                payload = policy2
            };
        }

        private void DoAssignSurgeryButtons(Rect rect, Pawn pawn)
        {
            Rect rect1 = rect.ContractedBy(0.0f, 2f);
            SurgeryPolicy policy = PawnSurgeryPolicyStorage.Instance.GetPawnSurgeryPolicy(pawn);
            Widgets.Dropdown<Pawn, SurgeryPolicy>(
                rect1,
                pawn,
                // Getter：获取当前pawn的policy
                (Func<Pawn, SurgeryPolicy>)(p => PawnSurgeryPolicyStorage.Instance.GetPawnSurgeryPolicy(p)),
                // 菜单生成器
                (Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<SurgeryPolicy>>>)(GenerateDropdownMenu),
                // 显示文本（截断处理）
                policy.label.Truncate(rect1.width),
                // 拖拽标签（可选）
                dragLabel: policy.label,
                // 可绘画样式
                paintable: true
            );
        }
    }
}