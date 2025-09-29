using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace TESTING
{
    /// <summary>
    /// 文本构建器控制类，用于测试对话系统的文本显示功能。
    /// 该类通过键盘输入控制对话文本的构建和追加操作。
    /// </summary>
    public class Texting_Architect : MonoBehaviour
    {
        private DialogueSystem ds;
        private TextArchitect architect;
        
        public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.instant;
        
        private string[] lines = new string[5]
        {
            "This is a random line of dialogue.",
            "I want to say something, come over here.",
            "The world is a crazy place sometimes.",
            "Don't lose hope, things will get better!",
            "It's a bird? It's a plane? No! – It's Super Sheltie!"
        };
        
        /// <summary>
        /// 初始化对话系统和文本构建器组件。
        /// 设置文本构建方式为淡入效果，速度为0.5。
        /// </summary>
        private void Start()
        {
            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.fade;
            architect.speed = 0.5f;
        }

        /// <summary>
        /// 每帧检测键盘输入并执行相应的文本操作。
        /// 空格键用于开始/加速/完成文本构建，A键用于追加文本，S键用于停止构建。
        /// </summary>
        private void Update()
        {
            // 如果当前设置的构建方式与实际使用的不同，则更新并停止当前构建
            if (bm != architect.buildMethod)
            {
                architect.buildMethod = bm;
                architect.Stop();
            }
            
            // 检测S键按下事件，用于停止文本构建
            if (Keyboard.current.sKey.wasPressedThisFrame)
                architect.Stop();
            
            // 定义一个长文本用于测试
            string longLine =
                "This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue. This is a random line of dialogue.";
            
            // 检测空格键按下事件
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // 如果正在构建文本
                if (architect.isBuilding)
                {
                    // 如果没有加速，则启用加速；否则强制完成构建
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else 
                        architect.ForceComplete();
                }
                // 如果没有在构建文本，则开始构建长文本
                else
                    architect.Build(longLine);
                    //architect.Build(lines[Random.Range(0, lines.Length)]);
            }
            // 检测A键按下事件，用于追加文本
            else if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                architect.Append(longLine);
                //architect.Append(lines[Random.Range(0, lines.Length)]);
            }
        }
    }
}
