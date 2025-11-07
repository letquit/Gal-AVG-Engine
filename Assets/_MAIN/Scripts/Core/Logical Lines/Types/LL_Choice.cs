using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 实现对话系统中的“选择”逻辑行类型。该类用于解析并执行包含多个选项的对话块，
    /// 并根据玩家的选择跳转到对应的对话分支。
    /// 
    /// 此类实现了 ILogicalLine 接口，表示一种特殊的对话逻辑行（以 speaker 名称为 "choice" 标识），
    /// 其作用是收集一组可供玩家选择的选项，并在运行时显示 UI 面板供用户交互，随后将所选结果插入对话流程中。
    /// </summary>
    public class LL_Choice : ILogicalLine
    {
        /// <summary>
        /// 获取当前逻辑行的关键字标识符，固定为 "choice"。
        /// </summary>
        public string keyword => "choice";
        
        private const char CHOICE_IDENTIFIER = '-';

        /// <summary>
        /// 执行当前逻辑行的核心方法。此方法会解析当前上下文中的选择数据、构建选项列表、
        /// 展示选择界面、等待用户输入，并最终将选中项的结果作为新的对话内容入队。
        /// </summary>
        /// <param name="line">当前要处理的对话行对象，其中包含原始对话文本信息。</param>
        /// <returns>返回一个协程迭代器，支持异步操作控制流。</returns>
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            // 获取当前对话和进度
            var currentConversation = DialogueSystem.instance.conversationManager.conversation;
            var progress = DialogueSystem.instance.conversationManager.conversationProgress;
            // 调用RipEncapsulationData方法从当前会话中提取封装数据
            EncapsulatedData data = RipEncapsulationData(currentConversation, progress, ripHeaderAndEncapsulators: true, parentStartingIndex: currentConversation.fileStartIndex);

            // 解析出具体的选择项
            List<Choice> choices = GetChoicesFromData(data);

            // 获取标题文本
            string title = line.dialogueData.rawData;

            // 获取全局唯一的选择面板实例
            ChoicePanel panel = ChoicePanel.instance;

            // 构造选项标题数组
            string[] choiceTitle = choices.Select(c => c.title).ToArray();

            // 显示选择面板
            panel.Show(title, choiceTitle);

            // 等待用户做出选择
            while (panel.isWaitingOnUserChoice)
                yield return null;

            // 取得用户的最终选择
            Choice selectedChoice = choices[panel.lastDecision.answerIndex];

            // 创建一个新的对话对象来承载被选中的后续对话内容
            Conversation newConversation = new Conversation(selectedChoice.resultLines, file: currentConversation.file, fileStartIndex: selectedChoice.startIndex, fileEndIndex: selectedChoice.endIndex);

            // 设置主对话进度到指定的结束索引位置即跳转到对话中的特定节点
            DialogueSystem.instance.conversationManager.conversation.SetProgress(data.endingIndex - currentConversation.fileStartIndex);

            // 将新构造的对话内容插入到对话管理器的高优先级队列中
            DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
        }

        /// <summary>
        /// 判断给定的对话行是否匹配当前逻辑行的关键字。
        /// 匹配条件为：该行具有发言者且其名称等于关键字 "choice"（忽略大小写）。
        /// </summary>
        /// <param name="line">需要判断的对话行对象。</param>
        /// <returns>如果匹配返回 true，否则返回 false。</returns>
        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.name.ToLower() == keyword);
        }

        /// <summary>
        /// 从封装数据中提取选项列表
        /// </summary>
        /// <param name="data">包含选项信息的封装数据</param>
        /// <returns>解析出的选项列表</returns>
        private List<Choice> GetChoicesFromData(EncapsulatedData data)
        {
            List<Choice> choices = new List<Choice>();
            int encapsulationDepth = 0;
            bool isFirstChoice = true;

            Choice choice = new Choice
            {
                title = string.Empty,
                resultLines = new List<string>(),
            };
            
            // 遍历数据行，解析选项内容并构建选项列表
            // 该循环处理从第二行开始的所有数据行，识别选项起始位置，
            // 提取选项标题，并将选项内容按起始和结束索引分组存储
            int choiceIndex = 0, i = 0;
            for (i = 1; i < data.lines.Count; i++)
            {
                var line = data.lines[i];
                
                // 判断当前行是否为新选项的开始，并且处于正确的封装层级
                if (IsChoiceStart(line) && encapsulationDepth == 1)
                {
                    // 如果不是第一个选项，则将当前选项添加到结果列表中
                    if (!isFirstChoice)
                    {
                        choice.startIndex = data.startingIndex + (choiceIndex + 1);
                        choice.endIndex = data.startingIndex + (i - 1);
                        choices.Add(choice);
                        choice = new Choice
                        {
                            title = string.Empty,
                            resultLines = new List<string>(),
                        };
                    }

                    choiceIndex = i;
                    // 设置选项标题，去掉开头的 '-' 符号
                    choice.title = line.Trim().Substring(1);
                    isFirstChoice = false;
                    continue;
                }

                // 将当前行添加到结果列表中，并更新封装层级
                AddLineToResults(line, ref choice, ref encapsulationDepth);
            }

            // 处理最后一个选项，如果尚未添加到结果列表中
            if (!choices.Contains(choice))
            {
                choice.startIndex = data.startingIndex + (choiceIndex + 1);
                choice.endIndex = data.startingIndex + (i - 2);
                choices.Add(choice);
            }

            return choices;
        }

        /// <summary>
        /// 将指定行添加到当前选项的结果行中，并维护嵌套层级计数。
        /// 忽略最外层的大括号，仅记录内层有效内容。
        /// </summary>
        /// <param name="line">要添加的行内容。</param>
        /// <param name="choice">目标选项对象的引用。</param>
        /// <param name="encapsulationDepth">当前嵌套层级深度的引用。</param>
        private void AddLineToResults(string line, ref Choice choice, ref int encapsulationDepth)
        {
            line = line.Trim();

            // 处理嵌套开始标记
            if (IsEncapsulationStart(line))
            {
                if (encapsulationDepth > 0)
                    choice.resultLines.Add(line);
                encapsulationDepth++;
                return;
            }

            // 处理嵌套结束标记
            if (IsEncapsulationEnd(line))
            {
                encapsulationDepth--;

                if (encapsulationDepth > 0)
                    choice.resultLines.Add(line);

                return;
            }

            // 添加普通行内容
            choice.resultLines.Add(line);
        }

        /// <summary>
        /// 检查给定行是否为选项的起始标记（'-'）。
        /// </summary>
        /// <param name="line">要检查的行内容。</param>
        /// <returns>如果是选项起始标记返回 true，否则返回 false。</returns>
        private bool IsChoiceStart(string line) => line.Trim().StartsWith(CHOICE_IDENTIFIER);

        /// <summary>
        /// 表示一个选项的结构体，包括标题和结果对话行。
        /// 用于存储解析后的单个选项信息。
        /// </summary>
        private struct Choice
        {
            /// <summary>
            /// 选项显示在界面上的文字标题。
            /// </summary>
            public string title;

            /// <summary>
            /// 用户选择此项后应播放的一系列对话行。
            /// </summary>
            public List<string> resultLines;

            /// <summary>
            /// 起始索引字段，用于标识某个范围或序列的开始位置
            /// </summary>
            public int startIndex;
            
            /// <summary>
            /// 结束索引字段，用于标识某个范围或序列的结束位置
            /// </summary>
            public int endIndex;
        }
    }
}
