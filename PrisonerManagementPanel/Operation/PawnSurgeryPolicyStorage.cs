using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace PrisonerManagementPanel.Operation
{
    // 存储 Pawn 的手术方案
    public class PawnSurgeryPolicyStorage : IExposable
    {
        private Dictionary<Pawn, SurgeryPolicy> _pawnPolicies = new Dictionary<Pawn, SurgeryPolicy>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _pawnPolicies, "pawnPolicies", LookMode.Reference, LookMode.Reference);
        }

        // 设置 Pawn 的手术策略
        public void SetPolicyForPawn(Pawn pawn, SurgeryPolicy policy)
        {
            _pawnPolicies[pawn] = policy;
        }

        // 获取 Pawn 的手术策略
        public SurgeryPolicy GetPolicyForPawn(Pawn pawn)
        {
            if (_pawnPolicies.TryGetValue(pawn, out SurgeryPolicy policy))
            {
                return policy;
            }
            return null;
        }

        public IEnumerable<Pawn> GetAllPawns()
        {
            return _pawnPolicies.Keys.ToList();
        }
        
        public IEnumerable<Pawn> GetAllPawnsWithPolicy()
        {
            return _pawnPolicies.Keys.Where(p => _pawnPolicies[p] != null).ToList();
        }
    }
}