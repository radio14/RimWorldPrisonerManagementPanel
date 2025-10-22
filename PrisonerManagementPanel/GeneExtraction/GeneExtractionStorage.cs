using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace PrisonerManagementPanel.GeneExtraction;

[StaticConstructorOnStartup]
public class GeneExtractionStorage : GameComponent
{
    public static GeneExtractionStorage Instance { get; private set; }
    private int _dataVersion = 1;
    private List<GeneDef> selectedGenes = new List<GeneDef>();

    public GeneExtractionStorage()
    {
    }

    public GeneExtractionStorage(Game game) : base()
    {
        Instance = this;
        Init();
    }

    private void Init()
    {
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref _dataVersion, "dataVersion", 1);
        Scribe_Collections.Look(ref selectedGenes, "selectedGenes", LookMode.Def);
    }
    
    public List<GeneDef> GetSelectedGenes()
    {
        return selectedGenes;
    }
    
    public void SetSelectedGenes(List<GeneDef> genes)
    {
        selectedGenes = genes;
    }
}