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
            // StartCoroutine(Test2());
        }
        
        private IEnumerator Test()
        {
            Character_Model3D PunChan = CreateCharacter("PunChan") as Character_Model3D;
            // Character_Sprite Guard = CreateCharacter("Guard as Generic") as Character_Sprite;
            // Character_Sprite GuardRed = CreateCharacter("Guard Red as Generic") as Character_Sprite;
            // Character_Sprite Raelin = CreateCharacter("Raelin") as Character_Sprite;
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            // Character_Sprite Student = CreateCharacter("Female Student 2") as Character_Sprite;
            
            Character_Live2D Mao = CreateCharacter("Mao") as Character_Live2D;
            
            // Mao.SetPosition(new Vector2(.3f, 0));
            // Stella.SetPosition(new Vector2(.4f, 0));
            // Raelin.SetPosition(new Vector2(.5f, 0));
            // Student.SetPosition(new Vector2(.6f, 0));
            Mao.SetPosition(new Vector2(.5f, 0));
            Stella.SetPosition(new Vector2(0, 0));
            PunChan.SetPosition(new Vector2(1, 0));
            
            yield return new WaitForSeconds(2f);

            // Stella.Hide();
            // Mao.Hide();
            // Stella.SetColor(Color.red);
            // Mao.SetColor(Color.red);
            // Stella.TransitionColor(Color.red);
            // Mao.TransitionColor(Color.red);
            // Stella.UnHighlight();
            // Mao.Highlight();
            // Stella.FaceRight();
            // Mao.FaceRight();
            // CharacterManager.instance.SortCharacters(new string[] {"Mao", "Raelin", "Stella", "Female Student 2"});
            // Stella.TransitionColor(Color.red, 0.1f);
            // Mao.TransitionColor(Color.red, 0.1f);
            // PunChan.TransitionColor(Color.red, 0.1f);
            
            // yield return new WaitForSeconds(2f);
            
            // Stella.Show();
            // Mao.Show();
            // Stella.SetColor(Color.white);
            // Mao.SetColor(Color.white);
            // Stella.TransitionColor(Color.white);
            // Mao.TransitionColor(Color.white);
            // Stella.Highlight();
            // Mao.UnHighlight();
            // Stella.Flip();
            // Mao.Flip();
            // Mao.SetPriority(5);

            Stella.FaceRight();
            Mao.FaceRight();
            PunChan.FaceRight();
            
            yield return new WaitForSeconds(2f);

            Stella.FaceLeft();
            Mao.FaceLeft();
            PunChan.FaceLeft();
        }

        private IEnumerator Test1()
        {
            Character_Model3D PunChan = CreateCharacter("PunChan") as Character_Model3D;
            // PunChan.isVisible = false;
            
            yield return new WaitForSeconds(1f);

            // yield return PunChan.Show();
            //
            // yield return new WaitForSeconds(1f);
            //
            // yield return PunChan.Hide();
            
            PunChan.SetColor(Color.red);
        }
    }
}