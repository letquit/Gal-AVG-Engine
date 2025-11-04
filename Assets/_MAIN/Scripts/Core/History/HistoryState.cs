using System;
using System.Collections.Generic;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 历史状态类，用于保存和恢复游戏中的各种状态数据
    /// 包括对话、角色、音频和图形等数据的快照
    /// </summary>
    [Serializable]
    public class HistoryState
    {
        public DialogueData dialogue;
        public List<CharacterData> characters;
        public List<AudioData> audio;
        public List<GraphicData> graphics;
        
        /// <summary>
        /// 捕获当前游戏状态并创建历史状态快照
        /// 通过调用各个数据类型的Capture方法来收集当前状态
        /// </summary>
        /// <returns>包含当前所有状态数据的HistoryState实例</returns>
        public static HistoryState Capture()
        { 
            // 创建新的历史状态实例
            HistoryState state = new HistoryState();
            // 捕获对话数据状态
            state.dialogue = DialogueData.Capture();
            // 捕获角色数据状态
            state.characters = CharacterData.Capture();
            // 捕获音频数据状态
            state.audio = AudioData.Capture();
            // 捕获图形数据状态
            state.graphics = GraphicData.Capture();
            
            return state;
        }

        /// <summary>
        /// 加载对话、角色、音频和图形数据配置
        /// </summary>
        /// <remarks>
        /// 该方法依次应用四种数据配置：
        /// 1. 对话数据配置
        /// 2. 角色数据配置
        /// 3. 音频数据配置
        /// 4. 图形数据配置
        /// </remarks>
        public void Load()
        {
            // 应用对话数据配置
            DialogueData.Apply(dialogue);
            // 应用角色数据配置
            CharacterData.Apply(characters);
            // 应用音频数据配置
            AudioData.Apply(audio);
            // 应用图形数据配置
            GraphicData.Apply(graphics);
        }
    }
}

