using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 提供逻辑行处理相关的工具方法。
    /// </summary>
    public static class LogicalLineUtils
    {
        /// <summary>
        /// 封装相关功能的辅助类，用于解析和提取被大括号 `{}` 包裹的数据段。
        /// </summary>
        public static class Encapsulation
        {
            /// <summary>
            /// 存储原始选择数据的结构体。
            /// 包含原始行集合与结束索引，用于定位对话继续点。
            /// </summary>
            public struct EncapsulatedData
            {
                /// <summary>
                /// 被封装的内容行列表。
                /// </summary>
                public List<string> lines;

                /// <summary>
                /// 封装内容在对话中的起始索引。
                /// </summary>
                public int startingIndex;

                /// <summary>
                /// 封装内容在对话中的结束索引。
                /// </summary>
                public int endingIndex;
            }
            
            private const char ENCAPSULATION_START = '{';
            private const char ENCAPSULATION_END = '}';
            
            /// <summary>
            /// 从当前对话进度开始提取原始选择数据，直到遇到封闭符号 '}' 为止。
            /// 数据范围由一对大括号 `{}` 定义，内部可能嵌套其他结构。
            /// </summary>
            /// <param name="conversation">要从中提取数据的对话对象。</param>
            /// <param name="startingIndex">开始搜索封装内容的起始行索引。</param>
            /// <param name="ripHeaderAndEncapsulators">是否将起始和结束标记本身也加入结果中，默认为 false。</param>
            /// <returns>封装了原始选择数据的对象，包括所有相关行以及结束索引。</returns>
            public static EncapsulatedData RipEncapsulationData(Conversation conversation, int startingIndex, bool ripHeaderAndEncapsulators = false)
            {
                // 初始化封装深度计数器及返回数据结构
                int encapsulationDepth = 0;
                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startingIndex = startingIndex, endingIndex = 0 };

                // 遍历对话行以识别并收集封装区域内的所有有效行
                for (int i = startingIndex; i < conversation.Count; i++)
                {
                    string line = conversation.GetLines()[i];

                    // 根据配置决定是否保留头部和封装符，并排除已闭合的最后一行
                    if (ripHeaderAndEncapsulators || (encapsulationDepth > 0 && !IsEncapsulationEnd(line)))
                        data.lines.Add(line);

                    // 判断当前行为封装起始符，增加嵌套层级
                    if (IsEncapsulationStart(line))
                    {
                        encapsulationDepth++;
                        continue;
                    }

                    // 判断当前行为封装结束符，减少嵌套层级
                    if (IsEncapsulationEnd(line))
                    {
                        encapsulationDepth--;

                        // 嵌套层级归零表示完成整个封装块的读取
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
            /// 检查给定行是否为嵌套结构的起始标记（'{'）。
            /// </summary>
            /// <param name="line">要检查的行内容。</param>
            /// <returns>如果是起始标记返回 true，否则返回 false。</returns>
            public static bool IsEncapsulationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START);

            /// <summary>
            /// 检查给定行是否为嵌套结构的结束标记（'}'）。
            /// </summary>
            /// <param name="line">要检查的行内容。</param>
            /// <returns>如果是结束标记返回 true，否则返回 false。</returns>
            public static bool IsEncapsulationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END);
        }
    }
}
