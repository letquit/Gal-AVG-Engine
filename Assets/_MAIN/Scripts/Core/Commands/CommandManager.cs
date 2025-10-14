using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace COMMANDS
{

    /// <summary>
    /// 命令管理器，用于执行注册的命令。该类继承自MonoBehaviour，作为Unity组件使用。
    /// 通过反射加载所有CMD_DatabaseExtension子类并扩展命令数据库。
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static CommandManager instance { get; private set; }

        /// <summary>
        /// 命令数据库，存储所有可用的命令
        /// </summary>
        private CommandDatabase database;
        
        /// <summary>
        /// 存储当前活动的命令处理进程列表
        /// </summary>
        private List<CommandProcess> activeProcesses = new List<CommandProcess>();

        /// <summary>
        /// 获取当前顶层的命令处理进程
        /// </summary>
        /// <returns>返回活动进程列表中的最后一个进程，即顶层进程</returns>
        private CommandProcess topProcess => activeProcesses.Last();

        /// <summary>
        /// Unity生命周期函数，在对象启用时调用。
        /// 初始化单例实例，并通过反射加载所有CMD_DatabaseExtension子类来扩展命令数据库。
        /// 如果已有实例则销毁当前游戏对象。
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

                database = new CommandDatabase();

                // 获取当前程序集中的所有类型
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] extensionTypes =
                    assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CMD_DatabaseExtension))).ToArray();

                // 遍历所有扩展类型并调用其Extend方法来扩展数据库
                foreach (Type extension in extensionTypes)
                {
                    MethodInfo extendMethod = extension.GetMethod("Extend");
                    extendMethod.Invoke(null, new object[] { database });
                }
            }
            else
                DestroyImmediate(gameObject);
        }

        /// <summary>
        /// 执行指定名称的命令
        /// </summary>
        /// <param name="commandName">要执行的命令名称</param>
        /// <param name="args">传递给命令的参数数组</param>
        /// <returns>返回启动的协程，如果命令不存在则返回null</returns>
        public CoroutineWrapper Execute(string commandName, params string[] args)
        {
            // 从数据库中获取指定名称的命令
            Delegate command = database.GetCommand(commandName);

            // 如果命令不存在，直接返回null
            if (command == null)
                return null;

            // 启动命令执行进程并返回协程包装器
            return StartProcess(commandName, command, args);
        }

        /// <summary>
        /// 启动一个新的命令执行进程
        /// </summary>
        /// <param name="commandName">命令名称（未使用，仅用于标识）</param>
        /// <param name="command">要执行的命令委托</param>
        /// <param name="args">传递给命令的参数数组</param>
        /// <returns>返回新启动的协程</returns>
        private CoroutineWrapper StartProcess(string commandName, Delegate command, string[] args)
        {
            // 生成唯一的进程ID
            Guid processID = Guid.NewGuid();
            
            // 创建命令进程对象
            CommandProcess cmd = new CommandProcess(processID, commandName, command, null, args, null);
            
            // 将命令进程添加到活跃进程列表中
            activeProcesses.Add(cmd);

            // 启动协程执行命令进程
            Coroutine co = StartCoroutine(RunningProcess(cmd));
            
            // 创建协程包装器并赋值给命令进程
            cmd.runningProgress = new CoroutineWrapper(this, co);

            // 返回协程包装器
            return cmd.runningProgress;
        }

        /// <summary>
        /// 停止当前正在运行的命令进程
        /// </summary>
        public void StopCurrentProcess()
        {
            // 如果存在正在运行的顶级进程，则终止该进程
            if (topProcess != null)
                KillProcess(topProcess);
        }

        /// <summary>
        /// 停止所有正在运行的进程
        /// </summary>
        public void StopAllProcesses()
        {
            // 遍历所有活动进程并终止它们
            foreach (var c in activeProcesses)
            {
                // 如果进程有正在运行的进度且未完成，则停止进度
                if (c.runningProgress != null && !c.runningProgress.IsDone)
                    c.runningProgress.Stop();
                
                // 执行进程终止时的回调动作
                c.onTerminateAction?.Invoke();
            }
            
            // 清空所有活动进程列表
            activeProcesses.Clear();
        }

        /// <summary>
        /// 运行命令的协程处理器
        /// </summary>
        /// <param name="command">要执行的命令委托</param>
        /// <param name="args">传递给命令的参数数组</param>
        /// <returns>返回一个IEnumerator用于协程控制</returns>
        private IEnumerator RunningProcess(CommandProcess process)
        {
            // 等待进程执行完成
            yield return WaitingForProcessToComplete(process.command, process.args);

            // 进程完成后终止该进程
            KillProcess(process);
        }

        /// <summary>
        /// 终止指定的命令进程
        /// </summary>
        /// <param name="cmd">要终止的命令进程对象</param>
        public void KillProcess(CommandProcess cmd)
        {
            // 从活动进程列表中移除该进程
            activeProcesses.Remove(cmd);

            // 如果进程有正在运行的进度且未完成，则停止进度
            if (cmd.runningProgress != null && !cmd.runningProgress.IsDone)
                cmd.runningProgress.Stop();
            
            // 执行进程终止时的回调动作
            cmd.onTerminateAction?.Invoke();
        }

        /// <summary>
        /// 等待命令执行完成的协程逻辑
        /// 根据命令委托的类型调用相应的执行方式
        /// </summary>
        /// <param name="command">要执行的命令委托</param>
        /// <param name="args">传递给命令的参数数组</param>
        /// <returns>返回一个IEnumerator用于协程控制</returns>
        private IEnumerator WaitingForProcessToComplete(Delegate command, string[] args)
        {
            if (command is Action)
                command.DynamicInvoke();

            else if (command is Action<string>)
                command.DynamicInvoke(args[0]);

            else if (command is Action<string[]>)
                command.DynamicInvoke((object)args);

            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();

            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args[0]);

            else if (command is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)command)(args);
        }

        /// <summary>
        /// 为当前进程添加终止时执行的动作
        /// </summary>
        /// <param name="action">进程终止时要执行的UnityAction回调函数</param>
        public void AddTerminationActionToCurrentProcess(UnityAction action)
        {
            // 获取当前顶层进程
            CommandProcess process = topProcess;
            
            // 如果当前没有进程则直接返回
            if (process == null)
                return;

            // 为进程创建终止事件并添加回调动作
            process.onTerminateAction = new UnityEvent();
            process.onTerminateAction.AddListener(action);
        }
    }
}