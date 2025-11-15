using System;
using System.Collections;
using ADVENTUREGAME;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单控制类，负责处理游戏主菜单的各种功能，包括开始新游戏、加载游戏等操作
/// </summary>
public class MainMenu : MonoBehaviour
{
    public const string MAIN_MENU_SCENE = "Main Menu";
    
    public static MainMenu instance { get; private set; }
    
    public AudioClip menuMusic;
    public CanvasGroup mainPanel;
    private CanvasGroupController mainCG;

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
        AudioManager.instance.PlayTrack(menuMusic, channel: 0, startingVolume: 0.5f);
    }

    /// <summary>
    /// 开始新游戏，创建新的游戏存档并启动游戏
    /// </summary>
    public void StartNewGame()
    {
        AVGGameSave.activeFile = new AVGGameSave();
        StartCoroutine(StartingGame());
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
    /// 启动游戏的协程，隐藏主菜单面板，停止菜单音乐，然后加载游戏场景
    /// </summary>
    /// <returns>IEnumerator用于协程执行</returns>
    private IEnumerator StartingGame()
    {
        // 隐藏主菜单面板并停止菜单音乐
        mainCG.Hide(speed: 0.1f);
        AudioManager.instance.StopTrack(0);

        // 等待面板完全隐藏
        while (mainCG.isVisible)
            yield return null;
        
        // 加载游戏主场景
        SceneManager.LoadScene("GalAVG");
    }
}
