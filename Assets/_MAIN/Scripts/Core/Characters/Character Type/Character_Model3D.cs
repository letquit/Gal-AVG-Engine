using UnityEngine;

namespace CHARACTERS
{
    public class Character_Model3D : Character
    {
        public Character_Model3D(string name, CharacterConfigData config) : base(name, config)
        {
            Debug.Log($"Created Model3D Character: '{name}'");
        }
    }
}
