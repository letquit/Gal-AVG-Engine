using UnityEngine;

/// <summary>
/// 文件路径管理类，用于定义和管理游戏数据的文件路径
/// </summary>
public class FilePaths
{
    /// <summary>
    /// 游戏数据根目录路径，指向Unity项目Assets文件夹下的gameData目录
    /// </summary>
    public static readonly string root = $"{Application.dataPath}/gameData/";
    
    /// <summary>
    /// 图形资源目录路径常量
    /// </summary>
    public static readonly string resources_graphics = "Graphics/";
    
    /// <summary>
    /// 背景图片资源目录路径常量
    /// </summary>
    public static readonly string resources_backgroundImages = $"{resources_graphics}BG Images/";
    
    /// <summary>
    /// 背景视频资源目录路径常量
    /// </summary>
    public static readonly string resources_backgroundVideos = $"{resources_graphics}BG Videos/";
    
    /// <summary>
    /// 过渡效果纹理资源目录路径常量
    /// </summary>
    public static readonly string resources_blendTextures = $"{resources_graphics}Transition Effects/";
}

