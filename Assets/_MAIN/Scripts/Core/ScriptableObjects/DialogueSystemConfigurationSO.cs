using CHARACTERS;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话系统配置ScriptableObject类，用于存储和管理对话系统的全局配置信息
    /// </summary>
    [CreateAssetMenu(fileName = "Dialogue System Configuration", menuName = "Dialogue System/Dialogue Configuration Asset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public const float DEFAULT_FONTSIZE_DIALOGUE = 31f;
        public const float DEFAULT_FONTSIZE_NAME = 35f;
        
        /// <summary>
        /// 角色配置资源引用，用于获取角色相关的配置信息
        /// </summary>
        public CharacterConfigSO characterConfigurationAsset;
        
        /// <summary>
        /// 对话文本的默认颜色设置
        /// </summary>
        public Color defaultTextColor = Color.white;
        
        /// <summary>
        /// 对话文本的默认字体资源
        /// </summary>
        public TMP_FontAsset defaultFont;

        /// <summary>
        /// 对话文本的字体缩放比例
        /// </summary>
        public float dialogueFontScale = 1f;

        /// <summary>
        /// 对话文本的默认字体大小
        /// </summary>
        public float defaultDialogueFontSize = DEFAULT_FONTSIZE_DIALOGUE;

        /// <summary>
        /// 角色名称文本的默认字体大小
        /// </summary>
        public float defaultNameFontSize = DEFAULT_FONTSIZE_NAME;
    }
}
