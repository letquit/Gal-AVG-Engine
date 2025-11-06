using UnityEngine;

/// <summary>
/// TABuilder_Instant类继承自TABuilder，用于立即显示文本内容
/// </summary>
public class TABuilder_Instant : TABuilder
{
    /// <summary>
    /// 立即构建并显示完整的文本内容
    /// </summary>
    /// <returns>返回null，因为是立即执行不涉及协程</returns>
    public override Coroutine Build()
    {
        // 设置文本颜色（此处为保持原有颜色不变）
        architect.tmpro.color = architect.tmpro.color;
        
        // 设置要显示的完整文本内容
        architect.tmpro.text = architect.fullTargetText;
        
        // 强制更新文本网格以确保显示正确
        architect.tmpro.ForceMeshUpdate();
        
        // 设置最大可见字符数为文本总字符数，确保所有文本都可见
        architect.tmpro.maxVisibleCharacters = architect.tmpro.textInfo.characterCount;

        // 调用完成回调方法
        OnComplete();

        return null;
    }
}