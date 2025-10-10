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
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;

            Raelin.SetPosition(new Vector2(0, 0));
            Stella.SetPosition(new Vector2(1, 0));

            yield return new WaitForSeconds(1);

            audioManager.SwitchAudioSet("Stella");
            Stella.TransitionSprite(Stella.GetSprite("2"));
            Stella.TransitionSprite(Stella.GetSprite("default 1"), layer: 1);
            Stella.Animate("Hop");
            yield return Stella.Say("Where did this wind chill come from?");

            audioManager.SwitchAudioSet("Raelin");
            Raelin.FaceRight();
            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Shocked"), layer: 1);
            Raelin.MoveToPosition(new Vector2(0.1f, 0));
            Raelin.Animate("Shiver", true);
            yield return Raelin.Say("I don't know -- but I hate it! {a} It's freezing!");

            audioManager.SwitchAudioSet("Stella");
            Stella.TransitionSprite(Stella.GetSprite("shy 1"), layer: 1);
            yield return Stella.Say("Oh, it's over!");

            audioManager.SwitchAudioSet("Raelin");
            Raelin.TransitionSprite(Raelin.GetSprite("A2"));
            Raelin.TransitionSprite(Raelin.GetSprite("A_Shocked"), layer: 1);
            Raelin.Animate("Shiver", false);
            yield return Raelin.Say("Thank the Lord... {a} I'm not wearing enough clothes for that crap.");
            
            yield return null;
        }
    }
}