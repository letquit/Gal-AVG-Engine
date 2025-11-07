using System;
using System.Collections.Generic;
using System.Linq;
using DIALOGUE;
using History;
using UnityEngine;

namespace ADVENTUREGAME
{
    /// <summary>
    /// 表示一个 AVG 游戏存档数据结构。用于序列化和反序列化游戏状态信息。
    /// </summary>
    [Serializable]
    public class AVGGameSave
    {
        /// <summary>
        /// 当前激活的存档文件实例。
        /// </summary>
        public static AVGGameSave activeFile = null;

        /// <summary>
        /// 存档文件扩展名常量。
        /// </summary>
        public const string FILE_TYPE = ".avg";

        /// <summary>
        /// 截图文件扩展名常量。
        /// </summary>
        public const string SCREENSHOT_FILE_TYPE = ".jpg";

        /// <summary>
        /// 是否启用加密存储功能（当前未实现）。
        /// </summary>
        public const bool ENCRYPT_FILES = false;

        /// <summary>
        /// 获取该存档对应的完整路径。
        /// </summary>
        public string filePath => $"{FilePaths.gameSaves}{slotNumber}{FILE_TYPE}";

        /// <summary>
        /// 获取该存档对应截图的完整路径。
        /// </summary>
        public string screenshotPath => $"{FilePaths.gameSaves}{slotNumber}{SCREENSHOT_FILE_TYPE}";

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string playerName;

        /// <summary>
        /// 存档槽位编号，默认为 1。
        /// </summary>
        public int slotNumber = 1;

        //两种不同的保存 其一是通过引用文件名和对话开始结束的索引 其二是保存对话的所有内容-所有行和进度(RPG)

        /// <summary>
        /// 正在进行中的对话数据数组，每项是 JSON 序列化的字符串表示。
        /// </summary>
        public string[] activeConversations;

        /// <summary>
        /// 当前历史状态快照。
        /// </summary>
        public HistoryState activeState;

        /// <summary>
        /// 历史记录日志数组。
        /// </summary>
        public HistoryState[] historyLogs;

        /// <summary>
        /// 将当前游戏状态保存到指定路径中。
        /// 包括捕获当前的历史状态、获取历史日志以及正在运行的对话数据，并将其以 JSON 格式写入磁盘。
        /// </summary>
        public void Save()
        {
            activeState = HistoryState.Capture();
            historyLogs = HistoryManager.instance.history.ToArray();
            activeConversations = GetConversationData();

            string saveJSON = JsonUtility.ToJson(this);
            FileManager.Save(filePath, saveJSON);
        }

        /// <summary>
        /// 加载已保存的游戏状态并恢复相关系统组件的状态。
        /// 包括加载历史状态、重建历史日志管理器、设置对话队列等操作。
        /// </summary>
        public void Load()
        {
            if (activeState != null)
                activeState.Load();

            HistoryManager.instance.history = historyLogs.ToList();
            HistoryManager.instance.logManager.Clear();
            HistoryManager.instance.logManager.Rebuild();
            
            SetConversationData();
            
            DialogueSystem.instance.prompt.Hide();
        }

        /// <summary>
        /// 收集当前正在进行的所有对话的数据，并根据其来源（外部文件或直接定义）分别打包成压缩格式或完整格式。
        /// 返回一个包含这些对话数据的字符串数组。
        /// </summary>
        /// <returns>表示各对话数据的 JSON 字符串数组。</returns>
        private string[] GetConversationData()
        {
            List<string> retData = new List<string>();

            var conversations = DialogueSystem.instance.conversationManager.GetConversationQueue();

            for (int i = 0; i < conversations.Length; i++)
            {
                var conversation = conversations[i];
                string data = "";

                if (conversation.file != string.Empty)
                {
                    var compressedData = new AVG_ConversationDataCompressed();
                    compressedData.fileName = conversation.file;
                    compressedData.progress = conversation.GetProgress();
                    compressedData.startIndex = conversation.fileStartIndex;
                    compressedData.endIndex = conversation.fileEndIndex;
                    data = JsonUtility.ToJson(compressedData);
                    
                }
                else
                {
                    var fullData = new AVG_ConversationData();
                    fullData.conversation = conversation.GetLines();
                    fullData.progress = conversation.GetProgress();
                    data = JsonUtility.ToJson(fullData);
                }
                
                retData.Add(data);
            }
            
            return retData.ToArray();
        }

        /// <summary>
        /// 解析并还原之前保存的对话数据，重新构建对话队列。
        /// 对于每个对话条目尝试解析为完整对话或压缩对话格式，并依次启动或加入对话管理系统。
        /// </summary>
        private void SetConversationData()
        {
            for (int i = 0; i < activeConversations.Length; i++)
            {
                try
                {
                    string data = activeConversations[i];
                    Conversation conversation = null;

                    var fullData = JsonUtility.FromJson<AVG_ConversationData>(data);
                    if (fullData != null && fullData.conversation != null && fullData.conversation.Count > 0)
                    {
                        conversation = new Conversation(fullData.conversation, fullData.progress);
                    }
                    else
                    {
                        var compressedData = JsonUtility.FromJson<AVG_ConversationDataCompressed>(data);
                        if (compressedData != null && compressedData.fileName != string.Empty)
                        {
                            TextAsset file = Resources.Load<TextAsset>(compressedData.fileName);
                            
                            int count = compressedData.endIndex - compressedData.startIndex;
                            
                            List<string> lines = FileManager.ReadTextAsset(file).Skip(compressedData.startIndex).Take(count + 1).ToList();

                            conversation = new Conversation(lines, compressedData.progress, compressedData.fileName,
                                compressedData.startIndex, compressedData.endIndex);
                        }
                        else
                        {
                            Debug.LogError($"Unknow conversation format! Unable to reload conversation from AVGGameSave using data '{data}'");
                        }
                    }

                    if (conversation != null && conversation.GetLines().Count > 0)
                    {
                        if (i == 0)
                            DialogueSystem.instance.conversationManager.StartConversation(conversation);
                        else
                            DialogueSystem.instance.conversationManager.Enqueue(conversation);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Encountered error while extracting saved conversation data! {e}");
                    continue;
                }
            }
        }
    }
}