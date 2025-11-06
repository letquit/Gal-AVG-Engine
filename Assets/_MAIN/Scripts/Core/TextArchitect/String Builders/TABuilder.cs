using System;
using UnityEngine;


/// <summary>
/// 文本构建器抽象基类，用于定义不同类型的文本显示构建器
/// </summary>
public abstract class TABuilder
{

    /// <summary>
    /// 文本架构师引用，用于管理文本显示的核心逻辑
    /// </summary>
    public TextArchitect architect = null;
    
    /// <summary>
    /// 文本构建完成时触发的事件委托
    /// </summary>
    public delegate void TA_Event();
    
    /// <summary>
    /// 文本构建完成事件
    /// </summary>
    public event TA_Event onComplete;

    /// <summary>
    /// 当单个字符被输入时触发的事件
    /// </summary>
    public event Action<char> OnCharacterTyped;
    
    /// <summary>
    /// 当多个字符被输入时触发的事件
    /// </summary>
    public event Action<int> OnMultipleCharactersTyped;
    
    /// <summary>
    /// 调用 OnCharacterTyped 事件
    /// </summary>
    /// <param name="character">被输入的字符</param>
    protected void FireOnCharacterTyped(char character) => OnCharacterTyped?.Invoke(character);

    /// <summary>
    /// 调用 OnMultipleCharactersTyped 事件
    /// </summary>
    /// <param name="count">被输入的字符数量</param>
    protected void FireOnMultipleCharactersTyped(int count) => OnMultipleCharactersTyped?.Invoke(count);
    
    /// <summary>
    /// 类名前缀常量，用于标识TABuilder相关类
    /// </summary>
    public const string CLASS_NAME_PREFIX = "TABuilder_";

    /// <summary>
    /// 构建器类型枚举，定义了不同的文本显示方式
    /// </summary>
    public enum BuilderTypes
    {
        /// <summary>
        /// 打字机效果，逐字显示
        /// </summary>
        Typewriter,
        /// <summary>
        /// 瞬间显示，立即显示全部文本
        /// </summary>
        Instant,
        /// <summary>
        /// 淡入效果，文本逐渐显现
        /// </summary>
        Fade
    }

    /// <summary>
    /// 执行文本构建的核心方法，返回协程引用
    /// </summary>
    /// <returns>Unity协程对象，可用于控制构建过程</returns>
    public virtual Coroutine Build() => null;

    /// <summary>
    /// 强制完成文本构建过程
    /// </summary>
    public virtual void ForceComplete()
    {

    }

    /// <summary>
    /// 触发构建完成事件
    /// </summary>
    protected void OnComplete() => onComplete?.Invoke();
}
