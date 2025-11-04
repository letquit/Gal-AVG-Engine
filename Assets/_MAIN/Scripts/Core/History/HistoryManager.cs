using System;
using System.Collections.Generic;
using DIALOGUE;
using NUnit.Framework;
using UnityEngine;


namespace History
{
    /// <summary>
    /// 历史管理器类，负责管理和记录对话历史状态
    /// 该类继承自MonoBehaviour，作为Unity组件使用
    /// </summary>
    [RequireComponent(typeof(HistoryNavigation))]
    public class HistoryManager : MonoBehaviour
    {
        /// <summary>
        /// 历史缓存的最大限制数量
        /// </summary>
        public const int HISTORY_CACHE_LIMIT = 100;
        
        /// <summary>
        /// HistoryManager的单例实例
        /// </summary>
        public static HistoryManager instance { get; private set; }
        
        /// <summary>
        /// 存储历史状态的列表
        /// </summary>
        public List<HistoryState> history = new List<HistoryState>();

        private HistoryNavigation navigation;

        /// <summary>
        /// Unity生命周期函数，在对象初始化时调用
        /// 设置单例实例并获取HistoryNavigation组件
        /// </summary>
        private void Awake()
        {
            instance = this;
            navigation = GetComponent<HistoryNavigation>();
        }

        /// <summary>
        /// Unity生命周期函数，在Awake之后调用
        /// 订阅DialogueSystem的onClear事件，在对话清除时记录当前状态
        /// </summary>
        private void Start()
        {
            DialogueSystem.instance.onClear += LogCurrentState;
        }

        /// <summary>
        /// 记录当前的历史状态
        /// 捕获当前状态并添加到历史列表中，如果超过缓存限制则移除最早的状态
        /// </summary>
        public void LogCurrentState()
        {
            HistoryState state = HistoryState.Capture();
            history.Add(state);
            
            // 当历史记录数量超过限制时，移除最旧的记录
            if (history.Count > HISTORY_CACHE_LIMIT)
                history.RemoveAt(0);
        }

        /// <summary>
        /// 加载指定的历史状态
        /// </summary>
        /// <param name="state">要加载的历史状态对象</param>
        public void LoadState(HistoryState state)
        {
            state.Load();
        }
        
        /// <summary>
        /// 导航到下一个历史状态
        /// </summary>
        public void GoForward() => navigation.GoForward();
        
        /// <summary>
        /// 返回到上一个历史状态
        /// </summary>
        public void GoBack() => navigation.GoBack();
    }
}