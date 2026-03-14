using System.Collections.Generic;
using PrisonerManagementPanel.Utils;
using RimWorld;
using Verse;

namespace PrisonerManagementPanel.PrisonerInteraction;

public class RaceGenderInteractionMode
{
    public ThingDef Race;
    public Gender Gender;
    public PrisonerInteractionModeDef Mode;
}

[StaticConstructorOnStartup]
public class PrisonerInteractionSettingStorage : GameComponent
{
    public static PrisonerInteractionSettingStorage Instance { get; private set; }

    // 版本号
    private int _dataVersion = 1;
    // 当前模式
    private PrisonerInteractionSettingMode _currentMode = PrisonerInteractionSettingMode.EqualTreatment;
    
    // 默认互动模式(一视同仁)
    private PrisonerInteractionModeDef _defaultInteractionMode;
    
    // 分门别类互动模式
    private Dictionary<ThingDef, PrisonerInteractionModeDef> _raceInteractionModes = new Dictionary<ThingDef, PrisonerInteractionModeDef>();

    // 因性制宜互动模式
    private Dictionary<Gender, PrisonerInteractionModeDef> _genderInteractionModes = new Dictionary<Gender, PrisonerInteractionModeDef>();

    // 因族因性互动模式
    private List<RaceGenderInteractionMode> _raceGenderInteractionModesList = new List<RaceGenderInteractionMode>();

    public PrisonerInteractionSettingStorage()
    {
    }

    public PrisonerInteractionSettingStorage(Game game) : base()
    {
        Instance = this;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref _dataVersion, "dataVersion", 1);
        Scribe_Values.Look(ref _currentMode, "currentMode", PrisonerInteractionSettingMode.EqualTreatment);
        Scribe_Defs.Look(ref _defaultInteractionMode, "defaultInteractionMode");
        Scribe_Collections.Look(ref _raceInteractionModes, "raceInteractionModes", LookMode.Def, LookMode.Def);
        Scribe_Collections.Look(ref _genderInteractionModes, "genderInteractionModes", LookMode.Value, LookMode.Def);
        Scribe_Collections.Look(ref _raceGenderInteractionModesList, "raceGenderInteractionModes", LookMode.Deep);
    }

    public PrisonerInteractionSettingMode GetCurrentMode()
    {
        return _currentMode;
    }

    public void SetCurrentMode(PrisonerInteractionSettingMode mode)
    {
        _currentMode = mode;
    }

    public PrisonerInteractionModeDef GetDefaultInteractionMode()
    {
        return _defaultInteractionMode;
    }

    public void SetDefaultInteractionMode(PrisonerInteractionModeDef mode)
    {
        _defaultInteractionMode = mode;
    }

    public PrisonerInteractionModeDef GetRaceInteractionMode(ThingDef race)
    {
        if (_raceInteractionModes.TryGetValue(race, out PrisonerInteractionModeDef mode))
        {
            return mode;
        }
        return null;
    }

    public void SetRaceInteractionMode(ThingDef race, PrisonerInteractionModeDef mode)
    {
        _raceInteractionModes[race] = mode;
    }

    public Dictionary<ThingDef, PrisonerInteractionModeDef> GetAllRaceInteractionModes()
    {
        return _raceInteractionModes;
    }

    public PrisonerInteractionModeDef GetGenderInteractionMode(Gender gender)
    {
        if (_genderInteractionModes.TryGetValue(gender, out PrisonerInteractionModeDef mode))
        {
            return mode;
        }
        return null;
    }

    public void SetGenderInteractionMode(Gender gender, PrisonerInteractionModeDef mode)
    {
        _genderInteractionModes[gender] = mode;
    }

    public Dictionary<Gender, PrisonerInteractionModeDef> GetAllGenderInteractionModes()
    {
        return _genderInteractionModes;
    }

    private Dictionary<ThingDef, Dictionary<Gender, PrisonerInteractionModeDef>> _raceGenderCache = null;

    private void BuildRaceGenderCache()
    {
        if (_raceGenderCache != null) return;

        _raceGenderCache = new Dictionary<ThingDef, Dictionary<Gender, PrisonerInteractionModeDef>>();
        foreach (var item in _raceGenderInteractionModesList)
        {
            if (!_raceGenderCache.TryGetValue(item.Race, out var genderDict))
            {
                genderDict = new Dictionary<Gender, PrisonerInteractionModeDef>();
                _raceGenderCache[item.Race] = genderDict;
            }
            genderDict[item.Gender] = item.Mode;
        }
    }

    public PrisonerInteractionModeDef GetRaceGenderInteractionMode(ThingDef race, Gender gender)
    {
        BuildRaceGenderCache();

        if (_raceGenderCache.TryGetValue(race, out var genderDict))
        {
            if (genderDict.TryGetValue(gender, out PrisonerInteractionModeDef mode))
            {
                return mode;
            }
        }
        return null;
    }

    public void SetRaceGenderInteractionMode(ThingDef race, Gender gender, PrisonerInteractionModeDef mode)
    {
        _raceGenderCache = null;

        var existing = _raceGenderInteractionModesList.FindIndex(x => x.Race == race && x.Gender == gender);
        if (existing >= 0)
        {
            _raceGenderInteractionModesList[existing].Mode = mode;
        }
        else
        {
            _raceGenderInteractionModesList.Add(new RaceGenderInteractionMode
            {
                Race = race,
                Gender = gender,
                Mode = mode
            });
        }
    }

    public Dictionary<ThingDef, Dictionary<Gender, PrisonerInteractionModeDef>> GetAllRaceGenderInteractionModes()
    {
        BuildRaceGenderCache();
        return _raceGenderCache;
    }

    public PrisonerInteractionModeDef GetInteractionModeForPawn(Pawn pawn)
    {
        if (pawn == null || pawn.guest == null)
        {
            return null;
        }

        switch (_currentMode)
        {
            case PrisonerInteractionSettingMode.EqualTreatment:
                return _defaultInteractionMode;

            case PrisonerInteractionSettingMode.ByRace:
                return GetRaceInteractionMode(pawn.def);

            case PrisonerInteractionSettingMode.ByGender:
                return GetGenderInteractionMode(pawn.gender);

            case PrisonerInteractionSettingMode.ByRaceAndGender:
                return GetRaceGenderInteractionMode(pawn.def, pawn.gender);

            default:
                return _defaultInteractionMode;
        }
    }
    
    public void SetPrisonerInteractionModeForPawn(Pawn pawn)
    {
        if (pawn == null || pawn.guest == null)
        {
            return;
        }
        PrisonerInteractionModeDef mode = GetInteractionModeForPawn(pawn);
        if (mode != null && PrisonerInteractionUtils.CanUse(pawn, mode))
        {
            pawn.guest.SetExclusiveInteraction(mode);
        }
    }

}
