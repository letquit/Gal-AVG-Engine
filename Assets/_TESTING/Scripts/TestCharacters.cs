using System;
using System.Collections;
using System.Collections.Generic;
using CHARACTERS;
using DIALOGUE;
using TMPro;
using UnityEngine;

namespace TESTING
{
    public class TestCharacters : MonoBehaviour
    {
        public TMP_FontAsset tempFont;

        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);
        
        private void Start()
        {
            // Character Stella = CharacterManager.instance.CreateCharacter("Stella");
            // Character Stella = CharacterManager.instance.CreateCharacter("Female Student 2");
            // Character Realin = CharacterManager.instance.CreateCharacter("Raelin");
            // Character Generic = CharacterManager.instance.CreateCharacter("Generic");
            StartCoroutine(Test());
        }
        
        private IEnumerator Test()
        {
            Character guard1 = CreateCharacter("Guard1 as Generic");
            Character guard2 = CreateCharacter("Guard2 as Generic");
            Character guard3 = CreateCharacter("Guard3 as Generic");
            
            guard1.Show();
            guard2.Show();
            guard3.Show();

            guard1.SetDialogueFont(tempFont);
            guard1.SetNameFont(tempFont);
            guard2.SetDialogueColor(Color.cyan);
            guard3.SetNameColor(Color.red);
            
            yield return guard1.Say("I want to say something important.");
            yield return guard2.Say("Hold your peace.");
            yield return guard3.Say("Let him speak...");
        }
    }
}
