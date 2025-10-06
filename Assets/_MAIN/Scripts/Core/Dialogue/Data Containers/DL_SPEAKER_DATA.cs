namespace DIALOGUE
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;

    /// <summary>
    /// 表示对话系统中发言者的数据结构。
    /// 解析原始字符串格式的发言者信息，提取名称、别名、位置和表情表达式等数据。
    /// </summary>
    public class DL_SPEAKER_DATA
    {
        /// <summary>
        /// 发言者的原始名称（未经过"as"替换的部分）
        /// </summary>
        public string name, castName;

        /// <summary>
        /// 获取显示名称：如果设置了castName则使用castName，否则使用name
        /// </summary>
        public string displayname => (castName != string.Empty ? castName : name);

        /// <summary>
        /// 发言者在场景中的坐标位置
        /// </summary>
        public Vector2 castPosition;

        /// <summary>
        /// 发言者各图层的表情表达式列表，每个元素为(layer, expression)元组
        /// </summary>
        public List<(int layer, string expression)> CastExpressions { get; set; }

        // 常量定义用于解析输入字符串的不同部分标识符
        private const string NAMECAST_ID = " as ";
        private const string POSITIONCAST_ID = " at ";
        private const string EXPRESSIONCAST_ID = " [";
        private const char AXISDELIMITER = ':';
        private const char EXPRESSIONLAYER_JOINER = ',';
        private const char EXPRESSIONLAYER_DELIMITER = ':';

        /// <summary>
        /// 根据给定的原始发言者字符串初始化DL_SPEAKER_DATA实例。
        /// 支持解析以下三种字段：
        /// - 名称别名："name as castName"
        /// - 显示位置："name at x:y"
        /// - 表情表达式："name [0:expression0,1:expression1]"
        /// 各字段可以组合出现，顺序不限。
        /// </summary>
        /// <param name="rawSpeaker">包含所有发言者设置的原始字符串</param>
        public DL_SPEAKER_DATA(string rawSpeaker)
        {
            // 构造正则表达式匹配三种关键字段标识符
            string pattern =
                @$"{NAMECAST_ID}|{POSITIONCAST_ID}|{EXPRESSIONCAST_ID.Insert(EXPRESSIONCAST_ID.Length - 1, @"\")}";
            MatchCollection matches = Regex.Matches(rawSpeaker, pattern);

            castName = "";
            castPosition = Vector2.zero;
            CastExpressions = new List<(int layer, string expression)>();

            // 如果没有找到任何标识符，则整个字符串作为基础名称处理
            if (matches.Count == 0)
            {
                name = rawSpeaker;
                return;
            }

            int index = matches[0].Index;

            // 提取第一个标识符前的基础名称
            name = rawSpeaker.Substring(0, index);

            // 遍历所有匹配到的关键字并分别处理对应的内容段落
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int startIndex = 0, endIndex = 0;

                if (match.Value == NAMECAST_ID)
                {
                    // 处理别名字段内容
                    startIndex = match.Index + NAMECAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    castName = rawSpeaker.Substring(startIndex, endIndex - startIndex);
                }
                else if (match.Value == POSITIONCAST_ID)
                {
                    // 处理位置字段内容，并分割x/y轴数值
                    startIndex = match.Index + POSITIONCAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    string castPos = rawSpeaker.Substring(startIndex, endIndex - startIndex);

                    string[] axis = castPos.Split(AXISDELIMITER, StringSplitOptions.RemoveEmptyEntries);

                    float.TryParse(axis[0], out castPosition.x);

                    if (axis.Length > 1)
                        float.TryParse(axis[1], out castPosition.y);
                }
                else if (match.Value == EXPRESSIONCAST_ID)
                {
                    // 处理表情表达式字段内容，按逗号拆分多个图层表达式
                    startIndex = match.Index + EXPRESSIONCAST_ID.Length;
                    endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                    string castExp = rawSpeaker.Substring(startIndex, endIndex - (startIndex + 1));

                    CastExpressions = castExp.Split(EXPRESSIONLAYER_JOINER)
                        .Select(x =>
                        {
                            var parts = x.Trim().Split(EXPRESSIONLAYER_DELIMITER);
                            return (int.Parse(parts[0]), parts[1]);
                        }).ToList();
                }
            }
        }
    }

}