using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTERS
{
    /// <summary>
    /// 精灵角色类，继承自Character基类，用于处理基于精灵图像的角色显示和隐藏
    /// </summary>
    public class Character_Sprite : Character
    {
        private const string SPRITE_RENDERERD_PARENT_NAME = "Renderers";
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default";
        private const char SPRITESHEET_TEX_SPRITE_DELIMITTER = '-';
        
        /// <summary>
        /// 获取根对象的CanvasGroup组件的快捷属性
        /// </summary>
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();

        /// <summary>
        /// 存储角色的所有精灵图层
        /// </summary>
        public List<CharacterSpriteLayer> layers = new List<CharacterSpriteLayer>();

        /// <summary>
        /// 角色美术资源的目录路径
        /// </summary>
        private string artAssetsDirectory = "";

        /// <summary>
        /// 获取或设置角色是否可见。当正在显示或透明度为1时认为可见。
        /// </summary>
        public override bool isVisible
        {
            get { return isRevealing || rootCG.alpha == 1f; }
            set { rootCG.alpha = value ? 1f : 0f;}
        }
        
        /// <summary>
        /// 创建一个新的精灵角色实例
        /// </summary>
        /// <param name="name">角色的名称</param>
        /// <param name="config">角色配置数据</param>
        /// <param name="prefab">角色的预制体对象</param>
        /// <param name="rootAssetsFolder">资源根目录路径</param>
        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {
            // 设置根对象的CanvasGroup的透明度
            rootCG.alpha = ENABLE_ON_START ? 1f : 0f;
            artAssetsDirectory = rootAssetsFolder + "/Images";

            GetLayers();

            // Show();
            Debug.Log($"Created Sprite Character: '{name}'");
        }

        /// <summary>
        /// 初始化并获取角色的所有精灵图层
        /// </summary>
        private void GetLayers()
        {
            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERERD_PARENT_NAME);

            if (rendererRoot == null)
                return;

            for (int i = 0; i < rendererRoot.transform.childCount; i++)
            {
                Transform child = rendererRoot.transform.GetChild(i);

                Image rendererImage = child.GetComponentInChildren<Image>();

                if (rendererImage != null)
                {
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                    layers.Add(layer);
                    child.name = $"Layer: {i}";
                }
            }
        }

        /// <summary>
        /// 设置指定图层的精灵图像
        /// </summary>
        /// <param name="sprite">要设置的精灵图像</param>
        /// <param name="layer">目标图层索引，默认为0</param>
        public void SetSprite(Sprite sprite, int layer = 0)
        {
            layers[layer].SetSprite(sprite);
        }

        /// <summary>
        /// 根据名称获取对应的精灵资源
        /// </summary>
        /// <param name="spriteName">精灵名称，支持格式："图集名-精灵名" 或 "精灵名"</param>
        /// <returns>找到的精灵资源，未找到则返回null</returns>
        public Sprite GetSprite(string spriteName)
        {
            if (config.characterType == CharacterType.SpriteSheet)
            {
                string[] data = spriteName.Split(SPRITESHEET_TEX_SPRITE_DELIMITTER);
                string textureName;
                string spriteSearchName;

                // 确定纹理名和查找名（不修改原始 spriteName）
                if (data.Length == 2)
                {
                    textureName = data[0];
                    spriteSearchName = data[1]; // 用新变量存储查找名
                }
                else
                {
                    textureName = SPRITESHEET_DEFAULT_SHEETNAME;
                    spriteSearchName = spriteName; // 保持原始 spriteName 用于查找
                }

                // 统一加载精灵数组
                Sprite[] spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{textureName}");
        
                // 统一检查长度（在 if 分支外执行）
                if (spriteArray.Length == 0)
                    Debug.LogWarning($"Character '{name}' does not have a default art asset called '{textureName}'");
            
                // 统一返回（在 if 分支外执行）
                return Array.Find(spriteArray, sprite => sprite.name == spriteSearchName);
            }
            else
            {
                return Resources.Load<Sprite>($"{artAssetsDirectory}/{spriteName}");
            }
        }

        /// <summary>
        /// 在指定图层上过渡到新的精灵图像
        /// </summary>
        /// <param name="sprite">目标精灵图像</param>
        /// <param name="layer">目标图层索引，默认为0</param>
        /// <param name="speed">过渡速度，默认为1</param>
        /// <returns>过渡动画的协程</returns>
        public Coroutine TransitionSprite(Sprite sprite, int layer = 0, float speed = 1)
        {
            CharacterSpriteLayer spriteLayer = layers[layer];
            
            return spriteLayer.TransitionSprite(sprite, speed);
        }

        /// <summary>
        /// 显示或隐藏角色的协程方法，通过渐变透明度实现淡入淡出效果
        /// </summary>
        /// <param name="show">true表示显示角色，false表示隐藏角色</param>
        /// <returns>IEnumerator协程迭代器</returns>
        public override IEnumerator ShowingOrHiding(bool show)
        {
            // 计算目标透明度值
            float targetAlpha = show ? 1f : 0;
            CanvasGroup self = rootCG;

            // 使用MoveTowards方法平滑过渡透明度直到达到目标值
            while (self.alpha != targetAlpha)
            {
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha, Time.deltaTime * 3f);
                yield return null;
            }
            
            co_revealing = null;
            co_hiding = null;
        }
    }
}
