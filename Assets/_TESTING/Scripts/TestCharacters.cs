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
        public AudioManager audioManager;
        
        private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);
        
        private void Start()
        {
            // 初始化音效引用
            if (audioManager == null)
            {
                audioManager = FindFirstObjectByType<AudioManager>();
            }
            
            StartCoroutine(Test());
        }
        
        private IEnumerator Test()
        {
            Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            Character_Sprite GuardRed = CreateCharacter("Guard Red as Generic") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;

            GuardRed.SetColor(Color.red);
            
            Raelin.SetPosition(new Vector2(.3f, 0));
            Stella.SetPosition(new Vector2(.45f, 0));
            Guard.SetPosition(new Vector2(.6f, 0));
            GuardRed.SetPosition(new Vector2(.75f, 0));
            
            GuardRed.SetPriority(1000);
            Stella.SetPriority(15);
            Raelin.SetPriority(8);
            Guard.SetPriority(30);
            
            yield return new WaitForSeconds(1);
            
            CharacterManager.instance.SortCharacters(new string[] {"Stella", "Raelin"});
            
            yield return new WaitForSeconds(1);
            
            CharacterManager.instance.SortCharacters();
            
            yield return new WaitForSeconds(1);
            
            CharacterManager.instance.SortCharacters(new string[] {"Raelin", "Guard Red", "Guard", "Stella"});
            
            yield return null;
        }
    }
}