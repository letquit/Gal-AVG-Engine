using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ChoicePanel 类用于显示一个选择面板，允许用户从多个选项中做出选择。
/// </summary>
public class ChoicePanel : MonoBehaviour
{
    /// <summary>
    /// 获取 ChoicePanel 的单例实例。
    /// </summary>
    public static ChoicePanel instance { get; private set; }

    // 按钮最小宽度常量
    private const float BUTTON_MIN_WIDTH = 50f;
    // 按钮最大宽度常量
    private const float BUTTON_MAX_WIDTH = 1000f;
    // 按钮宽度填充常量
    private const float BUTTON_WIDTH_PADDING = 25f;

    // 每行按钮高度常量
    private const float BUTTON_HEIGHT_PER_LINE = 75f;
    // 按钮高度填充常量
    private const float BUTTON_HEIGHT_PADDING = 25f;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private VerticalLayoutGroup buttonLayoutGroup;

    private CanvasGroupController cg = null;
    private List<ChoiceButton> buttons = new List<ChoiceButton>();
    
    /// <summary>
    /// 上一次用户的选择结果。
    /// </summary>
    public ChoicePanelDecision lastDecision { get; private set; } = null;
    
    /// <summary>
    /// 表示当前是否正在等待用户的输入选择。
    /// </summary>
    public bool isWaitingOnUserChoice { get; private set; } = false;
    
    /// <summary>
    /// 在对象被唤醒时调用。初始化单例实例并创建 CanvasGroupController。
    /// </summary>
    private void Awake()
    { 
        instance = this;
        cg = new CanvasGroupController(this, canvasGroup);
        cg.alpha = 0;
        cg.SetInteractableState(active: false);
    }

    /// <summary>
    /// 显示选择面板，并根据提供的问题和选项生成按钮。
    /// </summary>
    /// <param name="question">要展示给用户的问题文本。</param>
    /// <param name="choices">可供用户选择的答案数组。</param>
    public void Show(string question, string[] choices)
    {
        lastDecision = new ChoicePanelDecision(question, choices);
        
        isWaitingOnUserChoice = true;

        cg.Show();
        cg.SetInteractableState(active: true);
        
        titleText.text = question;
        StartCoroutine(GenerateChoices(choices));
    }

    /// <summary>
    /// 异步协程方法，负责动态生成选择按钮并调整其尺寸以适应内容。
    /// </summary>
    /// <param name="choices">需要生成对应按钮的字符串数组。</param>
    /// <returns>IEnumerator 接口，支持协程执行。</returns>
    private IEnumerator GenerateChoices(string[] choices)
    {
        float maxWidth = 0;

        // 遍历所有选项，创建或复用按钮组件
        for (int i = 0; i < choices.Length; i++)
        {
            ChoiceButton choiceButton;
            if (i < buttons.Count)
            {
                choiceButton = buttons[i];
            }
            else
            {
                GameObject newButtonObject = Instantiate(choiceButtonPrefab, buttonLayoutGroup.transform);
                newButtonObject.SetActive(true);
                
                Button newButton = newButtonObject.GetComponent<Button>();
                TextMeshProUGUI newTitle = newButton.GetComponentInChildren<TextMeshProUGUI>();
                LayoutElement newLayout = newButton.GetComponent<LayoutElement>();

                choiceButton = new ChoiceButton { button = newButton, title = newTitle, layout = newLayout };
                
                buttons.Add(choiceButton);
            }
            
            choiceButton.button.onClick.RemoveAllListeners();
            int buttonIndex = i;
            choiceButton.button.onClick.AddListener(() => AcceptAnswer(buttonIndex));
            choiceButton.title.text = choices[i];

            float buttonWidth = Mathf.Clamp(BUTTON_WIDTH_PADDING + choiceButton.title.preferredWidth, BUTTON_MIN_WIDTH, BUTTON_MAX_WIDTH);
            maxWidth = Mathf.Max(maxWidth, buttonWidth);
        }

        // 统一设置所有按钮的最大宽度
        foreach (var button in buttons)
        {
            button.layout.preferredWidth = maxWidth;
        }

        // 根据传入选项数量决定哪些按钮可见
        for (int i = 0; i < buttons.Count; i++)
        {
            bool show = i < choices.Length;
            buttons[i].button.gameObject.SetActive(show);
        }

        yield return new WaitForEndOfFrame();

        // 设置每个按钮的高度基于文字换行数
        foreach (var button in buttons)
        {
            int lines = button.title.textInfo.lineCount;
            Debug.Log(lines);
            button.layout.preferredHeight = BUTTON_HEIGHT_PADDING + (BUTTON_HEIGHT_PER_LINE * lines);
        }
    }
    
    /// <summary>
    /// 隐藏当前的选择面板。
    /// </summary>
    public void Hide()
    {
        cg.Hide();
    }

    /// <summary>
    /// 处理用户点击某个按钮后的响应逻辑。
    /// </summary>
    /// <param name="index">被选中的按钮索引。</param>
    private void AcceptAnswer(int index)
    {
        if (index < 0 || index >= lastDecision.choices.Length)
            return;
        
        lastDecision.answerIndex = index;
        isWaitingOnUserChoice = false;
        Hide();
    }

    /// <summary>
    /// ChoicePanelDecision 类表示一次完整的用户决策过程，包括问题、答案及用户最终选择。
    /// </summary>
    public class ChoicePanelDecision
    {
        /// <summary>
        /// 用户提出的问题文本。
        /// </summary>
        public string question = string.Empty;
        
        /// <summary>
        /// 用户所作选择的索引（-1 表示尚未选择）。
        /// </summary>
        public int answerIndex = -1;
        
        /// <summary>
        /// 可供选择的所有选项列表。
        /// </summary>
        public string[] choices = new string[0];

        /// <summary>
        /// 构造函数：初始化一个新的 ChoicePanelDecision 实例。
        /// </summary>
        /// <param name="question">问题文本。</param>
        /// <param name="choices">可选答案列表。</param>
        public ChoicePanelDecision(string question, string[] choices)
        {
            this.question = question;
            this.choices = choices;
            answerIndex = -1;
        }
        
    }

    /// <summary>
    /// ChoiceButton 结构体封装了与单个按钮相关的 UI 元素引用。
    /// </summary>
    private struct ChoiceButton
    {
        /// <summary>
        /// Unity 原生按钮组件。
        /// </summary>
        public Button button;
        
        /// <summary>
        /// 显示按钮标题的文字组件。
        /// </summary>
        public TextMeshProUGUI title;
        
        /// <summary>
        /// 控制按钮布局大小的元素。
        /// </summary>
        public LayoutElement layout;
    }
}