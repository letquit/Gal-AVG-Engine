using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace COMMANDS
{
    /// <summary>
    /// 扩展命令数据库以支持图形面板相关操作。
    /// 此类用于注册并处理设置图层媒体的自定义指令。
    /// </summary>
    public class CMD_DatabaseExtension_GraphicPanels : CMD_DatabaseExtension
    {
        // 参数键名数组，用于解析传入命令中的参数
        private static string[] PARAM_PANEL = new string[] { "-p", "-panel" };
        private static string[] PARAM_LAYER = new string[] { "-l", "-layer" };
        private static string[] PARAM_MEDIA = new string[] { "-m", "-media" };
        private static string[] PARAM_SPEED = new string[] { "-spd", "-speed" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static string[] PARAM_BLENDTEX = new string[] { "-b", "-blend" };
        private static string[] PARAM_USEVIDEOAUDIO = new string[] { "-aud", "-audio" };
        
        // root目录符号常量，表示资源路径从根目录开始
        private const string HOME_DIRECTORY_SYMBOL = "~/";
        
        /// <summary>
        /// 扩展命令数据库，添加图层媒体相关的命令
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例</param>
        /// <returns>无返回值</returns>
        new public static void Extend(CommandDatabase database)
        {
            // 添加设置图层媒体的命令
            database.AddCommand("setlayermedia", new Func<string[], IEnumerator>(SetLayerMedia));
            // 添加清除图层媒体的命令
            database.AddCommand("clearlayermedia", new Func<string[], IEnumerator>(ClearLayerMedia));
        }

        /// <summary>
        /// 设置指定图形面板上某一层的媒体内容（纹理或视频）。
        /// 支持过渡动画、混合贴图以及是否立即显示等选项。
        /// </summary>
        /// <param name="data">来自命令调用时传递的数据字符串数组。</param>
        /// <returns>协程迭代器对象，用于异步执行媒体加载与切换逻辑。</returns>
        private static IEnumerator SetLayerMedia(string[] data)
        {
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            bool useAudio = false;

            string pathToGraphic = "";
            Object graphic = null;
            Texture blendTex = null;

            // 将输入数据转换为参数字典以便访问
            var parameters = ConvertDataToParameters(data);
            
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"Unable to grab panel '{panelName}' because it is not a valid panel. Please check the panel name and adjust the command.");
                yield break;
            }

            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);
            
            parameters.TryGetValue(PARAM_MEDIA, out mediaName);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 若不是立即模式，则尝试获取转场速度参数
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);
            
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);

            parameters.TryGetValue(PARAM_USEVIDEOAUDIO, out useAudio, defaultValue: false);

            // 构造图像文件在Resources下的完整路径，并尝试加载为Texture
            pathToGraphic = GetPathToGraphic(FilePaths.resources_backgroundImages, mediaName);
            graphic = Resources.Load<Texture>(pathToGraphic);

            // 如果未找到Texture，则尝试作为VideoClip加载
            if (graphic == null)
            {
                pathToGraphic = GetPathToGraphic(FilePaths.resources_backgroundVideos, mediaName);
                graphic = Resources.Load<VideoClip>(pathToGraphic);
            }

            // 检查最终是否成功加载到媒体资源
            if (graphic == null)
            {
                Debug.LogError(
                    $"Could not find media file called '{mediaName}' in the Resources directories. Please specify the full path within resources and make sure that the file exists!");
                yield break;
            }
            
            // 非立即模式且提供了混合贴图名称时才进行加载
            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);
            
            // 获取目标图层对象，若不存在则创建之
            GraphicLayer graphicLayer = panel.GetLayer(layer, createIfDoesNotExist: true);

            // 根据媒体类型分别调用对应的设置方法
            if (graphic is Texture)
            {
                yield return graphicLayer.SetTexture(graphic as Texture, transitionSpeed, blendTex, pathToGraphic, immediate);
            }
            else
            {
                yield return graphicLayer.SetVideo(graphic as VideoClip, transitionSpeed, useAudio, blendTex, pathToGraphic, immediate);
            }
        }

        /// <summary>
        /// 清除指定图层面板的媒体内容
        /// </summary>
        /// <param name="data">包含清除参数的字符串数组</param>
        /// <returns>IEnumerator迭代器，用于协程执行</returns>
        private static IEnumerator ClearLayerMedia(string[] data)
        {
            string panelName = "";
            int layer = 0;
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";

            Texture blendTex = null;

            // 将输入数据转换为参数字典以便访问
            var parameters = ConvertDataToParameters(data);
            
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"Unable to grab panel '{panelName}' because it is not a valid panel. Please check the panel name and adjust the command.");
                yield break;
            }
            
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: -1);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 若不是立即模式，则尝试获取转场速度参数
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);
            
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);
            
            // 非立即模式且提供了混合贴图名称时才进行加载
            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);

            // 根据是否指定图层来执行清除操作
            if (layer == -1)
                panel.Clear(transitionSpeed, blendTex, immediate);
            else
            {
                GraphicLayer graphicLayer = panel.GetLayer(layer);
                if (graphicLayer == null)
                {
                    Debug.LogError($"Could not clear layer [{layer}] on panel '{panel.panelName}'");
                    yield break;
                }
                
                graphicLayer.Clear(transitionSpeed, blendTex, immediate);
            }
        }
        
        /// <summary>
        /// 根据默认路径和媒体名称构造完整的资源路径。
        /// 若媒体名称以家目录符号开头，则直接使用其后部分作为相对路径。
        /// </summary>
        /// <param name="defaultPath">默认的基础路径。</param>
        /// <param name="graphicName">媒体文件名或带子目录的路径。</param>
        /// <returns>构建完成的资源路径字符串。</returns>
        private static string GetPathToGraphic(string defaultPath, string graphicName)
        {
            if (graphicName.StartsWith(HOME_DIRECTORY_SYMBOL))
                return graphicName.Substring(HOME_DIRECTORY_SYMBOL.Length);
            
            return defaultPath + graphicName;
        }
    }
}
