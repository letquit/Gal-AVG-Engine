using System;
using System.Collections.Generic;
using UnityEngine;

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
    }
}