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
            yield return new WaitForSeconds(1f);
            
            Character Stella = CharacterManager.instance.CreateCharacter("Stella");

            yield return new WaitForSeconds(1f);
            
            yield return Stella.Hide();
            
            yield return new WaitForSeconds(0.5f);
            
            yield return Stella.Show();
            
            yield return Stella.Say("Hello!");
        }
    }
}
