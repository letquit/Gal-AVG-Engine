using System;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset fileToRead = null;
    private void Start()
    {
        StartConversation();
    }

    private void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset(fileToRead);

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
        
        DialogueSystem.instance.Say(lines);
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
