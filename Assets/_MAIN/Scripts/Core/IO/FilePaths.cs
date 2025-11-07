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
    
    //运行时路径
    public static readonly string gameSaves = $"{runtimePath}Save Files/";
    
    /// <summary>
    /// 图形资源目录路径常量
    /// </summary>
    public static readonly string resources_graphics = "Graphics/";
    
    /// <summary>
    /// 字体资源路径常量定义
    /// </summary>
    public static readonly string resources_font = "Fonts/";
    
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
    /// 资源路径常量，表示对话文件在资源目录中的相对路径
    /// </summary>
    public static readonly string resources_dialogueFiles = $"Dialogue Files/";

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

    /// <summary>
    /// 获取应用程序运行时数据路径
    /// </summary>
    /// <returns>返回应用程序数据存储路径字符串</returns>
    public static string runtimePath
    {
        get
        {
            #if UNITY_EDITOR
                // 在Unity编辑器环境下，返回Assets目录下的appdata文件夹路径
                return "Assets/appdata/";
            #else
                // 在实际运行环境下，返回应用程序持久化数据路径下的appdata文件夹路径
                return Application.persistentDataPath + "/appdata/";
            #endif
        }
    }
}