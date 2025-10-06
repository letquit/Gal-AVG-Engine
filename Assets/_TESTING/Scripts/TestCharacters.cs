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
            // Character Elen = CharacterManager.instance.CreateCharacter("Elen");
            // Character Stella = CharacterManager.instance.CreateCharacter("Stella");
            // Character Stella2 = CharacterManager.instance.CreateCharacter("Stella");
            // Character Adam = CharacterManager.instance.CreateCharacter("Adam");
            StartCoroutine(Test());
        }
        
        private IEnumerator Test()
        {
            Character Elen = CharacterManager.instance.CreateCharacter("Elen");
            Character Adam = CharacterManager.instance.CreateCharacter("Adam");
            Character Ben = CharacterManager.instance.CreateCharacter("Benjamin");
            List<string> lines = new List<string>()
            {
                "Hi, there!",
                "My name is Elen.",
                "What's your name?",
                "Oh,{wa 1} that's very nice."
            };
            
            yield return Elen.Say(lines);
            
            Elen.SetNameColor(Color.red);
            Elen.SetDialogueColor(Color.green);
            Elen.SetNameFont(tempFont);
            Elen.SetDialogueFont(tempFont);

            yield return Elen.Say(lines);
            
            Elen.ResetConfigurationData();
            
            yield return Elen.Say(lines);
            
            lines = new List<string>()
            {
                "I am Adam.",
                "More lines{c}Here."
            };
            
            yield return Adam.Say(lines);

            yield return Ben.Say("This is a line that I want to say.{a} It is a simple line.");
            
            Debug.Log("Finished");
        }
    }
}
