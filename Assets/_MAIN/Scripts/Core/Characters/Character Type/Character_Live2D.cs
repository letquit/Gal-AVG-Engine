using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Rendering;
using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// Live2D角色类，继承自Character基类，用于控制Live2D动画角色
    /// </summary>
    public class Character_Live2D : Character
    {
        /// <summary>
        /// 默认的过渡动画速度
        /// </summary>
        public const float DEFAULT_TRANSITION_SPEED = 3f;

        /// <summary>
        /// 角色渲染时的排序深度间隔
        /// </summary>
        public const int CHARCTER_SORTING_DEPTH_SIZE = 250;
        
        private CubismRenderController renderController;
        private CubismExpressionController expressionController;
        private Animator motionAnimator;
        
        private List<CubismRenderController> oldRenderers = new List<CubismRenderController>();

        private float xScale;

        /// <summary>
        /// 获取或设置角色是否可见。当正在显示或隐藏过程中，或者透明度为1时返回true。
        /// </summary>
        public override bool isVisible
        {
            get => isRevealing || renderController.Opacity == 1;
            set => renderController.Opacity = value ? 1 : 0;
        }
        
        /// <summary>
        /// 构造函数，初始化Live2D角色实例
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <param name="config">角色配置数据</param>
        /// <param name="prefab">角色预制体对象</param>
        /// <param name="rootAssetsFolder">资源根目录路径</param>
        public Character_Live2D(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {
            // 获取动作动画控制器组件
            motionAnimator = animator.transform.GetChild(0).GetComponentInChildren<Animator>();
            renderController = motionAnimator.GetComponent<CubismRenderController>();
            expressionController = motionAnimator.GetComponent<CubismExpressionController>();

            xScale = renderController.transform.localScale.x;
        }

        /// <summary>
        /// 设置并播放指定的动作动画
        /// </summary>
        /// <param name="animationName">要播放的动画名称</param>
        public void SetMotion(string animationName)
        {
            motionAnimator.Play(animationName);
        }

        /// <summary>
        /// 根据表情索引设置当前角色的表情
        /// </summary>
        /// <param name="expressionIndex">目标表情在表情列表中的索引</param>
        public void SetExpression(int expressionIndex)
        {
            expressionController.CurrentExpressionIndex = expressionIndex;
        }
        
        /// <summary>
        /// 根据表情名称设置当前角色的表情
        /// </summary>
        /// <param name="expressionName">目标表情的名称（不区分大小写）</param>
        public void SetExpression(string expressionName)
        {
            expressionController.CurrentExpressionIndex = GetExpressionIndexByName(expressionName);
        }

        /// <summary>
        /// 根据表情名称查找其在表情列表中的索引
        /// </summary>
        /// <param name="expressionName">要查找的表情名称（不区分大小写）</param>
        /// <returns>找到则返回对应索引，未找到则返回-1</returns>
        private int GetExpressionIndexByName(string expressionName)
        {
            expressionName = expressionName.ToLower();

            // 遍历所有表情对象，匹配名称（去除文件扩展名后比较）
            for (int i = 0; i < expressionController.ExpressionsList.CubismExpressionObjects.Length; i++)
            {
                CubismExpressionData expr = expressionController.ExpressionsList.CubismExpressionObjects[i];
                if (expr.name.Split('.')[0] == expressionName)
                    return i;
            }
            
            return -1;
        }

        /// <summary>
        /// 显示或隐藏角色的协程方法，通过渐变透明度实现淡入淡出效果
        /// </summary>
        /// <param name="show">true表示显示角色，false表示隐藏角色</param>
        /// <returns>IEnumerator类型的协程迭代器</returns>
        public override IEnumerator ShowingOrHiding(bool show)
        {
            float targetAlpha = show ? 1 : 0;

            while (renderController.Opacity != targetAlpha)
            {
                renderController.Opacity = Mathf.MoveTowards(renderController.Opacity, targetAlpha,
                    DEFAULT_TRANSITION_SPEED * Time.deltaTime);
                yield return null;
            }
            
            co_revealing = null;
            co_hiding = null;
        }

        /// <summary>
        /// 设置角色整体颜色
        /// </summary>
        /// <param name="color">要设置的颜色值</param>
        public override void SetColor(Color color)
        {
            base.SetColor(color);

            foreach (CubismRenderer renderer in renderController.Renderers)
            {
                renderer.Color = color;
            }
        }

        /// <summary>
        /// 改变角色颜色的协程方法
        /// </summary>
        /// <param name="color">目标颜色</param>
        /// <param name="speed">颜色变化的速度倍数</param>
        /// <returns>IEnumerator类型的协程迭代器</returns>
        public override IEnumerator ChangingColor(Color color, float speed)
        {
            yield return ChangingColorL2D(color, speed);
            
            co_changingColor = null;
        }

        /// <summary>
        /// 高亮角色颜色的协程方法
        /// </summary>
        /// <param name="highlight">是否启用高亮</param>
        /// <param name="speedMultiplier">颜色变化速度倍数</param>
        /// <returns>IEnumerator类型的协程迭代器</returns>
        public override IEnumerator Highlighting(bool highlight, float speedMultiplier)
        {
            Color targetColor = displayColor;

            yield return ChangingColorL2D(targetColor, speedMultiplier);
            
            co_highlighting = null;
        }

        /// <summary>
        /// 实际执行颜色变化逻辑的内部协程方法
        /// </summary>
        /// <param name="targetColor">目标颜色</param>
        /// <param name="speed">颜色变化速度倍数</param>
        /// <returns>IEnumerator类型的协程迭代器</returns>
        private IEnumerator ChangingColorL2D(Color targetColor, float speed)
        {
            CubismRenderer[] renderers = renderController.Renderers;
            Color startColor = renderers[0].Color;
            
            float colorPrecent = 0;

            while (colorPrecent != 1)
            {
                colorPrecent = Mathf.Clamp01(colorPrecent + (DEFAULT_TRANSITION_SPEED * speed * Time.deltaTime));
                Color currentColor = Color.Lerp(startColor, targetColor, colorPrecent);

                foreach (CubismRenderer renderer in renderController.Renderers)
                {
                    renderer.Color = currentColor;
                }
                
                yield return null;
            }
        }

        /// <summary>
        /// 控制角色面向方向的协程方法，通过创建新控制器并进行透明度过渡实现翻转效果
        /// </summary>
        /// <param name="faceLeft">true表示面向左侧，false表示面向右侧</param>
        /// <param name="speedMultiplier">过渡速度倍数</param>
        /// <param name="immediate">是否立即完成（当前未使用）</param>
        /// <returns>IEnumerator类型的协程迭代器</returns>
        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            GameObject newLive2DCharacter = CreateNewCharacterController();
            newLive2DCharacter.transform.localScale = new Vector3(faceLeft ? xScale : -xScale, newLive2DCharacter.transform.localScale.y, newLive2DCharacter.transform.localScale.z);
            renderController.Opacity = 0;
            float transitionSpeed = DEFAULT_TRANSITION_SPEED * speedMultiplier * Time.deltaTime;

            while (renderController.Opacity < 1 || oldRenderers.Any(r => r.Opacity > 0))
            {
                renderController.Opacity = Mathf.MoveTowards(renderController.Opacity, 1, transitionSpeed);

                foreach (CubismRenderController oldRenderer in oldRenderers)
                    oldRenderer.Opacity = Mathf.MoveTowards(oldRenderer.Opacity, 0, transitionSpeed);
                
                yield return null;
            }

            foreach (CubismRenderController r in oldRenderers)
                Object.Destroy(r.gameObject);
            
            oldRenderers.Clear();

            co_flipping = null;
        }

        /// <summary>
        /// 创建新的角色控制器实例，并将其添加到旧渲染器列表中
        /// </summary>
        /// <returns>新创建的角色控制器游戏对象</returns>
        private GameObject CreateNewCharacterController()
        {
            oldRenderers.Add(renderController);

            GameObject newLive2DCharacter =
                Object.Instantiate(renderController.gameObject, renderController.transform.parent);
            newLive2DCharacter.name = name;
            renderController = newLive2DCharacter.GetComponent<CubismRenderController>();
            expressionController = newLive2DCharacter.GetComponent<CubismExpressionController>();
            motionAnimator = newLive2DCharacter.GetComponent<Animator>();
            
            return newLive2DCharacter;
        }

        /// <summary>
        /// 当角色层级排序发生变化时调用，更新渲染顺序
        /// </summary>
        /// <param name="sortingIndex">排序索引</param>
        public override void OnSort(int sortingIndex)
        {
            renderController.SortingOrder = sortingIndex * CHARCTER_SORTING_DEPTH_SIZE;
        }
    }
}
