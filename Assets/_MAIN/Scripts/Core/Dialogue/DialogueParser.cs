using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话解析器，用于将原始对话字符串解析为结构化的对话行对象。
    /// </summary>
    public class DialogueParser
    {
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";
        
        /// <summary>
        /// 解析原始对话行字符串，提取发言者、对话内容和命令部分，并返回结构化的对话行对象。
        /// </summary>
        /// <param name="rawLine">原始对话行字符串</param>
        /// <returns>包含解析后发言者、对话内容和命令的 DIALOGUE_LINE 对象</returns>
        public static DIALOGUE_LINE Parse(string rawLine)
        {
            //Debug.Log($"Parsing line - '{rawLine}'");
            
            (string speaker, string dialogue, string commands) = RipContent(rawLine);
            
            //Debug.Log($"Speaker = '{speaker}'\nDialogue = '{dialogue}'\nCommands = '{commands}'");
            
            return new DIALOGUE_LINE(speaker, dialogue, commands);
        }

        /// <summary>
        /// 从原始对话字符串中提取发言者、对话内容和命令部分。
        /// </summary>
        /// <param name="rawLine">原始对话行字符串</param>
        /// <returns>包含发言者、对话内容和命令的元组</returns>
        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker = "", dialogue = "", commands = "";

            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool isEscaped = false;

            // 查找对话内容的起始和结束位置（由双引号包裹的部分）
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

            // 使用正则表达式查找命令部分
            Regex commandRegex = new Regex(commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(rawLine);
            int commandStart = -1;
            foreach (Match match in matches)
            {
                // 确保命令不在对话内容内部
                if (match.Index < dialogueStart || match.Index > dialogueEnd)
                {
                    commandStart = match.Index;
                    break;
                }
            }

            // 根据解析结果组合返回值
            if (commandStart != -1 && (dialogueStart == -1 && dialogueEnd == -1))
                return ("", "", rawLine.Trim());
            
            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1)
                    commands = rawLine.Substring(commandStart).Trim();
            }
            else if (commandStart != -1 && dialogueStart > commandStart)
                commands = rawLine;
            else
                dialogue = rawLine;
            
            return (speaker, dialogue, commands);
        }
    }
}

