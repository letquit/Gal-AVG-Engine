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
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;
            
            // yield return new WaitForSeconds(1f);
            //
            // yield return Raelin.Unhighlight();
            //
            // yield return new WaitForSeconds(1f);
            //
            // yield return Raelin.TransitionColor(Color.red);
            //
            // yield return new WaitForSeconds(1f);
            //
            // yield return Raelin.Highlight();
            //
            // yield return new WaitForSeconds(1f);
            //
            // yield return Raelin.TransitionColor(Color.white);
            
            Raelin.SetPosition(Vector2.zero);
            Stella.SetPosition(new Vector2(1, 0));

            Stella.UnHighlight();
            yield return Raelin.Say("I want to say something.");

            Raelin.UnHighlight();
            Stella.Highlight();
            yield return Stella.Say("But I want to say something too!{c}Can I go first?");

            Raelin.Highlight();
            Stella.UnHighlight();
            yield return Raelin.Say("Sure, {a} be my guest.");

            Stella.Highlight();
            Raelin.UnHighlight();
            Stella.TransitionSprite(Stella.GetSprite("shy 1"), layer: 1);
            yield return Stella.Say("Yay!");
            
            yield return null;
        }
    }
}
