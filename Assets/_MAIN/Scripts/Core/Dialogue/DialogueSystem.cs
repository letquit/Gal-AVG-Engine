using System;
using System.Collections.Generic;
using CHARACTERS;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话系统类，用于管理游戏中的对话逻辑
    /// 该类继承自MonoBehaviour，可作为Unity组件使用
    /// 实现了单例模式，确保全局只有一个对话系统实例
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField]
        private DialogueSystemConfigurationSO _config;
        
        /// <summary>
        /// 获取对话系统的配置对象
        /// </summary>
        public DialogueSystemConfigurationSO config => _config;
        
        /// <summary>
        /// 对话容器，用于管理对话界面中的各个UI元素
        /// </summary>
        public DialogueContainer dialogueContainer = new DialogueContainer();
        
        /// <summary>
        /// 对话管理器，用于控制对话流程的执行
        /// </summary>
        private ConversationManager conversationManager;
        
        /// <summary>
        /// 文本构建器，用于逐字显示对话文本效果
        /// </summary>
        private TextArchitect architect;

        /// <summary>
        /// 对话系统的单例实例
        /// </summary>
        public static DialogueSystem instance { get; private set; }

        /// <summary>
        /// 对话系统事件委托，用于定义无参数的事件回调
        /// </summary>
        public delegate void DialogueSystemEvent();
        
        /// <summary>
        /// 用户按下“下一句”按钮时触发的事件
        /// </summary>
        public event DialogueSystemEvent onUserPrompt_Next;

        /// <summary>
        /// 获取当前是否正在运行对话流程
        /// </summary>
        public bool isRunningConversation => conversationManager.isRunning;

        /// <summary>
        /// Unity生命周期函数，在对象创建时调用
        /// 用于初始化对话系统实例，实现单例模式
        /// 如果当前没有实例，则将当前对象设为实例
        /// 如果已存在实例，则销毁当前对象
        /// </summary>
        private void Awake()
        {
            // 单例模式实现：确保全局只有一个对话系统实例
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
                DestroyImmediate(gameObject);
        }

        /// <summary>
        /// 初始化对话系统的各个子模块
        /// 防止重复初始化
        /// </summary>
        private bool _initialized = false;
        private void Initialize()
        {
            if (_initialized)
                return;
            
            architect = new TextArchitect(dialogueContainer.dialogueText);
            conversationManager = new ConversationManager(architect);
            _initialized = true;
        }
        
        /// <summary>
        /// 触发用户按下“下一句”按键的事件
        /// </summary>
        public void OnUserPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        /// <summary>
        /// 根据说话者名称获取角色配置数据并应用到对话容器中
        /// </summary>
        /// <param name="speakerName">说话者的名称</param>
        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            Character character = CharacterManager.instance.GetCharacter(speakerName);
            CharacterConfigData config = character != null ? character.config : CharacterManager.instance.GetCharacterConfig(speakerName);
            
            ApplySpeakerDataToDialogueContainer(config);
        }

        /// <summary>
        /// 应用指定的角色配置数据到对话容器中
        /// </summary>
        /// <param name="config">角色配置数据</param>
        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData config)
        {
            dialogueContainer.SetConfig(config);
            dialogueContainer.nameContainer.SetConfig(config);
        }

        /// <summary>
        /// 判断说话者是否为叙述者（narrator），如果是则不显示名称
        /// 否则显示说话者名称
        /// </summary>
        /// <param name="speakerName">说话者的名称</param>
        public void ShowSpeakerName(string speakerName = "")
        {
            if (speakerName.ToLower() != "narrator")
                dialogueContainer.nameContainer.Show(speakerName);
            else
                HideSpeakerName();
        }

        /// <summary>
        /// 隐藏说话者名称显示
        /// </summary>
        public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

        /// <summary>
        /// 让指定角色说出一段对话
        /// 将单条对话包装成列表形式传递给对话管理器
        /// </summary>
        /// <param name="speaker">说话者名称</param>
        /// <param name="dialogue">对话内容</param>
        /// <returns>返回一个协程对象，可用于控制对话播放过程</returns>
        public Coroutine Say(string speaker, string dialogue)
        { 
            List<string> conversation = new List<string>() { $"{speaker} \"{dialogue}\"" };
            return Say(conversation);
        }

        /// <summary>
        /// 开始播放一组对话
        /// 调用对话管理器启动对话流程
        /// </summary>
        /// <param name="conversation">对话内容列表，每条格式为 "角色名 \"对话内容\""</param>
        /// <returns>返回一个协程对象，可用于控制对话播放过程</returns>
        public Coroutine Say(List<string> conversation)
        {
            return conversationManager.StarConversation(conversation);
        }
    }
}
