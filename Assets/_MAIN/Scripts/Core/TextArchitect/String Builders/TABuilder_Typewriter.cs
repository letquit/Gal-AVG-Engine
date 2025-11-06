using System.Collections;
using UnityEngine;

/// <summary>
/// 实现打字机效果的文本动画构建器。
/// 继承自TABuilder，通过逐字符显示实现文本逐渐出现的效果。
/// </summary>
public class TABuilder_Typewriter : TABuilder
{
    /// <summary>
    /// 启动打字机动画构建过程。
    /// 首先调用Prepare方法初始化状态，然后启动Building协程来逐步显示文本。
    /// </summary>
    /// <returns>用于控制动画执行的协程对象。</returns>
    public override Coroutine Build()
    {
        Prepare();
        return architect.tmpro.StartCoroutine(Building());
    }

    /// <summary>
    /// 强制立即完成整个打字机动画。
    /// 将当前可见字符数设置为总字符数以瞬间展示全部内容。
    /// </summary>
    public override void ForceComplete()
    {
        architect.tmpro.maxVisibleCharacters = architect.tmpro.textInfo.characterCount;
    }

    /// <summary>
    /// 初始化文本组件的状态，准备开始打字机动画。
    /// 设置初始颜色、隐藏所有字符，并拼接预设文本和目标文本。
    /// </summary>
    private void Prepare()
    {
        architect.tmpro.color = architect.tmpro.color;
        architect.tmpro.maxVisibleCharacters = 0;
        architect.tmpro.text = architect.preText;

        if (architect.preText != "")
        {
            architect.tmpro.ForceMeshUpdate();
            architect.tmpro.maxVisibleCharacters = architect.tmpro.textInfo.characterCount;
        }

        architect.tmpro.text += architect.targetText;
        architect.tmpro.ForceMeshUpdate();
    }

    /// <summary>
    /// 执行实际的打字机动画逻辑，在协程中按周期递增可见字符数量。
    /// 根据是否加速调整每次增加的字符数及等待时间。
    /// 当有新字符被显示时会触发相应的事件回调。
    /// </summary>
    /// <returns>IEnumerator接口支持Unity协程机制。</returns>
    private IEnumerator Building()
    {
        // 循环直到所有字符都已显示
        while (architect.tmpro.maxVisibleCharacters < architect.tmpro.textInfo.characterCount)
        {
            int oldCharCount = architect.tmpro.maxVisibleCharacters;
            // 计算本次要增加的字符数量（根据是否加速决定）
            int charCountToAdd = architect.hurryUp ? architect.charactersPerCycle * 5 : architect.charactersPerCycle;
            architect.tmpro.maxVisibleCharacters += charCountToAdd;

            // 确保不超过总的字符数量
            int newCharCount = Mathf.Min(architect.tmpro.maxVisibleCharacters, architect.tmpro.textInfo.characterCount);
            
            // 检查是否有新增加的字符需要处理
            if (newCharCount > oldCharCount)
            {
                // 如果只增加了一个字符，则检查该字符类型并触发单个字符事件
                if (newCharCount - oldCharCount == 1)
                {
                    char typedChar = architect.tmpro.textInfo.characterInfo[oldCharCount].character;
                    if (char.IsLetterOrDigit(typedChar)) // 只对字母或数字触发事件
                    {
                        FireOnCharacterTyped(typedChar);
                    }
                }
                // 如果增加了多个字符，则统一触发批量字符事件
                else
                {
                    FireOnMultipleCharactersTyped(newCharCount - oldCharCount);
                }
            }

            // 控制播放速度：正常速度或加速模式下的间隔时间
            yield return new WaitForSeconds(0.05f / (architect.hurryUp ? architect.speed * 5 : architect.speed));
        }

        // 动画完成后调用结束回调
        OnComplete();
    }
}