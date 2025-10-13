using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTERS
{
    /// <summary>
    /// 精灵角色类，继承自Character基类，用于处理基于精灵图像的角色显示和隐藏
    /// </summary>
    public class Character_Sprite : Character
    {
        /// <summary>
        /// 精灵渲染器父对象名称常量
        /// 用于标识包含精灵渲染器组件的父级游戏对象名称
        /// </summary>
        private const string SPRITE_RENDERERD_PARENT_NAME = "Renderers";
        
        /// <summary>
        /// 精灵图集默认表单名称常量
        /// 当未指定具体精灵表单时使用的默认名称
        /// </summary>
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default";
        
        /// <summary>
        /// 精灵图表集纹理精灵分隔符常量
        /// 用于分隔纹理名称和精灵名称的字符分隔符
        /// </summary>
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
        /// 获取或设置控件的可见状态
        /// </summary>
        /// <remarks>
        /// 可见性由两个条件决定：是否正在显示(isRevealing)或根CanvasGroup的透明度是否大于0
        /// 设置可见性时，会直接控制根CanvasGroup的alpha值(1表示完全不透明，0表示完全透明)
        /// </remarks>
        public override bool isVisible
        {
            get { return isRevealing || rootCG.alpha > 0; }
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
            // Debug.Log($"Created Sprite Character: '{name}'");
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
        
        /// <summary>
        /// 设置角色精灵的整体颜色
        /// </summary>
        /// <param name="color">要设置的颜色</param>
        public override void SetColor(Color color)
        {
            base.SetColor(color);
            
            color = displayColor;

            // 为所有图层设置颜色
            foreach (CharacterSpriteLayer layer in layers)
            {
                layer.StopChangingColor();
                layer.SetColor(color);
            }
        }

        /// <summary>
        /// 异步改变角色精灵的颜色
        /// </summary>
        /// <param name="speed">颜色变换速度</param>
        /// <returns>用于协程的IEnumerator对象</returns>
        public override IEnumerator ChangingColor(float speed)
        {
            // 启动所有图层的颜色过渡动画
            foreach (CharacterSpriteLayer layer in layers)
                layer.TransitionColor(displayColor, speed);

            yield return null;

            // 等待所有图层完成颜色变换
            while (layers.Any(l => l.isChangingColor))
                yield return null;
            
            co_changingColor = null;
        }

        /// <summary>
        /// 高亮显示字符精灵的协程函数
        /// </summary>
        /// <param name="speedMultiplier">颜色变换的速度倍数</param>
        /// <returns>返回一个迭代器对象，用于协程执行</returns>
        public override IEnumerator Highlighting(float speedMultiplier)
        {
            Color targetColor = displayColor;

            // 为所有图层启动颜色过渡动画
            foreach (CharacterSpriteLayer layer in layers)
                layer.TransitionColor(targetColor, speedMultiplier);
            
            yield return null;
            
            // 等待所有图层完成颜色变换
            while (layers.Any(l => l.isChangingColor))
                yield return null;
            
            co_highlighting = null;
        }

        /// <summary>
        /// 使角色面向指定方向，并等待翻转动画完成
        /// </summary>
        /// <param name="faceLeft">true表示面向左侧，false表示面向右侧</param>
        /// <param name="speedMultiplier">翻转速度的倍数，默认为1.0</param>
        /// <param name="immediate">是否立即翻转，true表示跳过动画直接完成翻转</param>
        /// <returns>IEnumerator迭代器，用于协程执行</returns>
        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            // 通知所有图层开始面向指定方向
            foreach (CharacterSpriteLayer layer in layers)
            {
                if (faceLeft)
                    layer.FaceLeft(speedMultiplier, immediate);
                else
                    layer.FaceRight(speedMultiplier, immediate);
            }
            
            yield return null;

            // 等待所有图层完成翻转动画
            while (layers.Any(l => l.isFlipping))
                yield return null;
            
            co_flipping = null;
        }

        /// <summary>
        /// 当接收到角色表情变换表达式时的回调处理函数
        /// </summary>
        /// <param name="layer">表情图层索引，用于区分不同层次的表情显示</param>
        /// <param name="expression">表情表达式字符串，用于标识具体要显示的表情</param>
        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            // 根据表达式获取对应的表情精灵图片
            Sprite sprite = GetSprite(expression);

            // 检查获取到的精灵图片是否有效
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite '{expression}' could not be found for character '{name}'");
                return;
            }

            // 执行表情精灵的切换过渡动画
            TransitionSprite(sprite, layer);
        }
    }
}
