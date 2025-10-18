using System;
using System.Collections;
using CHARACTERS;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话框容器类，用于存储和管理对话系统中的UI组件引用
    /// 该类包含对话框的根对象以及用于显示角色名称和对话内容的文本组件
    /// </summary>
    [Serializable]
    public class DialogueContainer
    {
        /// <summary>
        /// 对话框的根游戏对象，用于控制整个对话框的显示和隐藏
        /// </summary>
        public GameObject root;
        
        /// <summary>
        /// 角色名称容器，用于存储和管理角色名称相关的UI组件引用
        /// </summary>
        public NameContainer nameContainer;

        /// <summary>
        /// 显示对话内容的TextMeshProUGUI文本组件
        /// </summary>
        public TextMeshProUGUI dialogueText;
        
        private CanvasGroupController cgController;
        
        private bool initialized = false;

        /// <summary>
        /// 初始化对话框容器，确保只初始化一次。创建并关联CanvasGroup控制器。
        /// </summary>
        public void Initialize()
        {
            if (initialized)
                return;

            cgController = new CanvasGroupController(DialogueSystem.instance, root.GetComponent<CanvasGroup>());
        }
        
        /// <summary>
        /// 获取当前对话框是否可见的状态
        /// </summary>
        public bool isVisible => cgController.isVisible;

        /// <summary>
        /// 显示对话框
        /// </summary>
        /// <param name="speed">显示动画的速度倍数，默认为1f</param>
        /// <param name="immediate">是否立即显示而不播放过渡动画，默认为false</param>
        /// <returns>执行显示操作的协程</returns>
        public Coroutine Show(float speed = 1f, bool immediate = false) => cgController.Show(speed, immediate);

        /// <summary>
        /// 隐藏对话框
        /// </summary>
        /// <param name="speed">隐藏动画的速度倍数，默认为1f</param>
        /// <param name="immediate">是否立即隐藏而不播放过渡动画，默认为false</param>
        /// <returns>执行隐藏操作的协程</returns>
        public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);
        
        /// <summary>
        /// 设置对话文本的颜色
        /// </summary>
        /// <param name="color">要设置的颜色值</param>
        public void SetDialogueColor(Color color) => dialogueText.color = color;
        
        /// <summary>
        /// 设置对话文本的字体
        /// </summary>
        /// <param name="font">要设置的字体资源</param>
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        
        /// <summary>
        /// 设置对话文本的字体大小
        /// </summary>
        /// <param name="size">要设置的字体大小</param>
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;
        
        /// <summary>
        /// 根据角色配置数据设置对话文本的样式
        /// </summary>
        /// <param name="config">包含对话文本颜色和字体配置的角色配置数据</param>
        public void SetConfig(CharacterConfigData config)
        {
            // 应用角色配置中的对话文本颜色
            SetDialogueColor(config.dialogueColor);
            
            // 应用角色配置中的对话文本字体
            SetDialogueFont(config.dialogueFont);
            
            // 应用角色配置中的对话文本字体大小，并考虑缩放因子
            SetDialogueFontSize(config.dialogueFontSize * config.dialogueFontScale);
        }
    }
}