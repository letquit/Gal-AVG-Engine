#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace TESTING
{
    public class TestRunFile : MonoBehaviour
    {
        [SerializeField] private TextAsset file;

        private void Start()
        {
            LoadFile();
        }
        
        private void LoadFile()
        {
            List<string> lines = FileManager.ReadTextAsset(file);
            Conversation conversation = new Conversation(lines);
            DialogueSystem.instance.Say(conversation);
        }
    }
}

#endif