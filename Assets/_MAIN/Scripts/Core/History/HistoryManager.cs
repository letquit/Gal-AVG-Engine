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
    [RequireComponent(typeof(HistoryLogManager))]
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

        /// <summary>
        /// 导航历史记录管理器
        /// </summary>
        private HistoryNavigation navigation;

        /// <summary>
        /// 获取当前是否正在查看历史记录的状态
        /// </summary>
        public bool isViewingHistory => navigation.isViewingHistory;
        
        /// <summary>
        /// 获取历史日志管理器实例
        /// </summary>
        public HistoryLogManager logManager { get; private set; }

        /// <summary>
        /// Unity生命周期函数，在对象初始化时调用
        /// </summary>
        private void Awake()
        {
            // 设置单例模式实例
            instance = this;
            // 获取历史导航组件
            navigation = GetComponent<HistoryNavigation>();
            // 获取历史日志管理器组件
            logManager = GetComponent<HistoryLogManager>();
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
            // 将当前状态添加到历史日志管理器中
            logManager.AddLog(state);
            
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