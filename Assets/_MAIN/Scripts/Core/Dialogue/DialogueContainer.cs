using System;
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
        /// 根据角色配置数据设置对话文本的样式
        /// </summary>
        /// <param name="config">包含对话文本颜色和字体配置的角色配置数据</param>
        public void SetConfig(CharacterConfigData config)
        {
            SetDialogueColor(config.dialogueColor);
            SetDialogueFont(config.dialogueFont);
        }
    }
}

