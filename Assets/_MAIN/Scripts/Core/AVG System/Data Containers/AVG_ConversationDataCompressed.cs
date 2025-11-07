using System;
using UnityEngine;

namespace ADVENTUREGAME
{
    /// <summary>
    /// AVG对话数据压缩类，用于存储对话文件的压缩信息
    /// </summary>
    [Serializable]
    public class AVG_ConversationDataCompressed
    {
        /// <summary>
        /// 对话文件名
        /// </summary>
        public string fileName;
        
        /// <summary>
        /// 对话开始索引
        /// </summary>
        public int startIndex;
        
        /// <summary>
        /// 对话结束索引
        /// </summary>
        public int endIndex;
        
        /// <summary>
        /// 对话进度
        /// </summary>
        public int progress;
    }
}