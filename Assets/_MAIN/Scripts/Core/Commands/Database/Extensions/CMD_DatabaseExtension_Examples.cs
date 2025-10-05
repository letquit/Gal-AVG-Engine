using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 示例类，用于扩展命令数据库，添加各种命令示例。
/// </summary>
public class CMD_DatabaseExtension_Examples : CMD_DatabaseExtension
{
    /// <summary>
    /// 扩展命令数据库，添加多个示例命令。
    /// </summary>
    /// <param name="database">要扩展的命令数据库实例。</param>
    new public static void Extend(CommandDatabase database)
    {
        // 添加打印相关命令
        database.AddCommand("print", new Action(PrintDefaultMessage));
        database.AddCommand("print_1p", new Action<string>(PrintUsermessage));
        database.AddCommand("print_mp", new Action<string[]>(PrintLines));
        
        // 添加使用 Lambda 表达式的命令
        database.AddCommand("lambda", new Action(() => { Debug.Log("Printing a default message to console from lambda command."); }));
        database.AddCommand("lambda_1p", new Action<string>((arg) => { Debug.Log($"Log User Lambda Message: '{arg}'"); }));
        database.AddCommand("lambda_mp", new Action<string[]>((args) => { Debug.Log(string.Join(", ", args)); }));
        
        // 添加协程处理相关命令
        database.AddCommand("process", new Func<IEnumerator>(SimpleProcess));
        database.AddCommand("process_1p", new Func<string, IEnumerator>(LineProcess));
        database.AddCommand("process_mp", new Func<string[], IEnumerator>(MultiLineProcess));
        
        // 添加角色移动演示命令
        database.AddCommand("moveCharDemo", new Func<string, IEnumerator>(MoveCharDemo));
    }

    /// <summary>
    /// 打印默认消息到控制台。
    /// </summary>
    private static void PrintDefaultMessage()
    {
        Debug.Log("Printing a default message to console.");
    }

    /// <summary>
    /// 打印用户提供的消息到控制台。
    /// </summary>
    /// <param name="message">要打印的消息内容。</param>
    private static void PrintUsermessage(string message)
    {
        Debug.Log($"User Message: '{message}'");
    }

    /// <summary>
    /// 打印多行消息到控制台，每行前带有编号。
    /// </summary>
    /// <param name="lines">要打印的字符串数组，每个元素代表一行。</param>
    private static void PrintLines(string[] lines)
    {
        int i = 1;
        foreach (string line in lines)
        {
            Debug.Log($"{i++}. '{line}'");
        }
    }

    /// <summary>
    /// 简单的协程处理示例，每隔一秒打印一次进度信息，共执行5次。
    /// </summary>
    /// <returns>IEnumerator类型的迭代器，用于协程执行。</returns>
    private static IEnumerator SimpleProcess()
    {
        for (int i = 1; i <= 5; i++)
        {
            Debug.Log($"Process Running... [{i}]");
            yield return new WaitForSeconds(1);
        }
    }

    /// <summary>
    /// 根据输入参数执行指定次数的循环处理。
    /// </summary>
    /// <param name="data">表示循环次数的字符串。</param>
    /// <returns>IEnumerator类型的迭代器，用于协程执行。</returns>
    private static IEnumerator LineProcess(string data)
    {
        if (int.TryParse(data, out int num))
        {
            for (int i = 0; i < num; i++)
            {
                Debug.Log($"Process Running... [{i}]");
                yield return new WaitForSeconds(1);
            }
        }
    }

    /// <summary>
    /// 处理多个字符串参数，逐个打印并等待一段时间。
    /// </summary>
    /// <param name="data">包含多个消息的字符串数组。</param>
    /// <returns>IEnumerator类型的迭代器，用于协程执行。</returns>
    private static IEnumerator MultiLineProcess(string[] data)
    {
        foreach (string line in data)
        {
            Debug.Log($"Process Message: '{line}'");
            yield return new WaitForSeconds(0.5f);
        }
        
    }

    /// <summary>
    /// 演示角色移动的协程处理函数。
    /// </summary>
    /// <param name="direction">移动方向，"left" 或其他值（默认为 right）。</param>
    /// <returns>IEnumerator类型的迭代器，用于协程执行。</returns>
    private static IEnumerator MoveCharDemo(string direction)
    {
        bool left = direction.ToLower() == "left";

        Transform character = GameObject.Find("Image").transform;
        float moveSpeed = 15;
        
        float targetX = left ? -8 : 8;
        
        float currentX = character.position.x;

        // 移动角色直到接近目标位置
        while (Mathf.Abs(targetX - currentX) > 0.1f)
        {
            currentX = Mathf.MoveTowards(currentX, targetX, moveSpeed * Time.deltaTime);
            character.position = new Vector3(currentX, character.position.y, character.position.z);
            yield return null;
        }
    }
}
