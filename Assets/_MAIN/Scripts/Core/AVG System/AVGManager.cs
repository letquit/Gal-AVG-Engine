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
        /// 序列化的游戏配置数据对象引用
        /// </summary>
        [SerializeField] private AdventureGameSO config;
        
        /// <summary>
        /// 主摄像机引用变量
        /// </summary>
        public Camera mainCamera;

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

            // 创建一个空的AVG游戏保存实例
            if (AVGGameSave.activeFile == null)
                AVGGameSave.activeFile = new AVGGameSave();
        }

        /// <summary>
        /// Unity生命周期函数，在对象启用时调用，用于启动游戏加载流程
        /// </summary>
        private void Start()
        {
            LoadGame();
        }

        /// <summary>
        /// 加载游戏数据，根据是否为新游戏执行不同的加载逻辑
        /// </summary>
        private void LoadGame()
        {
            // 判断是否为新游戏
            if (AVGGameSave.activeFile.newGame)
            {
                // 新游戏：读取初始对话文件并开始对话
                List<string> lines = FileManager.ReadTextAsset(config.startingFile);
                Conversation start = new Conversation(lines);
                DialogueSystem.instance.Say(start);
            }
            else
            {
                // 存档游戏：激活已保存的游戏状态
                AVGGameSave.activeFile.Activate();
            }
        }
    }
}