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
            Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;

            Raelin.SetPosition(Vector2.zero);
            Stella.SetPosition(new Vector2(1, 0));

            audioManager.SwitchAudioSet("Raelin");
            Stella.UnHighlight();
            yield return Raelin.Say("我想说点什么。");

            audioManager.SwitchAudioSet("Stella");
            Raelin.UnHighlight();
            Stella.Highlight();
            yield return Stella.Say("但我也想说点什么！{c}我可以先说吗？");

            audioManager.SwitchAudioSet("Raelin");
            Raelin.Highlight();
            Stella.UnHighlight();
            yield return Raelin.Say("当然，{a}请便。");

            audioManager.SwitchAudioSet("Stella");
            Stella.Highlight();
            Raelin.UnHighlight();
            Stella.TransitionSprite(Stella.GetSprite("shy 1"), layer: 1);
            yield return Stella.Say("好耶！");
            
            yield return null;
        }
    }
}