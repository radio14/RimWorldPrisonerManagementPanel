using System;
using PrisonerManagementPanel.Surgery;
using Verse;

namespace PrisonerManagementPanel.Structure;

[Serializable]
public class PawnSurgeryPolicyPair : IExposable
{
    public int pawnID;
    public SurgeryPolicy SurgeryPolicy;

    public void ExposeData()
    {
        Scribe_Values.Look(ref pawnID, "pawnID");
        Scribe_References.Look(ref SurgeryPolicy, "SurgeryPolicy");
    }
}