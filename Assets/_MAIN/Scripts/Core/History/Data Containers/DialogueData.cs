using System;
using DIALOGUE;
using TMPro;
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
        
        /// <summary>
        /// 应用对话数据到对话系统界面
        /// </summary>
        /// <param name="data">包含对话内容、样式等信息的对话数据对象</param>
        public static void Apply(DialogueData data)
        { 
            // 获取对话系统实例和相关UI组件
            var ds = DialogueSystem.instance;
            var dialogueText = ds.dialogueContainer.dialogueText;
            var nameText = ds.dialogueContainer.nameContainer.nameText;
            
            // 设置对话文本内容和样式
            dialogueText.text = data.currentDialogue;
            dialogueText.color = data.dialogueColor;
            dialogueText.fontSize = data.dialogueScale;
            
            // 修复打字机效果下的历史记录显示问题
            dialogueText.maxVisibleCharacters = data.currentDialogue.Length;
            dialogueText.ForceMeshUpdate();
            
            // 设置说话者名称文本内容和样式
            nameText.text = data.currentSpeaker;
            
            // 如果名称不为空，则显示名称容器, 否则隐藏
            if (nameText.text != string.Empty)
                ds.dialogueContainer.nameContainer.Show();
            else
                ds.dialogueContainer.nameContainer.Hide();
            
            nameText.color = data.speakerNameColor;
            nameText.fontSize = data.speakerScale;

            // 如果对话文本字体需要更换，则加载并应用新字体
            if (data.dialogueFont != dialogueText.font.name)
            {
                TMP_FontAsset fontAsset = HistoryCache.LoadFont(data.dialogueFont);
                if (fontAsset != null)
                    dialogueText.font = fontAsset;
            }

            // 如果说话者名称字体需要更换，则加载并应用新字体
            if (data.speakerFont != nameText.font.name)
            {
                TMP_FontAsset fontAsset = HistoryCache.LoadFont(data.speakerFont);
                if (fontAsset != null)
                    nameText.font = fontAsset;
            }
        }
    }
}