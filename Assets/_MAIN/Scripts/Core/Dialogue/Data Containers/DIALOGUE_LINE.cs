using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话行类，用于存储和管理对话系统中的一行完整对话信息
    /// 包括说话者信息、对话内容和命令数据
    /// </summary>
    public class DIALOGUE_LINE
    {
        public DL_SPEAKER_DATA speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMMAND_DATA commandData;
        
        /// <summary>
        /// 获取是否存在说话者数据
        /// </summary>
        public bool hasSpeaker => speakerData != null;
        
        /// <summary>
        /// 获取是否存在对话数据
        /// </summary>
        public bool hasDialogue => dialogueData != null;
        
        /// <summary>
        /// 获取是否存在命令数据
        /// </summary>
        public bool hasCommands => commandData != null;

        /// <summary>
        /// 构造函数，初始化对话行对象
        /// </summary>
        /// <param name="speaker">说话者信息字符串</param>
        /// <param name="dialogue">对话内容字符串</param>
        /// <param name="commands">命令数据字符串</param>
        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            // 根据字符串是否为空白来决定是否创建相应的数据对象
            this.speakerData = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogueData = (string.IsNullOrWhiteSpace(dialogue) ? null : new DL_DIALOGUE_DATA(dialogue));
            this.commandData = (string.IsNullOrWhiteSpace(commands) ? null : new DL_COMMAND_DATA(commands));
            
            
        }
    }
}

