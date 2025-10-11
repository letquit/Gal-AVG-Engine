using System;
using System.Collections.Generic;
using System.Linq;
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
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}";
        
        /// <summary>
        /// 获取角色预制体名称的格式化字符串
        /// </summary>
        /// <returns>返回格式化后的角色预制体名称字符串，格式为"Character - [角色名称ID]"</returns>
        public string characterPrefabNameFormat => $"Character - [{CHARACTER_NAME_ID}]";
        
        /// <summary>
        /// 获取角色预制体路径的属性
        /// </summary>
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}";

        /// <summary>
        /// 序列化字段，用于存储角色面板的RectTransform引用
        /// </summary>
        [SerializeField]
        private RectTransform _characterpanel = null;
        
        /// <summary>
        /// 序列化字段，用于引用Live2D角色面板的RectTransform组件
        /// </summary>
        [SerializeField]
        private RectTransform _characterpanel_live2D = null;
        
        /// <summary>
        /// 序列化字段，用于引用3D模型角色面板的RectTransform组件
        /// </summary>
        [SerializeField]
        private RectTransform _characterpanel_model3D = null;

        /// <summary>
        /// 获取角色面板RectTransform的公共属性
        /// </summary>
        public RectTransform characterPanel => _characterpanel;
        
        /// <summary>
        /// 获取角色面板Live2D的RectTransform引用
        /// </summary>
        public RectTransform characterPanelLive2D => _characterpanel_live2D;
        
        /// <summary>
        /// 获取角色面板3D模型的RectTransform引用
        /// </summary>
        public RectTransform characterPanelModel3D => _characterpanel_model3D;
        
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
            
            // 添加角色实例到字典中
            characters.Add(info.name.ToLower(), character);
            
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
            
            // 设置角色根目录文件夹路径
            // 通过格式化字符串和角色名称生成完整的角色根目录路径
            result.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName);
            
            
            return result;
        }

        /// <summary>
        /// 加载指定角色的预制体资源
        /// </summary>
        /// <param name="characterName">角色名称</param>
        /// <returns>加载到的角色预制体对象，若路径无效则返回null</returns>
        private GameObject GetPrefabForCharacter(string characterName)
        {
            // 首先尝试从配置中获取直接引用的预制体
            CharacterConfigData configData = config.GetConfig(characterName);
            if (configData.characterPrefab != null)
            {
                // Debug.Log("Using direct prefab reference for character: " + characterName);
                return configData.characterPrefab;
            }
    
            // 否则使用路径查找机制
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            // Debug.Log($"Prefab path: '{prefabPath}'");
            return Resources.Load<GameObject>(prefabPath);
        }

        /// <summary>
        /// 替换路径字符串中的占位符为实际角色名称
        /// </summary>
        /// <param name="path">原始路径模板</param>
        /// <param name="characterName">替换用的角色名称</param>
        /// <returns>格式化后的完整路径</returns>
        public string FormatCharacterPath(string path, string characterName) =>
            path.Replace(CHARACTER_NAME_ID, characterName);
        
        /// <summary>
        /// 根据角色信息创建对应类型的角色对象
        /// </summary>
        /// <param name="info">包含角色配置信息、预制体和文件路径的CHARACTER_INFO对象</param>
        /// <returns>根据角色类型创建的具体角色对象，如果类型不匹配则返回null</returns>
        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;
            
            // 根据角色配置中的角色类型创建对应的角色实例
            switch (config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config);

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, config, info.prefab, info.rootCharacterFolder);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, config, info.prefab, info.rootCharacterFolder);

                default:
                    return null;
            }
        }

        /// <summary>
        /// 对所有角色按照优先级进行排序，活跃角色优先，非活跃角色保持原有顺序
        /// </summary>
        public void SortCharacters()
        {
            // 分离活跃角色和非活跃角色
            List<Character> activeCharacters = characters.Values.Where(c => c.root.gameObject.activeInHierarchy && c.isVisible).ToList();
            List<Character> inactiveCharacters = characters.Values.Except(activeCharacters).ToList();
            
            // 按优先级对活跃角色进行排序
            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));
            activeCharacters.Concat(inactiveCharacters);
            
            SortCharacters(activeCharacters);
        }

        /// <summary>
        /// 根据指定的角色名称数组对角色进行排序，指定的角色将获得更高的优先级
        /// </summary>
        /// <param name="characterNames">需要优先排序的角色名称数组</param>
        public void SortCharacters(string[] characterNames)
        {
            List<Character> sortedCharacters = new List<Character>();

            // 根据名称获取角色对象
            sortedCharacters = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();
            
            // 获取未指定排序的角色并按优先级排序
            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacters)
                .OrderBy(character => character.priority)
                .ToList();
            
            // 反转指定排序的角色列表
            sortedCharacters.Reverse();

            // 为指定排序的角色设置新的优先级
            int startingPriority = remainingCharacters.Count > 0 ? remainingCharacters.Max(c => c.priority) : 0;
            for (int i = 0; i < sortedCharacters.Count; i++)
            {
                Character character = sortedCharacters[i];
                character.SetPriority(startingPriority + i + 1, false);
            }
            
            // 合并所有角色并进行排序
            List<Character> allCharacters = remainingCharacters.Concat(sortedCharacters).ToList();
            SortCharacters(allCharacters);
        }

        /// <summary>
        /// 根据角色列表设置角色在层级中的显示顺序
        /// </summary>
        /// <param name="charactersSortingOrder">按排序顺序排列的角色列表</param>
        private void SortCharacters(List<Character> charactersSortingOrder)
        {
            // 按顺序设置每个角色的层级索引和排序优先级
            int i = 0;
            foreach (Character character in charactersSortingOrder)
            {
                // Debug.Log($"{character.name} priority is {character.priority}");
                character.root.SetSiblingIndex(i++);
                character.OnSort(i);
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
            /// 根角色文件夹路径
            /// </summary>
            public string rootCharacterFolder = "";

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

