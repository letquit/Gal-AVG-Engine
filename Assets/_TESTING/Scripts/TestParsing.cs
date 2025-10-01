using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace TESTING
{
    public class TestParsing : MonoBehaviour
    {
        private void Start()
        {
            SendFileToParse();
        }

        private void SendFileToParse()
        {
            List<string> lines = FileManager.ReadTextAsset("testFile");

            foreach (string line in lines)
            {
                if (line == string.Empty)
                    continue;
                
                DIALOGUE_LINE dl = DialogueParser.Parse(line);
            }
        }
    }
}