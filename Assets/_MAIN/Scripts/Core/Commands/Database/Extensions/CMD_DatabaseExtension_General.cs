using System;
using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace COMMANDS
{
    /// <summary>
    /// 通用命令数据库扩展类，用于添加通用的命令功能
    /// </summary>
    public class CMD_DatabaseExtension_General : CMD_DatabaseExtension
    {
        /// <summary>
        /// 定义速度参数的标识符数组，支持 "-s" 和 "-spd"
        /// </summary>
        private static readonly string[] PARAM_SPEED = new string[] { "-s", "-spd" };
        
        /// <summary>
        /// 定义立即执行参数的标识符数组，支持 "-i" 和 "-immediate"
        /// </summary>
        private static readonly string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        
        /// <summary>
        /// 定义文件路径参数的标识符数组，支持 "-f"、"-file" 和 "-filepath"
        /// </summary>
        private static readonly string[] PARAM_FILEPATH = new string[] { "-f", "-file", "-filepath" };
        
        /// <summary>
        /// 定义入队参数的标识符数组，支持 "-e" 和 "-enqueue"
        /// </summary>
        private static readonly string[] PARAM_ENQUEUE = new string[] { "-e", "-enqueue" };
        
        /// <summary>
        /// 扩展命令数据库，向其中添加通用命令
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例</param>
        new public static void Extend(CommandDatabase database)
        {
            // 添加等待命令，用于暂停执行指定秒数
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));
            
            // 添加显示对话框系统命令
            database.AddCommand("showui", new Func<string[], IEnumerator>(ShowDialogueSystem));
            // 添加隐藏对话框系统命令
            database.AddCommand("hideui", new Func<string[], IEnumerator>(HideDialogueSystem));
            
            // 添加显示对话框命令
            database.AddCommand("showdb", new Func<string[], IEnumerator>(ShowDialogueBox));
            // 添加隐藏对话框命令
            database.AddCommand("hidedb", new Func<string[], IEnumerator>(HideDialogueBox));
            
            database.AddCommand("load", new Action<string[]>(LoadNewDialogueFile));
        }

        /// <summary>
        /// 加载新的对话文件并启动或入队新对话
        /// </summary>
        /// <param name="data">包含文件路径和入队标志等参数的字符串数组</param>
        private static void LoadNewDialogueFile(string[] data)
        {
            string fileName = string.Empty;
            bool enqueue = false;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_FILEPATH, out fileName);
            parameters.TryGetValue(PARAM_ENQUEUE, out enqueue, defaultValue: false);
            
            string filePath = FilePaths.GetPathToResource(FilePaths.resources_dialogueFiles, fileName);
            TextAsset file = Resources.Load<TextAsset>(filePath);

            if (file == null)
            {
                Debug.LogWarning($"File '{filePath}' could not be loaded from dialogue files. Please ensure it exists within the '{FilePaths.resources_dialogueFiles}' resources folder.");
                return;
            }

            List<string> lines = FileManager.ReadTextAsset(file, includeBlankLines: true);
            Conversation newConversation = new Conversation(lines);
            
            if (enqueue)
                DialogueSystem.instance.conversationManager.Enqueue(newConversation);
            else
                DialogueSystem.instance.conversationManager.StartConversation(newConversation);
        }

        /// <summary>
        /// 等待指定时间的协程命令
        /// </summary>
        /// <param name="data">等待时间（秒）的字符串表示</param>
        /// <returns>等待指定时间的IEnumerator枚举器</returns>
        private static IEnumerator Wait(string data)
        {
            // 解析等待时间并执行等待
            if (float.TryParse(data, out float time))
            {
                yield return new WaitForSeconds(time);
            }
        }

        /// <summary>
        /// 显示对话框的协程命令
        /// </summary>
        /// <param name="data">命令参数数组，支持 -spd（速度）和 -i（立即执行）参数</param>
        /// <returns>控制对话框显示过程的IEnumerator枚举器</returns>
        private static IEnumerator ShowDialogueBox(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            yield return DialogueSystem.instance.dialogueContainer.Show(speed, immediate);
        }
        
        /// <summary>
        /// 隐藏对话框的协程命令
        /// </summary>
        /// <param name="data">命令参数数组，支持 -spd（速度）和 -i（立即执行）参数</param>
        /// <returns>控制对话框隐藏过程的IEnumerator枚举器</returns>
        private static IEnumerator HideDialogueBox(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            yield return DialogueSystem.instance.dialogueContainer.Hide(speed, immediate);
        }
        
        /// <summary>
        /// 显示整个对话系统的协程命令
        /// </summary>
        /// <param name="data">命令参数数组，支持 -spd（速度）和 -i（立即执行）参数</param>
        /// <returns>控制对话系统显示过程的IEnumerator枚举器</returns>
        private static IEnumerator ShowDialogueSystem(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            yield return DialogueSystem.instance.Show(speed, immediate);
        }
        
        /// <summary>
        /// 隐藏整个对话系统的协程命令
        /// </summary>
        /// <param name="data">命令参数数组，支持 -spd（速度）和 -i（立即执行）参数</param>
        /// <returns>控制对话系统隐藏过程的IEnumerator枚举器</returns>
        private static IEnumerator HideDialogueSystem(string[] data)
        {
            float speed;
            bool immediate;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            yield return DialogueSystem.instance.Hide(speed, immediate);
        }
    }
}