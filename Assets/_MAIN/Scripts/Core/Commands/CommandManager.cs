using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using CHARACTERS;

namespace COMMANDS
{

    /// <summary>
    /// 命令管理器，用于执行注册的命令。该类继承自MonoBehaviour，作为Unity组件使用。
    /// 通过反射加载所有CMD_DatabaseExtension子类并扩展命令数据库。
    /// </summary>
    public class CommandManager : MonoBehaviour
    {
        /// <summary>
        /// 子命令标识符，用于分隔命令层级的常量字符
        /// </summary>
        private const char SUB_COMMAN_IDENTIFIER = '.';

        /// <summary>
        /// 数据库角色基础信息表名称常量
        /// </summary>
        public const string DATABASE_CHARACTERS_BASE = "characters";

        /// <summary>
        /// 数据库角色精灵图片表名称常量
        /// </summary>
        public const string DATABASE_CHARACTERS_SPRITE = "characters_sprite";

        /// <summary>
        /// 数据库角色Live2D模型表名称常量
        /// </summary>
        public const string DATABASE_CHARACTERS_LIVE2D = "characters_live2D";

        /// <summary>
        /// 数据库角色3D模型表名称常量
        /// </summary>
        public const string DATABASE_CHARACTERS_MODEL3D = "characters_model3D";
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static CommandManager instance { get; private set; }

        /// <summary>
        /// 命令数据库，存储所有可用的命令
        /// </summary>
        private CommandDatabase database;
        
        /// <summary>
        /// 存储子数据库的字典，用于管理多个命令数据库实例
        /// </summary>
        /// <remarks>
        /// 键为数据库名称，值为对应的CommandDatabase实例
        /// </remarks>
        private Dictionary<string, CommandDatabase> subDatabases = new Dictionary<string, CommandDatabase>();
        
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
            // 检查是否为子命令并执行
            if (commandName.Contains(SUB_COMMAN_IDENTIFIER))
                return ExecuteSubCommand(commandName, args);
            
            // 从数据库中获取指定名称的命令
            Delegate command = database.GetCommand(commandName);

            // 如果命令不存在，直接返回null
            if (command == null)
                return null;

            // 启动命令执行进程并返回协程包装器
            return StartProcess(commandName, command, args);
        }

        /// <summary>
        /// 执行一个子命令。该方法会解析命令名称中的数据库部分与具体子命令，并根据类型调用对应的处理逻辑。
        /// 支持两种模式：基于子数据库的命令执行和基于角色的命令执行。
        /// </summary>
        /// <param name="commandName">完整的命令名称，格式通常为 "database.subCommand"。</param>
        /// <param name="args">传递给命令的参数数组。</param>
        /// <returns>如果成功启动了命令处理流程则返回对应的协程包装器；否则返回 null。</returns>
        private CoroutineWrapper ExecuteSubCommand(string commandName, string[] args)
        {
            // 解析出数据库名和子命令名
            string[] parts = commandName.Split(SUB_COMMAN_IDENTIFIER);
            string databaseName = string.Join(SUB_COMMAN_IDENTIFIER, parts.Take(parts.Length - 1));
            string subCommandName = parts.Last();

            // 尝试在子数据库中查找并执行命令
            if (subDatabases.ContainsKey(databaseName))
            {
                Delegate command = subDatabases[databaseName].GetCommand(subCommandName);
                if (command != null)
                {
                    return StartProcess(commandName, command, args);
                }
                else
                {
                    Debug.LogError($"No command called '{subCommandName}' was found in sub database '{databaseName}'");
                    return null;
                }
            }

            // 若未找到对应数据库，则尝试作为角色命令进行处理
            string characterName = databaseName;
            if (CharacterManager.instance.HasCharacter(databaseName))
            {
                // 在参数列表最前插入角色名以供后续使用
                List<string> newArgs = new List<string>(args);
                newArgs.Insert(0, characterName);
                args = newArgs.ToArray();

                return ExecuteCharacterCommand(subCommandName, args);
            }
            
            Debug.LogError($"No sub database called '{databaseName}' exists! Command '{subCommandName}' could not be run.");
            return null;
        }
        
        /// <summary>
        /// 执行针对特定角色类型的命令。首先尝试从基础角色数据库获取命令，
        /// 如果失败，则依据角色配置信息选择合适的专用数据库再次尝试执行。
        /// </summary>
        /// <param name="commandName">要执行的角色相关命令名称。</param>
        /// <param name="args">传递给命令的参数数组，其中第一个元素应为角色名称。</param>
        /// <returns>如果成功启动了命令处理流程则返回对应的协程包装器；否则返回 null。</returns>
        private CoroutineWrapper ExecuteCharacterCommand(string commandName, params string[] args)
        {
            Delegate command = null;

            // 先尝试从基础角色数据库查找命令
            CommandDatabase db = subDatabases[DATABASE_CHARACTERS_BASE];
            if (db.HasCommand(commandName))
            {
                command = db.GetCommand(commandName);
                return StartProcess(commandName, command, args);
            }

            // 根据角色类型切换到相应的专用数据库
            CharacterConfigData characterConfigData = CharacterManager.instance.GetCharacterConfig(args[0]);
            switch (characterConfigData.characterType)
            {
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    db = subDatabases[DATABASE_CHARACTERS_SPRITE];
                    break;
                case Character.CharacterType.Live2D:
                    db = subDatabases[DATABASE_CHARACTERS_LIVE2D];
                    break;
                case Character.CharacterType.Model3D:
                    db = subDatabases[DATABASE_CHARACTERS_MODEL3D];
                    break;
            }
            
            command = db.GetCommand(commandName);
            
            // 最终尝试执行命令
            if (command != null)
                return StartProcess(commandName, command, args);
            
            Debug.LogError($"Command Manager was unable to execute command '{commandName}' on character '{args[0]}'. The character name or command may be invalid.");
            return null;
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
            // 根据委托类型执行相应的调用逻辑
            if (command is Action)
                command.DynamicInvoke();

            else if (command is Action<string>)
                command.DynamicInvoke(args.Length == 0 ? string.Empty : args[0]);

            else if (command is Action<string[]>)
                command.DynamicInvoke((object)args);

            // 处理返回IEnumerator的委托类型，支持协程等待
            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();

            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args.Length == 0 ? string.Empty : args[0]);

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

        public CommandDatabase CreateSubDatabase(string name)
        {
            name = name.ToLower();

            if (subDatabases.TryGetValue(name, out CommandDatabase db))
            {
                Debug.LogWarning($"A database by the name of '{name}' already exists!");
                return db;
            }
            
            CommandDatabase newDatabase = new CommandDatabase();
            subDatabases.Add(name, newDatabase);
            
            return newDatabase;
        }
    }
}