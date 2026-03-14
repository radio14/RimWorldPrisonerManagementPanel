using System;
using System.Collections.Generic;
using System.Linq;
using PrisonerManagementPanel.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PrisonerManagementPanel.PrisonerInteraction;

public class InteractionModeSelector
{
    public PrisonerInteractionModeDef SelectedMode;

    public InteractionModeSelector(PrisonerInteractionModeDef initialMode)
    {
        SelectedMode = initialMode;
    }
}

public class Dialog_ManagePrisonerInteraction : Window
{
    private PrisonerInteractionSettingMode _currentMode;
    private InteractionModeSelector _defaultModeSelector;
    private Dictionary<ThingDef, InteractionModeSelector> _raceModeSelectors = new Dictionary<ThingDef, InteractionModeSelector>();
    private Dictionary<Gender, InteractionModeSelector> _genderModeSelectors = new Dictionary<Gender, InteractionModeSelector>();
    private Dictionary<ThingDef, Dictionary<Gender, InteractionModeSelector>> _raceGenderModeSelectors = new Dictionary<ThingDef, Dictionary<Gender, InteractionModeSelector>>();
    
    private List<TabRecord> tabs;

    private List<ThingDef> _allRaces = new List<ThingDef>();
    private ThingDef _currentRace = RaceUtils.DefaultRace();
    
    private int _selectedRaceIndex = 0;

    private readonly Vector2 initialSize = new Vector2(900f, 600f);

    public override Vector2 InitialSize => initialSize;

    public Dialog_ManagePrisonerInteraction()
    {
        this.doCloseX = true;
        this.closeOnCancel = true;
        this.absorbInputAroundWindow = true;
        this.closeOnClickedOutside = true;

        Init();
    }

    private void Init()
    {
        _allRaces = RaceUtils.GetAllRaces().ToList();

        if (PrisonerInteractionSettingStorage.Instance != null)
        {
            _currentMode = PrisonerInteractionSettingStorage.Instance.GetCurrentMode();
            PrisonerInteractionModeDef defaultMode = PrisonerInteractionSettingStorage.Instance.GetDefaultInteractionMode();
            _defaultModeSelector = new InteractionModeSelector(defaultMode);

            var storedRaceModes = PrisonerInteractionSettingStorage.Instance.GetAllRaceInteractionModes();
            if (storedRaceModes != null)
            {
                foreach (var pair in storedRaceModes)
                {
                    _raceModeSelectors[pair.Key] = new InteractionModeSelector(pair.Value);
                }
            }

            var storedGenderModes = PrisonerInteractionSettingStorage.Instance.GetAllGenderInteractionModes();
            if (storedGenderModes != null)
            {
                foreach (var pair in storedGenderModes)
                {
                    _genderModeSelectors[pair.Key] = new InteractionModeSelector(pair.Value);
                }
            }

            var storedRaceGenderModes = PrisonerInteractionSettingStorage.Instance.GetAllRaceGenderInteractionModes();
            if (storedRaceGenderModes != null)
            {
                foreach (var racePair in storedRaceGenderModes)
                {
                    var genderDict = new Dictionary<Gender, InteractionModeSelector>();
                    foreach (var genderPair in racePair.Value)
                    {
                        genderDict[genderPair.Key] = new InteractionModeSelector(genderPair.Value);
                    }
                    _raceGenderModeSelectors[racePair.Key] = genderDict;
                }
            }
        }
        else
        {
            _defaultModeSelector = new InteractionModeSelector(null);
        }
        
        tabs = _allRaces.Select(race => new TabRecord(
            race.LabelCap,
            () =>
            {
                _currentRace = race;
            }, 
            () => _currentRace == race
        )).ToList();
    }

    public override void DoWindowContents(Rect inRect)
    {
        inRect.width -= 12f;
        Text.Font = GameFont.Medium;
        Rect titleRect = new Rect(0f, 0f, inRect.width, 30f);
        Widgets.Label(titleRect, "PrisonerInteraction_DefaultSettings".Translate());
        inRect.y += 45f;
        inRect.height -= 45f;

        Text.Font = GameFont.Small;

        Rect modeBackRect = new Rect(inRect.x, inRect.y, inRect.width, 60f);
        Rect modeRect = new Rect(inRect.x + 15f, inRect.y, inRect.width - 15f, 60f);
        Color lastColor = GUI.color;
        GUI.color = Widgets.MenuSectionBGFillColor;
        GUI.DrawTexture(modeBackRect, (Texture) BaseContent.WhiteTex);
        GUI.color = lastColor;
        DoModeSelection(modeRect);
        inRect.y += 75f;
        inRect.height -= 75f;

        Rect contentRect = new Rect(0f, inRect.y, inRect.width, inRect.height);
        DoModeContent(contentRect);

        Rect buttonRect = new Rect(inRect.width - 150f, inRect.height + 45f + 75f - 30f, 150f, 30f);

        if (Widgets.ButtonText(buttonRect, "Save".Translate()))
        {
            SaveSettings();
            SoundDefOf.Click.PlayOneShotOnCamera();
            Close();
        }
    }

    private void DoModeSelection(Rect rect)
    {
        Rect titleRect = rect.TopPart(0.4f);
        Widgets.Label(titleRect, "PrisonerInteraction_Mode".Translate());

        Rect buttonsRect = rect.BottomPart(0.6f);
        float buttonWidth = (buttonsRect.width / Enum.GetValues(typeof(PrisonerInteractionSettingMode)).Length) - 24f;

        Listing_Standard listing = new Listing_Standard();
        listing.ColumnWidth = buttonWidth;
        listing.Begin(buttonsRect);

        foreach (PrisonerInteractionSettingMode mode in Enum.GetValues(typeof(PrisonerInteractionSettingMode)))
        {
            string label = mode.GetLabel();
            bool selected = _currentMode == mode;

            listing.Gap(4f);

            Rect optionRect = listing.GetRect(Text.LineHeight);

            TooltipHandler.TipRegion(optionRect, mode.GetTipSignal());

            if (Widgets.RadioButtonLabeled(optionRect, label, selected))
            {
                _currentMode = mode;
                if ((_currentMode == PrisonerInteractionSettingMode.ByRace || _currentMode == PrisonerInteractionSettingMode.ByRaceAndGender) && _allRaces.Count > 0)
                {
                    _selectedRaceIndex = 0;
                }
            }
        }

        listing.End();
    }

    private void DoModeContent(Rect rect)
    {
        switch (_currentMode)
        {
            case PrisonerInteractionSettingMode.EqualTreatment:
                DoEqualTreatmentContent(rect);
                break;
            case PrisonerInteractionSettingMode.ByRace:
                DoByRaceContent(rect);
                break;
            case PrisonerInteractionSettingMode.ByGender:
                DoByGenderContent(rect);
                break;
            case PrisonerInteractionSettingMode.ByRaceAndGender:
                DoByRaceAndGenderContent(rect);
                break;
        }
    }

    private void DoEqualTreatmentContent(Rect rect)
    {
        DoInteractionModeSelector(rect, "PrisonerInteraction_DefaultValue".Translate(), _defaultModeSelector);
    }

    private void DoByRaceContent(Rect rect)
    {
        if (_allRaces.Count == 0)
        {
            Widgets.Label(rect, "PrisonerInteraction_NoRaces".Translate());
            return;
        }

        Rect tabRect = new Rect(rect.x, rect.y, rect.width, 35f);
        DoRaceTabs(tabRect);
        rect.y += 50f;
        rect.height -= 50f;

        Rect contentRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        ThingDef selectedRace = _allRaces[_selectedRaceIndex];

        if (!_raceModeSelectors.TryGetValue(selectedRace, out var modeSelector))
        {
            modeSelector = new InteractionModeSelector(_defaultModeSelector?.SelectedMode);
            _raceModeSelectors[selectedRace] = modeSelector;
        }

        DoInteractionModeSelector(contentRect, "PrisonerInteraction_DefaultValue".Translate(), modeSelector);
    }

    private void DoByGenderContent(Rect rect)
    {
        float sectionHeight = 50f;

        Rect maleRect = new Rect(rect.x, rect.y, rect.width, sectionHeight);
        Widgets.Label(new Rect(maleRect.x, maleRect.y, 200f, 25f), "Male".Translate());

        if (!_genderModeSelectors.TryGetValue(Gender.Male, out var maleSelector))
        {
            maleSelector = new InteractionModeSelector(_defaultModeSelector?.SelectedMode);
            _genderModeSelectors[Gender.Male] = maleSelector;
        }

        Rect maleContentRect = new Rect(maleRect.x, maleRect.y + 30f, maleRect.width, maleRect.height - 30f);
        DoInteractionModeSelector(maleContentRect, "PrisonerInteraction_DefaultValue".Translate(), maleSelector);

        Rect femaleRect = new Rect(rect.x, rect.y + sectionHeight + 10f, rect.width, sectionHeight);
        Widgets.Label(new Rect(femaleRect.x, femaleRect.y, 200f, 25f), "Female".Translate());

        if (!_genderModeSelectors.TryGetValue(Gender.Female, out var femaleSelector))
        {
            femaleSelector = new InteractionModeSelector(_defaultModeSelector?.SelectedMode);
            _genderModeSelectors[Gender.Female] = femaleSelector;
        }

        Rect femaleContentRect = new Rect(femaleRect.x, femaleRect.y + 30f, femaleRect.width, femaleRect.height - 30f);
        DoInteractionModeSelector(femaleContentRect, "PrisonerInteraction_DefaultValue".Translate(), femaleSelector);
    }

    private void DoByRaceAndGenderContent(Rect rect)
    {
        if (_allRaces.Count == 0)
        {
            Widgets.Label(rect, "PrisonerInteraction_NoRaces".Translate());
            return;
        }

        Rect tabRect = new Rect(rect.x, rect.y, rect.width, 35f);
        DoRaceTabs(tabRect);
        
        rect.y += 50f;
        rect.height -= 50f;

        Rect contentRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        ThingDef selectedRace = _allRaces[_selectedRaceIndex];

        if (!_raceGenderModeSelectors.TryGetValue(selectedRace, out var genderDict))
        {
            genderDict = new Dictionary<Gender, InteractionModeSelector>();
            _raceGenderModeSelectors[selectedRace] = genderDict;
        }

        float sectionHeight = 50f;

        Rect maleRect = new Rect(contentRect.x, contentRect.y, contentRect.width, sectionHeight);
        Widgets.Label(new Rect(maleRect.x, maleRect.y, 200f, 25f), "Male".Translate());

        if (!genderDict.TryGetValue(Gender.Male, out var maleSelector))
        {
            maleSelector = new InteractionModeSelector(_defaultModeSelector?.SelectedMode);
            genderDict[Gender.Male] = maleSelector;
        }

        Rect maleContentRect = new Rect(maleRect.x, maleRect.y + 30f, maleRect.width, maleRect.height - 30f);
        DoInteractionModeSelector(maleContentRect, "PrisonerInteraction_DefaultValue".Translate(), maleSelector);

        Rect femaleRect = new Rect(contentRect.x, contentRect.y + sectionHeight + 10f, contentRect.width, sectionHeight);
        Widgets.Label(new Rect(femaleRect.x, femaleRect.y, 200f, 25f), "Female".Translate());

        if (!genderDict.TryGetValue(Gender.Female, out var femaleSelector))
        {
            femaleSelector = new InteractionModeSelector(_defaultModeSelector?.SelectedMode);
            genderDict[Gender.Female] = femaleSelector;
        }

        Rect femaleContentRect = new Rect(femaleRect.x, femaleRect.y + 30f, femaleRect.width, femaleRect.height - 30f);
        DoInteractionModeSelector(femaleContentRect, "PrisonerInteraction_DefaultValue".Translate(), femaleSelector);
    }

    private void DoRaceTabs(Rect rect)
    {
        float tabHeight = Pmp_TabDrawer.GetOverflowTabHeight(rect, tabs, 80f, 200f);
        Rect tabRect = new Rect(rect.x, rect.y, rect.width, tabHeight);

        var selectedTab = Pmp_TabDrawer.DrawTabsOverflow(tabRect, tabs, 80f, 200f);

        if (selectedTab != null)
        {
            int newIndex = tabs.IndexOf(selectedTab);
            if (newIndex >= 0 && newIndex != _selectedRaceIndex)
            {
                _selectedRaceIndex = newIndex;
            }
        }
    }

    private void DoInteractionModeSelector(Rect rect, string label, InteractionModeSelector selector)
    {
        Widgets.Label(new Rect(rect.x, rect.y, 150f, 32f), label + ":");

        string buttonLabel = selector.SelectedMode?.LabelCap ?? "None".Translate();

        Rect buttonRect = new Rect(rect.x + 160f, rect.y, 150f, 32f);

        Widgets.Dropdown<InteractionModeSelector, PrisonerInteractionModeDef>(
            buttonRect,
            selector,
            s => s.SelectedMode,
            s => GenerateModeMenu(s),
            buttonLabel,
            dragLabel: buttonLabel,
            paintable: true
        );
    }

    private List<PrisonerInteractionModeDef> GetAvailableModes()
    {
        return DefDatabase<PrisonerInteractionModeDef>.AllDefsListForReading
            .Where(d => !d.isNonExclusiveInteraction)
            .OrderBy(d => d.listOrder)
            .ToList();
    }

    private IEnumerable<Widgets.DropdownMenuElement<PrisonerInteractionModeDef>> GenerateModeMenu(InteractionModeSelector selector)
    {
        List<PrisonerInteractionModeDef> modes = GetAvailableModes();

        foreach (var mode in modes)
        {
            yield return new Widgets.DropdownMenuElement<PrisonerInteractionModeDef>
            {
                option = new FloatMenuOption(mode.LabelCap, () =>
                {
                    selector.SelectedMode = mode;
                }),
                payload = mode
            };
        }
    }

    public static void SetInteractionModeForPawn(Pawn pawn)
    {
        if (PrisonerInteractionSettingStorage.Instance == null)
        {
            return;
        }

        PrisonerInteractionModeDef mode = PrisonerInteractionSettingStorage.Instance.GetInteractionModeForPawn(pawn);
        if (mode != null)
        {
            pawn.guest.SetExclusiveInteraction(mode);
        }
    }

    private void SaveSettings()
    {
        if (PrisonerInteractionSettingStorage.Instance == null)
        {
            return;
        }

        PrisonerInteractionSettingStorage.Instance.SetCurrentMode(_currentMode);
        PrisonerInteractionSettingStorage.Instance.SetDefaultInteractionMode(_defaultModeSelector?.SelectedMode);

        foreach (var pair in _raceModeSelectors)
        {
            PrisonerInteractionSettingStorage.Instance.SetRaceInteractionMode(pair.Key, pair.Value.SelectedMode);
        }

        foreach (var pair in _genderModeSelectors)
        {
            PrisonerInteractionSettingStorage.Instance.SetGenderInteractionMode(pair.Key, pair.Value.SelectedMode);
        }

        foreach (var racePair in _raceGenderModeSelectors)
        {
            foreach (var genderPair in racePair.Value)
            {
                PrisonerInteractionSettingStorage.Instance.SetRaceGenderInteractionMode(racePair.Key, genderPair.Key, genderPair.Value.SelectedMode);
            }
        }
    }
}
