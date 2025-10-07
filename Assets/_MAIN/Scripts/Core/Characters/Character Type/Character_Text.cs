using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 文本角色类，继承自Character基类
    /// 用于创建和管理文本类型的角色对象
    /// </summary>
    public class Character_Text : Character
    {
        /// <summary>
        /// 构造函数，初始化文本角色对象
        /// </summary>
        /// <param name="name">角色的名称</param>
        public Character_Text(string name, CharacterConfigData config) : base(name, config, prefab: null)
        {
            Debug.Log($"Created Text Character: '{name}'");
        }
    }
}

