namespace COMMANDS
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 命令数据库类，用于存储和管理命令委托的字典结构
    /// </summary>
    public class CommandDatabase
    {
        private Dictionary<string, Delegate> database = new Dictionary<string, Delegate>();

        /// <summary>
        /// 检查指定名称的命令是否存在于数据库中
        /// </summary>
        /// <param name="commandName">要检查的命令名称</param>
        /// <returns>如果命令存在则返回true，否则返回false</returns>
        public bool HasCommand(string commandName) => database.ContainsKey(commandName.ToLower());

        /// <summary>
        /// 向数据库中添加新的命令
        /// </summary>
        /// <param name="commandName">命令的名称</param>
        /// <param name="command">命令对应的委托</param>
        public void AddCommand(string commandName, Delegate command)
        {
            // 转换命令名称为小写
            commandName = commandName.ToLower();
            
            // 检查命令是否已存在
            if (!database.ContainsKey(commandName))
            {
                database.Add(commandName, command);
            }
            else
                Debug.LogError($"Command already exists in the database '{commandName}'");
        }

        /// <summary>
        /// 从数据库中获取指定名称的命令
        /// </summary>
        /// <param name="commandName">要获取的命令名称</param>
        /// <returns>返回对应的命令委托，如果命令不存在则返回null</returns>
        public Delegate GetCommand(string commandName)
        {
            // 转换命令名称为小写
            commandName = commandName.ToLower();
            
            // 验证命令是否存在
            if (!database.ContainsKey(commandName))
            {
                Debug.LogError($"Command '{commandName}' does not exist in the database!");
                return null;
            }

            return database[commandName];
        }
    }

}
