using UnityEngine;

/// <summary>
/// 游戏配置脚本对象类
/// 用于存储游戏的配置数据，可以通过Unity编辑器创建和配置
/// </summary>
[CreateAssetMenu(fileName = "Adventure Game Configuration", menuName = "Dialogue System/Adventure Game Configuration Asset")]
public class AdventureGameSO : ScriptableObject
{
    /// <summary>
    /// 起始文件配置
    /// 用于指定游戏开始时加载的文本资源文件
    /// </summary>
    public TextAsset startingFile;
}
