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
        /// 角色转换标识符常量，用于字符串转换操作
        /// </summary>
        private const string CHARACTER_CASTING_ID = " as ";

        /// <summary>
        /// 角色名称标识符常量，用作路径中的占位符
        /// </summary>
        private const string CHARACTER_NAME_ID = "<charname>";

        /// <summary>
        /// 获取角色根路径的属性
        /// </summary>
        private string characterRootPath => $"Characters/{CHARACTER_NAME_ID}";

        /// <summary>
        /// 获取角色预制体路径的属性
        /// </summary>
        private string characterPrefabPath => $"{characterRootPath}/Character - [{CHARACTER_NAME_ID}]";

        /// <summary>
        /// 序列化字段，用于存储角色面板的RectTransform引用
        /// </summary>
        [SerializeField]
        private RectTransform _characterpanel = null;

        /// <summary>
        /// 获取角色面板RectTransform的公共属性
        /// </summary>
        public RectTransform characterPanel => _characterpanel;
        
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

            // 解析角色名称和扮演ID，获取名称数据数组
            string[] nameData = characterName.Split(CHARACTER_CASTING_ID, StringSplitOptions.RemoveEmptyEntries);
            result.name = nameData[0];
            result.castingName = nameData.Length > 1 ? nameData[1] : result.name;

            // 根据扮演名称获取角色配置信息
            result.config = config.GetConfig(result.castingName);

            // 获取角色对应的预制体资源
            result.prefab = GetPrefabForCharacter(result.castingName);
            
            return result;
        }

        /// <summary>
        /// 加载指定角色的预制体资源
        /// </summary>
        /// <param name="characterName">角色名称</param>
        /// <returns>加载到的角色预制体对象，若路径无效则返回null</returns>
        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPath, characterName);
            // Debug.Log($"Prefab path: '{prefabPath}'");
            return Resources.Load<GameObject>(prefabPath);
        }

        /// <summary>
        /// 替换路径字符串中的占位符为实际角色名称
        /// </summary>
        /// <param name="path">原始路径模板</param>
        /// <param name="characterName">替换用的角色名称</param>
        /// <returns>格式化后的完整路径</returns>
        private string FormatCharacterPath(string path, string characterName) =>
            path.Replace(CHARACTER_NAME_ID, characterName);

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
                    return new Character_Sprite(info.name, config, info.prefab);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, config, info.prefab);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, config, info.prefab);

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
            /// 存储投射名称的公共字段
            /// </summary>
            public string castingName = "";

            /// <summary>
            /// 角色配置数据
            /// </summary>
            public CharacterConfigData config = null;

            /// <summary>
            /// 角色使用的预制体资源
            /// </summary>
            public GameObject prefab = null;
        }
    }
}

