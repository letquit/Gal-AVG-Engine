using System.Collections;
using System.Collections.Generic;
using CHARACTERS;
using COMMANDS;
using DIALOGUE.LogicalLines;
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
        public TextArchitect architect = null;

        /// <summary>
        /// 用户是否已触发下一步操作（如点击）。
        /// </summary>
        private bool userPrompt = false;
        
        /// <summary>
        /// LogicalLineManager类型的私有字段，用于管理逻辑行逻辑
        /// </summary>
        private LogicalLineManager logicalLineManager;

        /// <summary>
        /// 获取当前对话队列中的顶部对话
        /// </summary>
        /// <returns>如果对话队列为空则返回null，否则返回队列顶部的Conversation对象</returns>
        public Conversation conversation => (conversationQueue.IsEmpty() ? null : conversationQueue.top);

        /// <summary>
        /// 获取对话进度
        /// </summary>
        /// <returns>
        /// 如果对话队列为空，返回-1；否则返回队列顶部对话的进度值
        /// </returns>
        public int conversationProgress => (conversationQueue.IsEmpty() ? -1 : conversationQueue.top.GetProgress());
        
        /// <summary>
        /// 对话队列实例，用于管理多个对话的顺序执行
        /// </summary>
        private ConversationQueue conversationQueue;

        
        /// <summary>
        /// 初始化对话管理器实例
        /// </summary>
        /// <param name="architect">文本架构师实例，用于处理文本显示和格式化</param>
        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            // 订阅用户提示下一个事件
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;
            
            // 初始化逻辑行管理器
            logicalLineManager = new LogicalLineManager();
            // 创建对话队列实例
            conversationQueue = new ConversationQueue();
        }
        
        /// <summary>
        /// 将对话对象添加到队列的末尾
        /// </summary>
        /// <param name="conversation">要添加到队列的对话对象</param>
        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);

        /// <summary>
        /// 将对话对象添加到队列的优先级位置
        /// </summary>
        /// <param name="conversation">要添加到队列的对话对象</param>
        public void EnqueuePriority(Conversation conversation) => conversationQueue.EnqueuePriority(conversation);
        
        /// <summary>
        /// 用户触发下一步操作时调用，设置 userPrompt 标志为 true。
        /// </summary>
        private void OnUserPrompt_Next()
        {
            userPrompt = true;
        }

        /// <summary>
        /// 开始执行对话流程
        /// </summary>
        /// <param name="conversation">要执行的对话对象</param>
        /// <returns>返回启动的协程对象，可用于控制对话流程的暂停、停止等操作</returns>
        public Coroutine StartConversation(Conversation conversation)
        {
            // 停止当前正在进行的对话
            StopConversation();
            // 清空对话队列
            conversationQueue.Clear();
            
            // 将新的对话加入队列
            Enqueue(conversation);
            
            // 启动对话执行协程
            process = dialogueSystem.StartCoroutine(RunningConversation());
            
            return process;
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
        /// 运行对话流程的协程函数
        /// </summary>
        /// <returns>IEnumerator迭代器对象，用于协程执行</returns>
        IEnumerator RunningConversation()
        {
            // 循环处理对话队列中的所有对话
            while (!conversationQueue.IsEmpty())
            {
                Conversation currentConversation = conversation;

                // 如果当前对话已结束，则从队列中删除该对话并继续循环
                if (currentConversation.HasReachedEnd())
                {
                    conversationQueue.Dequeue();
                    continue;
                }
                
                string rawLine = currentConversation.CurrentLine();

                // 如果当前行为空白行，则尝试推进到下一对话行并继续循环
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    TryAdvanceConversation(currentConversation);
                    continue;
                }
                
                DIALOGUE_LINE line = DialogueParser.Parse(rawLine);
                // Debug.Log($"Parsed line - Speaker: {line.hasSpeaker}, Dialogue: {line.hasDialogue}, Commands: {line.hasCommands}");

                // 检查并执行逻辑管理器中的自定义逻辑
                if (logicalLineManager.TryGetLogic(line, out Coroutine logic))
                {
                    yield return logic;
                }
                else
                {
                    // 如果有对话内容，则执行对话逻辑
                    if (line.hasDialogue)
                        yield return Line_RunDialogue(line);
                
                    // 如果有命令内容，则执行命令逻辑
                    if (line.hasCommands)
                        yield return Line_RunCommands(line);
                
                    // 如果当前行包含对话内容，则等待用户输入后继续执行
                    if (line.hasDialogue)
                    {
                        yield return WaitForUserInput();
                    
                        CommandManager.instance.StopAllProcesses();
                    }
                }

                // 尝试推进到下一对话行
                TryAdvanceConversation(currentConversation);
            }

            process = null;
        }

        /// <summary>
        /// 尝试推进对话进度，如果对话已结束则从队列中移除
        /// </summary>
        /// <param name="conversation">要推进的对话对象</param>
        private void TryAdvanceConversation(Conversation conversation)
        {
            // 推进对话进度
            conversation.IncrementProgress();
            
            // 检查当前对话是否为队列中的第一个对话
            if (conversation != conversationQueue.top)
                return;
            
            // 检查对话是否已到达末尾，如果是则从队列中移除
            if (conversation.HasReachedEnd())
                conversationQueue.Dequeue();
        }

        /// <summary>
        /// 执行对话行中的对话部分。
        /// </summary>
        /// <param name="line">解析后的对话行对象。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            // 显示说话者名称（如果存在）
            // if (line.hasSpeaker)
            //     dialogueSystem.ShowSpeakerName(line.speakerData.displayname);
            // else if (!string.IsNullOrEmpty(dialogueSystem.dialogueContainer.nameContainer.nameText.text))
            //     // 如果当前已有名称显示，保持显示状态
            //     dialogueSystem.dialogueContainer.nameContainer.Show();
            if (line.hasSpeaker)
                HandleSpeakerLogic(line.speakerData);

            // 说话前如果隐藏了对话框则显示对话框
            if (!dialogueSystem.dialogueContainer.isVisible)
                dialogueSystem.dialogueContainer.Show();
            
            // 构建并显示对话段落
            yield return BuildLineSegments(line.dialogueData);
        }

        /// <summary>
        /// 处理说话者的逻辑，包括角色创建、显示、位置设置和表情设置
        /// </summary>
        /// <param name="speakerData">包含说话者相关信息的数据对象</param>
        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            // 判断是否需要创建角色的条件：角色进入、设置位置或设置表情
            bool characterMustBeCreated = (speakerData.makeCharacterEnter || speakerData.isCastingPosition ||
                                           speakerData.isCastingExpressions);
            
            Character character =
                CharacterManager.instance.GetCharacter(speakerData.name, createIfDoesNotExist: characterMustBeCreated);

            // 如果需要角色进入且角色当前不可见且不在显示过程中，则显示角色
            if (speakerData.makeCharacterEnter && (!character.isVisible && !character.isRevealing))
                character.Show();
                
            dialogueSystem.ShowSpeakerName(TagManager.Inject(speakerData.displayname));
                
            DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);

            // 如果需要设置角色位置，则移动到指定位置
            if (speakerData.isCastingPosition)
                // character.SetPosition(speakerData.castPosition);
                character.MoveToPosition(speakerData.castPosition);

            // 如果需要设置角色表情，则应用所有指定的表情设置
            if (speakerData.isCastingExpressions)
            {
                foreach (var ce in speakerData.CastExpressions)
                    character.OnReceiveCastingExpression(ce.layer, ce.expression);
            }
        }
        
        /// <summary>
        /// 执行对话行中的命令部分。
        /// </summary>
        /// <param name="line">解析后的对话行对象。</param>
        /// <returns>IEnumerator 用于协程执行。</returns>
        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            // 获取对话行中的所有命令
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;

            // 遍历并执行每个命令
            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                // 根据命令是否需要等待完成来决定执行方式
                if (command.waitForCompletion || command.name == "wait")
                {
                    // 执行需要等待完成的命令，直到命令执行完毕或用户中断
                    CoroutineWrapper cw = CommandManager.instance.Execute(command.name, command.arguments);
                    while (!cw.IsDone)
                    {
                        // 检查用户是否发出提示信号，如果是则停止当前进程
                        if (userPrompt)
                        {
                            CommandManager.instance.StopCurrentProcess();
                            userPrompt = false;
                        }
                        yield return null;
                    }
                }
                else
                {
                    // 执行不需要等待完成的命令
                    CommandManager.instance.Execute(command.name, command.arguments);
                }
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
        /// 获取或设置一个值，该值指示当前是否正在等待自动计时器触发
        /// </summary>
        public bool isWaitingOnAutoTimer { get; private set; } = false;
        
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
                    // 启用自动计时器
                    isWaitingOnAutoTimer = true;
                    // 等待指定延迟时间
                    yield return new WaitForSeconds(segment.signalDelay);
                    // 禁用自动计时器
                    isWaitingOnAutoTimer = false;
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
            // 注入标签内容
            dialogue = TagManager.Inject(dialogue);
            
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
            // 显示对话系统提示
            dialogueSystem.prompt.Show();
            
            // 等待用户输入完成
            while (!userPrompt)
                yield return null;
            
            // 隐藏对话系统提示并重置用户输入状态
            dialogueSystem.prompt.Hide();
            
            userPrompt = false;
        }
    }
}
