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
}

