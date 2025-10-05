using System;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;

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
    /// 当前正在运行的协程进程
    /// </summary>
    private static Coroutine process = null;

    /// <summary>
    /// 判断当前是否有命令正在执行
    /// </summary>
    public static bool isRunningProcess => process != null;
    
    /// <summary>
    /// 命令数据库，存储所有可用的命令
    /// </summary>
    private CommandDatabase database;
    
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
    public Coroutine Execute(string commandName, params string[] args)
    {
        Delegate command = database.GetCommand(commandName);
        
        if (command == null)
            return null;
        
        return StartProcess(commandName, command, args);
    }

    /// <summary>
    /// 启动一个新的命令执行进程
    /// </summary>
    /// <param name="commandName">命令名称（未使用，仅用于标识）</param>
    /// <param name="command">要执行的命令委托</param>
    /// <param name="args">传递给命令的参数数组</param>
    /// <returns>返回新启动的协程</returns>
    private Coroutine StartProcess(string commandName, Delegate command, string[] args)
    {
        StopCurrentProcess();

        process = StartCoroutine(RunningProcess(command, args));
        
        return process;
    }

    /// <summary>
    /// 停止当前正在运行的命令进程
    /// </summary>
    private void StopCurrentProcess()
    {
        if (process != null)
            StopCoroutine(process);
        
        process = null;
    }

    /// <summary>
    /// 运行命令的协程处理器
    /// </summary>
    /// <param name="command">要执行的命令委托</param>
    /// <param name="args">传递给命令的参数数组</param>
    /// <returns>返回一个IEnumerator用于协程控制</returns>
    private IEnumerator RunningProcess(Delegate command, string[] args)
    {
        yield return WaitingForProcessToComplete(command, args);
        
        process = null;
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
}
