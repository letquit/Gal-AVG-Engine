using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 文本构建器类，用于控制TextMeshPro文本的显示方式，支持即时显示、打字机效果和淡入效果。
/// </summary>
public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;
    
    /// <summary>
    /// 获取当前使用的TextMeshPro组件实例（UI或世界空间）
    /// </summary>
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;
    
    /// <summary>
    /// 获取当前显示的文本内容
    /// </summary>
    public string currentText => tmpro.text;
    
    /// <summary>
    /// 目标文本内容，即需要显示的完整文本
    /// </summary>
    public string targetText { get; private set; } = "";
    
    /// <summary>
    /// 前置文本内容，在追加模式下使用
    /// </summary>
    public string preText { get; private set; } = "";
    
    private int preTextLength = 0;
    
    /// <summary>
    /// 完整的目标文本，包含前置文本和目标文本
    /// </summary>
    public string fullTargetText => preText + targetText;
    
    /// <summary>
    /// 文本构建方式枚举
    /// </summary>
    public enum BuildMethod { instant, typewriter, fade }
    
    /// <summary>
    /// 当前使用的文本构建方式
    /// </summary>
    public BuildMethod buildMethod = BuildMethod.typewriter;
    
    /// <summary>
    /// 获取或设置文本颜色
    /// </summary>
    public Color textColor { get => tmpro.color; set => tmpro.color = value; }

    /// <summary>
    /// 获取或设置文本显示速度
    /// </summary>
    public float speed { get => baseSpeed * speedMultiplier; set => speedMultiplier = value; }
    
    private const float baseSpeed = 1;
    private float speedMultiplier = 1;

    /// <summary>
    /// 每个循环显示的字符数量，根据速度自动调整
    /// </summary>
    public int charactersPerCycle { get => speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; }
    
    /// <summary>
    /// 字符倍数，用于计算每个循环显示的字符数量
    /// </summary>
    public int characterMultiplier = 1;
    
    /// <summary>
    /// 是否加速显示文本
    /// </summary>
    public bool hurryUp = false;
    
    /// <summary>
    /// 构造函数，使用TextMeshProUGUI组件初始化
    /// </summary>
    /// <param name="tmpro_ui">TextMeshProUGUI组件实例</param>
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }
    
    /// <summary>
    /// 构造函数，使用TextMeshPro组件初始化
    /// </summary>
    /// <param name="tmpro_world">TextMeshPro组件实例</param>
    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    /// <summary>
    /// 构建并显示新文本
    /// </summary>
    /// <param name="text">要显示的文本内容</param>
    /// <returns>协程对象，可用于控制文本构建过程</returns>
    public Coroutine Build(string text)
    {
        preText = "";
        targetText = text;
        
        Stop();
        
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    /// <summary>
    /// 在现有文本基础上追加新文本
    /// </summary>
    /// <param name="text">要追加的文本内容</param>
    /// <returns>协程对象，可用于控制文本构建过程</returns>
    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;
        
        Stop();
        
        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }
    
    private Coroutine buildProcess = null;
    
    /// <summary>
    /// 检查是否正在构建文本
    /// </summary>
    public bool isBuilding => buildProcess != null;

    /// <summary>
    /// 停止当前的文本构建过程
    /// </summary>
    public void Stop()
    {
        if (!isBuilding)
            return;

        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }

    /// <summary>
    /// 文本构建的主协程方法
    /// </summary>
    /// <returns>IEnumerator对象，用于协程执行</returns>
    IEnumerator Building()
    {
        Prepare();
        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }
        
        OnComplete();
    }

    /// <summary>
    /// 文本构建完成后的处理方法
    /// </summary>
    private void OnComplete()
    {
        buildProcess = null;
        hurryUp = false;
    }

    /// <summary>
    /// 强制立即完成文本构建过程
    /// </summary>
    public void ForceComplete()
    {
        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                break;
        }
        
        Stop();
        OnComplete();
    }

    /// <summary>
    /// 根据构建方式准备文本显示
    /// </summary>
    private void Prepare()
    {
        switch (buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }
    
    /// <summary>
    /// 准备即时显示文本
    /// </summary>
    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }
    
    /// <summary>
    /// 准备打字机效果显示文本
    /// </summary>
    private void Prepare_Typewriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }
    
    /// <summary>
    /// 准备淡入效果显示文本
    /// </summary>
    private void Prepare_Fade()
    {
        
    }

    /// <summary>
    /// 打字机效果的实现协程
    /// </summary>
    /// <returns>IEnumerator对象，用于协程执行</returns>
    private IEnumerator Build_Typewriter()
    {
        while (tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp ? charactersPerCycle * 5 : charactersPerCycle;

            yield return new WaitForSeconds(0.015f / speed);
        }
    }

    /// <summary>
    /// 淡入效果的实现协程
    /// </summary>
    /// <returns>IEnumerator对象，用于协程执行</returns>
    private IEnumerator Build_Fade()
    {
        yield return null;
    }
}
