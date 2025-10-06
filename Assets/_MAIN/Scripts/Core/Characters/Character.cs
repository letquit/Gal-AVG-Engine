using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 角色基类，定义了所有角色类型的通用属性和功能
    /// </summary>
    public abstract class Character
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string name = "";

        /// <summary>
        /// 角色在UI中的根节点变换组件
        /// </summary>
        public RectTransform root = null;

        /// <summary>
        /// 初始化角色对象
        /// </summary>
        /// <param name="name">角色名称</param>
        public Character(string name)
        {
            this.name = name;
        }
        
        /// <summary>
        /// 角色类型枚举，定义了支持的不同角色表现形式
        /// </summary>
        public enum CharacterType
        {
            /// <summary>
            /// 文本类型角色
            /// </summary>
            Text,
            /// <summary>
            /// 精灵图片类型角色
            /// </summary>
            Sprite,
            /// <summary>
            /// 精灵图集类型角色
            /// </summary>
            SpriteSheet,
            /// <summary>
            /// Live2D类型角色
            /// </summary>
            Live2D,
            /// <summary>
            /// 3D模型类型角色
            /// </summary>
            Model3D
        }
    }
}
