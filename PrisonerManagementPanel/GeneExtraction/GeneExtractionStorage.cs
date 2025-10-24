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
    private bool isOpen = false;

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
        Scribe_Values.Look(ref isOpen, "isOpen", false);
        Scribe_Collections.Look(ref selectedGenes, "selectedGenes", LookMode.Def);
    }
    
    public List<GeneDef> GetSelectedGenes()
    {
        return selectedGenes;
    }
    
    public void SetSelectedGenes(List<GeneDef> genes)
    {
        selectedGenes = genes;
        GeneAllocator.ExecuteGeneExtraction();
    }
    
    public void Open()
    {
        isOpen = true;
        GeneAllocator.ExecuteGeneExtraction();
    }
    
    public void Close()
    {
        isOpen = false;
    }
    
    public bool IsOpen()
    {
        return isOpen;
    }
}