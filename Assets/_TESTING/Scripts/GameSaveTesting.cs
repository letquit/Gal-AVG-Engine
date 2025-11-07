using System;
using ADVENTUREGAME;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TESTING
{
    public class GameSaveTesting : MonoBehaviour
    {
        private void Start()
        {
            AVGGameSave.activeFile = new AVGGameSave();
        }

        private void Update()
        {
            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                AVGGameSave.activeFile.Save();
            }
            else if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                AVGGameSave.activeFile = FileManager.Load<AVGGameSave>($"{FilePaths.gameSaves}1{AVGGameSave.FILE_TYPE}");
                AVGGameSave.activeFile.Load();
            }
        }
    }
}
