using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// AVGMenuManager 是一个管理游戏菜单页面切换与显示的核心类。
/// 它控制多个菜单页的打开、关闭以及根CanvasGroup的交互状态。
/// </summary>
public class AVGMenuManager : MonoBehaviour
{
    /// <summary>
    /// 静态单例实例，用于全局访问该管理器。
    /// </summary>
    public static AVGMenuManager instance;
    
    /// <summary>
    /// 当前激活的菜单页面。
    /// </summary>
    private MenuPage activePage = null;
    
    /// <summary>
    /// 标记当前菜单是否处于打开状态。
    /// </summary>
    private bool isOpen = false;
    
    /// <summary>
    /// 菜单系统的根 CanvasGroup 组件，用于整体淡入淡出及交互控制。
    /// </summary>
    [SerializeField] private CanvasGroup root;
    
    /// <summary>
    /// 所有可被管理的菜单页面数组。
    /// </summary>
    [SerializeField] private MenuPage[] pages;

    /// <summary>
    /// 对根 CanvasGroup 的控制器封装对象。
    /// </summary>
    private CanvasGroupController rootCG;

    /// <summary>
    /// 在Awake阶段初始化静态单例引用。
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 在Start阶段初始化CanvasGroupController。
    /// </summary>
    private void Start()
    {
        rootCG = new CanvasGroupController(this, root);
    }

    /// <summary>
    /// 根据指定的页面类型查找对应的菜单页面。
    /// </summary>
    /// <param name="pageType">要查找的页面类型。</param>
    /// <returns>匹配的第一个菜单页面；如果没有找到则返回null。</returns>
    private MenuPage GetPage(MenuPage.PageType pageType)
    {
        return pages.FirstOrDefault(page => page.pageType == pageType);
    }

    /// <summary>
    /// 打开存档页面，并设置其功能模式为“保存”。
    /// </summary>
    public void OpenSavePage()
    {
        var page = GetPage(MenuPage.PageType.SaveAndLoad);
        var slm = page.anim.GetComponentInParent<SaveAndLoadMenu>();
        slm.menuFunction = SaveAndLoadMenu.MenuFunction.save;
        OpenPage(page);
    }
    
    /// <summary>
    /// 打开读档页面，并设置其功能模式为“加载”。
    /// </summary>
    public void OpenLoadPage()
    {
        var page = GetPage(MenuPage.PageType.SaveAndLoad);
        var slm = page.anim.GetComponentInParent<SaveAndLoadMenu>();
        slm.menuFunction = SaveAndLoadMenu.MenuFunction.load;
        OpenPage(page);
    }

    /// <summary>
    /// 打开配置页面。
    /// </summary>
    public void OpenConfigPage()
    {
        var page = GetPage(MenuPage.PageType.Config);
        OpenPage(page);
    }

    /// <summary>
    /// 打开帮助页面。
    /// </summary>
    public void OpenHelpPage()
    {
        var page = GetPage(MenuPage.PageType.Help);
        OpenPage(page);
    }

    /// <summary>
    /// 打开指定的菜单页面。如果已有其他页面正在显示，则先将其关闭。
    /// </summary>
    /// <param name="page">需要打开的菜单页面。</param>
    private void OpenPage(MenuPage page)
    {
        // 若传入页面为空则直接返回
        if (page == null)
            return;
        
        // 如果存在已激活但不是目标页面，则关闭它
        if (activePage != null && activePage != page)
            activePage.Close();
        
        // 打开新页面并更新当前活动页面
        page.Open();
        activePage = page;
        
        // 如果整个菜单系统尚未开启，则调用OpenRoot方法以启用界面
        if (!isOpen)
            OpenRoot();
    }

    /// <summary>
    /// 显示菜单根节点（即主菜单面板），使其可见且可以交互。
    /// </summary>
    public void OpenRoot()
    {
        rootCG.Show();
        rootCG.SetInteractableState(true);
        isOpen = true;
    }
    
    /// <summary>
    /// 隐藏菜单根节点（即主菜单面板），使其不可见且无法交互。
    /// </summary>
    public void CloseRoot()
    {
        rootCG.Hide();
        rootCG.SetInteractableState(false);
        isOpen = false;
    }
}