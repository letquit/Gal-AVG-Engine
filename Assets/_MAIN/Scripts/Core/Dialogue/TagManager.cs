using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 标签管理器类，用于管理和替换文本中的标签
/// </summary>
public class TagManager
{
    /// <summary>
    /// 标签字典，用于存储标签及其对应的值获取函数
    /// </summary>
    private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
    {
        { "<mainChar>", () => "Avira" },
        { "<time>", () => DateTime.Now.ToString("hh:mm tt") },
        { "<playerLevel>", () => "15" },
        { "<input>", () => InputPanel.instance.lastInput },
        { "<tempVal1>", () => "42" },
    };
    
    /// <summary>
    /// 标签正则表达式，用于匹配形如<word>格式的标签
    /// </summary>
    private static readonly Regex tagRegex = new Regex("<\\w+>");

    /// <summary>
    /// 对输入文本注入标签和变量值
    /// </summary>
    /// <param name="text">需要处理的原始文本</param>
    /// <param name="injectTags">是否注入标签（默认为true）</param>
    /// <param name="injectVariables">是否注入变量（默认为true）</param>
    /// <returns>经过标签与变量替换后的文本</returns>
    public static string Inject(string text, bool injectTags = true, bool injectVariables = true)
    {
        if (injectTags)
            text = InjectTags(text);
        
        if (injectVariables)
            text = InjectVariables(text);
        
        return text;
    }

    /// <summary>
    /// 替换文本中预定义的标签内容
    /// </summary>
    /// <param name="value">待处理的文本字符串</param>
    /// <returns>完成标签替换后的文本</returns>
    private static string InjectTags(string value)
    {
        // 检查文本中是否包含标签
        if (tagRegex.IsMatch(value))
        {
            // 遍历所有匹配的标签并进行替换
            foreach (Match match in tagRegex.Matches(value))
            {
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    value = value.Replace(match.Value, tagValueRequest());
                }
            }
        }
        
        return value;
    }

    /// <summary>
    /// 替换文本中的变量占位符为实际值
    /// </summary>
    /// <param name="value">待处理的文本字符串</param>
    /// <returns>完成变量替换后的文本</returns>
    private static string InjectVariables(string value)
    {
        var matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
        var matchesList = matches.Cast<Match>().ToList();

        // 从后往前遍历以避免索引偏移问题
        for (int i = matchesList.Count - 1; i >= 0; i--)
        {
            var match = matchesList[i];
            string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID);

            // 尝试获取变量值，若不存在则记录错误日志
            if (!VariableStore.TryGetValue(variableName, out object variableValue))
            {
                Debug.LogError($"Variable {variableName} not found in string assignment.");
                continue;
            }

            // 计算要删除的长度，防止越界
            int lengthToBeRemoved =
                match.Index + match.Length > value.Length ? value.Length - match.Index : match.Length;

            value = value.Remove(match.Index, lengthToBeRemoved);
            value = value.Insert(match.Index, variableValue.ToString());
        }
        
        return value;
    }
}