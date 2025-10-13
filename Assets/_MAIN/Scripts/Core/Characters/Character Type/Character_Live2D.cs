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
        /// 获取或设置对象的可见状态
        /// </summary>
        /// <remarks>
        /// 可见性由两个条件决定：是否正在显示(isRevealing) 或者 渲染控制器的不透明度大于0
        /// </remarks>
        public override bool isVisible
        {
            get => isRevealing || renderController.Opacity > 0;
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
        /// 改变模型颜色的协程方法
        /// </summary>
        /// <param name="speed">颜色变换速度</param>
        /// <returns>IEnumerator协程迭代器</returns>
        public override IEnumerator ChangingColor(float speed)
        {
            // 调用底层颜色变换逻辑
            yield return ChangingColorL2D(speed);
            
            // 清空颜色变换协程引用
            co_changingColor = null;
        }
        
        /// <summary>
        /// 高亮显示的协程方法
        /// </summary>
        /// <param name="speedMultiplier">速度倍数</param>
        /// <returns>IEnumerator协程迭代器</returns>
        public override IEnumerator Highlighting(float speedMultiplier)
        {
            // 如果当前没有进行颜色变换，则执行颜色变换
            if (!isChangingColor)
                yield return ChangingColorL2D(speedMultiplier);
            
            // 清空高亮协程引用
            co_highlighting = null;
        }
        
        /// <summary>
        /// Live2D模型颜色变换的核心实现逻辑
        /// </summary>
        /// <param name="speed">变换速度</param>
        /// <returns>IEnumerator协程迭代器</returns>
        private IEnumerator ChangingColorL2D(float speed)
        {
            // 获取所有渲染器组件
            CubismRenderer[] renderers = renderController.Renderers;
            // 记录起始颜色
            Color startColor = renderers[0].Color;
            
            // 颜色变换进度百分比
            float colorPrecent = 0;

            // 执行颜色渐变动画
            while (colorPrecent != 1)
            {
                // 根据时间和速度计算当前进度
                colorPrecent = Mathf.Clamp01(colorPrecent + (DEFAULT_TRANSITION_SPEED * speed * Time.deltaTime));
                // 计算当前帧的颜色值
                Color currentColor = Color.Lerp(startColor, displayColor, colorPrecent);

                // 将计算出的颜色应用到所有渲染器
                foreach (CubismRenderer renderer in renderController.Renderers)
                {
                    renderer.Color = currentColor;
                }
                
                // 等待下一帧继续执行
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

        /// <summary>
        /// 当接收到表情表达式时触发的回调方法
        /// </summary>
        /// <param name="layer">表情图层索引</param>
        /// <param name="expression">表情表达式字符串</param>
        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            SetExpression(expression);
        }
    }
}
