using UnityEngine;

namespace ADVENTUREGAME
{
    /// <summary>
    /// AVG数据库链接设置类
    /// 用于设置游戏中的外部链接变量
    /// </summary>
    public class AVGDatabaseLinkSetup : MonoBehaviour
    {
        /// <summary>
        /// 设置外部链接变量
        /// 创建一个名为"AVG.mainCharName"的变量，用于存储主角名称
        /// 该变量与游戏存档文件中的玩家名称进行双向绑定
        /// </summary>
        public void SetupExternalLinks()
        {
            // 创建一个变量，用于存储和同步主角名称
            // 变量的默认值为空字符串
            // Getter函数：当存档文件存在时，返回存档中的玩家名称；否则返回空字符串
            // Setter函数：当存档文件存在时，将新值保存到存档文件的玩家名称字段中
            VariableStore.CreateVariable("AVG.mainCharName", "", 
                () => AVGGameSave.activeFile != null ? AVGGameSave.activeFile.playerName : "",
                value => { if (AVGGameSave.activeFile != null) AVGGameSave.activeFile.playerName = value; });
        }
    }
}
