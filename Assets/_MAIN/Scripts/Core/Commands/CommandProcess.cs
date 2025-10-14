using System;
using COMMANDS;
using UnityEngine;
using UnityEngine.Events;

namespace COMMANDS
{
    /// <summary>
    /// 命令处理类，用于封装和管理命令执行的相关信息
    /// </summary>
    public class CommandProcess
    {
        public Guid ID;                           // 命令进程的唯一标识符
        public string processName;                // 进程名称
        public Delegate command;                  // 要执行的命令委托
        public CoroutineWrapper runningProgress;  // 正在运行的协程包装器
        public string[] args;                     // 命令参数数组

        public UnityEvent onTerminateAction;      // 命令终止时触发的事件

        /// <summary>
        /// 初始化CommandProcess实例
        /// </summary>
        /// <param name="id">命令进程的唯一标识符</param>
        /// <param name="processName">进程名称</param>
        /// <param name="command">要执行的命令委托</param>
        /// <param name="runningProgress">正在运行的协程包装器</param>
        /// <param name="args">命令参数数组</param>
        /// <param name="onTerminateAction">命令终止时触发的事件，默认为null</param>
        public CommandProcess(Guid id, string processName, Delegate command, CoroutineWrapper runningProgress,
            string[] args, UnityEvent onTerminateAction = null)
        {
            ID = id;
            this.processName = processName;
            this.command = command;
            this.runningProgress = runningProgress;
            this.args = args;
            this.onTerminateAction = onTerminateAction;
        }
    }
}

