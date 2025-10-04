using System;
using UnityEngine;

public class CommandTesting : MonoBehaviour
{
    private void Start()
    {
        CommandManager.instance.Execute("print");
    }
}
