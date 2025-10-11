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
        private CubismRenderController renderController;
        private CubismExpressionController expressionController;
        private Animator motionAnimator;
        
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
    }
}
