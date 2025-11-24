#if UNITY_EDITOR
using System;
using UnityEngine;

namespace TESTING
{
    public class TestCensor : MonoBehaviour
    {
        private void Start()
        {
            Check("This line has a badword1 in it?");
            Check("This should be clear of any bad words!");
            Check("This $t1nkiNG line should be bad as well.");
            Check("I want some TOFU in a warm bowl of Miso Soup. Don't forget the extratofu");
        }

        private void Check(string line)
        {
            if (CensorManager.Censor(ref line))
                Debug.Log($"<color=red>'{line}'</color>");
            else
                Debug.Log($"<color=green>'{line}'</color>");
        }
    }
}
#endif
