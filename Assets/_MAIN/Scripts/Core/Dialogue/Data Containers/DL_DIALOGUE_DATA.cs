using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 表示对话数据的容器类，用于解析和存储对话段落。
/// </summary>
public class DL_DIALOGUE_DATA
{
    /// <summary>
    /// 存储解析后的对话段落列表。
    /// </summary>
    public List<DIALOGUE_SEGMENT> segments;

    /// <summary>
    /// 正则表达式模式，用于识别对话中的控制信号标记（如 {c}, {a}, {wc 1.5} 等）。
    /// </summary>
    private const string segmentIdentifierPattern = @"\{[ca]\}|\{w[ca]\s\d*\.?\d*\}";

    /// <summary>
    /// 获取一个布尔值，表示当前对话是否包含任何段落。
    /// </summary>
    public bool hasDialogue => segments.Count > 0;

    /// <summary>
    /// 初始化一个新的 DL_DIALOGUE_DATA 实例，并解析原始对话字符串。
    /// </summary>
    /// <param name="rawDialogue">需要解析的原始对话字符串。</param>
    public DL_DIALOGUE_DATA(string rawDialogue)
    {
        segments = RipSegments(rawDialogue);
    }

    /// <summary>
    /// 解析原始对话字符串，提取出各个对话段落及其控制信号。
    /// </summary>
    /// <param name="rawDialogue">原始对话字符串。</param>
    /// <returns>解析后的对话段落列表。</returns>
    private List<DIALOGUE_SEGMENT> RipSegments(string rawDialogue)
    {
        List<DIALOGUE_SEGMENT> segments = new List<DIALOGUE_SEGMENT>();
        MatchCollection matches = Regex.Matches(rawDialogue, segmentIdentifierPattern);

        int lastIndex = 0;

        // 创建初始段落（在第一个控制信号之前的内容）
        DIALOGUE_SEGMENT segment = new DIALOGUE_SEGMENT();
        segment.dialogue = (matches.Count == 0 ? rawDialogue : rawDialogue.Substring(0, matches[0].Index));
        segment.startSignal = DIALOGUE_SEGMENT.StartSignal.NONE;
        segment.signalDelay = 0;
        if (!string.IsNullOrEmpty(rawDialogue))
            segments.Add(segment);

        // 如果没有匹配项，直接返回初始段落
        if (matches.Count == 0)
            return segments;
        else
            lastIndex = matches[0].Index;

        // 遍历所有匹配到的控制信号，构建对应的对话段落
        for (int i = 0; i < matches.Count; i++)
        {
            Match match = matches[i];
            segment = new DIALOGUE_SEGMENT();

            // 提取并解析控制信号内容
            string signalMatch = match.Value;
            signalMatch = signalMatch.Substring(1, match.Length - 2);
            string[] signalSplit = signalMatch.Split(' ');

            // 设置起始信号类型
            segment.startSignal =
                (DIALOGUE_SEGMENT.StartSignal)Enum.Parse(typeof(DIALOGUE_SEGMENT.StartSignal), signalSplit[0].ToUpper());

            // 若存在延迟参数，则尝试解析为浮点数
            if (signalSplit.Length > 1)
                float.TryParse(signalSplit[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out segment.signalDelay);

            // 提取当前段落的文本内容
            int nextIndex = i + 1 < matches.Count ? matches[i + 1].Index : rawDialogue.Length;
            segment.dialogue = rawDialogue.Substring(lastIndex + match.Length, nextIndex - (lastIndex + match.Length));
            lastIndex = nextIndex;

            segments.Add(segment);
        }

        return segments;
    }

    /// <summary>
    /// 表示对话的一个段落，包括文本内容、起始信号和延迟时间。
    /// </summary>
    public struct DIALOGUE_SEGMENT
    {
        /// <summary>
        /// 当前段落的文本内容。
        /// </summary>
        public string dialogue;

        /// <summary>
        /// 段落开始时的控制信号类型。
        /// </summary>
        public StartSignal startSignal;

        /// <summary>
        /// 控制信号的延迟时间（秒）。
        /// </summary>
        public float signalDelay;

        /// <summary>
        /// 定义可用的控制信号类型。
        /// </summary>
        public enum StartSignal { NONE, C, A, WA, WC }

        /// <summary>
        /// 获取一个布尔值，表示当前段落是否为追加文本类型（即 A 或 WA）。
        /// </summary>
        public bool appendText => (startSignal == StartSignal.A || startSignal == StartSignal.WA);
    }
}
