namespace DIALOGUE
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// 用于解析和存储命令数据的类。该类接收原始字符串格式的命令，并将其解析为结构化的命令列表。
    /// 每个命令包含名称、参数列表以及是否需要等待执行完成的标志。
    /// </summary>
    public class DL_COMMAND_DATA
    {
        /// <summary>
        /// 存储解析后的命令列表
        /// </summary>
        public List<Command> commands;

        /// <summary>
        /// 命令分隔符，用于将原始字符串拆分为多个命令
        /// </summary>
        private const char COMMANDSPLITTER_ID = ',';

        /// <summary>
        /// 参数容器标识符，表示命令参数开始的位置（左括号）
        /// </summary>
        private const char ARGUMENTSCONTAINER_ID = '(';

        /// <summary>
        /// 等待命令标识符，如果命令名以该字符串开头，则标记为需要等待执行完成
        /// </summary>
        private const string WAITCOMMAND_ID = "[wait]";

        /// <summary>
        /// 表示一个命令的结构体，包含命令名称、参数数组和是否等待执行完成的标志
        /// </summary>
        public struct Command
        {
            /// <summary>
            /// 命令名称
            /// </summary>
            public string name;

            /// <summary>
            /// 命令参数数组
            /// </summary>
            public string[] arguments;

            /// <summary>
            /// 是否需要等待命令执行完成
            /// </summary>
            public bool waitForCompletion;
        }

        /// <summary>
        /// 构造函数，初始化命令数据对象并解析原始命令字符串
        /// </summary>
        /// <param name="rawCommands">原始命令字符串，多个命令通过逗号分隔</param>
        public DL_COMMAND_DATA(string rawCommands)
        {
            commands = RipCommands(rawCommands);
        }

        /// <summary>
        /// 解析原始命令字符串，提取命令名称、参数和等待标志
        /// </summary>
        /// <param name="rawCommands">原始命令字符串</param>
        /// <returns>解析后的命令列表</returns>
        private List<Command> RipCommands(string rawCommands)
        {
            // 使用逗号分割原始命令字符串
            string[] data = rawCommands.Split(COMMANDSPLITTER_ID, StringSplitOptions.RemoveEmptyEntries);
            List<Command> result = new List<Command>();

            foreach (string cmd in data)
            {
                Command command = new Command();
                // 查找参数容器标识符的位置
                int index = cmd.IndexOf(ARGUMENTSCONTAINER_ID);
                // 提取命令名称并去除首尾空格
                command.name = cmd.Substring(0, index).Trim();

                // 判断命令是否需要等待执行完成
                if (command.name.ToLower().StartsWith(WAITCOMMAND_ID))
                {
                    // 移除等待标识符前缀
                    command.name = command.name.Substring(WAITCOMMAND_ID.Length);
                    command.waitForCompletion = true;
                }
                else
                    command.waitForCompletion = false;

                // 提取并解析命令参数
                command.arguments = GetArgs(cmd.Substring(index + 1, cmd.Length - index - 2));
                result.Add(command);
            }

            return result;
        }

        /// <summary>
        /// 解析命令参数字符串，支持引号包裹的参数
        /// </summary>
        /// <param name="args">参数字符串，参数之间通过空格分隔，引号包裹的参数可包含空格</param>
        /// <returns>解析后的参数数组</returns>
        private string[] GetArgs(string args)
        {
            List<string> argList = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < args.Length; i++)
            {
                // 处理引号，切换引号内/外状态
                if (args[i] == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                // 如果不在引号内且遇到空格，说明当前参数结束
                if (!inQuotes && args[i] == ' ')
                {
                    argList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }

                // 将字符添加到当前参数中
                currentArg.Append(args[i]);
            }

            // 添加最后一个参数（如果有）
            if (currentArg.Length > 0)
                argList.Add(currentArg.ToString());

            return argList.ToArray();
        }
    }

}