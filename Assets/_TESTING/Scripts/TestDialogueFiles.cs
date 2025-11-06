using System;
using System.Collections.Generic;
using System.IO;
using DIALOGUE;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset fileToRead = null;
    private void Start()
    {
        if (fileToRead == null)
        {
            Debug.LogError("对话文件(fileToRead)未在 Inspector 中设置！");
            return;
        }
        
        StartConversation();
    }

    private void StartConversation()
    {
        string filePath;
        
#if UNITY_EDITOR
        string fullPath = AssetDatabase.GetAssetPath(fileToRead);
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            // 使用 .Length 提高可读性
            string relativePath = fullPath.Substring(resourcesIndex + "Resources/".Length);
            filePath = Path.ChangeExtension(relativePath, null);
        }
        else
        {
            // 如果不在 Resources 文件夹，发出警告
            Debug.LogWarning($"文件 '{fileToRead.name}' 不在 Resources 文件夹内，运行时可能无法加载。将回退到使用文件名。");
            filePath = fileToRead.name;
        }
#else
        // 在打包版本中，我们依赖于文件在 Resources 文件夹内，以便 Resources.Load 可以找到它
        // 注意：如果文件在子文件夹（如 Resources/Dialogues/），这里的 fileToRead.name 是不够的
        filePath = fileToRead.name;
#endif
        
        Debug.Log($"尝试加载对话文件: '{filePath}'");
        AVGManager.instance.LoadFile(filePath);
        
        // List<string> lines = FileManager.ReadTextAsset(fileToRead);

        // foreach (string line in lines)
        // {
        //     if (string.IsNullOrWhiteSpace(line))
        //         continue;
        //     
        //     DIALOGUE_LINE dl = DialogueParser.Parse(line);
        //
        //     for (int i = 0; i < dl.commandData.commands.Count; i++)
        //     {
        //         DL_COMMAND_DATA.Command command = dl.commandData.commands[i];
        //         Debug.Log($"Command [{i}] '{command.name}' has arguments [{string.Join(", ", command.arguments)}]");
        //     }
        // }
        
        // DialogueSystem.instance.Say(lines);
    }

    
    /// <summary>
    /// 每帧检查键盘输入，处理对话框的显示和隐藏操作
    /// </summary>
    private void Update()
    {
        // 检查向下箭头键是否被按下
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            DialogueSystem.instance.dialogueContainer.Hide();
        // 检查向上箭头键是否被按下
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            DialogueSystem.instance.dialogueContainer.Show();
    }
}
