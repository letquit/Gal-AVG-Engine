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
            // Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            // Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;
            
            yield return new WaitForSeconds(1f);

            // Raelin.layers[1].SetColor(Color.red);
            yield return Raelin.TransitionColor(Color.red, 0.3f);
            yield return Raelin.TransitionColor(Color.blue);
            yield return Raelin.TransitionColor(Color.yellow);
            yield return Raelin.TransitionColor(Color.white);
            
            yield return null;
        }
    }
}
