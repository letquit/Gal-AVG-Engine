using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace History
{
    /// <summary>
    /// 管理历史记录日志的显示与交互逻辑。
    /// 负责打开/关闭日志界面、添加日志条目、调整日志缩放比例以及清理日志内容。
    /// </summary>
    public class HistoryLogManager : MonoBehaviour
    {
        private const float LOG_STARTING_HEIGHT = 2f;
        private const float LOG_HEIGHT_PER_LINE = 2f;
        private const float LOG_DEFAULT_HEIGHT = 1f;
        private const float TEXT_DEFAULT_SCALE = 1f;

        private const string NAME_TEXT_NAME = "NameText";
        private const string DIALOGUE_TEXT_NAME = "DialogueText";
        
        private float logScaling = 1f;

        [SerializeField] private Animator anim;
        [SerializeField] private GameObject logPrefab;
        
        HistoryManager manager => HistoryManager.instance;
        private List<HistoryLog> logs = new List<HistoryLog>();
        
        public bool isOpen { get; private set; } = false;

        [SerializeField] private Slider logScaleSlider;

        private float textScaling => logScaling * 3f;
        
        /// <summary>
        /// 打开历史记录界面。
        /// 若当前已打开，则不执行任何操作。
        /// </summary>
        public void Open()
        {
            if (isOpen)
                return;
            
            anim.Play("LogOpen");
            
            isOpen = true;
        }
        
        /// <summary>
        /// 关闭历史记录界面。
        /// 若当前已关闭，则不执行任何操作。
        /// </summary>
        public void Close()
        { 
            if (!isOpen)
                return;
            
            anim.Play("LogClose");
            
            isOpen = false;
        }

        /// <summary>
        /// 添加一条新的历史记录到日志中。
        /// 当日志数量超过缓存上限时，移除最早的一条记录以维持限制。
        /// </summary>
        /// <param name="state">要添加的历史状态信息</param>
        public void AddLog(HistoryState state)
        {
            // 如果日志数量达到上限，删除最旧的日志
            if (logs.Count >= HistoryManager.HISTORY_CACHE_LIMIT)
            {
                DestroyImmediate(logs[0].container);
                logs.RemoveAt(0);
            }
            
            CreateLog(state);
        }

        /// <summary>
        /// 创建一个新的日志条目并设置其内容和样式。
        /// </summary>
        /// <param name="state">用于填充日志内容的历史状态</param>
        private void CreateLog(HistoryState state)
        {
            HistoryLog log = new HistoryLog();

            log.container = Instantiate(logPrefab, logPrefab.transform.parent);
            log.container.SetActive(true);
            
            log.nameText = log.container.transform.Find(NAME_TEXT_NAME).GetComponent<TextMeshProUGUI>();
            log.dialogueText = log.container.transform.Find(DIALOGUE_TEXT_NAME).GetComponent<TextMeshProUGUI>();

            // 设置说话者名称文本
            if (state.dialogue.currentSpeaker == string.Empty)
            {
                log.nameText.text = string.Empty;
            }
            else
            {
                log.nameText.text = state.dialogue.currentSpeaker;
                log.nameText.font = HistoryCache.LoadFont(state.dialogue.speakerFont);
                log.nameText.color = state.dialogue.speakerNameColor;
                log.nameFontSize = TEXT_DEFAULT_SCALE * state.dialogue.speakerScale;
                log.nameText.fontSize = log.nameFontSize + textScaling;
            }
            
            // 设置对话文本
            log.dialogueText.text = state.dialogue.currentDialogue;
            log.dialogueText.font = HistoryCache.LoadFont(state.dialogue.dialogueFont);
            log.dialogueText.color = state.dialogue.dialogueColor;
            log.dialogueFontSize = TEXT_DEFAULT_SCALE * state.dialogue.dialogueScale;
            log.dialogueText.fontSize = log.dialogueFontSize + textScaling;
            
            FitLogToText(log);
            
            logs.Add(log);
        }

        /// <summary>
        /// 根据对话文本的高度调整日志容器的高度。
        /// </summary>
        /// <param name="log">需要调整高度的日志对象</param>
        private void FitLogToText(HistoryLog log)
        {
             RectTransform rect = log.dialogueText.GetComponent<RectTransform>();
             ContentSizeFitter textCSF = log.dialogueText.GetComponent<ContentSizeFitter>();
             
             textCSF.SetLayoutVertical();
             
             LayoutElement logLayout = log.container.GetComponent<LayoutElement>();
             float height = rect.rect.height;
             
             float perc = height / LOG_DEFAULT_HEIGHT;
             float extraScale = (LOG_HEIGHT_PER_LINE * perc) - LOG_HEIGHT_PER_LINE;
             float scale = LOG_STARTING_HEIGHT + extraScale;
             
             logLayout.preferredHeight = scale + textScaling;

             logLayout.preferredHeight += 2f * logScaling;
        }

        /// <summary>
        /// 根据滑动条的值更新所有日志的字体大小和布局。
        /// </summary>
        public void SetLogScaling()
        {
            logScaling = logScaleSlider.value;

            foreach (HistoryLog log in logs)
            {
                log.nameText.fontSize = log.nameFontSize + textScaling;
                log.dialogueText.fontSize = log.dialogueFontSize + textScaling;
                
                FitLogToText(log);
            }
        }

        /// <summary>
        /// 清除所有历史日志条目。
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < logs.Count; i++)
                DestroyImmediate(logs[i].container);
            
            logs.Clear();
        }
        
        /// <summary>
        /// 重建日志记录
        /// </summary>
        /// <remarks>
        /// 遍历管理器中的历史记录状态，为每个状态创建对应的日志条目
        /// </remarks>
        public void Rebuild()
        {
            // 遍历所有历史状态并创建相应的日志记录
            foreach (var state in manager.history)
                CreateLog(state);
        }

    }
}