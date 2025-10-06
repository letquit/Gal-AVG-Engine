using System;
using DIALOGUE;
using TMPro;
using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 角色配置数据类，用于存储和管理角色的相关配置信息
    /// </summary>
    [Serializable]
    public class CharacterConfigData
    {
        public string name;
        public string alias;
        public Character.CharacterType characterType;

        public Color nameColor;
        public Color dialogueColor;

        public TMP_FontAsset nameFont;
        public TMP_FontAsset dialogueFont;

        /// <summary>
        /// 创建当前角色配置数据的深拷贝副本
        /// </summary>
        /// <returns>返回一个新的CharacterConfigData实例，包含与当前实例相同的数据</returns>
        public CharacterConfigData Copy()
        {
            CharacterConfigData result = new CharacterConfigData();
            
            result.name = name;
            result.alias = alias;
            result.characterType = characterType;
            result.nameFont = nameFont;
            result.dialogueFont = dialogueFont;
            
            // 创建颜色的深拷贝，避免引用相同颜色对象
            result.nameColor = new Color(nameColor.r, nameColor.g, nameColor.b, nameColor.a);
            result.dialogueColor = new Color(dialogueColor.r, dialogueColor.g, dialogueColor.b, dialogueColor.a);
            
            return result;
        }

        /// <summary>
        /// 获取对话系统配置中的默认文本颜色
        /// </summary>
        private static Color defaultColor => DialogueSystem.instance.config.defaultTextColor;
        
        /// <summary>
        /// 获取对话系统配置中的默认字体
        /// </summary>
        private static TMP_FontAsset defaultFont => DialogueSystem.instance.config.defaultFont;

        /// <summary>
        /// 获取默认的角色配置数据实例
        /// </summary>
        public static CharacterConfigData Default
        {
            get
            { 
                CharacterConfigData result = new CharacterConfigData();
            
                result.name = "";
                result.alias = "";
                result.characterType = Character.CharacterType.Text;
                
                result.nameFont = defaultFont;
                result.dialogueFont = defaultFont;
            
                // 使用默认颜色初始化名称和对话颜色
                result.nameColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a);
                result.dialogueColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, defaultColor.a);
            
                return result;
            }
        }
    }
}

