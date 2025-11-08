using System;
using ADVENTUREGAME;
using UnityEngine;

namespace COMMANDS
{
    /// <summary>
    /// 冒险游戏命令数据库扩展类
    /// 用于向命令数据库添加冒险游戏相关的自定义命令
    /// </summary>
    public class CMD_DatabaseExtension_AdventureGame : CMD_DatabaseExtension
    {
        /// <summary>
        /// 扩展命令数据库，添加自定义命令
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例</param>
        new public static void Extend(CommandDatabase database)
        { 
            // 添加设置玩家名称变量的命令
            database.AddCommand("setplayername", new Action<string>(SetPlayerNameVariable));
        }
        
        /// <summary>
        /// 设置玩家名称变量
        /// 将指定的数据保存到当前游戏存档的玩家名称字段中
        /// </summary>
        /// <param name="data">要设置的玩家名称数据</param>
        private static void SetPlayerNameVariable(string data)
        {
            AVGGameSave.activeFile.playerName = data;
        }
    }
}