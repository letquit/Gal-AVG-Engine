using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DIALOGUE;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// 配置菜单类，用于管理游戏中的设置界面。
/// 继承自 MenuPage 类，提供分辨率、显示模式、音频等配置功能。
/// </summary>
public class ConfigMenu : MenuPage
{
    /// <summary>
    /// 单例实例访问器，确保全局只有一个 ConfigMenu 实例。
    /// </summary>
    public static ConfigMenu instance { get; private set; }
    
    /// <summary>
    /// 所有可切换的面板对象数组。
    /// </summary>
    [SerializeField] private GameObject[] panels;
    
    /// <summary>
    /// 当前激活的面板对象。
    /// </summary>
    private GameObject activePanel;

    /// <summary>
    /// UI 控件集合引用。
    /// </summary>
    public UI_ITEMS ui;
    
    /// <summary>
    /// 获取当前活动的游戏配置对象。
    /// </summary>
    private AVG_Configuration config => AVG_Configuration.activeConfig;

    /// <summary>
    /// 初始化单例实例。
    /// </summary>
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 启动时初始化默认面板状态并加载配置数据。
    /// </summary>
    private void Start()
    {
        // 默认只启用第一个面板
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0);
        }
        
        activePanel = panels[0];
        
        SetAvailableResolutions(); // 设置可用分辨率选项
        
        LoadConfig(); // 加载本地保存的配置文件
    }

    /// <summary>
    /// 加载游戏配置信息。如果存在本地配置则读取，否则创建一个新的默认配置。
    /// </summary>
    private void LoadConfig()
    {
        if (File.Exists(AVG_Configuration.filePath))
            AVG_Configuration.activeConfig = FileManager.Load<AVG_Configuration>(AVG_Configuration.filePath, encrypt: AVG_Configuration.ENCRYPT);
        else
            AVG_Configuration.activeConfig = new AVG_Configuration();
        
        AVG_Configuration.activeConfig.Load();
    }

    /// <summary>
    /// 应用退出时保存当前配置到本地，并清空活动配置引用。
    /// </summary>
    public void OnApplicationQuit()
    {
        AVG_Configuration.activeConfig.Save();
        AVG_Configuration.activeConfig = null;
    }

    /// <summary>
    /// 根据名称打开指定面板，并关闭其他面板。
    /// </summary>
    /// <param name="panelName">要打开的面板名称（忽略大小写）</param>
    public void OpenPanel(string panelName)
    {
        GameObject panel = panels.First(p => p.name.ToLower() == panelName.ToLower());

        if (panel == null)
        {
            Debug.LogWarning($"Did not find panel called '{panelName}' in config menu.");
            return;
        }

        if (activePanel != null && activePanel != panel)
            activePanel.SetActive(false);
        
        panel.SetActive(true);
        activePanel = panel;
    }

    /// <summary>
    /// 获取系统支持的所有屏幕分辨率，并填充下拉框选项。
    /// </summary>
    private void SetAvailableResolutions()
    {
        Resolution[] resolutions = Screen.resolutions;
        List<string> options = new List<string>();

        for (int i = resolutions.Length - 1; i >= 0; i--)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");
        }
        
        ui.resolutions.ClearOptions();
        ui.resolutions.AddOptions(options);
    }

    /// <summary>
    /// 存储所有与配置相关的 UI 元素组件引用。
    /// </summary>
    [Serializable]
    public class UI_ITEMS
    {
        private static Color button_selectedColor = new Color(1, 0.35f, 0, 1);
        private static Color button_unselectedColor = new Color(1, 1, 1, 1);
        private static Color text_selectedColor = new Color(1, 1, 0, 1);
        private static Color text_unselectedColor = new Color(0.25f, 0.25f, 0.25f, 1);
        public static Color musicOnColor = new Color(1, 0.65f, 0, 1);
        public static Color musicOffColor = new Color(0.5f, 0.5f, 0.5f, 1);
        
        [Header("General")]
        public Button fullscreen;
        public Button windowed;
        public TMP_Dropdown resolutions;
        public Button skippingContinue, skippingStop;
        public Slider architectSpeed, autoReaderSpeed;

        [Header("Audio")] 
        public Slider musicVolume;
        public Image musicFill;
        public Slider sfxVolume;
        public Image sfxFill;
        public Slider voiceVolume;
        public Image voiceFill;
        public Sprite mutedSymbol;
        public Sprite unmutedSymbol;
        public Image musicMute;
        public Image sfxMute;
        public Image voiceMute;

        /// <summary>
        /// 设置两个按钮的颜色样式以表示选中/未选中状态。
        /// </summary>
        /// <param name="A">第一个按钮</param>
        /// <param name="B">第二个按钮</param>
        /// <param name="selectedA">是否将第一个按钮设为选中状态</param>
        public void SetButtonColors(Button A, Button B, bool selectedA)
        {
            A.GetComponent<Image>().color = selectedA ? button_selectedColor : button_unselectedColor;
            B.GetComponent<Image>().color = !selectedA ? button_selectedColor : button_unselectedColor;
            
            A.GetComponentInChildren<TextMeshProUGUI>().color = selectedA ? text_selectedColor : text_unselectedColor;
            B.GetComponentInChildren<TextMeshProUGUI>().color = !selectedA ? text_selectedColor : text_unselectedColor;
        }
    }

    /// <summary>
    /// 切换全屏或窗口化显示模式，并更新对应按钮颜色。
    /// </summary>
    /// <param name="fullscreen">true 表示全屏；false 表示窗口化</param>
    public void SetDisplayToFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        ui.SetButtonColors(ui.fullscreen, ui.windowed, fullscreen);
    }

    /// <summary>
    /// 设置当前选择的屏幕分辨率，并应用更改。
    /// </summary>
    public void SetDisplayResolution()
    {
        string resolution = ui.resolutions.captionText.text;
        string[] values = resolution.Split(" x ");

        if (int.TryParse(values[0], out int width) && int.TryParse(values[1], out int height))
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
            config.display_resolution = resolution;
        }
        else
            Debug.LogError($"Parsing error for screen resolution! [{resolution}] could not be parsed into WIDTH x HEIGHT");
    }
    
    /// <summary>
    /// 设置在做出选择后是否继续跳过对话。
    /// </summary>
    /// <param name="continueSkipping">true 表示继续跳过；false 表示停止跳过</param>
    public void SetContinueSkippingAfterChoice(bool continueSkipping)
    {
        config.continueSkippingAfterChoice = continueSkipping;
        ui.SetButtonColors(ui.skippingContinue, ui.skippingStop, continueSkipping);
    }

    /// <summary>
    /// 更新文本构建速度，并同步至对话系统的文本架构师模块。
    /// </summary>
    public void SetTextArchitectSpeed()
    {
        // 更新配置中的文本构建速度
        config.dialogueTextSpeed = ui.architectSpeed.value;
        
        // 同步速度设置到对话系统的文本架构师模块
        if (DialogueSystem.instance != null)
            DialogueSystem.instance.conversationManager.architect.speed = config.dialogueTextSpeed;
    }
    
    /// <summary>
    /// 设置自动阅读的速度，并同步至自动阅读器模块。
    /// </summary>
    public void SetAutoReaderSpeed()
    {
        // 更新配置中的自动阅读速度
        config.dialogueAutoReadSpeed = ui.autoReaderSpeed.value;

        // 检查对话系统实例是否存在
        if (DialogueSystem.instance == null)
            return;
        
        // 同步速度设置到自动阅读器模块
        AutoReader autoReader = DialogueSystem.instance.autoReader;
        if (autoReader != null)
            autoReader.speed = config.dialogueAutoReadSpeed;
    }
    
    /// <summary>
    /// 设置音乐音量。将UI中的音乐音量值应用到配置和音频管理器中，并更新UI填充颜色。
    /// </summary>
    public void SetMusicVolume()
    {
        config.musicVolume = ui.musicVolume.value;
        AudioManager.instance.SetMusicVolume(config.musicVolume, config.musicMute);
        
        ui.musicFill.color = config.musicMute ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }
    
    /// <summary>
    /// 设置音效音量。将UI中的音效音量值应用到配置和音频管理器中，并更新UI填充颜色。
    /// </summary>
    public void SetSFXVolume()
    {
        config.sfxVolume = ui.sfxVolume.value;
        AudioManager.instance.SetSFXVolume(config.sfxVolume, config.sfxMute);
        
        ui.sfxFill.color = config.sfxMute ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }
    
    /// <summary>
    /// 设置语音音量。将UI中的语音音量值应用到配置和音频管理器中，并更新UI填充颜色。
    /// </summary>
    public void SetVoiceVolume()
    {
        config.voiceVolume = ui.voiceVolume.value;
        AudioManager.instance.SetVoicesVolume(config.voiceVolume, config.voiceMute);
        
        ui.voiceFill.color = config.voiceMute ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }

    /// <summary>
    /// 切换音乐静音状态。更新配置、UI元素（滑动条背景色和静音按钮图标），并通知音频管理器。
    /// </summary>
    public void SetMusicMute()
    {
        config.musicMute = !config.musicMute;
        // 更新音乐滑动条的背景颜色
        ui.musicVolume.fillRect.GetComponent<Image>().color = config.musicMute ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        // 更改静音按钮图标
        ui.musicMute.sprite = config.musicMute ? ui.mutedSymbol : ui.unmutedSymbol;
        
        AudioManager.instance.SetMusicVolume(config.musicVolume, config.musicMute);
    }
    
    /// <summary>
    /// 切换音效静音状态。更新配置、UI元素（滑动条背景色和静音按钮图标），并通知音频管理器。
    /// </summary>
    public void SetSFXMute()
    {
        config.sfxMute = !config.sfxMute;
        // 更新音效滑动条的背景颜色
        ui.sfxVolume.fillRect.GetComponent<Image>().color = config.sfxMute ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        // 更改静音按钮图标
        ui.sfxMute.sprite = config.sfxMute ? ui.mutedSymbol : ui.unmutedSymbol;
        
        AudioManager.instance.SetSFXVolume(config.sfxVolume, config.sfxMute);
    }
    
    /// <summary>
    /// 切换语音静音状态。更新配置、UI元素（滑动条背景色和静音按钮图标），并通知音频管理器。
    /// </summary>
    public void SetVoiceMute()
    {
        config.voiceMute = !config.voiceMute;
        // 更新语音滑动条的背景颜色
        ui.voiceVolume.fillRect.GetComponent<Image>().color = config.voiceMute ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        // 更改静音按钮图标
        ui.voiceMute.sprite = config.voiceMute ? ui.mutedSymbol : ui.unmutedSymbol;
        
        AudioManager.instance.SetVoicesVolume(config.voiceVolume, config.voiceMute);
    }
}