using System;
using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TESTING
{
    /// <summary>
    /// 测试对话队列功能的类
    /// 用于演示对话系统的队列管理和优先级处理
    /// </summary>
    public class TestConversationQueue : MonoBehaviour
    {
        /// <summary>
        /// Unity生命周期函数，在对象启用时调用
        /// 启动协程执行主要的对话测试逻辑
        /// </summary>
        private void Start()
        {
            StartCoroutine(Running());
        }
        
        /// <summary>
        /// 执行主要对话测试逻辑的协程
        /// 显示一组初始对话，然后隐藏对话框
        /// </summary>
        /// <returns>IEnumerator枚举器，用于协程执行</returns>
        private IEnumerator Running()
        { 
            // 创建初始对话行列表
            List<string> lines = new List<string>()
            {
                "This is line 1 from the original conversation.",
                "This is line 2 from the original conversation.",
                "This is line 3 from the original conversation."
            };

            // 显示对话并等待其完成
            yield return DialogueSystem.instance.Say(lines);

            // 隐藏对话界面
            DialogueSystem.instance.Hide();
        }

        /// <summary>
        /// Unity生命周期函数，每帧调用
        /// 处理键盘输入以测试对话队列功能
        /// Q键添加普通优先级对话到队列
        /// W键添加高优先级对话到队列
        /// </summary>
        private void Update()
        {
            List<string> lines = new List<string>();
            Conversation conversation = null;

            // 检测Q键按下，添加普通对话到队列
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                lines = new List<string>()
                {
                    "This is the start of an enqueued conversation.",
                    "We can keep it going!"
                };
                conversation = new Conversation(lines);
                DialogueSystem.instance.conversationManager.Enqueue(conversation);
            }

            // 检测W键按下，添加优先对话到队列
            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                lines = new List<string>()
                {
                    "This is an important conversation!",
                    "August 26, 2023 is international dog day!"
                };
                conversation = new Conversation(lines);
                DialogueSystem.instance.conversationManager.EnqueuePriority(conversation);
            }
        }
    }
}