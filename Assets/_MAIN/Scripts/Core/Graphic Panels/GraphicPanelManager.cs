using System;
using UnityEngine;

/// <summary>
/// 图形面板管理器类，用于管理和获取场景中的图形面板组件
/// 继承自MonoBehaviour，作为Unity组件使用
/// </summary>
public class GraphicPanelManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例属性，提供全局访问点
    /// </summary>
    public static GraphicPanelManager instance { get; private set; }

    /// <summary>
    /// 默认过渡动画速度常量
    /// </summary>
    public const float DEFAULT_TRANSITION_SPEED = 1f;
    
    /// <summary>
    /// 序列化字段，存储所有图形面板的数组
    /// </summary>
    [SerializeField] private GraphicPanel[] allPanels;

    /// <summary>
    /// Unity生命周期方法，在对象初始化时调用
    /// 设置单例实例为当前对象
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 根据名称获取指定的图形面板
    /// </summary>
    /// <param name="name">要查找的面板名称</param>
    /// <returns>找到的GraphicPanel对象，如果未找到则返回null</returns>
    public GraphicPanel GetPanel(string name)
    {
        // 将输入名称转换为小写以进行不区分大小写的比较
        name = name.ToLower();
        
        // 遍历所有面板查找匹配名称的面板
        foreach (var panel in allPanels)
        {
            if (panel.panelName.ToLower() == name)
                return panel;
        }
        
        return null;
    }
}

