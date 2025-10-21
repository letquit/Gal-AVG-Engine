using System.Collections;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 逻辑对话行接口，定义了逻辑对话行必须实现的基本功能
    /// </summary>
    public interface ILogicalLine
    {
        /// <summary>
        /// 获取逻辑行的关键字标识
        /// </summary>
        string keyword { get; }
        
        /// <summary>
        /// 检查给定的对话行是否与此逻辑行匹配
        /// </summary>
        /// <param name="line">要检查的对话行对象</param>
        /// <returns>如果匹配返回true，否则返回false</returns>
        bool Matches(DIALOGUE_LINE line);
        
        /// <summary>
        /// 执行逻辑行对应的操作
        /// </summary>
        /// <param name="line">要执行的对话行对象</param>
        /// <returns>协程枚举器，用于异步执行逻辑</returns>
        IEnumerator Execute(DIALOGUE_LINE line);
    }
}