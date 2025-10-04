using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话行类，用于存储对话系统中的一行对话信息
    /// 包含说话者、对话内容和命令信息
    /// </summary>
    public class DIALOGUE_LINE
    {
        /// <summary>
        /// 说话者名称
        /// </summary>
        public DL_SPEAKER_DATA speaker;
        
        /// <summary>
        /// 对话内容
        /// </summary>
        public DL_DIALOGUE_DATA dialogue;
        
        /// <summary>
        /// 命令字符串
        /// </summary>
        public string commands;

        /// <summary>
        /// 检查是否存在说话者
        /// </summary>
        public bool hasSpeaker => speaker != null;

        /// <summary>
        /// 检查是否存在对话内容
        /// </summary>
        public bool hasDialogue => dialogue.hasDialogue;
        
        /// <summary>
        /// 检查是否存在命令
        /// </summary>
        public bool hasCommands => commands != string.Empty;
        
        /// <summary>
        /// 构造函数，初始化对话行的各个属性
        /// </summary>
        /// <param name="speaker">说话者名称</param>
        /// <param name="dialogue">对话内容</param>
        /// <param name="commands">命令字符串</param>
        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            // 如果说话者字符串为空或只包含空白字符，则设置为null，否则创建新的说话者数据对象
            this.speaker = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            // 创建新的对话数据对象
            this.dialogue = new DL_DIALOGUE_DATA(dialogue);
            // 设置命令字符串
            this.commands = commands;
        }
    }
}

