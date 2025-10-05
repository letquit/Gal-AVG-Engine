using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 命令测试类，用于演示命令管理器的使用方式
/// 通过键盘输入和协程两种方式执行各种命令
/// </summary>
public class CommandTesting : MonoBehaviour
{
    /// <summary>
    /// 初始化方法，在游戏对象启动时调用
    /// 可以启用协程来执行一系列命令测试
    /// </summary>
    private void Start()
    {
        // StartCoroutine(Running());
    }
    
    /// <summary>
    /// 每帧更新方法，检测键盘输入并执行相应命令
    /// 监听左右箭头键的按下事件来触发角色移动命令
    /// </summary>
    private void Update()
    {
        // 检测左箭头键是否被按下，执行向左移动命令
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            CommandManager.instance.Execute("moveCharDemo", "left");
        // 检测右箭头键是否被按下，执行向右移动命令
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            CommandManager.instance.Execute("moveCharDemo", "right");
    }

    /// <summary>
    /// 协程方法，按顺序执行一系列测试命令
    /// 包括打印命令、Lambda命令和处理命令的单参数及多参数版本
    /// </summary>
    /// <returns>IEnumerator枚举器，用于协程执行</returns>
    private IEnumerator Running()
    {
        // 执行无参数的打印命令
        yield return CommandManager.instance.Execute("print");
        // 执行单参数的打印命令，参数为"Hello World!"
        yield return CommandManager.instance.Execute("print_1p", "Hello World!");
        // 执行多参数的打印命令，参数为三个字符串
        yield return CommandManager.instance.Execute("print_mp", "Line1", "Line2", "Line3");
        
        // 执行无参数的Lambda命令
        yield return CommandManager.instance.Execute("lambda");
        // 执行单参数的Lambda命令，参数为"Hello Lambda!"
        yield return CommandManager.instance.Execute("lambda_1p", "Hello Lambda!");
        // 执行多参数的Lambda命令，参数为三个字符串
        yield return CommandManager.instance.Execute("lambda_mp", "Lambda1", "Lambda2", "Lambda3");
        
        // 执行无参数的处理命令
        yield return CommandManager.instance.Execute("process");
        // 执行单参数的处理命令，参数为"3"
        yield return CommandManager.instance.Execute("process_1p", "3");
        // 执行多参数的处理命令，参数为三个处理行字符串
        yield return CommandManager.instance.Execute("process_mp", "Process Line 1", "Process Line 2", "Process Line 3");
    }
}

