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
        /// 是否启用加密存储功能。
        /// </summary>
        public const bool ENCRYPT = true;

        /// <summary>
        /// 截图缩放比例常量，用于指定截图时的缩放倍率
        /// </summary>
        public const float SCREENSHOT_DOWNSCALE_AMOUNT = 0.25f;


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

        /// <summary>
        /// 标识是否为新游戏的标志变量
        /// </summary>
        public bool newGame = true;

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
        /// 存储AVG变量数据的数组字段
        /// </summary>
        public AVG_VariableData[] variables;

        /// <summary>
        /// 时间戳字段，用于存储时间戳字符串
        /// </summary>
        public string timestamp;

        /// <summary>
        /// 从指定文件路径加载AVG游戏存档
        /// </summary>
        /// <param name="filePath">存档文件的完整路径</param>
        /// <param name="activateOnLoad">是否在加载完成后立即激活存档，默认为false</param>
        /// <returns>加载成功的AVGGameSave对象</returns>
        public static AVGGameSave Load(string filePath, bool activateOnLoad = false)
        {
            // 从文件加载游戏存档对象
            AVGGameSave save = FileManager.Load<AVGGameSave>(filePath, ENCRYPT);
            
            // 设置当前激活的存档文件
            activeFile = save;
            
            // 根据参数决定是否立即激活存档
            if (activateOnLoad)
                save.Activate();
            
            return save;
        }

        /// <summary>
        /// 保存游戏状态到文件
        /// </summary>
        public void Save()
        {
            // 设置新游戏标志
            newGame = false;
            
            // 捕获当前历史状态
            activeState = HistoryState.Capture();
            historyLogs = HistoryManager.instance.history.ToArray();
            activeConversations = GetConversationData();
            variables = GetVariableData();
            
            // 记录保存时间戳
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            // 截取游戏画面作为缩略图
            ScreenshotMaster.CaptureScreenshot(AVGManager.instance.mainCamera, Screen.width, Screen.height, SCREENSHOT_DOWNSCALE_AMOUNT, screenshotPath);

            // 将当前对象序列化为JSON并保存到文件
            string saveJSON = JsonUtility.ToJson(this);
            FileManager.Save(filePath, saveJSON, ENCRYPT);
        }

        /// <summary>
        /// 加载已保存的游戏状态并恢复相关系统组件的状态。
        /// 包括加载历史状态、重建历史日志管理器、设置对话队列等操作。
        /// </summary>
        public void Activate()
        {
            if (activeState != null)
                activeState.Load();

            HistoryManager.instance.history = historyLogs.ToList();
            HistoryManager.instance.logManager.Clear();
            HistoryManager.instance.logManager.Rebuild();
            
            SetVariableData();
            
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

        /// <summary>
        /// 获取所有变量数据的数组
        /// </summary>
        /// <returns>包含所有变量名称、值和类型的AVG_VariableData数组</returns>
        private AVG_VariableData[] GetVariableData()
        {
            List<AVG_VariableData> retData = new List<AVG_VariableData>();
            
            // 遍历所有数据库中的变量
            foreach (var database in VariableStore.databases.Values)
            {
                foreach (var variable in database.variables)
                {
                    AVG_VariableData variableData = new AVG_VariableData();
                    variableData.name = $"{database.name}.{variable.Key}";
                    string val = $"{variable.Value.Get()}";
                    variableData.value = val;
                    variableData.type = val == string.Empty ? "System.String" : variable.Value.Get().GetType().ToString();
                    retData.Add(variableData);
                }
            }
            return retData.ToArray();
        }
        
        /// <summary>
        /// 设置变量数据
        /// </summary>
        private void SetVariableData()
        {
            // 遍历所有变量并根据类型进行解析和设置
            foreach (var variable in variables)
            {
                string val = variable.value;
                switch (variable.type)
                {
                    case "System.Boolean":
                        if (bool.TryParse(val, out bool b_val))
                        {
                            VariableStore.TrySetValue(variable.name, b_val);
                            continue;
                        }
                        break;
                    case "System.Int32":
                        if (int.TryParse(val, out int i_val))
                        {
                            VariableStore.TrySetValue(variable.name, i_val);
                            continue;
                        }
                        break;
                    case "System.Single":
                        if (float.TryParse(val, out float f_val))
                        {
                            VariableStore.TrySetValue(variable.name, f_val);
                            continue;
                        }
                        break;
                    case "System.String":
                        VariableStore.TrySetValue(variable.name, val);
                        continue;
                }
                
                Debug.LogError($"Could not interpret variable type. {variable.name} = {variable.type}");
            }
        }
    }
}