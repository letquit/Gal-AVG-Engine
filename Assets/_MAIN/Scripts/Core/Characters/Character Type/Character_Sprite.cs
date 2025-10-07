using System.Collections;
using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 精灵角色类，继承自Character基类，用于处理基于精灵图像的角色显示和隐藏
    /// </summary>
    public class Character_Sprite : Character
    {
        /// <summary>
        /// 获取根对象的CanvasGroup组件的快捷属性
        /// </summary>
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();
        
        /// <summary>
        /// 创建一个新的精灵角色实例
        /// </summary>
        /// <param name="name">角色的名称</param>
        /// <param name="config">角色配置数据</param>
        /// <param name="prefab">角色的预制体对象</param>
        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab) : base(name, config, prefab)
        {
            //通过方法手动控制透明度
            rootCG.alpha = 0;
            // Show();
            Debug.Log($"Created Sprite Character: '{name}'");
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
