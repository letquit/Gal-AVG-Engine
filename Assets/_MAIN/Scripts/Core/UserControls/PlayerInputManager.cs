using UnityEngine;
using UnityEngine.InputSystem;

namespace DIALOGUE
{
    /// <summary>
    /// 玩家输入管理器类，用于处理玩家的键盘输入并触发对话系统的相关操作
    /// </summary>
    public class PlayerInputManager : MonoBehaviour
    {
        /// <summary>
        /// 每帧检查玩家的键盘输入，当检测到空格键或回车键被按下时触发对话继续
        /// </summary>
        private void Update()
        {
            // 检查空格键或回车键是否在当前帧被按下
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                PromptAdvance();
        }
        
        /// <summary>
        /// 触发对话系统继续显示下一个对话内容
        /// </summary>
        public void PromptAdvance()
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }
    }
}
