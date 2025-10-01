using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 管理对话流程的类，负责启动、停止和执行对话内容。
    /// </summary>
    public class ConversationManager
    {
        /// <summary>
        /// 获取对话系统的单例实例。
        /// </summary>
        private DialogueSystem dialogueSystem => DialogueSystem.instance;

        /// <summary>
        /// 当前正在运行的协程。
        /// </summary>
        private Coroutine process = null;

        /// <summary>
        /// 判断对话是否正在运行。
        /// </summary>
        public bool isRunning => process != null;

        /// <summary>
        /// 文本构建器，用于逐字显示对话文本。
        /// </summary>
        private TextArchitect architect = null;

        /// <summary>
        /// 用户是否已触发下一步操作（如点击）。
        /// </summary>
        private bool userPrompt = false;

        /// <summary>
        /// 构造函数，初始化对话管理器并绑定用户输入事件。
        /// </summary>
        /// <param name="architect">用于构建对话文本的文本构建器。</param>
        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;
        }
        
        /// <summary>
        /// 用户触发下一步操作时调用，设置 userPrompt 标志为 true。
        /// </summary>
        private void OnUserPrompt_Next()
        {
            userPrompt = true;
        }

        /// <summary>
        /// 启动一个新的对话流程。
        /// </summary>
        /// <param name="conversation">包含对话内容的字符串列表。</param>
        public void StarConversation(List<string> conversation)
        {
            StopConversation();
            
            process = dialogueSystem.StartCoroutine(RunningConversation(conversation));
        }

        /// <summary>
        /// 停止当前正在运行的对话流程。
        /// </summary>
        public void StopConversation()
        {
            if (!isRunning)
                return;
            
            dialogueSystem.StopCoroutine(process);
            process = null;
        }

        /// <summary>
        /// 执行对话内容的核心协程。
        /// </summary>
        /// <param name="conversation">包含对话内容的字符串列表。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator RunningConversation(List<string> conversation)
        {
            for (int i = 0; i < conversation.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(conversation[i]))
                    continue;
                
                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);

                if (line.hasDialogue)
                    yield return Line_RunDialogue(line);
                
                if (line.hasCommands)
                    yield return Line_RunCommands(line);
            }
        }

        /// <summary>
        /// 执行对话行中的对话部分。
        /// </summary>
        /// <param name="line">解析后的对话行对象。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            if (line.hasSpeaker)
                dialogueSystem.ShowSpeakerName(line.speaker);
            else
                dialogueSystem.HideSpeakerName();

            yield return BuildDialogue(line.dialogue);
            
            yield return WaitForUserInput();
        }
        
        /// <summary>
        /// 执行对话行中的命令部分。
        /// </summary>
        /// <param name="line">解析后的对话行对象。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            Debug.Log(line.commands);
            yield return null;
        }

        /// <summary>
        /// 构建并显示对话文本，支持用户快速跳过或强制完成。
        /// </summary>
        /// <param name="dialogue">要显示的对话文本。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator BuildDialogue(string dialogue)
        {
            
            architect.Build(dialogue);

            while (architect.isBuilding)
            {
                if (userPrompt)
                {
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else
                        architect.ForceComplete();
                    
                    userPrompt = false;
                }
                yield return null;
            }
        }

        /// <summary>
        /// 等待用户输入以继续下一步对话。
        /// </summary>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator WaitForUserInput()
        {
            while (!userPrompt)
                yield return null;
            
            userPrompt = false;
        }
    }
}
