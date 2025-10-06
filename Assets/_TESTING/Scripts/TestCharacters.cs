using System;
using CHARACTERS;
using UnityEngine;

namespace TESTING
{
    public class TestCharacters : MonoBehaviour
    {
        private void Start()
        {
            Character Elen = CharacterManager.instance.CreateCharacter("Elen");
            Character Stella = CharacterManager.instance.CreateCharacter("Stella");
            Character Stella2 = CharacterManager.instance.CreateCharacter("Stella");
            Character Adam = CharacterManager.instance.CreateCharacter("Adam");
        }
    }
}
