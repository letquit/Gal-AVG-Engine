using System.Collections;
using CHARACTERS;
using UnityEngine;

namespace TESTING
{
    public class AudioTesting : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(Running());
        }

        Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

        IEnumerator Running()
        {
            Character_Sprite Stella = CreateCharacter("Stella") as Character_Sprite;
            Stella.Show();

            // yield return new WaitForSeconds(0.5f);
            //
            // AudioManager.instance.PlaySoundEffect("Audio/SFX/thunder_strong_01");
            //
            // yield return new WaitForSeconds(1f);
            // Stella.Animate("Hop");
            // Stella.TransitionSprite(Stella.GetSprite("2"));
            // Stella.TransitionSprite(Stella.GetSprite("shy 1"), 1);
            // Stella.Say("Yikes!");
            
            AudioManager.instance.PlaySoundEffect("Audio/SFX/RadioStatic", loop: true);

            yield return Stella.Say("I'm going to turn off the radio.");
            
            AudioManager.instance.StopSoundEffect("RadioStatic");
            
            Stella.Say("It's off now!");
        }
    }
}
