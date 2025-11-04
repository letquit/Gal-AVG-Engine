using System;
using System.Collections.Generic;
using History;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TESTING
{
    public class HistoryTesting : MonoBehaviour
    {
        public HistoryState state = new HistoryState();

        private void Update()
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
                state = HistoryState.Capture();
            
            if (Keyboard.current.rKey.wasPressedThisFrame)
                state.Load();
        }
    }
}
