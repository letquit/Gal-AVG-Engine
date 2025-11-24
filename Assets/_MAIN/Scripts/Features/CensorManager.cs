using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 敏感词过滤管理器类
/// 负责检测和替换文本中的敏感词汇
/// </summary>
public class CensorManager
{
    /// <summary>
    /// 敏感词字典，键为替换后的词汇，值为匹配模式
    /// 包含常见的变体匹配规则
    /// </summary>
    private static Dictionary<string, string> badWords = new Dictionary<string, string>()
    {
        { "badword1", "b[a@4]dw[0o]rd1" },
        { "stinking", "[s\\$]t[i1]nk[i1]ng" }
    };
    
    /// <summary>
    /// 严格屏蔽词字典，键为替换后的词汇，值为匹配模式
    /// 这些词汇会被无条件屏蔽
    /// </summary>
    private static Dictionary<string, string> hardBlocks = new Dictionary<string, string>()
    {
        { "tofu", "t[oO]fu" }
    };

    /// <summary>
    /// 对输入文本进行敏感词过滤处理
    /// 将匹配到的敏感词替换为星号(*)字符
    /// </summary>
    /// <param name="text">需要进行过滤的文本，通过引用传递以便直接修改</param>
    /// <returns>如果文本中包含被过滤的敏感词则返回true，否则返回false</returns>
    public static bool Censor(ref string text)
    {
        bool isCensored = false;

        // 处理严格屏蔽词汇
        // 这些词汇无论在何处出现都会被屏蔽
        foreach (var pair in hardBlocks)
        {
            Regex regex = new Regex(pair.Value, RegexOptions.IgnoreCase);

            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, math => new string('*', math.Length));
                isCensored = true;
            }
        }
        
        // 处理普通敏感词汇
        // 使用单词边界确保只匹配完整的词汇
        foreach (var pair in badWords)
        {
            string pattern = $"(?<=\\W|^){pair.Value}(?=\\W|$)";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (regex.IsMatch(text))
            {
                text = regex.Replace(text, math => new string('*', math.Length));
                isCensored = true;
            }
        }
        
        return isCensored;
    }
}