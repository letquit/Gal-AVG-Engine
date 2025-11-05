using TMPro;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 历史记录类，用于存储和管理对话历史信息
    /// </summary>
    public class HistoryLog
    {
        public GameObject container;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        public float nameFontSize = 0;
        public float dialogueFontSize = 0;
    }
}
