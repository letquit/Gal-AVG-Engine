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
            // 遍历所有对话行
            for (int i = 0; i < conversation.Count; i++)
            {
                // 跳过空行
                if (string.IsNullOrWhiteSpace(conversation[i]))
                    continue;
                
                // 解析当前行
                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);

                // 如果有对话内容，则执行对话逻辑
                if (line.hasDialogue)
                    yield return Line_RunDialogue(line);
                
                // 如果有命令内容，则执行命令逻辑
                if (line.hasCommands)
                    yield return Line_RunCommands(line);
                // 如果当前行包含对话内容，则等待用户输入后继续执行
                if (line.hasDialogue)
                    yield return WaitForUserInput();
            }
        }

        /// <summary>
        /// 执行对话行中的对话部分。
        /// </summary>
        /// <param name="line">解析后的对话行对象。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            // 显示说话者名称（如果存在）
            if (line.hasSpeaker)
                dialogueSystem.ShowSpeakerName(line.speakerData.displayname);

            // 构建并显示对话段落
            yield return BuildLineSegments(line.dialogueData);
        }
        
        /// <summary>
        /// 执行对话行中的命令部分。
        /// </summary>
        /// <param name="line">解析后的对话行对象。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;

            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                if (command.waitForCompletion)
                    yield return CommandManager.instance.Execute(command.name, command.arguments);
                else
                    CommandManager.instance.Execute(command.name, command.arguments);
            }
            
            yield return null;
        }

        /// <summary>
        /// 构建并播放对话的所有段落。
        /// </summary>
        /// <param name="line">包含多个段落的对话数据。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            // 遍历所有对话段落
            for (int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];
                
                // 等待该段落开始信号被触发
                yield return WaitForDialogueSegmentSignalToBeTriggered(segment);
                
                // 构建并显示该段对话文本
                yield return BuildDialogue(segment.dialogue, segment.appendText);
            }
        }

        /// <summary>
        /// 等待特定对话段落的开始信号被触发。
        /// </summary>
        /// <param name="segment">当前处理的对话段落。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator WaitForDialogueSegmentSignalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    // 等待用户输入
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    // 等待指定延迟时间
                    yield return new WaitForSeconds(segment.signalDelay);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 构建并显示对话文本，支持用户快速跳过或强制完成。
        /// </summary>
        /// <param name="dialogue">要显示的对话文本。</param>
        /// <param name="append">是否将文本追加到现有内容之后。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            // 根据是否追加决定构建方式
            if (!append)
                architect.Build(dialogue);
            else
                architect.Append(dialogue);

            // 持续检查文本是否仍在构建中
            while (architect.isBuilding)
            {
                // 如果用户触发了下一步操作
                if (userPrompt)
                {
                    // 第一次点击加速显示，第二次强制完成
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
