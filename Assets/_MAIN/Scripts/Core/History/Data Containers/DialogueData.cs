using System;
using DIALOGUE;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 对话数据类，用于存储和捕获当前对话系统中的对话信息和样式设置
    /// 包括对话内容、说话者信息以及各自的字体、颜色和缩放设置
    /// </summary>
    [Serializable]
    public class DialogueData
    {
        public string currentDialogue = "";
        public string currentSpeaker = "";

        public string dialogueFont;
        public Color dialogueColor;
        public float dialogueScale;

        public string speakerFont;
        public Color speakerNameColor;
        public float speakerScale;

        /// <summary>
        /// 捕获当前对话系统中的对话数据
        /// 从DialogueSystem实例中获取当前显示的对话文本和说话者名称，
        /// 以及它们的字体、颜色和字体大小信息
        /// </summary>
        /// <returns>包含当前对话信息的DialogueData实例</returns>
        public static DialogueData Capture()
        {
            DialogueData data = new DialogueData();

            var ds = DialogueSystem.instance;
            var dialogueText = ds.dialogueContainer.dialogueText;
            var nameText = ds.dialogueContainer.nameContainer.nameText;
            
            // 捕获对话文本信息
            data.currentDialogue = dialogueText.text;
            data.dialogueFont = FilePaths.resources_font + dialogueText.font.name;
            data.dialogueColor = dialogueText.color;
            data.dialogueScale = dialogueText.fontSize;
            
            // 捕获说话者信息
            data.currentSpeaker = nameText.text;
            data.speakerFont = FilePaths.resources_font + nameText.font.name;
            data.speakerNameColor = nameText.color;
            data.speakerScale = nameText.fontSize;
            
            return data;
        }
    }
}