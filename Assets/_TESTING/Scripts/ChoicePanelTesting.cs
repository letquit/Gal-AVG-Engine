using System;
using System.Collections;
using UnityEngine;

namespace TESTING
{
    /// <summary>
    /// 用于测试选择面板功能的类
    /// </summary>
    public class ChoicePanelTesting : MonoBehaviour
    {
        private ChoicePanel panel;

        /// <summary>
        /// 初始化测试，获取ChoicePanel实例并启动测试协程
        /// </summary>
        private void Start()
        {
            panel = ChoicePanel.instance;

            StartCoroutine(Running());
        }

        /// <summary>
        /// 运行选择面板测试逻辑的协程
        /// 显示一组预定义的选择项，等待用户选择，然后输出选择结果
        /// </summary>
        /// <returns>IEnumerator用于协程执行</returns>
        IEnumerator Running()
        {
            // 定义测试用的选择项数组
            string[] choices = new string[]
            {
                "Witness? Is that camera on?",
                "Oh, nah!",
                "I didn't see nothin'!",
                "Matta' Fact- I'm blind in my left eye and 43% blind in my right eye."
            };

            // 显示选择面板，包含标题和选择项
            panel.Show("Did You Witness Anything Strange?", choices);

            // 等待用户做出选择
            while (panel.isWaitingOnUserChoice)
            {
                yield return null;
            }
            
            // 获取用户的选择结果
            var decision = panel.lastDecision;
            
            // 输出用户选择的索引和对应的内容
            Debug.Log($"Made choice {decision.answerIndex} '{decision.choices[decision.answerIndex]}'");
        }
    }
}
