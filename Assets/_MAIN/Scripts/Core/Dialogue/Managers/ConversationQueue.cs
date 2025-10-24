using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话队列管理类，用于管理对话的排队和优先级处理
    /// </summary>
    public class ConversationQueue
    {
        /// <summary>
        /// 会话队列，用于存储和管理Conversation对象的先进先出队列
        /// </summary>
        private Queue<Conversation> conversationQueue = new Queue<Conversation>();

        /// <summary>
        /// 获取队列中的第一个对话对象
        /// </summary>
        public Conversation top => conversationQueue.Peek();
        
        /// <summary>
        /// 将对话对象添加到队列末尾
        /// </summary>
        /// <param name="conversation">要添加的对话对象</param>
        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);

        /// <summary>
        /// 将对话对象添加到队列开头，使其具有最高优先级
        /// </summary>
        /// <param name="conversation">要添加的对话对象</param>
        public void EnqueuePriority(Conversation conversation)
        {
            // 创建新的队列，将优先对话放在队首
            Queue<Conversation> queue = new Queue<Conversation>();
            queue.Enqueue(conversation);

            // 将原队列中的所有对话依次添加到新队列中
            while (conversationQueue.Count > 0)
                queue.Enqueue(conversationQueue.Dequeue());
            
            // 替换原队列为新队列
            conversationQueue = queue;
        }
        
        /// <summary>
        /// 从队列中移除第一个对话对象
        /// </summary>
        public void Dequeue()
        {
            if (conversationQueue.Count > 0)
                conversationQueue.Dequeue();
        }
        
        /// <summary>
        /// 检查队列是否为空
        /// </summary>
        /// <returns>如果队列为空返回true，否则返回false</returns>
        public bool IsEmpty() => conversationQueue.Count == 0;
    }
}