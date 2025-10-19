using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 标签管理器类，用于管理和替换文本中的标签
/// </summary>
public class TagManager
{
    private readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>();
    private readonly Regex tagRegex = new Regex("<\\w+>");
    
    /// <summary>
    /// 初始化标签管理器实例
    /// </summary>
    public TagManager()
    {
        InitializeTags();
    }

    /// <summary>
    /// 初始化预定义的标签及其对应的值获取函数
    /// </summary>
    private void InitializeTags()
    {
        tags["<mainChar>"] = () => "Avira";
        tags["<time>"] = () => DateTime.Now.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        tags["<playerLevel>"] = () => "15";
        tags["<tempval1>"] = () => "42";
    }

    /// <summary>
    /// 将文本中的标签替换为对应的值
    /// </summary>
    /// <param name="text">包含标签的原始文本</param>
    /// <returns>替换标签后的新文本</returns>
    public string Inject(string text)
    {
        // 检查文本中是否包含标签
        if (tagRegex.IsMatch(text))
        {
            // 遍历所有匹配的标签并进行替换
            foreach (Match match in tagRegex.Matches(text))
            {
                if (tags.TryGetValue(match.Value, out var tagValueRequest))
                {
                    text = text.Replace(match.Value, tagValueRequest());
                }
            }
        }
        
        return text;
    }
}