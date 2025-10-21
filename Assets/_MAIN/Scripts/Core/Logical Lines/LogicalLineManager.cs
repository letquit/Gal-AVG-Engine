using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 逻辑行管理器，负责加载和管理所有逻辑行处理器
    /// 该类通过反射自动发现并实例化所有实现ILogicalLine接口的类型
    /// </summary>
    public class LogicalLineManager
    {
        private DialogueSystem dialogueSystem => DialogueSystem.instance;
        private List<ILogicalLine> logicalLines = new List<ILogicalLine>();
        
        /// <summary>
        /// 构造函数，初始化时自动加载所有逻辑行处理器
        /// </summary>
        public LogicalLineManager() => LoadLogicalLines();

        /// <summary>
        /// 通过反射加载当前程序集中所有实现ILogicalLine接口的类型，并创建其实例
        /// 这些实例将被存储在logicalLines列表中供后续使用
        /// </summary>
        private void LoadLogicalLines()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 获取所有实现了ILogicalLine接口且不是接口本身的类型
            Type[] lineTypes = assembly.GetTypes()
                .Where(t => typeof(ILogicalLine).IsAssignableFrom(t) && !t.IsInterface).ToArray();

            foreach (Type lineType in lineTypes)
            {
                ILogicalLine line = (ILogicalLine)Activator.CreateInstance(lineType);
                logicalLines.Add(line);
            }
        }

        /// <summary>
        /// 尝试获取与指定对话行匹配的逻辑处理器并执行
        /// </summary>
        /// <param name="line">要处理的对话行对象</param>
        /// <param name="logic">输出参数，如果找到匹配的逻辑处理器，则返回对应的协程；否则返回null</param>
        /// <returns>如果找到匹配的逻辑处理器则返回true，否则返回false</returns>
        public bool TryGetLogic(DIALOGUE_LINE line, out Coroutine logic)
        {
            // 遍历所有已加载的逻辑行处理器，查找第一个匹配的处理器
            foreach (var logicalLine in logicalLines)
            {
                if (logicalLine.Matches(line))
                {
                    logic = dialogueSystem.StartCoroutine(logicalLine.Execute(line));
                    return true;
                }
            }
            
            logic = null;
            return false;
        }
    }
}