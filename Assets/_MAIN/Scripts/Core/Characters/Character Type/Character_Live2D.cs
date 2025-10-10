using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// Live2D角色类，继承自Character基类，用于控制Live2D动画角色
    /// </summary>
    public class Character_Live2D : Character
    {
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
        }

        /// <summary>
        /// 设置并播放指定的动作动画
        /// </summary>
        /// <param name="animationName">要播放的动画名称</param>
        public void SetMotion(string animationName)
        {
            motionAnimator.Play(animationName);
        }
    }
}
