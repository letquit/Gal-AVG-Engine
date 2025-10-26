using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        private const char ENCAPSULATION_START = '{';
        private const char ENCAPSULATION_END = '}';
        private const char CHOICE_IDENTIFIER = '-';

        /// <summary>
        /// 执行当前逻辑行的核心方法。此方法会解析当前上下文中的选择数据、构建选项列表、
        /// 展示选择界面、等待用户输入，并最终将选中项的结果作为新的对话内容入队。
        /// </summary>
        /// <param name="line">当前要处理的对话行对象，其中包含原始对话文本信息。</param>
        /// <returns>返回一个协程迭代器，支持异步操作控制流。</returns>
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            // 提取原始选择数据
            RawChoiceData data = RipChoiceData();

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
            Conversation newConversation = new Conversation(selectedChoice.resultLines);

            // 设置主对话进度至当前选择块之后的位置
            DialogueSystem.instance.conversationManager.conversation.SetProgress(data.endingIndex);

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
        /// 从当前对话进度开始提取原始选择数据，直到遇到封闭符号 '}' 为止。
        /// 数据范围由一对大括号 `{}` 定义，内部可能嵌套其他结构。
        /// </summary>
        /// <returns>封装了原始选择数据的对象，包括所有相关行以及结束索引。</returns>
        private RawChoiceData RipChoiceData()
        {
            // 获取当前对话和进度信息
            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.instance.conversationManager.conversationProgress;
            int encapsulationDepth = 0;
            RawChoiceData data = new RawChoiceData { lines = new List<string>(), endingIndex = 0 };

            // 遍历对话行，提取选择数据范围
            for (int i = currentProgress; i < currentConversation.Count; i++)
            {
                string line = currentConversation.GetLines()[i];
                data.lines.Add(line);

                // 检查是否为封装开始符号，增加嵌套深度
                if (IsEncapsulationStart(line))
                {
                    encapsulationDepth++;
                    continue;
                }

                // 检查是否为封装结束符号，减少嵌套深度
                if (IsEncapsulationEnd(line))
                {
                    encapsulationDepth--;
                    // 当嵌套深度为0时，表示找到完整的数据范围
                    if (encapsulationDepth == 0)
                    {
                        data.endingIndex = i;
                        break;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 从原始选择数据中解析出所有可选的选项及其对应的结果对话行。
        /// 每个选项以 '-' 开头定义标题，在下一个选项或结束前的所有行为该选项的结果。
        /// 支持嵌套结构，通过封装层级进行识别。
        /// </summary>
        /// <param name="data">原始选择数据，包含多行字符串及结束位置。</param>
        /// <returns>解析后的选择列表，每个元素是一个完整的选项结构。</returns>
        private List<Choice> GetChoicesFromData(RawChoiceData data)
        {
            List<Choice> choices = new List<Choice>();
            int encapsulationDepth = 0;
            bool isFirstChoice = true;

            Choice choice = new Choice
            {
                title = string.Empty,
                resultLines = new List<string>(),
            };

            // 遍历所有行数据，跳过首行（即 keyword 行）
            foreach (var line in data.lines.Skip(1))
            {
                // 判断当前行是否为新选项的开始，并且处于正确的封装层级
                if (IsChoiceStart(line) && encapsulationDepth == 1)
                {
                    // 如果不是第一个选项，则将当前选项添加到结果列表中
                    if (!isFirstChoice)
                    {
                        choices.Add(choice);
                        choice = new Choice
                        {
                            title = string.Empty,
                            resultLines = new List<string>(),
                        };
                    }

                    // 设置选项标题，去掉开头的 '-' 符号
                    choice.title = line.Trim().Substring(1);
                    isFirstChoice = false;
                    continue;
                }

                // 将当前行添加到结果列表中，并更新封装层级
                AddLineToResults(line, ref choice, ref encapsulationDepth);
            }

            // 添加最后一个选项到结果列表中
            if (!choices.Contains(choice))
                choices.Add(choice);

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
        /// 检查给定行是否为嵌套结构的起始标记（'{'）。
        /// </summary>
        /// <param name="line">要检查的行内容。</param>
        /// <returns>如果是起始标记返回 true，否则返回 false。</returns>
        private bool IsEncapsulationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START);

        /// <summary>
        /// 检查给定行是否为嵌套结构的结束标记（'}'）。
        /// </summary>
        /// <param name="line">要检查的行内容。</param>
        /// <returns>如果是结束标记返回 true，否则返回 false。</returns>
        private bool IsEncapsulationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END);

        /// <summary>
        /// 检查给定行是否为选项的起始标记（'-'）。
        /// </summary>
        /// <param name="line">要检查的行内容。</param>
        /// <returns>如果是选项起始标记返回 true，否则返回 false。</returns>
        private bool IsChoiceStart(string line) => line.Trim().StartsWith(CHOICE_IDENTIFIER);

        /// <summary>
        /// 存储原始选择数据的结构体。
        /// 包含原始行集合与结束索引，用于定位对话继续点。
        /// </summary>
        private struct RawChoiceData
        {
            /// <summary>
            /// 原始选择相关的全部行内容。
            /// </summary>
            public List<string> lines;

            /// <summary>
            /// 当玩家做完选择后，对话系统知道该从哪一行继续往下走。
            /// </summary>
            public int endingIndex;
        }

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
        }
    }
}
