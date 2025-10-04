using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 解析对话文本行，提取说话者、对话内容和命令。
    /// </summary>
    public class DialogueParser
    {
        private const string commandRegexPattern = @"\w*[^\s]\(";
        
        /// <summary>
        /// 解析原始对话行并创建一个 DIALOGUE_LINE 对象。
        /// </summary>
        /// <param name="rawLine">需要解析的原始对话字符串。</param>
        /// <returns>包含说话者、对话内容和命令的 DIALOGUE_LINE 实例。</returns>
        public static DIALOGUE_LINE Parse(string rawLine)
        {
            Debug.Log($"Parsing line - '{rawLine}'");
            
            (string speaker, string dialogue, string commands) = RipContent(rawLine);
            
            Debug.Log($"Speaker = '{speaker}'\nDialogue = '{dialogue}'\nCommands = '{commands}'");
            
            return new DIALOGUE_LINE(speaker, dialogue, commands);
        }

        /// <summary>
        /// 从原始对话行中提取说话者名称、对话内容以及命令部分。
        /// </summary>
        /// <param name="rawLine">要处理的原始对话字符串。</param>
        /// <returns>元组形式返回：(说话者, 对话内容, 命令)</returns>
        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker = "", dialogue = "", commands = "";

            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool isEscaped = false;

            // 查找对话内容起始与结束位置（由双引号包裹的部分）
            for (int i = 0; i < rawLine.Length; i++)
            {
                char current = rawLine[i];
                if (current == '\\')
                    isEscaped = !isEscaped;
                else if (current == '"' && !isEscaped)
                {
                    if (dialogueStart == -1)
                        dialogueStart = i;
                    else if (dialogueEnd == -1)
                    {
                        dialogueEnd = i;
                        break;
                    }
                }
                else
                    isEscaped = false;
            }

            // 检查是否存在命令格式的内容（如 functionName( 参数 )）
            Regex commandRegex = new Regex(commandRegexPattern);
            Match match = commandRegex.Match(rawLine);
            int commandStart = -1;
            if (match.Success)
            {
                commandStart = match.Index;

                // 如果没有找到对话内容，则整个行作为命令处理
                if (dialogueStart == -1 && dialogueEnd == -1)
                    return ("", "", rawLine.Trim());
            }
            
            
            // 根据识别出的位置信息判断各部分内容，并进行赋值
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 ||  commandStart > dialogueEnd))
            {
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1)
                    commands = rawLine.Substring(commandStart).Trim();
            }
            else if (commandStart != -1 && dialogueStart > commandStart)
                commands = rawLine;
            else
                speaker = rawLine;
            
            return (speaker, dialogue, commands);
        }
    }
}

