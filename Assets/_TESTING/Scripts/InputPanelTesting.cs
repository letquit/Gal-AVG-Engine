using System;
using System.Collections;
using CHARACTERS;
using UnityEngine;

public class InputPanelTesting : MonoBehaviour
{
    public InputPanel inputPanel;

    private void Start()
    {
        StartCoroutine(Running());
    }
    
    private IEnumerator Running()
    {
        Character Stella = CharacterManager.instance.CreateCharacter("Stella", revealAfterCreation: true);
        
        yield return Stella.Say("Hi! What's your name?");

        inputPanel.Show("What Is Your Name?");

        while (inputPanel.isWaitingOnUserInput)
            yield return null;
        
        string characterName = inputPanel.lastInput;
        
        yield return Stella.Say($"It's very nice to meet you, {characterName}!");
    }
}
