using System;
using System.Collections;
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
        /// 扩展命令数据库，向其中添加通用命令
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例</param>
        new public static void Extend(CommandDatabase database)
        {
            // 添加等待命令，用于暂停执行指定秒数
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));
            // 添加显示对话框命令
            database.AddCommand("showdb", new Func<IEnumerator>(ShowDialogueBox));
            // 添加隐藏对话框命令
            database.AddCommand("hidedb", new Func<IEnumerator>(HideDialogueBox));
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
        /// <returns>显示对话框操作的IEnumerator枚举器</returns>
        private static IEnumerator ShowDialogueBox()
        {
            yield return DialogueSystem.instance.dialogueContainer.Show();
        }
        
        /// <summary>
        /// 隐藏对话框的协程命令
        /// </summary>
        /// <returns>隐藏对话框操作的IEnumerator枚举器</returns>
        private static IEnumerator HideDialogueBox()
        {
            yield return DialogueSystem.instance.dialogueContainer.Hide();
        }
    }
}