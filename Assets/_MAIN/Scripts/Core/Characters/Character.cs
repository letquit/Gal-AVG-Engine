using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 角色基类，定义了所有角色类型的通用属性和功能
    /// </summary>
    public abstract class Character
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string name = "";
        
        /// <summary>
        /// 角色显示名称，用于在对话中展示的角色名字
        /// </summary>
        public string displayName = "";

        /// <summary>
        /// 角色在UI中的根节点变换组件
        /// </summary>
        public RectTransform root = null;

        public CharacterConfigData config;
        
        /// <summary>
        /// 获取全局对话系统实例的引用
        /// </summary>
        public DialogueSystem dialogueSystem => DialogueSystem.instance;

        /// <summary>
        /// 初始化角色对象
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <param name="config">角色配置数据</param>
        public Character(string name, CharacterConfigData config)
        {
            this.name = name;
            displayName = name;
            this.config = config;
        }
        
        /// <summary>
        /// 让角色说出一段对话（单句）
        /// </summary>
        /// <param name="dialogue">要显示的对话文本</param>
        /// <returns>返回一个协程对象，可用于控制对话播放过程</returns>
        public Coroutine Say(string dialogue) => Say(new List<string>() { dialogue});

        /// <summary>
        /// 让角色说出多段对话
        /// </summary>
        /// <param name="dialogue">包含多条对话文本的列表</param>
        /// <returns>返回一个协程对象，可用于控制对话播放过程</returns>
        public Coroutine Say(List<string> dialogue)
        {
            // 如果角色有显示名称，则先显示说话者名字
            if (!string.IsNullOrEmpty(displayName))
            {
                dialogueSystem.ShowSpeakerName(displayName);
                dialogueSystem.ApplySpeakerDataToDialogueContainer(name);
            }
            return dialogueSystem.Say(dialogue);
        }
        
        /// <summary>
        /// 角色类型枚举，定义了支持的不同角色表现形式
        /// </summary>
        public enum CharacterType
        {
            /// <summary>
            /// 文本类型角色
            /// </summary>
            Text,
            /// <summary>
            /// 精灵图片类型角色
            /// </summary>
            Sprite,
            /// <summary>
            /// 精灵图集类型角色
            /// </summary>
            SpriteSheet,
            /// <summary>
            /// Live2D类型角色
            /// </summary>
            Live2D,
            /// <summary>
            /// 3D模型类型角色
            /// </summary>
            Model3D
        }
    }
}

