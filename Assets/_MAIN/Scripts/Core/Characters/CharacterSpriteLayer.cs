using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTERS
{
    /// <summary>
    /// 负责管理角色精灵图层的显示与过渡动画。
    /// 此类用于处理角色图像在切换时的淡入淡出效果，并支持多层级渲染。
    /// </summary>
    public class CharacterSpriteLayer
    {
        private CharacterManager characterManager => CharacterManager.instance;
        
        private const float DEFAULT_TRANSITION_SPEED = 3f;
        private float transitionSpeedMultiplier = 1f;
        
        /// <summary>
        /// 当前图层索引编号。
        /// </summary>
        public int layer { get; private set; } = 0;

        /// <summary>
        /// 当前正在使用的图像渲染器组件（Image）。
        /// </summary>
        public Image renderer { get; private set; } = null;

        /// <summary>
        /// 获取当前渲染器对应的 CanvasGroup 组件，用于控制透明度。
        /// </summary>
        public CanvasGroup rendererCG => renderer.GetComponent<CanvasGroup>();
        
        private List<CanvasGroup> oldRenderers = new List<CanvasGroup>();

        private Coroutine co_transitioningLayer = null;
        private Coroutine co_levelingAlpha = null;
        private Coroutine co_changingColor = null;

        /// <summary>
        /// 判断是否正在进行图层过渡动画。
        /// </summary>
        public bool isTransitioningLayer => co_transitioningLayer != null;

        /// <summary>
        /// 判断是否正在进行透明度渐变操作。
        /// </summary>
        public bool isLevelingAlpha => co_levelingAlpha != null;
        
        /// <summary>
        /// 判断是否正在进行颜色变换操作。
        /// </summary>
        public bool isChangingColor =>  co_changingColor != null;
        
        /// <summary>
        /// 构造函数，初始化一个角色精灵图层对象。
        /// </summary>
        /// <param name="defaultRenderer">默认的图像渲染器。</param>
        /// <param name="layer">该图层的层级编号，默认为 0。</param>
        public CharacterSpriteLayer(Image defaultRenderer,int layer= 0)
        {
            renderer = defaultRenderer;
            this.layer = layer;
        }

        /// <summary>
        /// 设置当前图层所显示的精灵图片。
        /// </summary>
        /// <param name="sprite">要设置的新精灵图片。</param>
        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        /// <summary>
        /// 平滑地将当前图层的精灵替换为目标精灵，并播放过渡动画。
        /// </summary>
        /// <param name="sprite">目标精灵图片。</param>
        /// <param name="speed">过渡速度倍数，默认为 1。</param>
        /// <returns>启动的协程实例，如果未执行则返回 null。</returns>
        public Coroutine TransitionSprite(Sprite sprite, float speed = 1)
        {
            // 若新旧精灵相同，则无需进行过渡
            if (sprite == renderer.sprite)
                return null;
            
            // 如果已有过渡协程运行中，则停止它
            if (isTransitioningLayer)
                characterManager.StopCoroutine(co_transitioningLayer);

            // 启动新的过渡协程并记录引用
            co_transitioningLayer = characterManager.StartCoroutine(TransitioningSprite(sprite, speed));
            
            return co_transitioningLayer;
        }

        /// <summary>
        /// 执行实际的精灵过渡逻辑：创建新渲染器、设置精灵并开始透明度变化过程。
        /// </summary>
        /// <param name="sprite">需要过渡到的目标精灵。</param>
        /// <param name="speedMultiplier">过渡的速度系数。</param>
        /// <returns>IEnumerator 类型，供 Unity 协程使用。</returns>
        private IEnumerator TransitioningSprite(Sprite sprite, float speedMultiplier)
        {
            transitionSpeedMultiplier = speedMultiplier;
            
            // 创建一个新的渲染器作为过渡目标
            Image newRenderer = CreateRenderer(renderer.transform.parent);
            newRenderer.sprite = sprite;

            // 尝试启动透明度渐变协程
            yield return TryStartLevelingAlphas();
            
            co_transitioningLayer = null;
        }

        /// <summary>
        /// 克隆当前渲染器以实现平滑过渡效果。
        /// 新建的渲染器会替代原渲染器成为当前活动渲染器。
        /// </summary>
        /// <param name="parent">新渲染器挂载的父级 Transform。</param>
        /// <returns>新建的 Image 渲染器组件。</returns>
        private Image CreateRenderer(Transform parent)
        {
            Image newRenderer = Object.Instantiate(renderer, parent);
            oldRenderers.Add(rendererCG);
            
            newRenderer.name = renderer.name;
            renderer = newRenderer;
            renderer.gameObject.SetActive(true);
            rendererCG.alpha = 0;
            
            return newRenderer;
        }

        /// <summary>
        /// 检查是否存在正在进行中的透明度调整协程，若无则尝试启动一个新的。
        /// </summary>
        /// <returns>已存在的或新启动的协程引用。</returns>
        private Coroutine TryStartLevelingAlphas()
        {
            if (isLevelingAlpha)
                return co_levelingAlpha;
            
            co_levelingAlpha = characterManager.StartCoroutine(RunAlphaLeveling());
            
            return co_levelingAlpha;
        }
        
        /// <summary>
        /// 实际执行所有相关渲染器透明度渐变的核心循环。
        /// 包括当前渲染器逐渐显现以及旧渲染器逐步消失的过程。
        /// </summary>
        /// <returns>IEnumerator 类型，供 Unity 协程使用。</returns>
        private IEnumerator RunAlphaLeveling()
        {
            // 循环直到当前渲染器完全可见且所有旧渲染器都不可见
            while (rendererCG.alpha < 1 || oldRenderers.Any(oldCG => oldCG.alpha > 0))
            {
                float speed = Time.deltaTime * DEFAULT_TRANSITION_SPEED * transitionSpeedMultiplier;

                // 增加当前渲染器的透明度至 1
                rendererCG.alpha = Mathf.MoveTowards(rendererCG.alpha, 1, speed);

                // 遍历旧渲染器列表，降低其透明度直至销毁
                for (int i = oldRenderers.Count - 1; i >= 0; i--)
                {
                    CanvasGroup oldCG = oldRenderers[i];
                    oldCG.alpha = Mathf.MoveTowards(oldCG.alpha, 0, speed);

                    // 如果某个旧渲染器已经完全不可见，则从列表移除并销毁对应 GameObject
                    if (oldCG.alpha <= 0)
                    {
                        oldRenderers.RemoveAt(i);
                        Object.Destroy(oldCG.gameObject);
                    }
                }
                
                yield return null;
            }
            
            co_levelingAlpha = null;
        }

        /// <summary>
        /// 设置当前及历史渲染器的颜色。
        /// </summary>
        /// <param name="color">要应用的颜色值。</param>
        public void SetColor(Color color)
        {
            renderer.color = color;

            foreach (CanvasGroup oldCG in oldRenderers)
            {
                oldCG.GetComponent<Image>().color = color;
            }
        }

        /// <summary>
        /// 对当前及历史渲染器执行颜色过渡动画。
        /// </summary>
        /// <param name="color">目标颜色。</param>
        /// <param name="speed">颜色过渡速度倍率。</param>
        /// <returns>启动的颜色过渡协程。</returns>
        public Coroutine TransitionColor(Color color, float speed)
        {
            if (isChangingColor)
                characterManager.StopCoroutine(co_changingColor);

            co_changingColor = characterManager.StartCoroutine(ChangingColor(color, speed));
            
            return co_changingColor;
        }
        
        /// <summary>
        /// 实现颜色渐变的具体逻辑。
        /// 在指定时间内线性插值当前和历史渲染器的颜色。
        /// </summary>
        /// <param name="color">目标颜色。</param>
        /// <param name="speedMultiplier">过渡速度乘数。</param>
        /// <returns>IEnumerator 类型，供 Unity 协程使用。</returns>
        private IEnumerator ChangingColor(Color color, float speedMultiplier)
        {
            Color oldColor = renderer.color;
            List<Image> oldImages = new List<Image>();

            foreach (var oldCG in oldRenderers)
            {
                oldImages.Add(oldCG.GetComponent<Image>());
            }

            float colorPercent = 0;
            while (colorPercent < 1)
            {
                colorPercent += DEFAULT_TRANSITION_SPEED * speedMultiplier * Time.deltaTime;
                
                renderer.color = Color.Lerp(oldColor, color, colorPercent);

                foreach (Image oldImage in oldImages)
                {
                    oldImage.color = renderer.color;
                }
                
                yield return null;
            }
            
            co_changingColor = null;
        }
    }
}
