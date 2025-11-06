using System.Collections.Generic;
using DIALOGUE;
using TMPro;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 历史导航控制类，用于管理对话历史记录的浏览功能。
    /// 提供前进和后退功能以查看历史状态，并更新相关显示文本。
    /// </summary>
    public class HistoryNavigation : MonoBehaviour
    {
        /// <summary>
        /// 当前进度值，表示对话或任务的完成进度
        /// </summary>
        public int progress = 0;
        
        /// <summary>
        /// 状态文本UI组件，用于显示当前状态信息
        /// </summary>
        [SerializeField] private TextMeshProUGUI statusText;
        
        /// <summary>
        /// 获取HistoryManager的单例实例
        /// </summary>
        HistoryManager manager => HistoryManager.instance;
        
        /// <summary>
        /// 获取历史记录列表的引用
        /// </summary>
        private List<HistoryState> history => manager.history;

        /// <summary>
        /// 缓存的历史状态，用于快速访问最近查看的历史记录
        /// </summary>
        private HistoryState cachedState = null;
        
        /// <summary>
        /// 标识当前是否处于缓存状态的标志位
        /// </summary>
        private bool isOnCachedState = false;
        
        /// <summary>
        /// 标识是否正在查看历史记录的标志位
        /// </summary>
        public bool isViewingHistory = false;

        /// <summary>
        /// 获取是否可以进行导航操作的状态
        /// 当对话系统不在逻辑行时返回true，否则返回false
        /// </summary>
        public bool canNavigate => !DialogueSystem.instance.conversationManager.isOnLogicalLine;
        
        /// <summary>
        /// 向前导航到下一个历史状态。
        /// 如果当前不在历史浏览模式中或不能进行导航操作则直接返回；
        /// 如果已到达最新状态，则加载缓存的状态并退出历史浏览模式。
        /// </summary>
        public void GoForward()
        {
            // 检查是否可以进行导航操作
            if (!isViewingHistory || !canNavigate)
                return;
            
            HistoryState state = null;

            // 根据进度判断是加载历史列表中的状态还是恢复缓存状态
            if (progress < history.Count - 1)
            {
                progress++;
                state = history[progress];
            }
            else
            { 
                isOnCachedState = true;
                state = cachedState;
            }
            
            state.Load();

            // 若回到当前最新状态，则结束历史浏览模式
            if (isOnCachedState)
            {
                isViewingHistory = false;
                DialogueSystem.instance.onUserPrompt_Next -= GoForward;
                statusText.text = "";
                DialogueSystem.instance.OnStopViewingHistory();
            }
            else
                UpdateStatusText();
        }
        
        /// <summary>
        /// 向后导航到上一个历史状态。
        /// 如果正在浏览历史，则向前移动进度；否则进入历史浏览模式并捕获当前状态作为缓存。
        /// </summary>
        public void GoBack()
        {
            // 检查是否可以进行导航操作
            if (history.Count == 0 || ((progress == 0 && isViewingHistory) || !canNavigate))
                return;
            
            progress = isViewingHistory ? progress - 1 : history.Count - 1;

            // 首次进入历史浏览时进行初始化操作
            if (!isViewingHistory)
            {
                isViewingHistory = true;
                isOnCachedState = false;
                cachedState = HistoryState.Capture();

                DialogueSystem.instance.onUserPrompt_Next += GoForward;
                DialogueSystem.instance.OnStartViewingHistory();
            }

            HistoryState state = history[progress];
            state.Load();
            UpdateStatusText();
        }

        /// <summary>
        /// 更新状态文本以反映当前在历史记录中的位置。
        /// 显示格式为："剩余未读条目数/总条目数"
        /// </summary>
        private void UpdateStatusText()
        {
            statusText.text = $"{history.Count - progress}/{history.Count}";
        }
    }
}