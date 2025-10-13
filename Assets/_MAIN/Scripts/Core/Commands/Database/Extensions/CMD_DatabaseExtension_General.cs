using System;
using System.Collections;
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
            database.AddCommand("wait", new Func<string, IEnumerator>(Wait));
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
    }
}

