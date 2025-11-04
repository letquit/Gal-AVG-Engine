using System;
using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 自动阅读器，用于控制对话系统的自动播放与跳过功能。
    /// </summary>
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 18;
        private const float RED_TIME_PADDING = 0.5f;
        private const float MAX_READ_TIME = 99f;
        private const float MIN_READ_TIME = 1f;
        private const string STATUS_TEXT_AUTO = "Auto";
        private const string STATUS_TEXT_SKIP = "Skipping";
        
        private ConversationManager conversationManager;
        private TextArchitect architect => conversationManager.architect;
        
        [SerializeField] private TextMeshProUGUI statusText;
        [HideInInspector] public bool allowToggle = true;

        public bool skip { get; set; } = false;
        public float speed { get; set; } = 1f;
        
        public bool isOn => co_running != null;
        private Coroutine co_running = null;

        /// <summary>
        /// 初始化自动阅读器组件。
        /// </summary>
        /// <param name="conversationManager">关联的对话管理器实例。</param>
        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;

            statusText.text = string.Empty;
        }

        /// <summary>
        /// 启用自动阅读功能。
        /// </summary>
        public void Enable()
        {
            if (isOn)
                return;
            
            co_running = StartCoroutine(AutoRead());
        }

        /// <summary>
        /// 禁用自动阅读功能，并重置相关状态。
        /// </summary>
        public void Disable()
        {
            if (!isOn)
                return;
            
            StopCoroutine(co_running);
            skip = false;
            co_running = null;
            statusText.text = string.Empty;
        }

        /// <summary>
        /// 自动阅读对话内容的协程函数
        /// </summary>
        /// <returns>IEnumerator迭代器，用于协程执行</returns>
        private IEnumerator AutoRead()
        {
            // 等待一帧
            yield return null;
            
            // 检查对话管理器是否正在运行，如果不是则禁用自动阅读
            if (!conversationManager.isRunning)
            {
                Disable();
                yield break;
            }
            
            // 如果建筑师不在构建中且当前文本不为空，则触发下一条对话
            if (!architect.isBuilding && architect.currentText != string.Empty)
                DialogueSystem.instance.OnSystemPrompt_Next();

            // 主循环：持续处理自动阅读逻辑
            while (conversationManager.isRunning)
            {
                if (!skip)
                {
                    // 等待建筑师完成构建且对话管理器不在自动计时器等待状态
                    while (!architect.isBuilding && !conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    float timeStarted = Time.time;

                    // 等待建筑师构建完成或对话管理器自动计时器等待结束
                    while (architect.isBuilding || conversationManager.isWaitingOnAutoTimer)
                        yield return null;

                    // 计算阅读时间：基于字符数和默认阅读速度，限制在最小和最大阅读时间之间
                    float timeToRead =
                        Mathf.Clamp(
                            ((float)architect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND),
                            MIN_READ_TIME, MAX_READ_TIME);
                    // 调整阅读时间，减去已用时间并重新限制范围
                    timeToRead = Mathf.Clamp((timeToRead - (Time.time - timeStarted)), MIN_READ_TIME, MAX_READ_TIME);
                    // 根据速度调整最终阅读时间并添加红色时间填充
                    timeToRead = (timeToRead / speed) + RED_TIME_PADDING;
                    
                    // 等待计算出的阅读时间
                    yield return new WaitForSeconds(timeToRead);
                }
                else
                {
                    // 如果需要跳过，强制完成建筑师的构建并短暂等待
                    architect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }

                // 触发下一条对话
                DialogueSystem.instance.OnSystemPrompt_Next();
            }
            
            // 对话结束，禁用自动阅读功能
            Disable();
        }

        /// <summary>
        /// 切换自动播放模式。如果当前处于跳过状态则关闭，否则切换到自动播放。
        /// </summary>
        public void Toggle_Auto()
        {
            // 不允许切换
            if (!allowToggle)
                return;
            
            bool prevState = skip;
            skip = false;
            
            if (prevState)
                Enable();
            else
            {
                if (!isOn)
                    Enable();
                else
                    Disable();
            }
            
            if (isOn)
                statusText.text = STATUS_TEXT_AUTO;
        }

        /// <summary>
        /// 切换跳过模式。如果当前未启用跳过则开启，否则根据运行状态决定是否关闭。
        /// </summary>
        public void Toggle_Skip()
        {
            // 不允许切换
            if (!allowToggle)
                return;

            bool prevState = skip;
            skip = true;
            
            if (!prevState)
                Enable();
            else
            {
                if (!isOn)
                    Enable();
                else
                    Disable();
            }
            
            if (isOn)
                statusText.text = STATUS_TEXT_SKIP;
        }
    }
}