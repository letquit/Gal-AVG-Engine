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
            // Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            // Character_Sprite GuardRed = CreateCharacter("Guard Red as Generic") as Character_Sprite;
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            // Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;
            
            Character_Live2D Mao = CreateCharacter("Mao") as Character_Live2D;
            
            Raelin.SetPosition(new Vector2(0, 0));
            Mao.SetPosition(new Vector2(1, 0));
            
            yield return new WaitForSeconds(1);
            
            Mao.SetExpression(5);
            Mao.SetMotion("Bounce");
            
            yield return null;
        }
    }
}