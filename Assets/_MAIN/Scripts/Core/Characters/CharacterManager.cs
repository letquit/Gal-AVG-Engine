using System;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 角色管理器，用于创建和管理游戏中的角色实例。
    /// 提供统一的角色创建接口，并维护已创建角色的字典。
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例，用于全局访问角色管理器
        /// </summary>
        public static CharacterManager instance { get; private set; }
        
        /// <summary>
        /// 存储已创建角色的字典，键为角色名（小写），值为角色实例
        /// </summary>
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        /// <summary>
        /// 获取对话系统配置中的角色配置资源
        /// </summary>
        private CharacterConfigSO config => DialogueSystem.instance.config.characterConfigurationAsset;
        
        /// <summary>
        /// 初始化单例实例
        /// </summary>
        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// 根据角色名称获取角色配置数据
        /// </summary>
        /// <param name="characterName">要查询的角色名称</param>
        /// <returns>对应的角色配置数据，如果未找到则返回null</returns>
        public CharacterConfigData GetCharacterConfig(string characterName)
        {
            return config.GetConfig(characterName);
        }

        /// <summary>
        /// 获取指定名称的角色实例
        /// </summary>
        /// <param name="characterName">要获取的角色名称</param>
        /// <param name="createIfDoesNotExist">当角色不存在时是否自动创建</param>
        /// <returns>找到或新创建的角色实例，如果未找到且不创建则返回null</returns>
        public Character GetCharacter(string characterName, bool createIfDoesNotExist = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
                return characters[characterName.ToLower()];
            else if (createIfDoesNotExist)
                return CreateCharacter(characterName);
            
            return null;
        }

        /// <summary>
        /// 创建指定名称的角色实例
        /// </summary>
        /// <param name="characterName">要创建的角色名称</param>
        /// <returns>创建成功的角色实例，如果角色已存在则返回null</returns>
        public Character CreateCharacter(string characterName)
        {
            // 检查角色是否已存在
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"A Character called '{characterName}' already exists. Did not create the character.");
                return null;
            }
            
            // 获取角色信息并创建角色实例
            CHARACTER_INFO info = GetCharacterInfo(characterName);
         
            Character character = CreateCharacterFromInfo(info);
            
            // 将新创建的角色添加到字典中
            characters.Add(characterName.ToLower(), character);
            
            return character;
        }

        /// <summary>
        /// 根据角色名称获取角色信息
        /// </summary>
        /// <param name="characterName">角色名称</param>
        /// <returns>包含角色名称和配置信息的结构体</returns>
        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();

            result.name = characterName;

            result.config = config.GetConfig(characterName);
            
            return result;
        }

        /// <summary>
        /// 根据角色信息创建对应类型的角色实例
        /// </summary>
        /// <param name="info">包含角色名称和配置信息的结构体</param>
        /// <returns>创建的角色实例，如果类型不支持则返回null</returns>
        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;
            
            // 根据角色配置中的类型创建对应的角色实例
            switch (config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config);

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, config);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, config);

                default:
                    return null;
            }
        }
        
        /// <summary>
        /// 内部结构体，用于存储角色的基本信息
        /// </summary>
        private class CHARACTER_INFO
        {
            /// <summary>
            /// 角色名称
            /// </summary>
            public string name = "";

            /// <summary>
            /// 角色配置数据
            /// </summary>
            public CharacterConfigData config = null;
        }
    }
}
