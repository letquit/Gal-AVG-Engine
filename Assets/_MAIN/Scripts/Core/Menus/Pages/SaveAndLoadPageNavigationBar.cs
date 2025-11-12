using System;
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
    /// Unity生命周期函数，在对象启用时调用。初始化菜单组件。
    /// </summary>
    private void Start()
    {
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
        
        // 创建分页按钮
        for (int i = 1; i <= pageButtonLimit; i++)
        {
            GameObject ob = Instantiate(buttonPrefab.gameObject, buttonPrefab.transform.parent);
            ob.SetActive(true);
            
            Button button = ob.GetComponent<Button>();

            ob.name = i.ToString();
            TextMeshProUGUI txt = ob.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = i.ToString();
            int closureIndex = i;
            button.onClick.AddListener(() => SelectSaveFilePage(closureIndex));
        }

        // 如果按钮数量小于总页数，则显示前后翻页按钮
        previousButton.SetActive(pageButtonLimit < maxPages);
        nextButton.SetActive(pageButtonLimit < maxPages);
        
        // 将下一页按钮置于层级最后
        nextButton.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 选择指定页面并更新菜单内容。
    /// </summary>
    /// <param name="pageNumber">要选择的页面编号（从1开始）</param>
    private void SelectSaveFilePage(int pageNumber)
    {
        selectedPage = pageNumber;
        menu.PopulateSaveSlotsForPage(pageNumber);
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