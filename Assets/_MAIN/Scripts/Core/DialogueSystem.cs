using System;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话系统类，用于管理游戏中的对话逻辑
    /// 该类继承自MonoBehaviour，可作为Unity组件使用
    /// 实现了单例模式，确保全局只有一个对话系统实例
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        public DialogueContainer dialogueContainer = new DialogueContainer();

        public static DialogueSystem instance;

        /// <summary>
        /// Unity生命周期函数，在对象创建时调用
        /// 用于初始化对话系统实例，实现单例模式
        /// 如果当前没有实例，则将当前对象设为实例
        /// 如果已存在实例，则销毁当前对象
        /// </summary>
        private void Awake()
        {
            // 单例模式实现：确保全局只有一个对话系统实例
            if (instance == null)
                instance = this;
            else
                DestroyImmediate(gameObject);
        }
    }
}