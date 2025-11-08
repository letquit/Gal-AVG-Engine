using System;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace ADVENTUREGAME
{
    /// <summary>
    /// AVG游戏管理器类，负责加载和管理AVG游戏的对话文件
    /// </summary>
    public class AVGManager : MonoBehaviour
    {
        /// <summary>
        /// AVGManager的单例实例
        /// </summary>
        public static AVGManager instance { get; private set; }

        /// <summary>
        /// Unity生命周期方法，在对象创建时初始化单例实例
        /// </summary>
        private void Awake()
        {
            instance = this;
            
            // 获取AVGDatabaseLinkSetup组件实例
            AVGDatabaseLinkSetup linkSetup = GetComponent<AVGDatabaseLinkSetup>();
            // 调用外部链接设置方法，建立数据库连接
            linkSetup.SetupExternalLinks();
        }

        /// <summary>
        /// 加载指定路径的对话文件并启动对话系统
        /// </summary>
        /// <param name="filePath">对话文件在Resources文件夹中的相对路径</param>
        public void LoadFile(string filePath)
        {
            // 创建存储文件行内容的列表
            List<string> lines = new List<string>();
            // 从Resources文件夹中加载文本资源
            TextAsset file = Resources.Load<TextAsset>(filePath);

            try
            {
                // 读取文本资源的内容
                lines = FileManager.ReadTextAsset(file);
            }
            catch
            {
                // 当文件不存在时输出错误日志并重新抛出异常
                Debug.LogError($"Dialogue file at path 'Resources/{filePath}' does not exist!");
                throw;
            }
        
            // 调用对话系统播放加载的对话内容
            DialogueSystem.instance.Say(lines, filePath);
        }
    }
}