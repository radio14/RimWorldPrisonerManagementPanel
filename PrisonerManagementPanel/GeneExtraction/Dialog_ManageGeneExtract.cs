using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonerManagementPanel.GeneExtraction;

public class Dialog_ManageGeneExtract : Window
{
    private Vector2 scrollPositionLeft = Vector2.zero;
    private Vector2 scrollPositionRight = Vector2.zero;
    private List<GeneDef> allGenes = new List<GeneDef>();
    private List<GeneDef> selectedGenes = new List<GeneDef>();

    // 保存原始基因顺序
    private List<GeneDef> originalGeneOrder = new List<GeneDef>();
    private readonly Vector2 initialSize = new Vector2(1200f, 700f);

    public override Vector2 InitialSize => initialSize;

    public Dialog_ManageGeneExtract()
    {
        this.doCloseX = true;
        this.closeOnCancel = true;
        this.absorbInputAroundWindow = true;
        // this.forcePause = true;
        this.resizeable = true;
        this.draggable = true;

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
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "基因提取设置");
        Text.Font = GameFont.Small;

        Rect contentRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
        DoGeneList(contentRect);

        Rect buttonRect = new Rect(inRect.width - 150f, inRect.height - 30f, 150f, 30f);
        if (Widgets.ButtonText(buttonRect, "完成"))
        {
            if (GeneExtractionStorage.Instance != null)
            {
                GeneExtractionStorage.Instance.SetSelectedGenes(selectedGenes);
            }

            Close();
        }
    }

    private void DoGeneList(Rect rect)
    {
        if (!ModsConfig.BiotechActive)
        {
            Widgets.Label(rect, "Biotech扩展未启用");
            return;
        }

        float columnWidth = (rect.width - 30f) / 2f;

        // // 搜索框
        // Rect searchRect = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, 24f);
        // state.quickSearch.OnGUI(searchRect);

        // 左侧区域 - 未选中的基因
        Rect leftRect = new Rect(rect.x, rect.y, columnWidth, rect.height - 40f);

        DrawGeneColumn(leftRect, "可选基因", allGenes, ref scrollPositionLeft, false);

        // 中间分隔线
        Rect separatorRect = new Rect(leftRect.xMax + 5f, rect.y, 20f, rect.height - 40f);
        Widgets.DrawLineVertical(separatorRect.x + 10f, separatorRect.y, separatorRect.height);

        // 右侧区域 - 已选中的基因
        Rect rightRect = new Rect(separatorRect.xMax + 5f, rect.y, columnWidth, rect.height - 40f);

        DrawGeneColumn(rightRect, "已选基因", selectedGenes, ref scrollPositionRight, true);
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
}