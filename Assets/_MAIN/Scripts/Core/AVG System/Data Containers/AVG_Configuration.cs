using System;
using UnityEngine;

/// <summary>
/// AVG游戏配置类，用于管理游戏的各种设置选项
/// </summary>
[Serializable]
public class AVG_Configuration
{
    /// <summary>
    /// 当前激活的配置实例
    /// </summary>
    public static AVG_Configuration activeConfig;

    /// <summary>
    /// 配置文件的完整路径
    /// </summary>
    public static string filePath => $"{FilePaths.root}avgconfig.cfg";
    
    /// <summary>
    /// 是否启用配置文件加密存储
    /// </summary>
    public const bool ENCRYPT = false;
    
    /// <summary>
    /// 显示模式：是否全屏显示
    /// </summary>
    public bool display_fullscreen = true;
    
    /// <summary>
    /// 显示分辨率设置
    /// </summary>
    public string display_resolution = "2560 x 1440";
    
    /// <summary>
    /// 选择后是否继续跳过对话
    /// </summary>
    public bool continueSkippingAfterChoice = false;
    
    /// <summary>
    /// 对话文本显示速度
    /// </summary>
    public float dialogueTextSpeed = 1f;
    
    /// <summary>
    /// 对话自动阅读速度
    /// </summary>
    public float dialogueAutoReadSpeed = 1f;
    
    /// <summary>
    /// 背景音乐音量
    /// </summary>
    public float musicVolume = 1f;
    
    /// <summary>
    /// 音效音量
    /// </summary>
    public float sfxVolume = 1f;
    
    /// <summary>
    /// 语音音量
    /// </summary>
    public float voiceVolume = 1f;
    
    /// <summary>
    /// 背景音乐是否静音
    /// </summary>
    public bool musicMute = false;
    
    /// <summary>
    /// 音效是否静音
    /// </summary>
    public bool sfxMute = false;
    
    /// <summary>
    /// 语音是否静音
    /// </summary>
    public bool voiceMute = false;
    
    /// <summary>
    /// 历史记录界面缩放比例
    /// </summary>
    public float historyLogScale = 1f;

    /// <summary>
    /// 加载配置并应用到UI界面
    /// </summary>
    public void Load()
    {
        var ui = ConfigMenu.instance.ui;

        // 应用全屏显示设置
        ConfigMenu.instance.SetDisplayToFullScreen(display_fullscreen);
        ui.SetButtonColors(ui.fullscreen, ui.windowed, display_fullscreen);

        // 查找并设置分辨率选项
        int res_index = 0;
        for (int i = 0; i < ui.resolutions.options.Count; i++)
        {
            string resolution = ui.resolutions.options[i].text;
            if (resolution == display_resolution)
            {
                res_index = i;
                break;
            }
        }
        ui.resolutions.value = res_index;
        
        // 设置跳过选项的按钮状态
        ui.SetButtonColors(ui.skippingContinue, ui.skippingStop, continueSkippingAfterChoice);
        
        // 设置对话速度滑动条的值
        ui.architectSpeed.value = dialogueTextSpeed;
        ui.autoReaderSpeed.value = dialogueAutoReadSpeed;
    }
    
    /// <summary>
    /// 保存当前配置到文件
    /// </summary>
    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this), encrypt: ENCRYPT);
    }
}