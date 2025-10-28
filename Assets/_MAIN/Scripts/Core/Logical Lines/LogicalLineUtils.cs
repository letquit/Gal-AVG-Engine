using System;
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
        
        /// <summary>
        /// 提供表达式解析与计算功能的静态工具类。
        /// </summary>
        public static class Expressions
        {
            /// <summary>
            /// 定义支持的操作符集合，包括赋值、算术运算等操作符。
            /// </summary>
            public static HashSet<string> OPERATORS = new HashSet<string>() { "-", "-=", "+", "+=", "*", "*=", "/", "/=", "=" };

            /// <summary>
            /// 正则表达式：用于匹配基本的算术操作符（如 +, -, *, /, = 及其复合形式）。
            /// </summary>
            public static readonly string REGEX_ARITHMATIC = @"([-+*/=]=?)";

            /// <summary>
            /// 正则表达式：用于识别以变量开头并可能带有赋值操作的语句行。
            /// </summary>
            public static readonly string REGEX_OPERATOR_LINE = @"^\$\w+\s*(=|\+=|-=|\*=|/=|)\s*";

            /// <summary>
            /// 根据给定的表达式部分数组进行数学计算，并返回最终结果。
            /// </summary>
            /// <param name="expressionParts">由空格或操作符分隔的字符串数组，表示一个表达式的各个组成部分。</param>
            /// <returns>表达式计算后的结果对象。</returns>
            public static object CalculateValue(string[] expressionParts)
            {
                // 分别存储操作数和操作符
                List<string> operandStrings = new List<string>();
                List<string> operatorStrings = new List<string>();
                List<object> operands = new List<object>();

                // 遍历所有表达式片段，将它们分类为操作数或操作符
                for (int i = 0; i < expressionParts.Length; i++)
                {
                    string part = expressionParts[i].Trim();

                    if (part == string.Empty)
                        continue;

                    if (OPERATORS.Contains(part))
                        operatorStrings.Add(part);
                    else
                        operandStrings.Add(part);
                }

                // 将字符串类型的操作数转换为实际值
                foreach (string operandString in operandStrings)
                {
                    operands.Add(ExtractValue(operandString));
                }

                // 按照优先级顺序执行乘除法
                CalculateValue_DivisionAndMultiplication(operatorStrings, operands);

                // 执行加减法
                CalculateValue_AdditionAndSubtraction(operatorStrings, operands);

                return operands[0];
            }

            /// <summary>
            /// 处理表达式中的乘法和除法操作。按照从左到右的顺序依次处理。
            /// </summary>
            /// <param name="operatorStrings">当前剩余的操作符列表。</param>
            /// <param name="operands">当前剩余的操作数列表。</param>
            private static void CalculateValue_DivisionAndMultiplication(List<string> operatorStrings,
                List<object> operands)
            {
                for (int i = 0; i < operatorStrings.Count; i++)
                {
                    string operatorString = operatorStrings[i];

                    if (operatorString == "*" || operatorString == "/")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (operatorString == "*")
                            operands[i] = leftOperand * rightOperand;
                        else
                        {
                            if (rightOperand == 0)
                            {
                                Debug.LogError("Cannot divide by zero!");
                                return;
                            }
                            operands[i] = leftOperand / rightOperand;
                        }
                    }

                    // 移除已处理过的操作数和操作符
                    operands.RemoveAt(i + 1);
                    operatorStrings.RemoveAt(i);
                    i--;
                }
            }

            /// <summary>
            /// 处理表达式中的加法和减法操作。在乘除之后按从左到右顺序处理。
            /// </summary>
            /// <param name="operatorStrings">当前剩余的操作符列表。</param>
            /// <param name="operands">当前剩余的操作数列表。</param>
            private static void CalculateValue_AdditionAndSubtraction(List<string> operatorStrings,
                List<object> operands)
            {
                for (int i = 0; i < operatorStrings.Count; i++)
                {
                    string operatorString = operatorStrings[i];

                    if (operatorString == "+" || operatorString == "-")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (operatorString == "+")
                            operands[i] = leftOperand + rightOperand;
                        else
                            operands[i] = leftOperand - rightOperand;

                        // 移除已处理过的操作数和操作符
                        operands.RemoveAt(i + 1);
                        operatorStrings.RemoveAt(i);
                        i--;
                    }
                }
            }

            /// <summary>
            /// 解析单个表达式元素（可能是变量、常量或带否定前缀的布尔值），提取其真实值。
            /// </summary>
            /// <param name="value">要解析的原始字符串值。</param>
            /// <returns>解析后的真实数据对象。</returns>
            private static object ExtractValue(string value)
            {
                bool negate = false;

                // 判断是否需要逻辑取反
                if (value.StartsWith("!"))
                {
                    negate = true;
                    value = value.Substring(1);
                }

                // 若是变量引用，则尝试获取变量值
                if (value.StartsWith(VariableStore.VARIABLE_ID))
                {
                    string variableName = value.TrimStart(VariableStore.VARIABLE_ID);
                    if (!VariableStore.HasVariable(variableName))
                    {
                        Debug.LogError($"Variable {variableName} does not exits!");
                        return null;
                    }

                    VariableStore.TryGetValue(variableName, out object val);

                    if (val is bool boolVal && negate)
                        return !boolVal;

                    return val;
                }
                // 若是字符串字面量，则去除引号并注入标签
                else if (value.StartsWith('\"') && value.EndsWith('\"'))
                {
                    value = TagManager.Inject(value, injectTags: true, injectVariables: true);
                    return value.Trim('"');
                }
                // 否则尝试将其解析为数字或布尔值
                else
                {
                    if (int.TryParse(value, out int intValue))
                    {
                        return intValue;
                    }
                    else if (float.TryParse(value, out float floatValue))
                    {
                        return floatValue;
                    }
                    else if (bool.TryParse(value, out bool boolValue))
                    {
                        return negate ? !boolValue : boolValue;
                    }
                    else
                        return value;
                }
            }
        }
    }
}
