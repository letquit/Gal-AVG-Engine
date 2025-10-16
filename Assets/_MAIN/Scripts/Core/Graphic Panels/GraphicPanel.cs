using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 图形面板类，用于管理UI面板及其图层
/// </summary>
[Serializable]
public class GraphicPanel
{
    public string panelName;
    public GameObject rootPanel;
    private List<GraphicLayer> layers = new List<GraphicLayer>();

    /// <summary>
    /// 根据图层深度获取对应的图形图层
    /// </summary>
    /// <param name="layerDepth">图层深度</param>
    /// <param name="createIfDoesNotExist">当图层不存在时是否创建新图层，默认为false</param>
    /// <returns>找到的图形图层，如果不存在且不创建则返回null</returns>
    public GraphicLayer GetLayer(int layerDepth, bool createIfDoesNotExist = false)
    {
        // 遍历现有图层列表查找指定深度的图层
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].layerDepth == layerDepth)
                return layers[i];
        }

        // 如果未找到且允许创建新图层，则创建并返回
        if (createIfDoesNotExist)
        {
            return CreateLayer(layerDepth);
        }
        
        return null;
    }

    /// <summary>
    /// 创建指定深度的新图层
    /// </summary>
    /// <param name="layerDepth">要创建的图层深度</param>
    /// <returns>新创建的图形图层</returns>
    private GraphicLayer CreateLayer(int layerDepth)
    {
        // 创建图层对象和对应的GameObject
        GraphicLayer layer = new GraphicLayer();
        GameObject panel = new GameObject(string.Format(GraphicLayer.LAYER_OBJECT_NAME_FORMAT, layerDepth));
        RectTransform rect = panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasGroup>();
        panel.transform.SetParent(rootPanel.transform,false);
        
        // 设置RectTransform锚点和偏移量，使其填充整个父容器
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;
        
        layer.panel = panel.transform;
        layer.layerDepth = layerDepth;
        
        // 按深度顺序插入图层列表
        int index = layers.FindIndex(l => l.layerDepth > layerDepth);
        if (index == -1)
            layers.Add(layer);
        else
            layers.Insert(index, layer);

        // 更新所有图层的显示顺序
        for (int i = 0; i < layers.Count; i++)
            layers[i].panel.SetSiblingIndex(layers[i].layerDepth);
        
        return layer;
    }
}

