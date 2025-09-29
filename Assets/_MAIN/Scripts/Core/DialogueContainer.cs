using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 对话框容器类，用于存储和管理对话系统中的UI组件引用
/// 该类包含对话框的根对象以及用于显示角色名称和对话内容的文本组件
/// </summary>
[Serializable]
public class DialogueContainer
{
    /// <summary>
    /// 对话框的根游戏对象，用于控制整个对话框的显示和隐藏
    /// </summary>
    public GameObject root;
    
    /// <summary>
    /// 显示角色名称的TextMeshProUGUI文本组件
    /// </summary>
    public TextMeshProUGUI nameText;
    
    /// <summary>
    /// 显示对话内容的TextMeshProUGUI文本组件
    /// </summary>
    public TextMeshProUGUI dialogueText;
}

