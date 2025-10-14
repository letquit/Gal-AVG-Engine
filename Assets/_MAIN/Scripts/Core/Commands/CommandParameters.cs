using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace COMMANDS
{
    /// <summary>
    /// 用于解析和管理命令行参数的类。
    /// 支持带标识符（如 -name value）和无标识符（位置参数）两种参数形式，并提供类型安全的参数获取方法。
    /// </summary>
    public class CommandParameters
    {
        private const char PARAMETER_IDENTIFIER = '-';
        
        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private List<string> unlabledParameters = new List<string>();

        /// <summary>
        /// 初始化 CommandParameters 实例并解析传入的参数数组。
        /// </summary>
        /// <param name="parameterArray">原始字符串参数数组</param>
        public CommandParameters(string[] parameterArray)
        {
            for (int i = 0; i < parameterArray.Length; i++)
            {
                //TODO: if (parameterArray[i].StartsWith(PARAMETER_IDENTIFIER) && !float.TryParse(parameterArray[i], NumberStyles.Float, CultureInfo.InvariantCulture, out _)) 
                if (parameterArray[i].StartsWith(PARAMETER_IDENTIFIER) && !float.TryParse(parameterArray[i], out _))
                {
                    string pName = parameterArray[i];
                    string pValue = "";

                    // 如果下一个元素不是参数标识符，则将其作为当前参数的值
                    if (i + 1 < parameterArray.Length && !parameterArray[i + 1].StartsWith(PARAMETER_IDENTIFIER))
                    {
                        pValue = parameterArray[i + 1];
                        i++;
                    }
                    
                    parameters.Add(pName, pValue);
                }
                else
                    unlabledParameters.Add(parameterArray[i]);
            }
        }

        /// <summary>
        /// 尝试从已命名参数或未标记参数中查找指定名称的参数，并尝试转换为目标类型。
        /// </summary>
        /// <typeparam name="T">目标类型（支持 bool、int、float 和 string）</typeparam>
        /// <param name="parameterName">要查找的参数名</param>
        /// <param name="value">输出参数：如果找到且能成功转换则返回对应值；否则为默认值</param>
        /// <param name="defaultValue">当找不到匹配项时使用的默认值</param>
        /// <returns>是否成功获取到有效参数值</returns>
        public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) =>
            TryGetValue(new string[] { parameterName }, out value, defaultValue);

        /// <summary>
        /// 尝试从多个候选参数名中依次查找参数，并尝试转换为目标类型。
        /// </summary>
        /// <typeparam name="T">目标类型（支持 bool、int、float 和 string）</typeparam>
        /// <param name="parameterNames">一组可能的参数名</param>
        /// <param name="value">输出参数：如果找到且能成功转换则返回对应值；否则为默认值</param>
        /// <param name="defaultValue">当找不到匹配项时使用的默认值</param>
        /// <returns>是否成功获取到有效参数值</returns>
        public bool TryGetValue<T>(string[] parameterNames, out T value, T defaultValue = default(T))
        {
            // 先在有标识符的参数中查找
            foreach (string parameterName in parameterNames)
            {
                if (parameters.TryGetValue(parameterName, out string parameterValue))
                {
                    if (TryCastParameter(parameterValue, out value))
                    {
                        return true;
                    }
                }
            }
            
            // 再在未标记参数中查找第一个可转换成功的项
            foreach (string parameterName in unlabledParameters)
            {
                if (TryCastParameter(parameterName, out value))
                {
                    unlabledParameters.Remove(parameterName);
                    return true;
                }
            }
            
            value = defaultValue;
            return false;
        }

        /// <summary>
        /// 根据目标泛型类型尝试将字符串参数值转换为对应的类型。
        /// </summary>
        /// <typeparam name="T">期望的目标类型</typeparam>
        /// <param name="parameterValue">待转换的字符串参数值</param>
        /// <param name="value">输出参数：若转换成功则返回结果；否则为该类型的默认值</param>
        /// <returns>是否转换成功</returns>
        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int intValue))
                {
                    value = (T)(object)intValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                //TODO: if (float.TryParse(parameterValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                // if (float.TryParse(parameterValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                if (float.TryParse(parameterValue, out float floatValue))
                {
                    value = (T)(object)floatValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true;
            }
            
            value = default(T);
            return false;
        }
    }
}
