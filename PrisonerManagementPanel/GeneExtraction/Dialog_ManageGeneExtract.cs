using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace PrisonerManagementPanel.GeneExtraction;

[StaticConstructorOnStartup]
public class Dialog_ManageGeneExtract : Window
{
    private static Texture2D InfoIcon;
    private Vector2 scrollPositionLeft = Vector2.zero;
    private Vector2 scrollPositionRight = Vector2.zero;
    private List<GeneDef> allGenes = new List<GeneDef>();
    private List<GeneDef> selectedGenes = new List<GeneDef>();

    private string searchText = "";
    private List<GeneDef> filteredAllGenes = new List<GeneDef>();
    private List<GeneDef> filteredSelectedGenes = new List<GeneDef>();

    // 原始基因顺序
    private List<GeneDef> originalGeneOrder = new List<GeneDef>();
    private readonly Vector2 initialSize = new Vector2(1200f, 700f);

    public override Vector2 InitialSize => initialSize;

    static Dialog_ManageGeneExtract()
    {
        InfoIcon = TexButton.Info;
    }

    public Dialog_ManageGeneExtract()
    {
        this.doCloseX = true;
        this.closeOnCancel = true;
        this.absorbInputAroundWindow = true;
        this.resizeable = true;
        this.draggable = true;
        this.closeOnClickedOutside = true;

        Init();
    }

    private void Init()
    {
        allGenes.Clear();
        selectedGenes.Clear();

        if (!ModsConfig.BiotechActive)
            return;

        // 获取所有基因定义
        var allGeneDefs = DefDatabase<GeneDef>.AllDefs.ToList();

        // 筛选可提取的基因: passOnDirectly为true且biostatArc为0
        var filteredGenes = allGeneDefs
            .Where(gene => gene.passOnDirectly && gene.biostatArc == 0)
            .ToList();

        // 保存原始顺序
        originalGeneOrder = new List<GeneDef>(filteredGenes);

        // 加载已选基因
        if (GeneExtractionStorage.Instance != null)
        {
            var storedGenes = GeneExtractionStorage.Instance.GetSelectedGenes();
            if (storedGenes != null)
            {
                selectedGenes = new List<GeneDef>(storedGenes);
                // 过滤已选基因
                foreach (var gene in selectedGenes)
                {
                    filteredGenes.Remove(gene);
                }
            }
        }

        allGenes = filteredGenes.OrderBy(gene => originalGeneOrder.IndexOf(gene)).ToList();

        // 初始化过滤列表
        filteredAllGenes = new List<GeneDef>(allGenes);
        filteredSelectedGenes = new List<GeneDef>(selectedGenes);
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Rect titleRect = new Rect(0f, 0f, inRect.width, 30f);
        Widgets.Label(titleRect, "GeneExtraction_Policy".Translate());
        AddInfoButton(titleRect);

        Text.Font = GameFont.Small;

        Rect contentRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
        DoGeneList(contentRect);

        Rect buttonRect = new Rect(inRect.width - 150f, inRect.height - 30f, 150f, 30f);

        if (Widgets.ButtonText(buttonRect, "GeneExtraction_Save".Translate()))
        {
            if (GeneExtractionStorage.Instance != null)
            {
                GeneExtractionStorage.Instance.SetSelectedGenes(selectedGenes);
            }

            SoundDefOf.Click.PlayOneShotOnCamera();
            // Close();
        }

        // 执行/取消执行按钮
        Rect executeButtonRect = new Rect(inRect.width - 310f, inRect.height - 30f, 150f, 30f);
        string executeButtonText = "GeneExtraction_Doing".Translate();

        if (GeneExtractionStorage.Instance != null && GeneExtractionStorage.Instance.IsOpen())
        {
            executeButtonText = "GeneExtraction_Cancel".Translate();
        }

        if (Widgets.ButtonText(executeButtonRect, executeButtonText))
        {
            if (GeneExtractionStorage.Instance != null)
            {
                if (GeneExtractionStorage.Instance.IsOpen())
                {
                    GeneExtractionStorage.Instance.Close();
                }
                else
                {
                    GeneExtractionStorage.Instance.Open();
                }
            }

            SoundDefOf.Click.PlayOneShotOnCamera();
        }
    }

    private void DoGeneList(Rect rect)
    {
        if (!ModsConfig.BiotechActive)
        {
            Widgets.Label(rect, "No Biotech DLC!");
            return;
        }

        float columnWidth = (rect.width - 30f) / 2f;

        // 搜索框
        Rect searchRect = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 24f);
        DrawSearchField(searchRect);

        // 左侧区域 - 未选中的基因
        Rect leftRect = new Rect(rect.x, rect.y + 30f, columnWidth, rect.height - 70f);

        DrawGeneColumn(leftRect, "GeneExtraction_Optional".Translate(), filteredAllGenes, ref scrollPositionLeft,
            false);

        // 中间分隔线
        Rect separatorRect = new Rect(leftRect.xMax + 5f, rect.y + 30f, 20f, rect.height - 70f);
        Widgets.DrawLineVertical(separatorRect.x + 10f, separatorRect.y, separatorRect.height);

        // 右侧区域 - 已选中的基因
        Rect rightRect = new Rect(separatorRect.xMax + 5f, rect.y + 30f, columnWidth, rect.height - 70f);

        DrawGeneColumn(rightRect, "GeneExtraction_Selected".Translate(), filteredSelectedGenes, ref scrollPositionRight,
            true);
    }

    private void DrawSearchField(Rect rect)
    {
        GUI.SetNextControlName("GeneSearchField");
        string oldSearchText = searchText;
        searchText = Widgets.TextField(rect, searchText);

        if (oldSearchText != searchText)
        {
            UpdateFilteredGenes();
        }

        // 提示文字
        if (string.IsNullOrEmpty(searchText) && !GUI.GetNameOfFocusedControl().Equals("GeneSearchField"))
        {
            GUI.color = Color.gray;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(rect.x + 5f, rect.y, rect.width - 10f, rect.height),
                "GeneExtraction_Search".Translate() + "...");
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

    private void UpdateFilteredGenes()
    {
        if (string.IsNullOrEmpty(searchText))
        {
            // 如果搜索文本为空，则显示所有基因
            filteredAllGenes = new List<GeneDef>(allGenes);
            filteredSelectedGenes = new List<GeneDef>(selectedGenes);
        }
        else
        {
            // 根据搜索文本过滤基因
            string searchLower = searchText.ToLower();

            filteredAllGenes = allGenes
                .Where(gene => MatchesSearchCriteria(gene, searchLower))
                .OrderBy(gene => originalGeneOrder.IndexOf(gene))
                .ToList();

            filteredSelectedGenes = selectedGenes
                .Where(gene => MatchesSearchCriteria(gene, searchLower))
                .OrderBy(gene => gene.label)
                .ToList();
        }
    }

    private bool MatchesSearchCriteria(GeneDef gene, string searchLower)
    {
        return gene.label.ToLower().Contains(searchLower) ||
               (gene.description != null && gene.description.ToLower().Contains(searchLower)) ||
               gene.defName.ToLower().Contains(searchLower);
    }

    private void DrawGeneColumn(Rect rect, string title, List<GeneDef> genes, ref Vector2 scrollPosition,
        bool isRightColumn)
    {
        Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), title);

        Rect genesRect = new Rect(rect.x, rect.y + 30f, rect.width, rect.height - 30f);
        Rect viewRect = new Rect(0f, 0f, genesRect.width - 16f, GetGenesHeight(genes, genesRect.width));

        Widgets.BeginScrollView(genesRect, ref scrollPosition, viewRect);
        try
        {
            DrawGenes(viewRect, genes, isRightColumn);
        }
        finally
        {
            Widgets.EndScrollView();
        }
    }

    private void DrawGenes(Rect rect, List<GeneDef> genes, bool isRightColumn)
    {
        float geneSize = 90f;
        float geneGap = 6f;
        int maxGenesPerRow = Mathf.FloorToInt((rect.width - 20f) / (geneSize + geneGap));

        int row = 0;
        int col = 0;

        for (int i = 0; i < genes.Count; i++)
        {
            if (col >= maxGenesPerRow)
            {
                col = 0;
                row++;
            }

            float xPos = 10f + col * (geneSize + geneGap);
            float yPos = row * (geneSize + geneGap);

            Rect geneRect = new Rect(xPos, yPos, geneSize, geneSize);

            GeneUIUtility.DrawGeneDef(genes[i], geneRect, GeneType.Endogene, null, true, false);
            if (Widgets.ButtonInvisible(geneRect))
            {
                if (isRightColumn)
                {
                    // 取消选择
                    GeneDef gene = genes[i];
                    selectedGenes.Remove(gene);
                    allGenes.Add(gene);
                    // 按原始顺序重新排列
                    allGenes = allGenes.OrderBy(g => originalGeneOrder.IndexOf(g)).ToList();
                }
                else
                {
                    // 选中
                    GeneDef gene = genes[i];
                    allGenes.Remove(gene);
                    selectedGenes.Add(gene);
                }

                // 更新过滤后的列表
                UpdateFilteredGenes();
            }

            col++;
        }
    }

    private float GetGenesHeight(List<GeneDef> genes, float width)
    {
        if (genes.Count == 0)
            return 1f;

        float geneSize = 90f;
        float geneGap = 6f;
        int maxGenesPerRow = Mathf.Max(1, Mathf.FloorToInt((width - 20f) / (geneSize + geneGap)));
        int rows = Mathf.CeilToInt((float)genes.Count / maxGenesPerRow);

        return rows * (geneSize + geneGap);
    }

    private void AddInfoButton(Rect titleRect)
    {
        // 获取标题文本的实际宽度
        string titleText = "GeneExtraction_Policy".Translate();
        Vector2 titleSize = Text.CalcSize(titleText);

        // 计算按钮位置 (紧邻标题文本右侧)
        Rect infoButtonRect = new Rect(
            titleRect.x + titleSize.x + 5f,
            titleRect.y + (titleRect.height - 24f) / 2f, // 垂直居中
            24f,
            24f
        );

        // 确保按钮不会超出窗口边界
        if (infoButtonRect.xMax > titleRect.xMax)
        {
            infoButtonRect.x = titleRect.xMax - 24f;
        }

        // 绘制信息按钮
        if (Widgets.ButtonImage(infoButtonRect, InfoIcon))
        {
            // 打开提示弹窗
            Find.WindowStack.Add(new Dialog_MessageBox(
                "GeneExtraction_Info".Translate(),
                "GeneExtraction_Info_AText".Translate(),
                null,
                null,
                null,
                "GeneExtraction_Info_Title".Translate()
            ));
        }
    }
}