using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIConfirmationMenu 类用于显示一个确认菜单界面，允许用户从多个选项中选择操作。
/// 该组件通过 Animator 控制进入和退出动画，并支持动态生成按钮选项。
/// </summary>
public class UIConfirmationMenu : MonoBehaviour
{
    /// <summary>
    /// 获取当前实例（单例模式）。
    /// </summary>
    public static UIConfirmationMenu instance { get; private set; }
    
    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private LayoutGroup choiceLayoutGroup;
    [SerializeField] private GameObject buttonPrefab;
    
    private List<GameObject> _activeOptions = new List<GameObject>();
    private Coroutine _creationCoroutine = null;

    /// <summary>
    /// 初始化组件，在场景加载时设置单例引用并禁用自身 GameObject。
    /// </summary>
    private void Awake()
    {
        instance = this;
        // 初始时禁用，由 Show 方法激活
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示确认菜单。
    /// </summary>
    /// <param name="title">要显示的标题文本。</param>
    /// <param name="options">可变数量的确认按钮配置项。</param>
    public void Show(string title, params ConfirmationButtopn[] options)
    {
        if (options.Length == 0)
        {
            Debug.LogError("Confirmation menu must have at least 1 option provided for the user to select.", this);
            return;
        }

        // 激活父级对象，以便协程和动画可以运行
        gameObject.SetActive(true);
        
        this.title.text = title;
        
        if (_creationCoroutine != null)
        {
            StopCoroutine(_creationCoroutine);
        }
        
        _creationCoroutine = StartCoroutine(CreateOptionButtonsCoroutine(options));
        
        anim.Play("Enter");
    }
    
    /// <summary>
    /// 触发隐藏菜单的退出动画。
    /// </summary>
    public void Hide()
    {
        anim.Play("Exit");
    }

    /// <summary>
    /// 此方法由 "Exit" 动画的 Animation Event 在最后一帧调用。
    /// </summary>
    public void OnHideAnimationComplete()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 异步创建选项按钮的协程函数。
    /// 清除旧有按钮后根据传入的选项数组生成新的按钮，并绑定点击事件。
    /// 同时确保按钮的位置正确以避免渲染问题。
    /// </summary>
    /// <param name="options">需要创建的按钮选项数组。</param>
    /// <returns>IEnumerator 对象，用于协程执行。</returns>
    private IEnumerator CreateOptionButtonsCoroutine(ConfirmationButtopn[] options)
    {
        foreach (GameObject g in _activeOptions)
        {
            Destroy(g);
        }
        _activeOptions.Clear();

        // 首先确保父级 Panel 的 Z 坐标是安全的
        EnsurePanelIsInVisibleRange();

        for (int i = 0; i < options.Length; i++)
        {
            ConfirmationButtopn option = options[i];
            GameObject ob = Instantiate(buttonPrefab, choiceLayoutGroup.transform);
            ob.SetActive(true);
            
            Button button = ob.GetComponent<Button>();
            
            if (option.action != null)
                button.onClick.AddListener(() => option.action.Invoke());
            
            if (option.autoCloseOnQuit)
                button.onClick.AddListener(() => Hide());
            
            TextMeshProUGUI txt = ob.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = option.title;

            _activeOptions.Add(ob);
        }

        // 等待一帧，让 LayoutGroup 完成布局
        yield return null; 
        
        // 布局完成后，强制重置所有新创建按钮的 Z 坐标
        foreach (GameObject ob in _activeOptions)
        {
            if (ob == null) continue;

            RectTransform rt = ob.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector3 pos = rt.localPosition;
                if (pos.z != 0)
                {
                    pos.z = 0;
                    rt.localPosition = pos;
                }
            }
        }
        _creationCoroutine = null;
    }
    
    /// <summary>
    /// 确保父级 Panel 本身在相机的可见范围内。
    /// 特别针对 ScreenSpaceCamera 模式的 Canvas 进行位置调整，
    /// 防止由于深度排序导致的不可见问题。
    /// </summary>
    private void EnsurePanelIsInVisibleRange()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            return; // 只在 ScreenSpaceCamera 模式下需要处理
        }

        Camera renderCamera = canvas.worldCamera;
        if (renderCamera == null) 
        {
            // 如果没有指定渲染相机，通常默认为主相机
            renderCamera = Camera.main;
            if (renderCamera == null) return;
        }

        Vector3 localPos = transform.localPosition;
        float planeDistance = canvas.planeDistance;

        // 我们不使用硬编码的值，而是基于 Canvas 的 planeDistance
        // 通常，UI 应该直接位于 planeDistance 上。我们取一个稍微靠近相机的值以确保它在前面。
        float safeZ = planeDistance - 1f; // 或 -0.1f，一个小的偏移量

        // Unity 的 UI Z 轴是反的，负数更靠近相机。我们需要一个负值。
        // Canvas 的 planeDistance 是正值，表示在相机前方多远。
        // RectTransform 的 z localPosition 是相对于父级的。
        // 在 ScreenSpaceCamera 中，Canvas 下的顶层UI，其z世界坐标应接近 -planeDistance。
        // 这里我们直接修正 localPosition.z, 假设其父级 Z 为 0.
        float targetZ = -(planeDistance - 1f);

        if (Mathf.Abs(localPos.z - targetZ) > 0.01f) // 使用浮点数比较
        {
            localPos.z = targetZ;
            transform.localPosition = localPos;
            // Debug.Log($"Panel position Z adjusted to '{targetZ}' for visibility in ScreenSpaceCamera.", this);
        }
    }
    
    /// <summary>
    /// 表示一个确认菜单中的按钮选项结构体。
    /// 包含标题、触发的动作以及是否自动关闭菜单的标志。
    /// </summary>
    public struct ConfirmationButtopn
    {
        /// <summary>
        /// 按钮上显示的文字内容。
        /// </summary>
        public string title;
        
        /// <summary>
        /// 当按钮被点击时所执行的操作委托。
        /// </summary>
        public Action action;
        
        /// <summary>
        /// 标识点击此按钮后是否会自动关闭菜单，默认为 true。
        /// </summary>
        public bool autoCloseOnQuit;
        
        /// <summary>
        /// 构造一个新的 ConfirmationButtopn 实例。
        /// </summary>
        /// <param name="title">按钮上的文字内容。</param>
        /// <param name="action">按钮点击后的回调动作。</param>
        /// <param name="autoCloseOnQuit">是否点击后自动关闭菜单，默认为 true。</param>
        public ConfirmationButtopn(string title, Action action, bool autoCloseOnQuit = true)
        {
            this.title = title;
            this.action = action;
            this.autoCloseOnQuit = autoCloseOnQuit;
        }
    }
}