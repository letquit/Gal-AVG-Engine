using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 图形层类，用于管理图形对象的显示和过渡效果
/// </summary>
public class GraphicLayer
{
    public const string LAYER_OBJECT_NAME_FORMAT = "Layer: {0}";
    public int layerDepth = 0;
    public Transform panel;
    
    public GraphicObject currentGraphic = null;
    private List<GraphicObject> oldGraphics = new List<GraphicObject>();

    /// <summary>
    /// 通过文件路径设置纹理，并可选地应用过渡效果
    /// </summary>
    /// <param name="filePath">资源文件路径（相对于Resources文件夹）</param>
    /// <param name="transitionSpeed">过渡速度，默认为1f</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    /// <param name="immediate">是否立即显示，不使用过渡效果，默认为false</param>
    /// <returns>用于控制过渡动画的协程</returns>
    public Coroutine SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null, bool immediate = false)
    {
        // 从Resources加载纹理资源
        Texture tex = Resources.Load<Texture2D>(filePath);

        if (tex == null)
        {
            Debug.LogError($"Could not load graphic texture from path '{filePath}'. Please ensure it exits within Resources!");
            return null;
        }
        
        return SetTexture(tex, transitionSpeed, blendingTexture, filePath);
    }
    
    /// <summary>
    /// 直接设置纹理对象，并可选地应用过渡效果
    /// </summary>
    /// <param name="tex">要设置的纹理对象</param>
    /// <param name="transitionSpeed">过渡速度，默认为1f</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    /// <param name="filepath">文件路径，用于标识纹理来源，默认为空字符串</param>
    /// <param name="immediate">是否立即显示，不使用过渡效果，默认为false</param>
    /// <returns>用于控制过渡动画的协程</returns>
    public Coroutine SetTexture(Texture tex, float transitionSpeed = 1f, Texture blendingTexture = null,
        string filepath = "", bool immediate = false)
    {
        return CreateGraphic(tex, transitionSpeed, filepath, blendingTexture: blendingTexture, immediate: immediate);
    }

    /// <summary>
    /// 通过文件路径加载视频剪辑并设置为当前图形内容
    /// </summary>
    /// <param name="filePath">资源文件路径（相对于Resources文件夹）</param>
    /// <param name="transitionSpeed">过渡速度，默认为1f</param>
    /// <param name="useAudio">是否启用音频播放，默认为true</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    /// <param name="immediate">是否立即显示，不使用过渡效果，默认为false</param>
    /// <returns>用于控制过渡动画的协程</returns>
    public Coroutine SetVideo(string filePath, float transitionSpeed = 1f, bool useAudio = true,
        Texture blendingTexture = null, bool immediate = false)
    {
        // 从Resources加载视频资源
        VideoClip clip = Resources.Load<VideoClip>(filePath);

        if (clip == null)
        {
            Debug.LogError($"Could not load graphic video from path '{filePath}'. Please ensure it exits within Resources!");
            return null;
        }
        
        return SetVideo(clip, transitionSpeed, useAudio, blendingTexture, filePath);
    }
    
    /// <summary>
    /// 直接设置视频剪辑对象，并可选地应用过渡效果
    /// </summary>
    /// <param name="video">要设置的视频剪辑对象</param>
    /// <param name="transitionSpeed">过渡速度，默认为1f</param>
    /// <param name="useAudio">是否启用音频播放，默认为true</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    /// <param name="filepath">文件路径，用于标识视频来源，默认为空字符串</param>
    /// <param name="immediate">是否立即显示，不使用过渡效果，默认为false</param>
    /// <returns>用于控制过渡动画的协程</returns>
    public Coroutine SetVideo(VideoClip video, float transitionSpeed = 1f, bool useAudio = true,
        Texture blendingTexture = null, string filepath = "", bool immediate = false)
    {
        return CreateGraphic(video, transitionSpeed, filepath, useAudio, blendingTexture, immediate);
    }

    /// <summary>
    /// 创建图形对象并应用淡入效果
    /// </summary>
    /// <typeparam name="T">图形数据类型，支持Texture或VideoClip</typeparam>
    /// <param name="graphicData">图形数据对象</param>
    /// <param name="transitionSpeed">过渡速度</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="useAudioForVideo">是否为视频使用音频，默认为true</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    /// <param name="immediate">是否立即显示，不使用过渡效果，默认为false</param>
    /// <returns>用于控制过渡动画的协程</returns>
    private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = true,
        Texture blendingTexture = null, bool immediate = false)
    {
        GraphicObject newGraphic = null;
        
        // 根据图形数据类型创建相应的图形对象
        if (graphicData is Texture)
            newGraphic = new GraphicObject(this, filePath, graphicData as Texture, immediate);
        else if (graphicData is VideoClip)
            newGraphic = new GraphicObject(this, filePath, graphicData as VideoClip, useAudioForVideo, immediate);
        
        if (currentGraphic != null && !oldGraphics.Contains(currentGraphic))
            oldGraphics.Add(currentGraphic);
        
        currentGraphic = newGraphic;

        // 应用淡入效果
        if (!immediate)
            return currentGraphic.FadeIn(transitionSpeed, blendingTexture);

        DestroyOldGraphics();
        return null;
    }

    /// <summary>
    /// 销毁旧的图形对象
    /// </summary>
    public void DestroyOldGraphics()
    {
        // 遍历并销毁所有旧图形对象的游戏对象
        foreach (var g in oldGraphics)
            Object.Destroy(g.renderer.gameObject);
        
        oldGraphics.Clear();
    }

    /// <summary>
    /// 清除当前和旧的图形对象，应用淡出效果
    /// </summary>
    public void Clear()
    {
        // 对当前图形对象应用淡出效果
        if (currentGraphic != null)
            currentGraphic.FadeOut();

        // 对所有旧图形对象应用淡出效果
        foreach (var g in oldGraphics)
            g.FadeOut();
    }
}
