using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 输入面板类，用于处理用户输入的UI界面
/// 控制输入面板的显示、隐藏以及用户输入的获取
/// </summary>
public class InputPanel : MonoBehaviour
{
    /// <summary>
    /// 单例实例，提供全局访问点
    /// </summary>
    public static InputPanel instance { get; private set; } = null;
    
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private TMP_InputField inputField;

    private CanvasGroupController cg;

    /// <summary>
    /// 上一次用户输入的内容
    /// </summary>
    public string lastInput { get; private set; } = string.Empty;

    /// <summary>
    /// 标识当前是否正在等待用户输入
    /// </summary>
    public bool isWaitingOnUserInput { get; private set; }

    /// <summary>
    /// 在Awake阶段初始化单例实例
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 初始化输入面板组件引用和事件监听器
    /// 设置初始状态为隐藏
    /// </summary>
    private void Start()
    {
        cg = new CanvasGroupController(this, canvasGroup);
        
        cg.alpha = 0;
        cg.SetInteractableState(active: false);
        acceptButton.gameObject.SetActive(false);
        
        inputField.onValueChanged.AddListener(OnInputChanged);
        acceptButton.onClick.AddListener(OnAcceptInput);
    }

    /// <summary>
    /// 显示输入面板
    /// </summary>
    /// <param name="title">要显示在面板上的标题文本</param>
    public void Show(string title)
    {
        titleText.text = title;
        inputField.text = string.Empty;
        cg.Show();
        cg.SetInteractableState(active: true);
        isWaitingOnUserInput = true;
    }
    
    /// <summary>
    /// 隐藏输入面板
    /// </summary>
    public void Hide()
    {
        cg.Hide();
        cg.SetInteractableState(active: false);
        isWaitingOnUserInput = false;
    }

    /// <summary>
    /// 处理用户确认输入的回调方法
    /// 验证输入内容并保存有效输入后隐藏面板
    /// </summary>
    public void OnAcceptInput()
    {
        // 检查输入是否为空，如果为空则不处理
        if (inputField.text == string.Empty)
            return;

        string input = inputField.text;
        if (CensorManager.Censor(ref input))
        {
            UIConfirmationMenu.instance.Show(
                "You're input was not accepted due to a profanity filter! Please Try Again!",
                new UIConfirmationMenu.ConfirmationButtopn(title: "Okay", () => inputField.text = ""));
        }
        else
        {
            lastInput = inputField.text;
            Hide();
        }
    }

    /// <summary>
    /// 输入框内容变化时的回调方法
    /// 根据输入内容的有效性控制确认按钮的显示状态
    /// </summary>
    /// <param name="value">当前输入框中的文本内容</param>
    public void OnInputChanged(string value)
    {
        acceptButton.gameObject.SetActive(HasValidText());
    }

    /// <summary>
    /// 检查当前输入文本是否有效（非空）
    /// </summary>
    /// <returns>如果输入文本非空返回true，否则返回false</returns>
    private bool HasValidText()
    {
        return inputField.text != string.Empty;
    }
}
