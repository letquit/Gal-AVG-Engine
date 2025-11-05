using System;
using System.Collections.Generic;
using History;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DIALOGUE
{
    /// <summary>
    /// 玩家输入管理器类，用于处理玩家的键盘输入并触发对话系统的相关操作
    /// </summary>
    public class PlayerInputManager : MonoBehaviour
    {
        private PlayerInput input;

        /// <summary>
        /// 存储输入动作和对应命令的元组列表
        /// </summary>
        private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions =
            new List<(InputAction action, Action<InputAction.CallbackContext> command)>();

        /// <summary>
        /// 在Awake阶段获取PlayerInput组件并初始化输入动作
        /// </summary>
        private void Awake()
        {
            input = GetComponent<PlayerInput>();

            InitializeActions();
        }

        /// <summary>
        /// 初始化输入动作列表，将输入动作与对应的处理函数进行绑定
        /// </summary>
        private void InitializeActions()
        {
            actions.Add((input.actions["Next"], OnNext));
            actions.Add((input.actions["HistoryBack"], OnHistoryBack));
            actions.Add((input.actions["HistoryForward"], OnHistoryForward));
            actions.Add((input.actions["HistoryLogs"], OnHistoryToggleLog));
        }

        /// <summary>
        /// 当组件启用时，为所有输入动作注册事件处理函数
        /// </summary>
        private void OnEnable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed += inputAction.command;
        }

        /// <summary>
        /// 当组件禁用时，为所有输入动作注销事件处理函数，防止内存泄漏
        /// </summary>
        private void OnDisable()
        {
            foreach (var inputAction in actions)
                inputAction.action.performed -= inputAction.command;
        }

        /// <summary>
        /// 触发对话系统继续显示下一个对话内容
        /// </summary>
        /// <param name="c">输入动作的回调上下文信息</param>
        public void OnNext(InputAction.CallbackContext c)
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }
        
        /// <summary>
        /// 处理历史记录后退操作的回调函数
        /// </summary>
        /// <param name="c">输入动作的回调上下文信息</param>
        public void OnHistoryBack(InputAction.CallbackContext c)
        {
            HistoryManager.instance.GoBack();
        }
        
        /// <summary>
        /// 处理历史记录前进操作的回调函数
        /// </summary>
        /// <param name="c">输入动作的回调上下文信息</param>
        public void OnHistoryForward(InputAction.CallbackContext c)
        {
            HistoryManager.instance.GoForward();
        }
        
        /// <summary>
        /// 历史记录切换日志显示状态的回调函数
        /// </summary>
        /// <param name="c">输入动作的回调上下文</param>
        public void OnHistoryToggleLog(InputAction.CallbackContext c)
        {
            // 获取历史管理器中的日志管理器实例
            var logs = HistoryManager.instance.logManager;
            
            // 根据当前日志窗口的开启状态进行切换
            if (!logs.isOpen)
                logs.Open();
            else
                logs.Close();
        }
    }
}

