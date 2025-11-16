using System;
using System.Collections;
using ADVENTUREGAME;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 主菜单控制类，负责处理游戏主菜单的各种功能，包括开始新游戏、加载游戏等操作
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// 主菜单场景名称常量
    /// </summary>
    public const string MAIN_MENU_SCENE = "Main Menu";
    
    /// <summary>
    /// 获取当前类的单例实例
    /// </summary>
    public static MainMenu instance { get; private set; }
    
    /// <summary>
    /// 菜单背景音乐音频剪辑
    /// </summary>
    public AudioClip menuMusic;

    /// <summary>
    /// 主面板的CanvasGroup组件引用
    /// </summary>
    public CanvasGroup mainPanel;

    /// <summary>
    /// 控制主面板显示与隐藏的CanvasGroupController对象
    /// </summary>
    private CanvasGroupController mainCG;
    
    /// <summary>
    /// 黑色过渡图像组件引用
    /// </summary>
    public Image blackImage;

    /// <summary>
    /// 黑色图像的CanvasGroup组件，用于控制透明度变化
    /// </summary>
    private CanvasGroup blackImageCG;
    
    /// <summary>
    /// 确认对话框UI菜单的快捷访问属性
    /// </summary>
    private UIConfirmationMenu uiChoiceMenu => UIConfirmationMenu.instance;

    /// <summary>
    /// 在对象唤醒时设置单例实例
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 初始化主菜单，设置画布组控制器并播放菜单音乐
    /// </summary>
    private void Start()
    {
        mainCG = new CanvasGroupController(this, mainPanel);
        if (blackImage != null)
        {
            blackImageCG = blackImage.GetComponent<CanvasGroup>();
            if (blackImageCG != null)
                blackImageCG.alpha = 0;
        }
        AudioManager.instance.PlayTrack(menuMusic, channel: 0, startingVolume: 0.5f);
    }

    /// <summary>
    /// 开始新游戏，弹出确认对话框让用户选择是否开启新游戏
    /// </summary>
    public void Click_StartNewGame()
    {
        uiChoiceMenu.Show("Start a new game?", new UIConfirmationMenu.ConfirmationButtopn("Yes", StartNewGame),
            new UIConfirmationMenu.ConfirmationButtopn("No", null));
    }

    /// <summary>
    /// 加载已有的游戏存档并启动游戏
    /// </summary>
    /// <param name="file">要加载的游戏存档文件</param>
    public void LoadGame(AVGGameSave file)
    {
        AVGGameSave.activeFile = file;
        StartCoroutine(StartingGame());
    }

    /// <summary>
    /// 创建一个新的游戏存档，并启动游戏流程
    /// </summary>
    private void StartNewGame()
    {
        AVGGameSave.activeFile = new AVGGameSave();
        StartCoroutine(StartingGame());
    }
    
    /// <summary>
    /// 启动游戏的协程，隐藏主菜单面板，停止菜单音乐，然后加载游戏场景
    /// </summary>
    /// <returns>IEnumerator用于协程执行</returns>
    private IEnumerator StartingGame()
    {
        // 隐藏主菜单面板并停止菜单音乐
        mainCG.Hide(speed: 0.3f);
        AudioManager.instance.StopTrack(0);

        // 等待面板完全隐藏
        while (mainCG.isVisible)
            yield return null;
    
        // 执行黑色遮罩淡入效果
        if (blackImageCG != null)
        {
            float fadeDuration = 1.0f;
            float elapsedTime = 0f;
        
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                blackImageCG.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
        
            blackImageCG.alpha = 1f;
        }
    
        // 加载游戏主场景
        SceneManager.LoadScene("GalAVG");
    }
}