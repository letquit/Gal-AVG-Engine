using System;
using System.Collections.Generic;
using UnityEngine;

namespace ADVENTUREGAME
{
    /// <summary>
    /// AVG对话数据类，用于存储和管理对话内容及进度信息
    /// </summary>
    [Serializable]
    public class AVG_ConversationData
    {
        /// <summary>
        /// 对话内容列表，存储对话的文本信息
        /// </summary>
        public List<string> conversation = new List<string>();
        
        /// <summary>
        /// 对话进度索引，表示当前对话进行到第几句
        /// </summary>
        public int progress;
    }
}