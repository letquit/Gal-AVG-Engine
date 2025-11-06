using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 实现文字淡入效果的构建器类，继承自TABuilder。
/// 该类通过逐字符控制顶点颜色透明度，实现文本淡入显示效果。
/// </summary>
public class TABuilder_Fade : TABuilder
{
    private int pretextlength = 0;

    /// <summary>
    /// 启动构建过程，开始执行文字淡入动画。
    /// </summary>
    /// <returns>用于控制协程的Coroutine对象</returns>
    public override Coroutine Build()
    {
        Prepare();

        return architect.tmpro.StartCoroutine(Building());
    }

    /// <summary>
    /// 强制立即完成构建过程，跳过动画直接显示完整文本。
    /// </summary>
    public override void ForceComplete()
    {
        architect.tmpro.ForceMeshUpdate();
    }

    /// <summary>
    /// 准备阶段：初始化文本显示状态，设置预文本和目标文本，
    /// 并根据是否为预文本字符设置初始透明度（预文本可见，目标文本不可见）。
    /// </summary>
    private void Prepare()
    {
        architect.tmpro.text = architect.preText;
        if (architect.preText != "")
        {
            architect.tmpro.ForceMeshUpdate();
            pretextlength = architect.tmpro.textInfo.characterCount;
        }
        else
            pretextlength = 0;

        architect.tmpro.text += architect.targetText;
        architect.tmpro.maxVisibleCharacters = int.MaxValue;
        architect.tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = architect.tmpro.textInfo;

        Color colorVisible = new Color(architect.textColor.r, architect.textColor.g, architect.textColor.b, 1);
        Color colorHidden = new Color(architect.textColor.r, architect.textColor.g, architect.textColor.b, 0);

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        // 遍历所有字符，根据是否为预文本字符设置初始透明度
        for(int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            if (i < pretextlength)
            {
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v] = colorVisible;
            }
            else
            {
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v] = colorHidden;
            }
        }

        architect.tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    /// <summary>
    /// 构建协程：逐字符控制透明度，实现淡入动画效果。
    /// 使用alpha阈值判断字符是否开始淡入，并动态扩展淡入范围。
    /// </summary>
    /// <returns>IEnumerator用于协程控制</returns>
    private IEnumerator Building()
    {
        // minRange表示当前已完全显示的字符范围，maxRange表示正在处理的字符范围
        int minRange = pretextlength;
        int maxRange = minRange + 1;

        // alpha阈值，用于判断字符是否开始淡入
        byte alphaThreshold = 15;

        TMP_TextInfo textInfo = architect.tmpro.textInfo;

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alphas = new float[textInfo.characterCount];

        while(true)
        {
            // 计算淡入速度，支持加速模式
            float fadeSpeed = (architect.hurryUp ? architect.charactersPerCycle * 5 : architect.charactersPerCycle) * architect.speed * 4f;
            
            // 处理当前范围内的字符，逐步增加透明度
            for (int i = minRange; i < maxRange; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                // 跳过不可见字符或索引异常的字符
                if (!charInfo.isVisible || charInfo.index < minRange)
                    continue;

                alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);

                // 更新字符四个顶点的透明度
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];

                // 如果字符已完全显示，扩展已显示范围
                if (alphas[i] >= 255)
                    minRange++;
            }

            architect.tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // 判断是否需要扩展处理范围
            bool lastCharacterIsInvisible = !textInfo.characterInfo[maxRange - 1].isVisible;
            
            if(lastCharacterIsInvisible || alphas[maxRange - 1] > alphaThreshold)
            {
                if (maxRange < textInfo.characterCount)
                    maxRange++;
                else if (alphas[maxRange - 1] >= 255)
                    break;
            }

            yield return null;
        }

        OnComplete();
    }
}