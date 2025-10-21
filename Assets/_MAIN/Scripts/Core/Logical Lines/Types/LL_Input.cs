using System.Collections;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 逻辑行处理器，用于处理输入相关的对话逻辑
    /// 实现ILogicalLine接口，专门处理关键字为"input"的对话行
    /// </summary>
    public class LL_Input : ILogicalLine
    {
        /// <summary>
        /// 获取该逻辑行处理器对应的关键字
        /// </summary>
        public string keyword => "input";
        
        /// <summary>
        /// 执行输入逻辑行的处理
        /// 显示输入面板并等待用户输入完成
        /// </summary>
        /// <param name="line">要处理的对话行数据</param>
        /// <returns>协程迭代器，用于异步等待用户输入</returns>
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            // 获取对话行的原始数据作为输入面板的标题
            string title = line.dialogueData.rawData;
            
            // 获取输入面板实例并显示
            InputPanel panel = InputPanel.instance;
            panel.Show(title);

            // 等待用户完成输入操作
            while (panel.isWaitingOnUserInput)
                yield return null;
        }
        
        /// <summary>
        /// 判断给定的对话行是否匹配当前逻辑行处理器
        /// 匹配条件：对话行具有说话者且说话者名称等于关键字"input"
        /// </summary>
        /// <param name="line">要检查的对话行</param>
        /// <returns>如果匹配返回true，否则返回false</returns>
        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.name.ToLower() == keyword);
        }
    }
}