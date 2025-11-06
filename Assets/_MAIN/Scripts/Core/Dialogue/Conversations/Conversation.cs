using System.Collections.Generic;

namespace DIALOGUE
{
    /// <summary>
    /// 对话会话类，用于管理一系列对话行及其进度
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// 存储文本行的列表，用于保存处理过程中的字符串数据
        /// </summary>
        private List<string> lines = new List<string>();

        /// <summary>
        /// 进度计数器，用于跟踪当前处理的进度状态
        /// </summary>
        private int progress = 0;
        
        /// <summary>
        /// 获取文件路径
        /// </summary>
        public string file { get; private set; }
        
        /// <summary>
        /// 获取文件开始索引位置
        /// </summary>
        public int fileStartIndex { get; private set; }
        
        /// <summary>
        /// 获取文件结束索引位置
        /// </summary>
        public int fileEndIndex { get; private set; }
        
        /// <summary>
        /// 初始化Conversation实例
        /// </summary>
        /// <param name="lines">对话行列表</param>
        /// <param name="progress">初始进度值，默认为0</param>
        /// <param name="file">文件路径，默认为空字符串</param>
        /// <param name="fileStartIndex">文件开始索引，默认为-1，表示从0开始</param>
        /// <param name="fileEndIndex">文件结束索引，默认为-1，表示到最后一行</param>
        public Conversation(List<string> lines,int progress = 0, string file = "", int fileStartIndex = -1, int fileEndIndex = -1)
        {
            this.lines = lines;
            this.progress = progress;
            this.file = file;
            
            // 设置默认的文件开始索引
            if (fileStartIndex == -1)
                fileStartIndex = 0;
            // 设置默认的文件结束索引
            if (fileEndIndex == -1)
                fileEndIndex = lines.Count - 1;
            
            this.fileStartIndex = fileStartIndex;
            this.fileEndIndex = fileEndIndex;
        }
        
        /// <summary>
        /// 获取当前对话进度
        /// </summary>
        /// <returns>当前进度值</returns>
        public int GetProgress() => progress;
        
        /// <summary>
        /// 设置对话进度
        /// </summary>
        /// <param name="value">要设置的进度值</param>
        public void SetProgress(int value) => progress = value;
        
        /// <summary>
        /// 递增对话进度
        /// </summary>
        public void IncrementProgress() => progress++;
        
        /// <summary>
        /// 获取对话行总数
        /// </summary>
        public int Count => lines.Count;
        
        /// <summary>
        /// 获取所有对话行
        /// </summary>
        /// <returns>对话行列表</returns>
        public List<string> GetLines() => lines;
        
        /// <summary>
        /// 获取当前进度对应的对话行。
        /// </summary>
        /// <returns>当前对话行内容。如果进度超出范围，则返回 null。</returns>
        public string CurrentLine()
        {
            if (HasReachedEnd())
            {
                return null;
            }
            return lines[progress];
        }
        
        /// <summary>
        /// 检查是否已到达对话末尾
        /// </summary>
        /// <returns>如果进度大于等于对话行数则返回true，否则返回false</returns>
        public bool HasReachedEnd() => progress >= lines.Count;
    }
}