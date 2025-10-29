using System.Collections;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Conditions;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 实现条件逻辑行（if/else）的执行逻辑。
    /// 该类用于解析并执行对话系统中的条件语句，根据条件结果选择性地执行对应的对话分支。
    /// </summary>
    public class LL_Condition : ILogicalLine
    {
        /// <summary>
        /// 获取当前逻辑行的关键字标识符。
        /// </summary>
        public string keyword => "if";

        private const string ELSE = "else";
        private readonly string[] CONTAINERS = new string[] { "(", ")" };

        /// <summary>
        /// 执行指定的条件逻辑行。
        /// 解析条件表达式，并根据其真假决定执行 if 或 else 分支的内容。
        /// </summary>
        /// <param name="line">要执行的对话行对象。</param>
        /// <returns>协程迭代器，用于异步控制流程。</returns>
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            // 提取原始条件字符串
            string rawCondition = ExtractCondition(line.rawData.Trim());

            // 计算条件判断的结果
            bool conditionResult = EvaluateCondition(rawCondition);

            // 获取当前会话及进度信息
            Conversation currentConversation = DialogueSystem.instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.instance.conversationManager.conversationProgress;

            // 提取 if 块的数据封装
            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, false);

            // 初始化 else 数据块
            EncapsulatedData elseData = new EncapsulatedData();

            // 检查是否存在 else 分支
            if (ifData.endingIndex + 1 < currentConversation.Count)
            {
                string nextLine = currentConversation.GetLines()[ifData.endingIndex + 1].Trim();
                if (nextLine == ELSE)
                {
                    // 存在 else 分支时提取其数据
                    elseData = RipEncapsulationData(currentConversation, ifData.endingIndex + 1, false);
                    ifData.endingIndex = elseData.endingIndex;
                }
            }

            // 更新会话进度到条件结构末尾
            currentConversation.SetProgress(ifData.endingIndex);

            // 根据条件结果选择需要执行的数据块
            EncapsulatedData selData = conditionResult ? ifData : elseData;

            // 若选中数据有效且包含内容，则将其作为优先级对话入队处理
            if ((selData.lines != null) && selData.lines.Count > 0)
            {
                Conversation newConversation = new Conversation(selData.lines);
                DialogueSystem.instance.conversationManager.EnqueuePriority(newConversation);
            }

            yield return null;
        }

        /// <summary>
        /// 判断给定的对话行是否匹配当前逻辑行类型。
        /// </summary>
        /// <param name="line">待检测的对话行。</param>
        /// <returns>如果以关键字"if"开头则返回true；否则返回false。</returns>
        public bool Matches(DIALOGUE_LINE line)
        {
            return line.rawData.Trim().StartsWith(keyword);
        }

        /// <summary>
        /// 从一行文本中提取出括号内的条件表达式部分。
        /// </summary>
        /// <param name="line">原始输入行文本。</param>
        /// <returns>去除首尾空格后的条件表达式字符串。</returns>
        private string ExtractCondition(string line)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.IndexOf(CONTAINERS[1]);

            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}
