using System;
using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Structure;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

// TODO 还有两个翻译 SurgeryPolicy_NonePolicy_Info SurgeryPolicy_ClearPolicy_Info

namespace PrisonerManagementPanel.Surgery
{
    [StaticConstructorOnStartup]
    public class PawnSurgeryPolicyStorage : GameComponent
    {
        public static PawnSurgeryPolicyStorage Instance { get; private set; }

        public static readonly IEnumerable<RecipeDef> AllRecipes =
            DefDatabase<RecipeDef>.AllDefs.Where(r => r.IsSurgery);

        public SurgeryPolicy NonePolicy = new SurgeryPolicy(1, "None".Translate());
        public SurgeryPolicy ClearPolicy = new SurgeryPolicy(2, "SurgeryPolicy_ClearPolicy".Translate());
        private SurgeryPolicy _defaultPolicy;

        // 存储所有 Pawn Id 和 SurgeryPolicy 对应关系
        private List<PawnSurgeryPolicyPair> _pawnPolicyPairs = new List<PawnSurgeryPolicyPair>();

        // 所有 SurgeryPolicy
        private List<SurgeryPolicy> _allSurgeryPolicy = new List<SurgeryPolicy>();

        // 记录每个 Policy => Pawn 列表
        [Unsaved] private Dictionary<SurgeryPolicy, HashSet<Pawn>> _policyToPawnsMap =
            new Dictionary<SurgeryPolicy, HashSet<Pawn>>();

        // 存储所有 Pawn => SurgeryPolicy 对应关系
        [Unsaved] private static Dictionary<Pawn, SurgeryPolicy> _pawnPolicies = new Dictionary<Pawn, SurgeryPolicy>();

        // 数据版本 2025/07/21: 1
        private int _dataVersion = 1;


        public PawnSurgeryPolicyStorage()
        {
        }

        public PawnSurgeryPolicyStorage(Game game) : base()
        {
            Instance = this;
            InitializeDefaultPolicies();
        }

        private void InitializeDefaultPolicies()
        {
            if (_allSurgeryPolicy.Count == 0)
            {
                _defaultPolicy = NonePolicy;
                _allSurgeryPolicy.Clear();
                _allSurgeryPolicy.Add(NonePolicy);
                _allSurgeryPolicy.Add(ClearPolicy);
            }
        }

        public override void FinalizeInit()
        {
            // Log.Message("加载中...");
            // Log.Message($"FinalizeInit共有 {_allSurgeryPolicy.Count} 个手术策略");
            _pawnPolicies.Clear();
            _policyToPawnsMap.Clear();

            // 获取所有囚犯
            List<Pawn> allPrisoners = Find.Maps
                .SelectMany(map => map.mapPawns.PrisonersOfColony)
                .ToList();
            // Log.Message($"共有 {allPrisoners.Count} 人");
            // Log.Message($"共有 {_pawnPolicyPairs.Count} 个 _pawnPolicyPairs 映射关系");

            // 创建一个字典用于快速查找
            Dictionary<int, SurgeryPolicy> pawnPolicyDict = _pawnPolicyPairs
                .ToDictionary(pair => pair.pawnID, pair => pair.SurgeryPolicy);

            // 遍历所有囚犯
            foreach (Pawn pawn in allPrisoners)
            {
                if (!ShouldApplyPolicy(pawn))
                {
                    continue;
                }

                if (pawnPolicyDict.TryGetValue(pawn.thingIDNumber, out SurgeryPolicy policy))
                {
                    // 如果有对应的策略，使用它
                    // Log.Message($"为 Pawn {pawn.thingIDNumber} 加载策略: {policy.label}");
                    _pawnPolicies[pawn] = policy;
                }
                else
                {
                    // 否则使用 NonePolicy
                    // Log.Message($"为 Pawn {pawn.thingIDNumber} 使用默认策略: None");
                    _pawnPolicies[pawn] = NonePolicy;
                }
            }

            RebuildPolicyToPawnsMap();

            // Log.Message("加载完成");
            // Log.Message($"共有 {_pawnPolicies.Count} 个 Pawn，共有 {_allSurgeryPolicy.Count} 个手术策略");
            // Log.Message("policy*---------");
            // foreach (var policy in _allSurgeryPolicy)
            // {
            //     Log.Message($"共有 {policy.label}");
            // }
            // Log.Message("policy*---------");
            // Log.Message("policy*--pawn-------");
            // foreach (var pawn in _pawnPolicies)
            // {
            //     Log.Message($"pawn --- {pawn.Key.Name.ToStringFull} -- policy {pawn.Value.label}");
            // }
            // Log.Message("policy*--pawn-------");
            // foreach (var policyToPawn in _policyToPawnsMap)
            // {
            //     Log.Message($" policy {policyToPawn.Key.label} -- pawns {policyToPawn.Value.Count} ");
            //     foreach (var pawn in policyToPawn.Value)
            //     {
            //         Log.Message($"pawn --- {pawn.Name.ToStringFull}");
            //     }
            // }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _pawnPolicyPairs.Clear();
                foreach (var pair in _pawnPolicies)
                {
                    _pawnPolicyPairs.Add(new PawnSurgeryPolicyPair
                    {
                        pawnID = pair.Key.thingIDNumber,
                        SurgeryPolicy = pair.Value
                    });
                }
            }

            Scribe_Values.Look(ref _dataVersion, "dataVersion", 1);
            Scribe_Collections.Look(ref _pawnPolicyPairs, "pawnPolicyPairs", LookMode.Deep);
            Scribe_Collections.Look(ref _allSurgeryPolicy, "allSurgeryPolicy", LookMode.Deep);
            Scribe_References.Look(ref _defaultPolicy, "defaultPolicy");
        }

        private void RebuildPolicyToPawnsMap()
        {
            _policyToPawnsMap.Clear();

            Dictionary<int, SurgeryPolicy> pawnPolicyDict = _pawnPolicyPairs
                .ToDictionary(pair => pair.pawnID, pair => pair.SurgeryPolicy);

            // 获取所有囚犯
            List<Pawn> allPrisoners = Find.Maps
                .SelectMany(map => map.mapPawns.PrisonersOfColony)
                .ToList();

            foreach (Pawn pawn in allPrisoners)
            {
                SurgeryPolicy policy = NonePolicy;
                if (pawnPolicyDict.TryGetValue(pawn.thingIDNumber, out SurgeryPolicy storedPolicy) &&
                    storedPolicy != null)
                {
                    policy = storedPolicy;
                }

                if (!_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawnsSet))
                {
                    pawnsSet = new HashSet<Pawn>();
                    _policyToPawnsMap[policy] = pawnsSet;
                }

                pawnsSet.Add(pawn);
            }
        }

        public SurgeryPolicy GetDefaultPolicy()
        {
            return _defaultPolicy;
        }

        // 获取所有的 SurgeryPolicy
        public List<SurgeryPolicy> GetAllSurgeryPolicy()
        {
            return _allSurgeryPolicy;
        }

        // 添加 SurgeryPolicy
        public void AddSurgeryPolicy(SurgeryPolicy policy)
        {
            _allSurgeryPolicy.Add(policy);
        }

        // 移除 SurgeryPolicy
        public void RemoveSurgeryPolicy(SurgeryPolicy policy)
        {
            _allSurgeryPolicy.Remove(policy);
        }

        // 设置 Pawn 的手术策略
        public void SetPolicyForPawn(Pawn pawn, SurgeryPolicy policy)
        {
            SurgeryPolicy selfPolicy = policy;
            if (IsNullPolicy(policy))
            {
                selfPolicy = NonePolicy;
            }

            if (_policyToPawnsMap == null)
            {
                _policyToPawnsMap = new Dictionary<SurgeryPolicy, HashSet<Pawn>>();
            }

            _pawnPolicies.TryGetValue(pawn, out SurgeryPolicy oldPolicy);
            if (oldPolicy != null && _policyToPawnsMap.TryGetValue(oldPolicy, out HashSet<Pawn> oldPawns))
            {
                oldPawns.Remove(pawn);
            }

            _pawnPolicies[pawn] = selfPolicy;

            if (!_policyToPawnsMap.TryGetValue(selfPolicy, out HashSet<Pawn> pawnsSet))
            {
                pawnsSet = new HashSet<Pawn>();
                _policyToPawnsMap[policy] = pawnsSet;
            }

            pawnsSet.Add(pawn);
        }

        // 获取 Pawn 的手术策略
        public SurgeryPolicy GetPolicyForPawn(Pawn pawn)
        {
            if (_pawnPolicies.TryGetValue(pawn, out SurgeryPolicy policy))
            {
                if (IsClearPolicy(policy))
                {
                    return ClearPolicy;
                }

                return policy;
            }

            return NonePolicy;
        }

        // 移除方法
        public void RemovePawn(Pawn pawn)
        {
            if (_pawnPolicies.ContainsKey(pawn))
            {
                SurgeryPolicy policy = _pawnPolicies[pawn];
                _pawnPolicies.Remove(pawn);

                if (_policyToPawnsMap != null && policy != null &&
                    _policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawnsSet))
                {
                    pawnsSet.Remove(pawn);
                }
            }
        }

        public IEnumerable<Pawn> GetAllPawns()
        {
            return _pawnPolicies.Keys.ToList();
        }

        public void SetDefaultSurgeryPolicy(SurgeryPolicy policy)
        {
            if (IsClearPolicy(policy))
            {
                return;
            }

            _defaultPolicy = policy;
        }

        // 给Pawn 应用默认的手术策略
        public void ApplyDefaultSurgeryPolicy(Pawn pawn)
        {
            SurgeryPolicy policy = _defaultPolicy;
            SetPawnSurgeryPolicy(pawn, policy);
        }

        public SurgeryPolicy GetPawnSurgeryPolicy(Pawn pawn)
        {
            if (pawn == null)
            {
                return NonePolicy;
            }

            return GetPolicyForPawn(pawn);
        }

        public void SetPawnSurgeryPolicy(Pawn pawn, SurgeryPolicy policy)
        {
            // 确保 _policyToPawnsMap 被初始化
            if (_policyToPawnsMap == null)
            {
                _policyToPawnsMap = new Dictionary<SurgeryPolicy, HashSet<Pawn>>();
            }

            // 如果 policy 是 null 则清空该 Pawn 关联的 policy
            if (IsNullPolicy(policy))
            {
                SetPolicyForPawn(pawn, NonePolicy);
                if (_policyToPawnsMap.TryGetValue(NonePolicy, out HashSet<Pawn> pawns))
                {
                    pawns.Add(pawn);
                }

                return;
            }

            // 检查是否已经是相同策略
            SurgeryPolicy currentPolicy = GetPolicyForPawn(pawn);

            if (currentPolicy == policy || (currentPolicy != null && currentPolicy.id == policy.id))
            {
                return;
            }

            if (IsClearPolicy(policy))
            {
                ClearSurgeryPolicyForPawn(pawn);
                return;
            }

            // 旧 Policy
            SurgeryPolicy oldPolicy = GetPolicyForPawn(pawn);
            if (!IsNonePolicy(oldPolicy))
            {
                if (_policyToPawnsMap.TryGetValue(oldPolicy, out HashSet<Pawn> oldPawns))
                {
                    oldPawns.Remove(pawn);
                }
            }

            SetPolicyForPawn(pawn, policy);

            if (!IsNonePolicy(policy))
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
        }

        public AcceptanceReport TryDeleteSurgeryPolicy(SurgeryPolicy policy)
        {
            if (IsNonePolicy(policy) || IsClearPolicy(policy))
            {
                return new AcceptanceReport("SurgeryPolicy_Delete_Report".Translate());
            }

            if (policy == _defaultPolicy)
            {
                return new AcceptanceReport("SurgeryPolicy_Delete_Default_Report".Translate());
            }

            // 检查该策略是否被 Pawn 使用
            if (_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
            {
                // 并且 获取 使用此策略的Pawn, 输出Pawn的名称
                string pawnNames = string.Join(", ", pawns.Select(p => p.Name));
                if (pawns.Count > 0)
                {
                    return new AcceptanceReport("SurgeryPolicy_WhenUseDelete_Report".Translate(pawnNames));
                }
            }

            RemoveSurgeryPolicy(policy);
            return true;
        }

        // 创建
        public SurgeryPolicy CreateSurgeryPolicy()
        {
            var policy = new SurgeryPolicy(GetAllSurgeryPolicy().Count + 1,
                $"{"SurgeryPolicy_TitleKey".Translate()}{GetAllSurgeryPolicy().Count + 1}");
            AddSurgeryPolicy(policy);
            return policy;
        }

        public void UpdatePawnMedicalBills(Pawn pawn, SurgeryPolicy policy)
        {
            if (!ShouldApplyPolicy(pawn))
            {
                return;
            }

            BillStack billStack = pawn?.health?.surgeryBills;
            if (billStack == null || IsNonePolicy(policy))
            {
                return;
            }

            List<RecipeFilterItem> items = policy.RecipeFilter.AllowedItems.ToList();

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

                if (item.Recipe.targetsBodyPart)
                {
                    if (item.SelectedParts != null && item.SelectedParts.Count > 0)
                    {
                        foreach (BodyPartRecord part in item.SelectedParts)
                        {
                            HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, part);
                        }
                    }
                    else
                    {
                        foreach (BodyPartRecord part in item.Recipe.Worker.GetPartsToApplyOn(pawn, item.Recipe))
                        {
                            if (item.Recipe.AvailableOnNow((Thing)pawn, part))
                            {
                                HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, part);
                            }
                        }
                    }
                }
                else
                {
                    HealthCardUtility.CreateSurgeryBill(pawn, item.Recipe, null);
                }
            }
        }

        public void UpdatePawnsWithPolicy(SurgeryPolicy policy)
        {
            if (policy == null || !_policyToPawnsMap.TryGetValue(policy, out HashSet<Pawn> pawns))
                return;

            foreach (Pawn pawn in pawns)
            {
                UpdatePawnMedicalBills(pawn, policy);
            }
        }

        public void RefreshDirtyPolicies()
        {
            // 遍历 Pawn 检查是否为 囚犯 且 活着
            foreach (Pawn pawn in GetAllPawns())
            {
                if (!ShouldApplyPolicy(pawn))
                {
                    RemovePawnFromStorage(pawn);
                }
            }

            // 检查 DefaultPolicy 和 ClearPolicy
            NonePolicy.RenamableLabel = "None".Translate();
            ClearPolicy.RenamableLabel = "SurgeryPolicy_ClearPolicy".Translate();

            var changedPolicies = GetAllSurgeryPolicy().Where(p => p.IsDirty).ToList();

            foreach (var policy in changedPolicies)
            {
                UpdatePawnsWithPolicy(policy);
                policy.ClearDirty();
            }
        }

        // 清空某个 Pawn 的手术列表
        public void ClearSurgeryPolicyForPawn(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            SetPolicyForPawn(pawn, ClearPolicy);

            pawn.health.surgeryBills?.Clear();
        }


        // 移除 Pawn
        public void RemovePawnFromStorage(Pawn pawn)
        {
            if (pawn == null) return;

            // 1. 从策略存储中移除
            RemovePawn(pawn);

            pawn.health.surgeryBills?.Clear();
        }

        // 是否是 无
        public bool IsNonePolicy(SurgeryPolicy policy)
        {
            return policy == NonePolicy || policy == null || policy.id == NonePolicy.id;
        }

        // 是否是 null
        public bool IsNullPolicy(SurgeryPolicy policy)
        {
            return policy == null;
        }

        // 是否是清空所有手术
        public bool IsClearPolicy(SurgeryPolicy policy)
        {
            return policy.id == ClearPolicy.id;
        }

        // 是否可以应用
        public bool ShouldApplyPolicy(Pawn pawn)
        {
            if (pawn == null || !Find.Maps.Any(map => map.mapPawns.PrisonersOfColony.Contains(pawn)))
            {
                return false;
            }

            return pawn.IsPrisonerOfColony && !pawn.Dead && !pawn.Destroyed;
        }
    }
}