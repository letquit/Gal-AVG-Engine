using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// GraphicObject 类用于管理图形对象（如图片或视频）在 UI 上的显示与过渡效果。
/// 支持淡入、淡出以及混合纹理过渡功能，并可附加音频源和视频播放器组件。
/// </summary>
public class GraphicObject
{
    private const string NAME_FORMAT = "Graphic - [{0}]";
    private const string MATERIAL_PATH = "Materials/layerTransitionMaterial";
    private const string MATERIAL_FIELD_COLOR = "_Color";
    private const string MATERIAL_FIELD_MAINTEX = "_MainTex";
    private const string MATERIAL_FIELD_BLENDTEX = "_BlendTex";
    private const string MATERIAL_FIELD_BLEND = "_Blend";
    private const string MATERIAL_FIELD_ALPHA = "_Alpha";
    public RawImage renderer;

    /// <summary>
    /// 判断当前图形是否是视频类型。
    /// </summary>
    public bool isVideo { get { return video != null; }}

    public VideoPlayer video = null;
    public AudioSource audio = null;
    
    public string graphicPath = "";

    /// <summary>
    /// 获取图形名称，该属性只读。
    /// </summary>
    public string graphicName { get; private set; }
    
    private Coroutine co_fadingIn = null;
    private Coroutine co_fadingOut = null;
    
    /// <summary>
    /// 构造一个新的 GraphicObject 实例。
    /// 创建一个带有 RawImage 组件的游戏对象并设置其材质及主纹理。
    /// </summary>
    /// <param name="layer">所属图层面板。</param>
    /// <param name="graphicPath">图形资源路径。</param>
    /// <param name="tex">要显示的纹理资源。</param>
    public GraphicObject(GraphicLayer layer, string graphicPath, Texture tex)
    {
        this.graphicPath = graphicPath;

        GameObject ob = new GameObject();
        ob.transform.SetParent(layer.panel);
        renderer = ob.AddComponent<RawImage>();

        graphicName = tex.name;

        InitGraphic();
        
        renderer.name = string.Format(NAME_FORMAT, graphicName);
        renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, tex);
    }
    
    /// <summary>
    /// GraphicObject构造函数，用于创建一个图形对象并初始化相关的视频播放组件
    /// </summary>
    /// <param name="layer">图形对象所属的图形层</param>
    /// <param name="graphicPath">图形资源的路径</param>
    /// <param name="clip">要播放的视频剪辑</param>
    /// <param name="useAudio">是否启用音频播放</param>
    public GraphicObject(GraphicLayer layer, string graphicPath, VideoClip clip, bool useAudio)
    {
        this.graphicPath = graphicPath;

        // 创建游戏对象并设置父级容器
        GameObject ob = new GameObject();
        ob.transform.SetParent(layer.panel);
        renderer = ob.AddComponent<RawImage>();

        graphicName = clip.name;
        renderer.name = string.Format(NAME_FORMAT, graphicName);

        InitGraphic();

        // 创建渲染纹理并设置材质
        RenderTexture tex = new RenderTexture(Mathf.RoundToInt(clip.width), Mathf.RoundToInt(clip.height), 0);
        renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, tex);
        
        // 初始化音频组件
        audio = renderer.gameObject.AddComponent<AudioSource>();
        audio.volume = 0;
        if (!useAudio)
            audio.mute = true;
        
        // 初始化视频播放器组件并配置相关参数
        video = renderer.gameObject.AddComponent<VideoPlayer>();
        video.playOnAwake = true;
        video.source = VideoSource.VideoClip;
        video.clip = clip;
        video.renderMode = VideoRenderMode.RenderTexture;
        video.targetTexture = tex;
        video.isLooping = true;
        video.audioOutputMode = VideoAudioOutputMode.AudioSource;
        video.SetTargetAudioSource(0, audio);
        video.frame = 0;
        video.Prepare();
        video.Play();
    }

    /// <summary>
    /// 初始化图形渲染相关参数：位置缩放、锚点偏移、加载过渡材质等。
    /// </summary>
    private void InitGraphic()
    {
        renderer.transform.localPosition = Vector3.zero;
        renderer.transform.localScale = Vector3.one;

        RectTransform rect = renderer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.one;
        
        renderer.material = GetTransitionMaterial();
        
        renderer.material.SetFloat(MATERIAL_FIELD_BLEND, 0);
        renderer.material.SetFloat(MATERIAL_FIELD_ALPHA, 0);
    }

    /// <summary>
    /// 加载并返回过渡用的 Shader 材质副本。
    /// 若未找到指定路径下的材质则返回 null。
    /// </summary>
    /// <returns>复制后的过渡材质实例；若失败则返回 null。</returns>
    private Material GetTransitionMaterial()
    {
        Material mat = Resources.Load<Material>(MATERIAL_PATH);
        
        if (mat != null)
            return new Material(mat);

        return null;
    }
    
    GraphicPanelManager pandManager => GraphicPanelManager.instance;

    /// <summary>
    /// 启动图形淡入协程。如果正在执行淡出操作，则先停止它。
    /// </summary>
    /// <param name="speed">过渡速度系数。</param>
    /// <param name="blend">用于混合过渡的纹理，默认为 null。</param>
    /// <returns>启动的协程引用。</returns>
    public Coroutine FadeIn(float speed, Texture blend = null)
    {
        if (co_fadingOut != null)
            pandManager.StopCoroutine(co_fadingOut);
        
        if (co_fadingIn != null)
            return co_fadingIn;
        
        co_fadingIn = pandManager.StartCoroutine(Fading(1, speed, blend));
        
        return co_fadingIn;
    }
    
    /// <summary>
    /// 启动图形淡出协程。如果正在执行淡入操作，则先停止它。
    /// </summary>
    /// <param name="speed">过渡速度系数。</param>
    /// <param name="blend">用于混合过渡的纹理，默认为 null。</param>
    /// <returns>启动的协程引用。</returns>
    public Coroutine FadeOut(float speed, Texture blend = null)
    {
        if (co_fadingIn != null)
            pandManager.StopCoroutine(co_fadingIn);
        
        if (co_fadingOut != null)
            return co_fadingOut;
        
        co_fadingOut = pandManager.StartCoroutine(Fading(0, speed, blend));
        
        return co_fadingOut;
    }

    /// <summary>
    /// 执行实际的颜色/纹理渐变逻辑，控制透明度从当前值向目标值变化。
    /// </summary>
    /// <param name="target">目标透明度值（0 表示完全透明，1 表示完全不透明）。</param>
    /// <param name="speed">过渡速率。</param>
    /// <param name="blend">混合使用的纹理。</param>
    /// <returns>IEnumerator 接口供协程使用。</returns>
    private IEnumerator Fading(float target, float speed, Texture blend)
    {
        bool isBlending = blend != null;
        bool fadingIn = target > 0;
        
        // 设置混合纹理和初始透明度状态
        renderer.material.SetTexture(MATERIAL_FIELD_BLENDTEX, blend);
        renderer.material.SetFloat(MATERIAL_FIELD_ALPHA, isBlending ? 1 : fadingIn ? 0 : 1);
        renderer.material.SetFloat(MATERIAL_FIELD_BLEND, isBlending ? fadingIn ? 0 : 1 : 1);
        
        string opacityParam = isBlending ? MATERIAL_FIELD_BLEND : MATERIAL_FIELD_ALPHA;

        // 持续更新透明度直到达到目标值
        while (renderer.material.GetFloat(opacityParam) != target)
        {
            float opacity = Mathf.MoveTowards(renderer.material.GetFloat(opacityParam), target, speed * Time.deltaTime * GraphicPanelManager.DEFAULT_TRANSITION_SPEED);
            renderer.material.SetFloat(opacityParam, opacity);
            
            if (isVideo)
                audio.volume = opacity;
            
            yield return null;
        }

        // 清除协程引用以避免重复调用问题
        co_fadingIn = null;
        co_fadingOut = null;
    }
}
