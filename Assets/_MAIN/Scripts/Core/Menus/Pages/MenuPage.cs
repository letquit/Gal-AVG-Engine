using UnityEngine;

/// <summary>
/// 菜单页面基类，用于控制菜单页面的打开和关闭动画
/// </summary>
public class MenuPage : MonoBehaviour
{
    /// <summary>
    /// 页面类型枚举，定义了三种页面类型：存档读档、配置、帮助
    /// </summary>
    public enum PageType { SaveAndLoad, Config, Help }
    
    /// <summary>
    /// 当前页面的类型
    /// </summary>
    public PageType pageType;
    
    /// <summary>
    /// 打开动画状态常量
    /// </summary>
    private const string OPEN = "Open";
    
    /// <summary>
    /// 关闭动画状态常量
    /// </summary>
    private const string CLOSE = "Close";
    
    /// <summary>
    /// 动画控制器组件引用
    /// </summary>
    public Animator anim;
    
    /// <summary>
    /// 打开菜单页面，触发动画控制器中的打开状态
    /// </summary>
    public virtual void Open()
    {
        anim.SetTrigger(OPEN);
    }
    
    /// <summary>
    /// 关闭菜单页面，触发动画控制器中的关闭状态
    /// </summary>
    /// <param name="closeAllMenus">是否同时关闭所有菜单，默认为false</param>
    public virtual void Close(bool closeAllMenus = false)
    {
        anim.SetTrigger(CLOSE);
        
        // 如果需要关闭所有菜单，则调用菜单管理器关闭根菜单
        if (closeAllMenus)
            AVGMenuManager.instance.CloseRoot();
    }
}