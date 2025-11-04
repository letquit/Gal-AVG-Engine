using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace History
{
    /// <summary>
    /// 历史资源缓存管理类，用于加载和缓存各种资源对象
    /// </summary>
    public class HistoryCache
    {
        /// <summary>
        /// 已加载资源的缓存字典，键为资源路径，值为资源对象和过期索引的元组
        /// </summary>
        public static Dictionary<string, (object asset, int staleIndex)> loadedAssets = new Dictionary<string, (object asset, int staleIndex)>();

        /// <summary>
        /// 尝试从缓存中加载指定类型的资源对象
        /// </summary>
        /// <typeparam name="T">要加载的资源类型</typeparam>
        /// <param name="key">资源的路径键名</param>
        /// <returns>成功加载则返回对应类型的资源对象，否则返回默认值</returns>
        public static T TryLoadObject<T>(string key)
        {
            object resource = null;
            
            // 检查资源是否已缓存
            if (loadedAssets.ContainsKey(key))
                resource = (T)loadedAssets[key].asset;
            else
            {
                // 从Resources目录加载资源
                resource = Resources.Load(key);
                if (resource != null)
                {
                    // 将新加载的资源添加到缓存中
                    loadedAssets[key] = (resource, 0);
                }
            }

            if (resource != null)
            {
                // 验证资源类型是否匹配
                if (resource is T)
                    return (T)resource;
                else
                    Debug.LogWarning($"Retrieved object '{key}' was not the expected type!");   
            }

            Debug.LogWarning($"Could not load object from cache '{key}'");
            return default(T);
        }
        
        /// <summary>
        /// 加载字体资源
        /// </summary>
        /// <param name="key">字体资源的路径键名</param>
        /// <returns>成功加载则返回TMP_FontAsset对象，否则返回null</returns>
        public static TMP_FontAsset LoadFont(string key) => TryLoadObject<TMP_FontAsset>(key);
        
        /// <summary>
        /// 加载音频资源
        /// </summary>
        /// <param name="key">音频资源的路径键名</param>
        /// <returns>成功加载则返回AudioClip对象，否则返回null</returns>
        public static AudioClip LoadAudio(string key) => TryLoadObject<AudioClip>(key);
        
        /// <summary>
        /// 加载图片资源
        /// </summary>
        /// <param name="key">图片资源的路径键名</param>
        /// <returns>成功加载则返回Texture2D对象，否则返回null</returns>
        public static Texture2D LoadImage(string key) => TryLoadObject<Texture2D>(key);
        
        /// <summary>
        /// 加载视频资源
        /// </summary>
        /// <param name="key">视频资源的路径键名</param>
        /// <returns>成功加载则返回VideoClip对象，否则返回null</returns>
        public static VideoClip LoadVideo(string key) => TryLoadObject<VideoClip>(key);
    }
}