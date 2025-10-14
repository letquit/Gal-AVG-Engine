using System;
using UnityEngine;
using UnityEngine.Events;

namespace COMMANDS
{
    /// <summary>
    /// 协程包装器类，用于管理和控制Unity协程的执行
    /// </summary>
    public class CoroutineWrapper
    {
        private MonoBehaviour owner;
        private Coroutine coroutine;

        /// <summary>
        /// 协程是否已完成的标志位
        /// </summary>
        public bool IsDone = false;
        
        /// <summary>
        /// 构造函数，初始化协程包装器
        /// </summary>
        /// <param name="owner">协程的所有者MonoBehaviour对象，用于调用StopCoroutine方法</param>
        /// <param name="coroutine">要被包装管理的协程实例</param>
        public CoroutineWrapper(MonoBehaviour owner, Coroutine coroutine)
        {
            this.owner = owner;
            this.coroutine = coroutine;
        }
        
        /// <summary>
        /// 停止并清理当前包装的协程
        /// </summary>
        public void Stop()
        {
            owner.StopCoroutine(coroutine);
            IsDone = true;
        }
    }
}

