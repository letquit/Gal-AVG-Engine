using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 图库配置管理类，用于管理已解锁图片的配置数据
/// 负责图片解锁状态的保存、加载和查询功能
/// </summary>
[Serializable]
public class GalleryConfig
{
    /// <summary>
    /// 当前激活的图库配置实例
    /// </summary>
    public static GalleryConfig activeConfig;
    
    /// <summary>
    /// 是否启用加密存储的常量标识
    /// </summary>
    public const bool ENCRYPT = false;
    
    /// <summary>
    /// 配置文件的完整路径
    /// </summary>
    public static string filePath => $"{FilePaths.root}gallery.avg";
    
    /// <summary>
    /// 已解锁图片名称列表
    /// </summary>
    public List<string> unlockedImages = new List<string>();

    /// <summary>
    /// 加载图库配置数据
    /// 如果配置文件存在则从文件加载，否则创建新的配置实例
    /// </summary>
    public static void Load()
    {
        // 检查配置文件是否存在
        if (File.Exists(filePath))
        {
            // 从文件加载配置数据
            activeConfig = FileManager.Load<GalleryConfig>(filePath, encrypt: ENCRYPT);
        }
        else
        {
            // 创建新的配置实例
            activeConfig = new GalleryConfig();
        }
    }

    /// <summary>
    /// 保存当前图库配置数据到文件
    /// 使用JSON序列化将配置数据保存到指定路径
    /// </summary>
    public static void Save() => FileManager.Save(filePath, JsonUtility.ToJson(activeConfig), encrypt: ENCRYPT);

    /// <summary>
    /// 清除所有已解锁图片记录
    /// 重置解锁列表并保存配置
    /// </summary>
    public static void Erase()
    {
        // 确保配置实例存在
        if (activeConfig == null)
            activeConfig = new GalleryConfig();

        // 重置解锁图片列表
        activeConfig.unlockedImages = new List<string>();
        
        // 保存更改
        Save();
    }

    /// <summary>
    /// 解锁指定名称的图片
    /// 如果图片尚未解锁，则添加到解锁列表并保存配置
    /// </summary>
    /// <param name="imageName">要解锁的图片名称</param>
    public static void UnlockImage(string imageName)
    {
        // 确保配置已加载
        if (activeConfig == null)
            Load();

        // 检查图片是否已解锁，避免重复添加
        if (!activeConfig.unlockedImages.Contains(imageName))
        {
            // 添加到解锁列表
            activeConfig.unlockedImages.Add(imageName);
            
            // 保存更新后的配置
            Save();
        }
    }

    /// <summary>
    /// 检查指定名称的图片是否已解锁
    /// 如果配置未加载则先加载配置数据
    /// </summary>
    /// <param name="imageName">要检查的图片名称</param>
    /// <returns>如果图片已解锁返回true，否则返回false</returns>
    public static bool ImageIsUnlocked(string imageName)
    {
        // 确保配置已加载
        if (activeConfig == null)
            Load();
        
        // 返回图片解锁状态
        return activeConfig.unlockedImages.Contains(imageName);
    }
}