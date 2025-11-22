using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 保存和加载页面导航栏控制类，用于管理分页按钮以及页面切换逻辑。
/// </summary>
public class SaveAndLoadPageNavigationBar : MonoBehaviour
{
    [SerializeField] private SaveAndLoadMenu menu;

    private bool initialized = false;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    
    private const int MAX_BUTTONS = 5;

    /// <summary>
    /// 获取当前选中的页面编号（从1开始）。
    /// </summary>
    public int selectedPage { get; private set; } = 1;
    private int maxPages = 0;
    
    /// <summary>
    /// 当前显示的起始页码（滑动窗口起始点）
    /// </summary>
    private int visiblePageStart = 1;
    
    /// <summary>
    /// 缓存分页按钮列表
    /// </summary>
    private List<Button> pageButtons = new List<Button>();

    private Color selectedPageColor;

    /// <summary>
    /// Unity生命周期函数，在对象启用时调用。初始化菜单组件。
    /// </summary>
    private void Start()
    {
        // 解析高亮颜色 #FFE7B6
        ColorUtility.TryParseHtmlString("#FFE7B6", out selectedPageColor);
        InitializedMenu();
    }

    /// <summary>
    /// 初始化导航栏界面，包括创建分页按钮、设置最大页数等。
    /// 只会执行一次。
    /// </summary>
    private void InitializedMenu()
    {
        // 防止重复初始化
        if (initialized)
            return;
        
        initialized = true;

        // 计算总页数：文件总数除以每页显示的槽位数量并向上取整
        maxPages = Mathf.CeilToInt((float)SaveAndLoadMenu.MAX_FILES / menu.slotsPerPage);
        // 确定实际要显示的按钮数量，不超过最大按钮限制或总页数
        int pageButtonLimit = MAX_BUTTONS < maxPages ? MAX_BUTTONS : maxPages;
        
        pageButtons.Clear();

        // 创建分页按钮
        for (int i = 0; i < pageButtonLimit; i++)
        {
            GameObject ob = Instantiate(buttonPrefab.gameObject, buttonPrefab.transform.parent);
            ob.SetActive(true);
            
            Button button = ob.GetComponent<Button>();
            pageButtons.Add(button);

            int buttonIndex = i;
            button.onClick.AddListener(() => SelectSaveFilePage(visiblePageStart + buttonIndex));
        }

        // 如果按钮数量小于总页数，则显示前后翻页按钮
        previousButton.SetActive(pageButtonLimit < maxPages);
        nextButton.SetActive(pageButtonLimit < maxPages);
        
        // 将下一页按钮置于层级最后
        nextButton.transform.SetAsLastSibling();
        
        // 初始化 UI 状态
        SelectSaveFilePage(1);
    }

    /// <summary>
    /// 选择指定页面并更新菜单内容。
    /// </summary>
    /// <param name="pageNumber">要选择的页面编号（从1开始）</param>
    private void SelectSaveFilePage(int pageNumber)
    {
        selectedPage = pageNumber;
        
        if (selectedPage < visiblePageStart)
        {
            visiblePageStart = selectedPage;
        }
        else if (selectedPage >= visiblePageStart + pageButtons.Count)
        {
            visiblePageStart = selectedPage - pageButtons.Count + 1;
        }
        
        int maxStart = Mathf.Max(1, maxPages - pageButtons.Count + 1);
        if (visiblePageStart > maxStart) visiblePageStart = maxStart;
        if (visiblePageStart < 1) visiblePageStart = 1;

        UpdateNavBarUI();

        // 填充对应页面的存档槽位
        menu.PopulateSaveSlotsForPage(pageNumber);
    }
    
    /// <summary>
    /// 更新导航栏按钮的数字和颜色
    /// </summary>
    private void UpdateNavBarUI()
    {
        for (int i = 0; i < pageButtons.Count; i++)
        {
            Button btn = pageButtons[i];
            int pageNum = visiblePageStart + i;
            
            btn.name = pageNum.ToString();
            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = pageNum.ToString();
            }
            
            // 高亮当前选中页
            if (pageNum == selectedPage)
            {
                btn.image.color = selectedPageColor;
            }
            else
            {
                btn.image.color = Color.white;
            }
        }
    }

    /// <summary>
    /// 切换到下一页。
    /// </summary>
    public void ToNextPage()
    {
        if (selectedPage < maxPages)
            SelectSaveFilePage(selectedPage + 1);
    }
    
    /// <summary>
    /// 切换到上一页。
    /// </summary>
    public void ToPreviousPage()
    {
        if (selectedPage > 1)
            SelectSaveFilePage(selectedPage - 1);
    }
}