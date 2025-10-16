using UnityEngine;

/// <summary>
/// 图形层类，用于管理图形对象的显示和过渡效果
/// </summary>
public class GraphicLayer
{
    public const string LAYER_OBJECT_NAME_FORMAT = "Layer: {0}";
    public int layerDepth = 0;
    public Transform panel;
    
    public GraphicObject currentGraphic { get; private set; } = null;

    /// <summary>
    /// 通过文件路径设置纹理，并可选地应用过渡效果
    /// </summary>
    /// <param name="filePath">资源文件路径（相对于Resources文件夹）</param>
    /// <param name="transitionSpeed">过渡速度，默认为1f</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    public void SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null)
    {
        // 从Resources加载纹理资源
        Texture tex = Resources.Load<Texture>(filePath);

        if (tex == null)
        {
            Debug.LogError($"Could not load graphic texture from path '{filePath}'. Please ensure it exits within Resources!");
            return;
        }
        
        SetTexture(tex, transitionSpeed, blendingTexture, filePath);
    }
    
    /// <summary>
    /// 直接设置纹理对象，并可选地应用过渡效果
    /// </summary>
    /// <param name="tex">要设置的纹理对象</param>
    /// <param name="transitionSpeed">过渡速度，默认为1f</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    /// <param name="filepath">文件路径，用于标识纹理来源，默认为空字符串</param>
    public void SetTexture(Texture tex, float transitionSpeed = 1f, Texture blendingTexture = null,
        string filepath = "")
    {
        CreateGraphic(tex, transitionSpeed, filepath, blendingTexture: blendingTexture);
    }

    /// <summary>
    /// 创建图形对象并应用淡入效果
    /// </summary>
    /// <typeparam name="T">图形数据类型</typeparam>
    /// <param name="graphicData">图形数据对象</param>
    /// <param name="transitionSpeed">过渡速度</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="useAudioForVideo">是否为视频使用音频，默认为true</param>
    /// <param name="blendingTexture">混合纹理，默认为null</param>
    private void CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = true,
        Texture blendingTexture = null)
    {
        GraphicObject newGraphic = null;
        
        // 根据图形数据类型创建相应的图形对象
        if (graphicData is Texture)
            newGraphic = new GraphicObject(this, filePath, graphicData as Texture);
        
        currentGraphic = newGraphic;

        // 应用淡入效果
        currentGraphic.FadeIn(transitionSpeed, blendingTexture);
    }
}

