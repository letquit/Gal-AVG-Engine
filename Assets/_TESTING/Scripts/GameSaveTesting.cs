using System;
using ADVENTUREGAME;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TESTING
{
    public class GameSaveTesting : MonoBehaviour
    {
        public AVGGameSave save;
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
                try
                {
                    save = AVGGameSave.Load($"{FilePaths.gameSaves}1{AVGGameSave.FILE_TYPE}", activateOnLoad: true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Do something because we found an error. {e.ToString()}");
                }
            }
        }
    }
}
