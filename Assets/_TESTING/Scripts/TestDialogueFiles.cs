using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

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
}
