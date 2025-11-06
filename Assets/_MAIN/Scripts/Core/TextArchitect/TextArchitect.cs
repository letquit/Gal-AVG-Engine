using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// 文本构建器架构类，用于控制TMP文本组件的文字显示过程。
/// 支持多种构建类型（如即时、逐字等），并提供事件回调机制。
/// </summary>
public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;
    
    /// <summary>
    /// 获取当前使用的TMP文本对象（UI或世界空间）
    /// </summary>
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;
    
    /// <summary>
    /// 获取当前正在显示的文本内容
    /// </summary>
    public string currentText => tmpro.text;
    
    /// <summary>
    /// 目标要显示的文本内容
    /// </summary>
    public string targetText { get; private set; } = "";
    
    /// <summary>
    /// 完整的目标文本（包括前置文本和目标文本）
    /// </summary>
    public string fullTargetText => preText + targetText;
    
    /// <summary>
    /// 前置文本，在新文本之前显示的内容
    /// </summary>
    public string preText { get; private set; } = "";
    
    /// <summary>
    /// 文本颜色属性封装
    /// </summary>
    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }
    
    /// <summary>
    /// 显示速度设置，基于基础速度和倍数计算实际速度
    /// </summary>
    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    
    private const float baseSpeed = 1;
    private float speedMultiplier = 1;
    
    /// <summary>
    /// 每次更新周期中处理的字符数量，根据速度动态调整
    /// </summary>
    public int charactersPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; } }
    
    public int characterMultiplier = 1;
    public bool hurryUp = false;

    private Dictionary<string, Type> builders = new Dictionary<string, Type>();
    private TABuilder builder = null;
    private TABuilder.BuilderTypes _builderType;
    
    /// <summary>
    /// 当前使用的构建器类型
    /// </summary>
    public TABuilder.BuilderTypes builderType => _builderType;

    /// <summary>
    /// 字符输入事件，每当有字符被添加到文本时触发
    /// </summary>
    public event Action<char> OnCharacterTyped
    {
        add { if (builder != null) builder.OnCharacterTyped += value; }
        remove { if (builder != null) builder.OnCharacterTyped -= value; }
    }

    /// <summary>
    /// 多字符输入事件，每次批量添加字符时触发
    /// </summary>
    public event Action<int> OnMultipleCharactersTyped
    {
        add { if (builder != null) builder.OnMultipleCharactersTyped += value; }
        remove { if (builder != null) builder.OnMultipleCharactersTyped -= value; }
    }
    
    /// <summary>
    /// 对话开始事件，参数为即将显示的文本和预计耗时
    /// </summary>
    public event Action<string, float> OnDialogueStart;
    
    /// <summary>
    /// 计算文本中的有效字符数量（排除特殊指令等）
    /// </summary>
    private int CountEffectiveCharacters(string text)
    {
        // 使用正则表达式来移除所有 {command} 格式的标签
        string cleanText = Regex.Replace(text, @"\{.*?\}", "");
        return cleanText.Length;
    }

    /// <summary>
    /// 计算文本显示所需的总时间
    /// </summary>
    private float CalculateDisplayTime()
    {
        if (builderType == TABuilder.BuilderTypes.Instant)
            return 0;
            
        int effectiveCharCount = CountEffectiveCharacters(targetText);
        
        // 这个时间计算可以根据你的需要调整，这里使用了你原来的逻辑
        float timePerChar = 0.05f / speed;
        return effectiveCharCount * timePerChar;
    }

    private Coroutine buildProcess = null;
    
    /// <summary>
    /// 判断是否正在进行文本构建过程
    /// </summary>
    public bool isBuilding => buildProcess != null;

    /// <summary>
    /// 构造函数，初始化UI文本对象和默认构建器类型
    /// </summary>
    /// <param name="uiTextObject">TMP UI文本组件</param>
    /// <param name="builderType">构建器类型，默认为Instant</param>
    public TextArchitect(TextMeshProUGUI uiTextObject, TABuilder.BuilderTypes builderType = TABuilder.BuilderTypes.Instant)
    { 
        tmpro_ui = uiTextObject;
        AddBuilderTypes();
        SetBuilderType(builderType);
    }

    /// <summary>
    /// 构造函数，初始化世界空间文本对象和默认构建器类型
    /// </summary>
    /// <param name="worldTextObject">TMP世界空间文本组件</param>
    /// <param name="builderType">构建器类型，默认为Instant</param>
    public TextArchitect(TextMeshPro worldTextObject, TABuilder.BuilderTypes builderType = TABuilder.BuilderTypes.Instant)
    {
        tmpro_world = worldTextObject;
        AddBuilderTypes();
        SetBuilderType(builderType);
    }

    /// <summary>
    /// 动态加载所有继承自TABuilder的构建器类型
    /// </summary>
    private void AddBuilderTypes()
    {
        builders = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(TABuilder)))
            .ToDictionary(t => t.Name, t => t);
    }

    /// <summary>
    /// 设置当前使用的构建器类型，并创建对应的实例
    /// </summary>
    /// <param name="builderType">新的构建器类型</param>
    public void SetBuilderType(TABuilder.BuilderTypes builderType)
    {
        // 如果旧的 builder 存在，先移除事件监听，避免内存泄漏
        if (builder != null)
            builder.onComplete -= OnComplete;

        string name = TABuilder.CLASS_NAME_PREFIX + builderType.ToString();
        Type classType = builders[name];

        builder = Activator.CreateInstance(classType) as TABuilder;
        builder.architect = this;
        builder.onComplete += OnComplete;

        _builderType = builderType;
    }

    /// <summary>
    /// 开始构建指定文本内容
    /// </summary>
    /// <param name="text">要显示的文本内容</param>
    /// <returns>协程引用，可用于外部控制</returns>
    public Coroutine Build(string text)
    {
        preText = "";
        targetText = text;

        Stop();
        
        // 触发对话开始事件
        OnDialogueStart?.Invoke(targetText, CalculateDisplayTime());

        buildProcess = builder.Build();
        return buildProcess;
    }

    /// <summary>
    /// 在现有文本后追加新文本进行构建
    /// </summary>
    /// <param name="text">要追加的文本内容</param>
    /// <returns>协程引用，可用于外部控制</returns>
    public Coroutine Append(string text)
    {
        preText = currentText;
        targetText = text;

        Stop();
        
        // 触发对话开始事件
        OnDialogueStart?.Invoke(targetText, CalculateDisplayTime());

        buildProcess = builder.Build();
        return buildProcess;
    }

    /// <summary>
    /// 立即设置文本内容，跳过动画效果
    /// </summary>
    /// <param name="text">要立即显示的文本内容</param>
    public void SetText(string text)
    {
        preText = "";
        targetText = text;

        Stop();

        tmpro.text = targetText;
        builder.ForceComplete();
    }

    /// <summary>
    /// 停止当前正在进行的文本构建过程
    /// </summary>
    public void Stop()
    {
        if (isBuilding)
            tmpro.StopCoroutine(buildProcess);

        buildProcess = null;
    }

    /// <summary>
    /// 强制完成当前文本构建过程
    /// </summary>
    public void ForceComplete()
    {
        if (isBuilding)
            builder.ForceComplete();

        Stop();
        OnComplete();
    }
    
    /// <summary>
    /// 构建完成后调用的内部方法，重置状态标志
    /// </summary>
    private void OnComplete()
    {
        hurryUp = false;
        buildProcess = null;
    }
}