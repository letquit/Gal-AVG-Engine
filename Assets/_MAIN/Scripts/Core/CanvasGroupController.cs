using System.Collections;
using DIALOGUE;
using UnityEngine;

/// <summary>
/// 控制CanvasGroup的显示与隐藏，支持淡入淡出动画效果。
/// 用于管理UI元素（如对话框）的可见性状态和过渡动画。
/// </summary>
public class CanvasGroupController
{
    private const float DEFAULT_FADE_SPEED = 3f;
    
    private MonoBehaviour owner;
    private CanvasGroup rootCG;

    private Coroutine co_showing = null;
    private Coroutine co_hiding = null;
        
    /// <summary>
    /// 指示当前是否正在执行显示动画
    /// </summary>
    public bool isShowing => co_showing != null;
        
    /// <summary>
    /// 指示当前是否正在执行隐藏动画
    /// </summary>
    public bool isHiding => co_hiding != null;
        
    /// <summary>
    /// 指示当前是否正在进行淡入或淡出操作
    /// </summary>
    public bool isFading => isShowing || isHiding;
        
    /// <summary>
    /// 判断对话框当前是否可见（包括正在显示的状态）
    /// </summary>
    public bool isVisible => co_showing != null || rootCG.alpha > 0;
    
    /// <summary>
    /// 初始化CanvasGroupController实例
    /// </summary>
    /// <param name="owner">拥有此控制器的MonoBehaviour对象，用于启动协程</param>
    /// <param name="rootCg">要控制的CanvasGroup组件</param>
    public CanvasGroupController(MonoBehaviour owner, CanvasGroup rootCg)
    {
        this.owner = owner;
        this.rootCG = rootCg;
    }
    
    /// <summary>
    /// 显示对话框并启动淡入动画。如果已经在显示则直接返回当前协程；如果正在隐藏，则先停止隐藏协程。
    /// </summary>
    /// <param name="speed">淡入速度倍数，默认为1</param>
    /// <param name="immediate">是否立即完成显示，不播放动画</param>
    /// <returns>表示淡入过程的协程引用</returns>
    public Coroutine Show(float speed = 1f, bool immediate = false)
    {
        if (isShowing)
            return co_showing;
        else if (isHiding)
        {
            DialogueSystem.instance.StopCoroutine(co_hiding);
            co_hiding = null;
        }
            
        co_showing = DialogueSystem.instance.StartCoroutine(Fading(1, speed, immediate));
            
        return co_showing;
    }
        
    /// <summary>
    /// 隐藏对话框并启动淡出动画。如果已经在隐藏则直接返回当前协程；如果正在显示，则先停止显示协程。
    /// </summary>
    /// <param name="speed">淡出速度倍数，默认为1</param>
    /// <param name="immediate">是否立即完成隐藏，不播放动画</param>
    /// <returns>表示淡出过程的协程引用</returns>
    public Coroutine Hide(float speed = 1f, bool immediate = false)
    {
        if (isHiding)
            return co_hiding;
        else if (isShowing)
        {
            DialogueSystem.instance.StopCoroutine(co_showing);
            co_showing = null;
        }
            
        co_hiding = DialogueSystem.instance.StartCoroutine(Fading(0, speed, immediate));
            
        return co_hiding;
    }

    /// <summary>
    /// 执行淡入/淡出的核心逻辑，通过修改CanvasGroup的alpha值实现渐变效果
    /// </summary>
    /// <param name="alpha">目标alpha值：1表示完全显示，0表示完全隐藏</param>
    /// <param name="speed">动画速度倍数</param>
    /// <param name="immediate">是否立即设置为目标alpha值，跳过动画过程</param>
    /// <returns>可枚举的协程迭代器</returns>
    private IEnumerator Fading(float alpha, float speed, bool immediate)
    {
        CanvasGroup cg = rootCG;

        // 如果需要立即显示/隐藏，则直接设置alpha值
        if (immediate)
            cg.alpha = alpha;
        
        // 动画循环直到达到目标alpha值
        while (cg.alpha != alpha)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * DEFAULT_FADE_SPEED * speed);
            yield return null;
        }
            
        // 清理协程引用
        co_showing = null;
        co_hiding = null;
    }
}