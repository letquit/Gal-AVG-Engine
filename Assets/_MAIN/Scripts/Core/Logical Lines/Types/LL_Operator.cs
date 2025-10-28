using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Expressions;

namespace DIALOGUE.LogicalLines
{
    /// <summary>
    /// 实现逻辑行接口，用于处理变量赋值与运算操作（如 +=、-= 等）。
    /// </summary>
    public class LL_Operator : ILogicalLine
    {
        /// <summary>
        /// 获取该逻辑行的关键字标识符。
        /// </summary>
        public string keyword { get; }

        /// <summary>
        /// 执行当前逻辑行的操作。解析原始命令字符串并执行相应的变量操作。
        /// </summary>
        /// <param name="line">包含要执行的原始数据的对话行对象。</param>
        /// <returns>一个 IEnumerator 对象，支持协程执行。</returns>
        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            // 去除首尾空白字符后按正则表达式分割命令行
            string trimmedLine = line.rawData.Trim();
            string[] parts = Regex.Split(trimmedLine, REGEX_ARITHMATIC);

            // 检查是否至少有三个部分：变量名、操作符、值
            if (parts.Length < 3)
            {
                Debug.LogError($"Invalid command: {trimmedLine}");
                yield break;
            }
            
            // 提取变量名、操作符以及剩余的部分作为值表达式
            string variable = parts[0].Trim().TrimStart(VariableStore.VARIABLE_ID);
            string op = parts[1].Trim();
            string[] remainingParts = new string[parts.Length - 2];
            Array.Copy(parts, 2, remainingParts, 0, parts.Length - 2);

            // 计算右侧表达式的值
            object value = CalculateValue(remainingParts);
            
            if (value == null)
                yield break;
            
            // 处理具体的操作
            ProcessOperator(variable, op, value);
        }

        /// <summary>
        /// 根据操作符类型决定是创建新变量还是更新已有变量。
        /// </summary>
        /// <param name="variable">目标变量名称。</param>
        /// <param name="op">操作符（例如 =, += 等）。</param>
        /// <param name="value">要设置或参与计算的值。</param>
        private void ProcessOperator(string variable, string op, object value)
        {
            if (VariableStore.TryGetValue(variable, out object currentValue))
            {
                ProcessOperatorOnVariable(variable, op, value, currentValue);
            }
            else if (op == "=")
            {
                VariableStore.CreateVariable(variable, value);
            }
        }

        /// <summary>
        /// 在已存在变量上应用指定的操作。
        /// 支持赋值和基本数学运算（加减乘除），以及字符串拼接。
        /// </summary>
        /// <param name="variable">目标变量名称。</param>
        /// <param name="op">操作符。</param>
        /// <param name="value">参与运算的新值。</param>
        /// <param name="currentValue">变量当前的值。</param>
        private void ProcessOperatorOnVariable(string variable, string op, object value, object currentValue)
        {
            switch (op)
            {
                case "=":
                    VariableStore.TrySetValue(variable, value);
                    break;
                case "+=":
                    VariableStore.TrySetValue(variable, ConcatenateOrAdd(value, currentValue));
                    break;
                case "-=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) - Convert.ToDouble(value));
                    break;
                case "*=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) * Convert.ToDouble(value));
                    break;
                case "/=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) / Convert.ToDouble(value));
                    break;
                default:
                    Debug.LogError($"Invalid operator: {op}");
                    break;
            }
        }

        /// <summary>
        /// 判断传入的值是否可以进行连接（字符串）或数值相加，并返回对应结果。
        /// </summary>
        /// <param name="value">待处理的值。</param>
        /// <param name="currentValue">当前变量中的值。</param>
        /// <returns>如果是字符串则返回拼接后的字符串；否则返回两个数之和。</returns>
        private object ConcatenateOrAdd(object value, object currentValue)
        {
            if (value is string)
                return currentValue.ToString() + value;
            
            return Convert.ToDouble(currentValue) + Convert.ToDouble(value);
        }

        /// <summary>
        /// 判断给定的对话行是否匹配本逻辑行的格式要求。
        /// 使用预定义的正则表达式来判断。
        /// </summary>
        /// <param name="line">需要检查的对话行。</param>
        /// <returns>如果匹配成功返回 true，否则返回 false。</returns>
        public bool Matches(DIALOGUE_LINE line)
        {
            Match match = Regex.Match(line.rawData.Trim(), REGEX_OPERATOR_LINE);
            
            return match.Success;
        }
    }
}
