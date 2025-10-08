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
            Character Guard = CreateCharacter("Guard as Generic");
            Character Raelin = CreateCharacter("Raelin");
            Character Stella = CreateCharacter("Stella");
            Character Student = CreateCharacter("Female Student 2");
            
            // Guard.SetPosition(Vector2.zero);
            Raelin.SetPosition(new Vector2(0.5f, 0.5f));
            Stella.SetPosition(Vector2.one);
            Student.SetPosition(Vector2.zero);
            
            // Guard.Show();
            Raelin.Show();
            Stella.Show();
            
            yield return Student.Show();
            
            yield return Student.MoveToPosition(Vector2.one, smooth: true);
            yield return Student.MoveToPosition(Vector2.zero, smooth: true);

            Guard.SetDialogueFont(tempFont);
            Guard.SetNameFont(tempFont);
            Raelin.SetDialogueColor(Color.cyan);
            Stella.SetNameColor(Color.red);
            
            yield return Guard.Say("I want to say something important.");
            yield return Raelin.Say("Hold your peace.");
            yield return Stella.Say("Let him speak...");
        }
    }
}
