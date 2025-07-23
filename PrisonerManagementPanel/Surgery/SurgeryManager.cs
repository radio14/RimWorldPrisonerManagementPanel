using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Structure;
using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonerManagementPanel.Surgery;

// [StaticConstructorOnStartup]
public static class SurgeryManager
{
    // private static List<SurgeryPolicy> AllSurgeryPolicy = new List<SurgeryPolicy>();

    // public static SurgeryPolicy NonePolicy = new SurgeryPolicy(1, "None".Translate(), new List<RecipeDef>());
    // public static SurgeryPolicy DefaultPolicy;
    // public static readonly SurgeryPolicy ClearPolicy = new SurgeryPolicy(2, "清空所有手术", new List<RecipeDef>());
    // public static readonly IEnumerable<RecipeDef> AllRecipes = DefDatabase<RecipeDef>.AllDefs.Where(r => r.IsSurgery);

    // 存储 Pawn 和 Policy 的绑定关系
    // private static PawnSurgeryPolicyStorage _pawnPolicyStorage = new PawnSurgeryPolicyStorage();

    // 记录每个 Policy 关联的 Pawn 列表
    // private static Dictionary<SurgeryPolicy, HashSet<Pawn>> _policyToPawnsMap =
        // new Dictionary<SurgeryPolicy, HashSet<Pawn>>();

    // static SurgeryManager()
    // {
    //     Init();
    // }

    // public static void Init()
    // {
    //     if (DefaultPolicy == null && AllSurgeryPolicy.Count == 0)
    //     {
    //         DefaultPolicy = NonePolicy;
    //         AllSurgeryPolicy.Add(DefaultPolicy);
    //         AllSurgeryPolicy.Add(ClearPolicy);
    //     }
    // }

    // public static List<SurgeryPolicy> GetAllSurgeryPolicy()
    // {
    //     return PawnSurgeryPolicyStorage.GetAllSurgeryPolicy();
    // }

    // public static void SetDefaultSurgeryPolicy(SurgeryPolicy policy)
    // {
    //     if (IsClearPolicy(policy))
    //     {
    //         return;
    //     }
    //
    //     DefaultPolicy = policy;
    // }

    // public static void SetPawnSurgeryPolicy(Pawn pawn, SurgeryPolicy policy)
    // {
    //     // 确保 _policyToPawnsMap 被初始化
    //     if (_policyToPawnsMap == null)
    //     {
    //         _policyToPawnsMap = new Dictionary<SurgeryPolicy, HashSet<Pawn>>();
    //     }
    //
    //     if (IsClearPolicy(policy))
    //     {
    //         ClearSurgeryPolicyForPawn(pawn);
    //         return;
    //     }
    //
    //     // 如果 policy 是 null 则清空该 Pawn 关联的 policy
    //     if (IsNonePolicy(policy))
    //     {
    //         _pawnPolicyStorage.SetPolicyForPawn(pawn, null);
    //         if (_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
    //         {
    //             pawns.Remove(pawn);
    //         }
    //
    //         return;
    //     }
    //
    //     // 检查是否已经是相同策略
    //     SurgeryPolicy currentPolicy = _pawnPolicyStorage.GetPolicyForPawn(pawn);
    //     if (currentPolicy == policy || (currentPolicy != null && currentPolicy.Equals(policy)))
    //     {
    //         return;
    //     }
    //
    //     // 旧 Policy
    //     SurgeryPolicy oldPolicy = _pawnPolicyStorage.GetPolicyForPawn(pawn);
    //     if (!IsNonePolicy(oldPolicy))
    //     {
    //         if (_policyToPawnsMap.TryGetValue(oldPolicy, out HashSet<Pawn> oldPawns))
    //         {
    //             oldPawns.Remove(pawn);
    //         }
    //     }
    //
    //     _pawnPolicyStorage.SetPolicyForPawn(pawn, policy);
    //
    //     if (!IsNonePolicy(policy))
    //     {
    //         if (!_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
    //         {
    //             pawns = new HashSet<Pawn>();
    //             _policyToPawnsMap[policy] = pawns;
    //         }
    //
    //         pawns.Add(pawn);
    //     }
    //
    //     // 自动更新手术任务
    //     UpdatePawnMedicalBills(pawn, policy);
    // }

    // public static void SyncAllPolicies()
    // {
    //     // 确保所有策略都被正确注册
    //     foreach (var policy in PawnSurgeryPolicyStorage.GetAllSurgeryPolicy())
    //     {
    //         if (IsNonePolicy(policy)) continue;
    //
    //         if (!_policyToPawnsMap.ContainsKey(policy))
    //         {
    //             _policyToPawnsMap[policy] = new HashSet<Pawn>();
    //         }
    //     }
    //
    //     // 同步默认策略
    //     if (DefaultPolicy != null && !_policyToPawnsMap.ContainsKey(DefaultPolicy))
    //     {
    //         _policyToPawnsMap[DefaultPolicy] = new HashSet<Pawn>();
    //     }
    //
    //     // 从存储中同步所有pawn
    //     // foreach (var pawn in _pawnPolicyStorage.GetAllPawns())
    //     // {
    //     //     var policy = _pawnPolicyStorage.GetPolicyForPawn(pawn);
    //     //     if (policy != null && _policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
    //     //     {
    //     //         pawns.Add(pawn);
    //     //     }
    //     // }
    // }

    // public static SurgeryPolicy GetPawnSurgeryPolicy(Pawn pawn)
    // {
    //     if (pawn == null)
    //     {
    //         return null;
    //     }
    //
    //     SurgeryPolicy p = _pawnPolicyStorage.GetPolicyForPawn(pawn);
    //     return _pawnPolicyStorage.GetPolicyForPawn(pawn);
    // }

    // public static AcceptanceReport TryDeleteSurgeryPolicy(SurgeryPolicy policy)
    // {
    //     if (IsNonePolicy(policy) || IsClearPolicy(policy))
    //     {
    //         return new AcceptanceReport("不能删除系统预设的手术方案");
    //     }
    //
    //     if (policy == DefaultPolicy)
    //     {
    //         return new AcceptanceReport("不能删除默认手术方案");
    //     }
    //
    //     // 检查该策略是否被 Pawn 使用
    //     if (_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
    //     {
    //         // 并且 获取 使用此策略的Pawn, 输出Pawn的名称
    //         string pawnNames = string.Join(", ", pawns.Select(p => p.Name));
    //         if (pawns.Count > 0)
    //         {
    //             return new AcceptanceReport("该手术方案正在使用中，请先将所有使用此方案的Pawn移除: {0}".Formatted(pawnNames));
    //         }
    //     }
    //
    //     PawnSurgeryPolicyStorage.RemoveSurgeryPolicy(policy);
    //     return true;
    // }

    // 创建
    // public static SurgeryPolicy CreateSurgeryPolicy()
    // {
    //     var policy = new SurgeryPolicy(PawnSurgeryPolicyStorage.GetAllSurgeryPolicy().Count + 1, $"手术方案{PawnSurgeryPolicyStorage.GetAllSurgeryPolicy().Count + 1}",
    //         new List<RecipeDef>());
    //     PawnSurgeryPolicyStorage.AddSurgeryPolicy(policy);
    //     return policy;
    // }

    // 保存数据
    // public static void ExposeData()
    // {
    //     // _pawnPolicyStorage.ExposeData();
    //     // Scribe_Collections.Look(ref AllSurgeryPolicy, "AllSurgeryPolicy", LookMode.Reference, LookMode.Reference);
    //     // Scribe_References.Look(ref DefaultPolicy, "DefaultPolicy");
    //     // Scribe_Collections.Look(ref _policyToPawnsMap, "policyToPawnsMap", LookMode.Reference, LookMode.Reference);
    // }

    // public static void UpdatePawnMedicalBills(Pawn pawn, SurgeryPolicy policy)
    // {
    //     if (!ShouldApplyPolicy(pawn))
    //     {
    //         return;
    //     }
    //
    //     BillStack billStack = pawn?.health?.surgeryBills;
    //     if (billStack == null || IsNonePolicy(policy))
    //     {
    //         return;
    //     }
    //
    //     List<RecipeFilterItem> items = policy.RecipeFilter.AllowedItems.ToList();
    //
    //     if (policy.RecipeFilter.ApplyMode == SurgeryApplyMode.ReplaceAll)
    //     {
    //         // 清空所有现有 Bill
    //         billStack.Clear();
    //     }
    //
    //     // 添加新的 Recipe（根据当前模式）
    //     foreach (RecipeFilterItem item in items)
    //     {
    //         if (policy.RecipeFilter.ApplyMode == SurgeryApplyMode.PartialUpdate &&
    //             billStack.Bills.Any(b => b is Bill_Medical med && med.recipe == item.Recipe))
    //         {
    //             continue; // 已存在，跳过
    //         }
    //
    //         if (item.Recipe.targetsBodyPart)
    //         {
    //             if (item.SelectedParts != null && item.SelectedParts.Count > 0)
    //             {
    //                 foreach (BodyPartRecord part in item.SelectedParts)
    //                 {
    //                     HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, part);
    //                 }
    //             }
    //             else
    //             {
    //                 foreach (BodyPartRecord part in item.Recipe.Worker.GetPartsToApplyOn(pawn, item.Recipe))
    //                 {
    //                     if (item.Recipe.AvailableOnNow((Thing)pawn, part))
    //                     {
    //                         HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, part);
    //                     }
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, null);
    //         }
    //     }
    // }

    // public static void UpdateAllPawnMedicalBills()
    // {
    //     foreach (Pawn pawn in _pawnPolicyStorage.GetAllPawnsWithPolicy())
    //     {
    //         SurgeryPolicy policy = GetPawnSurgeryPolicy(pawn);
    //         if (policy != null)
    //         {
    //             UpdatePawnMedicalBills(pawn, policy);
    //         }
    //     }
    // }

    // public static void UpdatePawnsWithPolicy(SurgeryPolicy policy)
    // {
    //     if (policy == null || !_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
    //         return;
    //
    //     foreach (Pawn pawn in pawns)
    //     {
    //         UpdatePawnMedicalBills(pawn, policy);
    //     }
    // }

    // 只更新发生变更的 Policy
    // public static void RefreshDirtyPolicies()
    // {
    //     // 遍历 Pawn 检查是否为 囚犯 且 活着
    //     foreach (Pawn pawn in _pawnPolicyStorage.GetAllPawns())
    //     {
    //         if (!ShouldApplyPolicy(pawn))
    //         {
    //             RemovePawnFromStorage(pawn);
    //         }
    //     }
    //
    //     // 检查 DefaultPolicy 和 ClearPolicy
    //     DefaultPolicy.RenamableLabel = "None".Translate();
    //     ClearPolicy.RenamableLabel = "清空所有手术";
    //
    //     var changedPolicies = PawnSurgeryPolicyStorage.GetAllSurgeryPolicy().Where(p => p.IsDirty).ToList();
    //
    //     foreach (var policy in changedPolicies)
    //     {
    //         UpdatePawnsWithPolicy(policy);
    //         policy.ClearDirty();
    //     }
    // }

    // 清空某个 Pawn 的手术列表
    // public static void ClearSurgeryPolicyForPawn(Pawn pawn)
    // {
    //     if (pawn == null)
    //     {
    //         return;
    //     }
    //
    //     _pawnPolicyStorage.SetPolicyForPawn(pawn, ClearPolicy);
    //
    //     if (!_policyToPawnsMap.ContainsKey(ClearPolicy))
    //     {
    //         _policyToPawnsMap[ClearPolicy] = new HashSet<Pawn>();
    //     }
    //
    //     foreach (var policy in _policyToPawnsMap.Keys.ToList())
    //     {
    //         if (policy != ClearPolicy && _policyToPawnsMap[policy].Contains(pawn))
    //         {
    //             _policyToPawnsMap[policy].Remove(pawn);
    //         }
    //     }
    //
    //     _policyToPawnsMap[ClearPolicy].Add(pawn);
    //
    //     pawn.health.surgeryBills?.Clear();
    // }

    // 移除 Pawn
    // public static void RemovePawnFromStorage(Pawn pawn)
    // {
    //     if (pawn == null) return;
    //
    //     // 1. 从策略存储中移除
    //     _pawnPolicyStorage.RemovePawn(pawn);
    //
    //     foreach (var policy in _policyToPawnsMap.Keys.ToList())
    //     {
    //         if (_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
    //         {
    //             pawns.Remove(pawn);
    //         }
    //     }
    //
    //     pawn.health.surgeryBills?.Clear();
    // }

    // public static bool ShouldApplyPolicy(Pawn pawn)
    // {
    //     // || !Find.CurrentMap.mapPawns.PrisonersOfColony.Contains(pawn)
    //     if (pawn == null || !Find.Maps.Any(map => map.mapPawns.PrisonersOfColony.Contains(pawn)))
    //     {
    //         return false;
    //     }
    //
    //     return pawn.IsPrisonerOfColony && !pawn.Dead && !pawn.Destroyed;
    // }

    // public static void ApplyDefaultSurgeryPolicy(Pawn pawn)
    // {
    //     SurgeryPolicy policy = DefaultPolicy;
    //     SetPawnSurgeryPolicy(pawn, policy);
    // }

    // public static bool IsNonePolicy(SurgeryPolicy policy)
    // {
    //     return policy == NonePolicy || policy == null || policy.id == NonePolicy.id;
    // }

    // public static bool IsClearPolicy(SurgeryPolicy policy)
    // {
    //     return policy.id == ClearPolicy.id;
    // }
}