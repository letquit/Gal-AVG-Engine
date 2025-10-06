using System;
using System.Collections;
using System.Collections.Generic;
using CHARACTERS;
using DIALOGUE;
using UnityEngine;

namespace TESTING
{
    public class TestCharacters : MonoBehaviour
    {
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
            Character Sarah = CharacterManager.instance.CreateCharacter("Sarah");
            List<string> lines = new List<string>()
            {
                "Hi, there!",
                "My name is Elen.",
                "What's your name?",
                "Oh,{wa 1} that's very nice."
            };
            
            yield return Elen.Say(lines);
            
            lines = new List<string>()
            {
                "I am Adam.",
                "More lines{c}Here."
            };
            
            yield return Adam.Say(lines);

            yield return Sarah.Say("This is a line that I want to say.{a} It is a simple line.");
            
            Debug.Log("Finished");
        }
    }
}
