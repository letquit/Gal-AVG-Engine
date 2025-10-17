using UnityEngine;

/// <summary>
/// 文件路径管理类，用于定义和管理游戏数据的文件路径
/// </summary>
public class FilePaths
{
    private const string HOME_DIRECTORY_SYMBOL = "~/";
    
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
    
    /// <summary>
    /// 音频资源目录路径常量
    /// </summary>
    public static readonly string resources_audio = "Audio/";
    
    /// <summary>
    /// 音效资源目录路径常量
    /// </summary>
    public static readonly string resources_sfx = $"{resources_audio}SFX/";
    
    /// <summary>
    /// 语音资源目录路径常量
    /// </summary>
    public static readonly string resources_voices = $"{resources_audio}Voices/";
    
    /// <summary>
    /// 音乐资源目录路径常量
    /// </summary>
    public static readonly string resources_music = $"{resources_audio}Music/";
    
    /// <summary>
    /// 环境音效资源目录路径常量
    /// </summary>
    public static readonly string resources_ambience = $"{resources_audio}Ambience/";

    /// <summary>
    /// 获取资源的完整路径
    /// </summary>
    /// <param name="defaultPath">默认的基础路径</param>
    /// <param name="resourceName">资源名称，如果以~/开头则视为绝对路径</param>
    /// <returns>资源的完整路径</returns>
    public static string GetPathToResource(string defaultPath, string resourceName)
    {
        // 如果资源名称以root目录符号开头，则去掉前缀作为绝对路径返回
        if (resourceName.StartsWith(HOME_DIRECTORY_SYMBOL))
            return resourceName.Substring(HOME_DIRECTORY_SYMBOL.Length);
        
        // 否则将资源名称拼接到默认路径后面
        return defaultPath + resourceName;
    }
}