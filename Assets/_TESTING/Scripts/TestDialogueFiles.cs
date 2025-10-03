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
        
        DialogueSystem.instance.Say(lines);
    }
}
