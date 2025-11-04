using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace History
{
    /// <summary>
    /// 图形数据类，用于序列化和存储图形面板的相关信息
    /// </summary>
    [Serializable]
    public class GraphicData
    {
        public string panelName;
        public List<LayerData> layers;

        /// <summary>
        /// 图层数据类，用于存储单个图层的信息
        /// </summary>
        [Serializable]
        public class LayerData
        {
            public int depth = 0;
            public string graphicName;
            public string graphicPath;
            public bool isVideo;
            public bool useAudio;

            /// <summary>
            /// 从GraphicLayer对象构造LayerData实例
            /// </summary>
            /// <param name="layer">源GraphicLayer对象，用于提取图层信息</param>
            public LayerData(GraphicLayer layer)
            {
                depth = layer.layerDepth;
                
                // 如果当前图形为空，则直接返回
                if (layer.currentGraphic == null)
                    return;

                var graphic = layer.currentGraphic;
                
                graphicName = graphic.graphicName;
                graphicPath = graphic.graphicPath;
                isVideo = graphic.isVideo;
                useAudio = graphic.useAudio;
            }
        }
        
        /// <summary>
        /// 捕获当前所有图形面板的数据，生成GraphicData列表
        /// </summary>
        /// <returns>包含所有非空面板数据的GraphicData列表</returns>
        public static List<GraphicData> Capture()
        {
            List<GraphicData> graphicPanels = new List<GraphicData>();
                
            // 遍历所有图形面板
            foreach (var panel in GraphicPanelManager.instance.allPanels)
            { 
                // 跳过已清除的面板
                if (panel.isClear)
                    continue;
                    
                GraphicData data = new GraphicData();
                data.panelName = panel.panelName;
                data.layers = new List<LayerData>();
                    
                // 遍历面板中的所有图层
                foreach (var layer in panel.layers)
                {
                    LayerData entry = new LayerData(layer);
                    data.layers.Add(entry);
                }
                    
                graphicPanels.Add(data);
            }
            return graphicPanels;
        }

        /// <summary>
        /// 应用图形数据到对应的图形面板中
        /// </summary>
        /// <param name="data">包含面板和图层信息的图形数据列表</param>
        public static void Apply(List<GraphicData> data)
        {
            // 创建缓存列表，用于记录已处理的面板名称
            List<string> cache = new List<string>();

            // 遍历所有图形数据，应用到对应的面板和图层
            foreach (var panelData in data)
            {
                var panel = GraphicPanelManager.instance.GetPanel(panelData.panelName);

                // 遍历当前面板的所有图层数据
                foreach (var layerData in panelData.layers)
                {
                    var layer = panel.GetLayer(layerData.depth, createIfDoesNotExist: true);
                    // 检查当前图层是否需要更新纹理或视频
                    if (layer.currentGraphic == null || layer.currentGraphic.graphicName != layerData.graphicName)
                    {
                        if (!layerData.isVideo)
                        {
                            // 加载并设置图像纹理
                            Texture tex = HistoryCache.LoadImage(layerData.graphicPath);
                            if (tex != null)
                                layer.SetTexture(tex, filePath: layerData.graphicPath, immediate: true);
                            else
                                Debug.LogWarning($"History State: Could not load image from path '{layerData.graphicPath}");
                        }
                        else
                        {
                            // 加载并设置视频剪辑
                            VideoClip clip = HistoryCache.LoadVideo(layerData.graphicPath);
                            if (clip != null)
                                layer.SetVideo(clip, filePath: layerData.graphicPath, immediate: true);
                            else
                                Debug.LogWarning($"History State: Could not load video from path '{layerData.graphicPath}");
                        }
                    }
                }
                
                cache.Add(panel.panelName);
            }

            // 清理未在数据中指定的面板
            foreach (var panel in GraphicPanelManager.instance.allPanels)
            {
                if (!cache.Contains(panel.panelName))
                    panel.Clear(immediate: true);
            }
        }
    }
}