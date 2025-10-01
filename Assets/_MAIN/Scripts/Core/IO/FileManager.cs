using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 文件管理器类，提供读取文本文件和文本资源的功能
/// </summary>
public class FileManager
{
    /// <summary>
    /// 读取指定路径的文本文件，返回文件中的所有行
    /// </summary>
    /// <param name="filePath">要读取的文件路径，如果路径不以'/'开头，则会自动拼接FilePaths.root路径</param>
    /// <param name="includeBlankLines">是否包含空行，默认为true</param>
    /// <returns>包含文件所有行的字符串列表，如果文件不存在则返回空列表</returns>
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        // 如果路径不以'/'开头，则拼接根路径
        if (!filePath.StartsWith('/'))
            filePath = FilePaths.root + filePath;
        
        List<string> lines = new List<string>();

        try
        {
            // 使用StreamReader逐行读取文件内容
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    // 根据参数决定是否包含空行
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // 记录文件未找到的错误日志
            Debug.LogError($"File not found: '{ex.FileName}'");
        }
        
        return lines;
    }
    
    /// <summary>
    /// 从Resources文件夹中读取文本资源，返回资源中的所有行
    /// </summary>
    /// <param name="filePath">资源文件路径（相对于Resources文件夹）</param>
    /// <param name="includeBlankLines">是否包含空行，默认为true</param>
    /// <returns>包含资源所有行的字符串列表，如果资源不存在则返回null</returns>
    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        // 从Resources中加载文本资源
        TextAsset asset = Resources.Load<TextAsset>(filePath);
        if (asset == null)
        {
            // 记录资源未找到的错误日志
            Debug.LogError($"Asset not found: '{filePath}'");
            return null;
        }
        
        return ReadTextAsset(asset, includeBlankLines);
    }

    /// <summary>
    /// 读取TextAsset对象中的文本内容，返回所有行
    /// </summary>
    /// <param name="asset">要读取的TextAsset对象</param>
    /// <param name="includeBlankLines">是否包含空行，默认为true</param>
    /// <returns>包含文本所有行的字符串列表</returns>
    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlankLines = true)
    {
        List<string> lines = new List<string>();
        // 使用StringReader读取TextAsset中的文本内容
        using (StringReader sr = new StringReader(asset.text))
        {
            while (sr.Peek() > -1)
            {
                string line = sr.ReadLine();
                // 根据参数决定是否包含空行
                if (includeBlankLines || !string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }
        }
        
        return lines;
    }
}

