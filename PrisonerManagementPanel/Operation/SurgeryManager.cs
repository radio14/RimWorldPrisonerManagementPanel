using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Structure;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.Operation;

public static class SurgeryManager
{
    public static List<SurgeryPolicy> AllSurgeryPolicy = new List<SurgeryPolicy>();

    public static SurgeryPolicy DefaultPlan;
    public static readonly IEnumerable<RecipeDef> AllRecipes = DefDatabase<RecipeDef>.AllDefs.Where(r => r.IsSurgery);

    // 存储 Pawn 和 Policy 的绑定关系
    private static PawnSurgeryPolicyStorage _pawnPolicyStorage = new PawnSurgeryPolicyStorage();

    // 记录每个 Policy 关联的 Pawn 列表
    private static Dictionary<SurgeryPolicy, HashSet<Pawn>> _policyToPawnsMap =
        new Dictionary<SurgeryPolicy, HashSet<Pawn>>();

    public static void Init()
    {
        // 初始化默认数据
        var defaultPlan = new SurgeryPolicy(1, "默认手术方案", new List<RecipeDef>());
        DefaultPlan = defaultPlan;
    }

    public static void SetDefaultSurgeryPolicy(SurgeryPolicy policy)
    {
        Log.Message($"设置默认手术方案为：{policy.label ?? "None"}");
        DefaultPlan = policy;
    }

    public static void SetPawnSurgeryPolicy(Pawn pawn, SurgeryPolicy policy)
    {
        // 确保 _policyToPawnsMap 被初始化
        if (_policyToPawnsMap == null)
        {
            _policyToPawnsMap = new Dictionary<SurgeryPolicy, HashSet<Pawn>>();
        }

        // 旧 Policy
        // SurgeryPolicy oldPolicy = _pawnPolicyStorage.GetPolicyForPawn(pawn);
        // if (oldPolicy != null)
        // {
        //     _policyToPawnsMap[oldPolicy].Remove(pawn);
        // }

        // 旧 Policy
        SurgeryPolicy oldPolicy = _pawnPolicyStorage.GetPolicyForPawn(pawn);
        if (oldPolicy != null)
        {
            if (_policyToPawnsMap.TryGetValue(oldPolicy, out HashSet<Pawn> oldPawns))
            {
                oldPawns.Remove(pawn);
            }
        }

        _pawnPolicyStorage.SetPolicyForPawn(pawn, policy);

        if (policy != null)
        {
            if (!_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
            {
                pawns = new HashSet<Pawn>();
                _policyToPawnsMap[policy] = pawns;
            }

            pawns.Add(pawn);
        }

        // 自动更新手术任务
        UpdatePawnMedicalBills(pawn, policy);
        Log.Message($"{pawn.Name} assigned to {policy.label ?? "None"} 手术数量 {0}");
    }

    public static void SyncAllPolicies()
    {
        // 确保所有策略都被正确注册
        foreach (var policy in AllSurgeryPolicy)
        {
            if (policy == null) continue; // 添加空值检查
        
            if (!_policyToPawnsMap.ContainsKey(policy))
            {
                _policyToPawnsMap[policy] = new HashSet<Pawn>();
            }
        }

        // 同步默认策略（添加空值检查）
        if (DefaultPlan != null && !_policyToPawnsMap.ContainsKey(DefaultPlan))
        {
            _policyToPawnsMap[DefaultPlan] = new HashSet<Pawn>();
        }

        // 从存储中同步所有pawn
        foreach (var pawn in _pawnPolicyStorage.GetAllPawns())
        {
            var policy = _pawnPolicyStorage.GetPolicyForPawn(pawn);
            if (policy != null && _policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
            {
                pawns.Add(pawn);
            }
        }
    }

    public static SurgeryPolicy GetPawnSurgeryPolicy(Pawn pawn)
    {
        return _pawnPolicyStorage.GetPolicyForPawn(pawn);
    }

    public static AcceptanceReport TryDeleteSurgeryPolicy(SurgeryPolicy policy)
    {
        if (policy == DefaultPlan)
        {
            return new AcceptanceReport("不能删除默认手术方案");
        }

        AllSurgeryPolicy.Remove(policy);
        return true;
    }

    // 创建
    public static SurgeryPolicy CreateSurgeryPolicy(string label)
    {
        var policy = new SurgeryPolicy(AllSurgeryPolicy.Count + 1, label, new List<RecipeDef>());
        AllSurgeryPolicy.Add(policy);
        return policy;
    }

    // 保存数据
    public static void ExposeData()
    {
        _pawnPolicyStorage.ExposeData();
    }

    public static void UpdatePawnMedicalBills(Pawn pawn, SurgeryPolicy policy)
    {
        BillStack billStack = pawn.health.surgeryBills;
        if (billStack == null || policy == null)
            return;

        List<RecipeFilterItem> items = policy.RecipeFilter.AllowedItems.ToList();

        // BillStack billStack = pawn.health.surgeryBills;
        // if (billStack == null || policy == null)
        // return;

        // List<RecipeDef> newRecipes = policy.RecipeFilter.AllowedRecipes.ToList();

        if (policy.RecipeFilter.ApplyMode == SurgeryApplyMode.ReplaceAll)
        {
            // 清空所有现有 Bill
            billStack.Clear();
        }

        // 添加新的 Recipe（根据当前模式）
        foreach (RecipeFilterItem item in items)
        {
            if (policy.RecipeFilter.ApplyMode == SurgeryApplyMode.PartialUpdate &&
                billStack.Bills.Any(b => b is Bill_Medical med && med.recipe == item.Recipe))
            {
                continue; // 已存在，跳过
            }

            if (item.Recipe.defName == "RemoveBodyPart")
            {
                if (item.SelectedParts != null && item.SelectedParts.Count > 0)
                {
                    foreach (BodyPartRecord part in item.SelectedParts)
                    {
                        HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, part);
                    }
                }
            }
            else
            {
                HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, null);
            }
        }
    }

    public static void UpdateAllPawnMedicalBills()
    {
        foreach (Pawn pawn in _pawnPolicyStorage.GetAllPawnsWithPolicy())
        {
            SurgeryPolicy policy = GetPawnSurgeryPolicy(pawn);
            if (policy != null)
            {
                UpdatePawnMedicalBills(pawn, policy);
            }
        }
    }

    public static void UpdatePawnsWithPolicy(SurgeryPolicy policy)
    {
        if (policy == null || !_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
            return;

        foreach (Pawn pawn in pawns)
        {
            UpdatePawnMedicalBills(pawn, policy);
        }
    }

    // 只更新发生变更的 Policy
    public static void RefreshDirtyPolicies()
    {
        var changedPolicies = AllSurgeryPolicy.Where(p => p.IsDirty).ToList();

        foreach (var policy in changedPolicies)
        {
            UpdatePawnsWithPolicy(policy);
            policy.ClearDirty();
        }
    }
}