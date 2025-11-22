using System;
using System.Collections;
using UnityEngine;

namespace COMMANDS
{
    /// <summary>
    /// 提供画廊图像显示与隐藏功能的命令扩展类。
    /// </summary>
    public class CMD_DatabaseExtension_Gallery : CMD_DatabaseExtension
    {
        // 参数键名数组，用于解析传入命令中的参数
        private static string[] PARAM_MEDIA = new string[] { "-m", "-media" };
        private static string[] PARAM_SPEED = new string[] { "-spd", "-speed" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static string[] PARAM_BLENDTEX = new string[] { "-b", "-blend" };
        
        /// <summary>
        /// 扩展命令数据库，注册画廊图像相关的命令。
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例。</param>
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("showgalleryimage", new Func<string[], IEnumerator>(ShowGalleryImage));
            database.AddCommand("hidegalleryimage", new Func<string[], IEnumerator>(HideGalleryImage));
        }

        /// <summary>
        /// 隐藏当前画廊图像，并支持过渡效果。
        /// </summary>
        /// <param name="data">来自命令调用的原始参数数据。</param>
        /// <returns>协程迭代器对象。</returns>
        public static IEnumerator HideGalleryImage(string[] data)
        {
            GraphicLayer graphicLayer = GraphicPanelManager.instance.GetPanel("Cinematic")
                .GetLayer(0, createIfDoesNotExist: true);
            
            if (graphicLayer.currentGraphic == null)
                yield break;
            
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            Texture blendTex = null;
            
            // 将输入数据转换为参数字典以便访问
            var parameters = ConvertDataToParameters(data);
            
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 若不是立即模式，则尝试获取转场速度参数
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);
            
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);
            
            // 非立即模式且提供了混合贴图名称时才进行加载
            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);
            
            if (!immediate)
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    // Debug.Log("CLEAR");
                    graphicLayer.Clear(immediate: true);
                });
            
            graphicLayer.Clear(transitionSpeed, blendTex, immediate);

            if (graphicLayer.currentGraphic != null)
            {
                var graphicObject = graphicLayer.currentGraphic;
                yield return new WaitUntil(() => graphicObject == null);
            }
        }
        
        /// <summary>
        /// 显示指定的画廊图像，并支持过渡效果。
        /// </summary>
        /// <param name="data">来自命令调用的原始参数数据。</param>
        /// <returns>协程迭代器对象。</returns>
        public static IEnumerator ShowGalleryImage(string[] data)
        {
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            Texture blendTex = null;
            
            // 将输入数据转换为参数字典以便访问
            var parameters = ConvertDataToParameters(data);
            
            parameters.TryGetValue(PARAM_MEDIA, out mediaName);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 若不是立即模式，则尝试获取转场速度参数
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);
            
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);
            
            string pathToGraphic = FilePaths.resources_gallery + mediaName;
            Texture graphic = Resources.Load<Texture>(pathToGraphic);

            if (graphic == null)
            {
                Debug.LogError($"Could not find gallery image called '{mediaName}' in the Resources '{FilePaths.resources_gallery}' directory.");
                yield break;
            }
            
            // 非立即模式且提供了混合贴图名称时才进行加载
            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);

            GraphicLayer graphicLayer = GraphicPanelManager.instance.GetPanel("Cinematic")
                .GetLayer(0, createIfDoesNotExist: true);
            
            if (!immediate)
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    graphicLayer?.SetTexture(graphic, filePath: pathToGraphic, immediate: true); 
                });
            
            GalleryConfig.UnlockImage(mediaName);

            yield return graphicLayer.SetTexture(graphic, transitionSpeed, blendTex, pathToGraphic, immediate);
        }
    }
}