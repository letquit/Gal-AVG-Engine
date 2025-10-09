using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using TMPro;
using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 角色基类，定义了所有角色类型的通用属性和功能
    /// </summary>
    public abstract class Character
    {
        /// <summary>
        /// 启动时是否启用的常量配置
        /// </summary>
        public const bool ENABLE_ON_START = true;

        
        /// <summary>
        /// 角色名称
        /// </summary>
        public string name = "";
        
        /// <summary>
        /// 角色显示名称，用于在对话中展示的角色名字
        /// </summary>
        public string displayName = "";

        /// <summary>
        /// 角色在UI中的根节点变换组件
        /// </summary>
        public RectTransform root = null;

        /// <summary>
        /// 角色配置数据，包含与该角色相关的样式和行为设置
        /// </summary>
        public CharacterConfigData config;

        /// <summary>
        /// 角色动画控制器组件
        /// </summary>
        public Animator animator;
        
        /// <summary>
        /// 获取角色管理器实例的引用
        /// </summary>
        protected CharacterManager manager => CharacterManager.instance;
        
        /// <summary>
        /// 获取全局对话系统实例的引用
        /// </summary>
        public DialogueSystem dialogueSystem => DialogueSystem.instance;
        
        /// <summary>
        /// 当前正在进行的揭示协程
        /// </summary>
        protected Coroutine co_revealing;

        /// <summary>
        /// 当前正在进行的隐藏协程
        /// </summary>
        protected Coroutine co_hiding;

        /// <summary>
        /// 当前正在进行的移动协程
        /// </summary>
        protected Coroutine co_moving;

        /// <summary>
        /// 指示角色是否正在揭示过程中
        /// </summary>
        public bool isRevealing => co_revealing != null;

        /// <summary>
        /// 指示角色是否正在隐藏过程中
        /// </summary>
        public bool isHiding => co_hiding != null;
        
        /// <summary>
        /// 指示角色是否正在移动过程中
        /// </summary>
        public bool isMoving => co_moving != null;

        /// <summary>
        /// 获取或设置一个值，该值指示当前对象是否可见。
        /// </summary>
        public virtual bool isVisible { get; set; }


        /// <summary>
        /// 初始化角色对象
        /// </summary>
        /// <param name="name">角色名称</param>
        /// <param name="config">角色配置数据</param>
        /// <param name="prefab">角色预制体对象</param>
        public Character(string name, CharacterConfigData config, GameObject prefab)
        {
            this.name = name;
            displayName = name;
            this.config = config;

            if (prefab != null)
            {
                GameObject ob = Object.Instantiate(prefab, manager.characterPanel);
                ob.name = manager.FormatCharacterPath(manager.characterPrefabNameFormat, name);
                ob.SetActive(true);
                root = ob.GetComponent<RectTransform>();
                animator = root.GetComponentInChildren<Animator>();
            }
        }
        
        /// <summary>
        /// 让角色说出一段对话（单句）
        /// </summary>
        /// <param name="dialogue">要显示的对话文本</param>
        /// <returns>返回一个协程对象，可用于控制对话播放过程</returns>
        public Coroutine Say(string dialogue) => Say(new List<string>() { dialogue });

        /// <summary>
        /// 让角色说出多段对话
        /// </summary>
        /// <param name="dialogue">包含多条对话文本的列表</param>
        /// <returns>返回一个协程对象，可用于控制对话播放过程</returns>
        public Coroutine Say(List<string> dialogue)
        {
            // 如果角色有显示名称，则先显示说话者名字
            if (!string.IsNullOrEmpty(displayName))
            {
                dialogueSystem.ShowSpeakerName(displayName);
                UpdateTextCustomizationsOnScreen();
            }
            return dialogueSystem.Say(dialogue);
        }
        
        /// <summary>
        /// 设置角色名称的颜色
        /// </summary>
        /// <param name="color">要应用的颜色值</param>
        public void SetNameColor(Color color) => config.nameColor = color;

        /// <summary>
        /// 设置角色对话文本的颜色
        /// </summary>
        /// <param name="color">要应用的颜色值</param>
        public void SetDialogueColor(Color color) => config.dialogueColor = color;

        /// <summary>
        /// 设置角色名称使用的字体
        /// </summary>
        /// <param name="font">要应用的字体资源</param>
        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;

        /// <summary>
        /// 设置角色对话文本使用的字体
        /// </summary>
        /// <param name="font">要应用的字体资源</param>
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;
        
        /// <summary>
        /// 重置当前角色的配置数据为默认配置
        /// </summary>
        public void ResetConfigurationData() => config = CharacterManager.instance.GetCharacterConfig(name);
        
        /// <summary>
        /// 更新屏幕上当前角色的文本自定义样式
        /// </summary>
        public void UpdateTextCustomizationsOnScreen() => dialogueSystem.ApplySpeakerDataToDialogueContainer(config);

        /// <summary>
        /// 显示角色
        /// </summary>
        /// <returns>返回表示显示操作的协程</returns>
        public virtual Coroutine Show()
        {
            if (isRevealing)
                return co_revealing;
            
            if (isHiding)
                manager.StopCoroutine(co_hiding);
            
            co_revealing = manager.StartCoroutine(ShowingOrHiding(true));
            
            return co_revealing;
        }
        
        /// <summary>
        /// 隐藏角色
        /// </summary>
        /// <returns>返回表示隐藏操作的协程</returns>
        public virtual Coroutine Hide()
        {
            if (isHiding)
                return co_hiding;
            
            if (isRevealing)
                manager.StopCoroutine(co_revealing);
            
            co_hiding = manager.StartCoroutine(ShowingOrHiding(false));
            
            return co_hiding;
        }

        /// <summary>
        /// 控制角色显示或隐藏的核心协程方法，需由派生类实现具体逻辑
        /// </summary>
        /// <param name="show">true 表示显示角色；false 表示隐藏角色</param>
        /// <returns>IEnumerator 类型，供协程使用</returns>
        public virtual IEnumerator ShowingOrHiding(bool show)
        {
            Debug.Log("Show/Hide cannot be called from a base character type.");
            yield return null;
        }
        
        /// <summary>
        /// 设置UI元素的位置，通过将目标位置转换为相对角色锚点目标来实现。
        /// </summary>
        /// <param name="position">目标位置（Vector2）</param>
        public virtual void SetPosition(Vector2 position)
        {
            // 如果根节点为空，则直接返回
            if (root == null)
                return;
            
            // 将UI目标位置转换为相对角色锚点的目标值
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);
           
            // 设置锚点范围
            root.anchorMin = minAnchorTarget;
            root.anchorMax = maxAnchorTarget;
        }

        /// <summary>
        /// 启动协程，将UI元素平滑移动到指定位置。
        /// </summary>
        /// <param name="position">目标位置（Vector2）</param>
        /// <param name="speed">移动速度，默认为2f</param>
        /// <param name="smooth">是否使用平滑插值移动，默认为false</param>
        /// <returns>启动的协程对象，如果根节点为空则返回null</returns>
        public virtual Coroutine MoveToPosition(Vector2 position, float speed = 2f, bool smooth = false)
        {
            // 如果根节点为空，无法执行移动操作
            if (root == null)
                return null;
            
            // 如果当前正在移动，先停止之前的协程
            if (isMoving)
                manager.StopCoroutine(co_moving);
            
            // 启动新的移动协程
            co_moving = manager.StartCoroutine(MovingToPosition(position, speed, smooth));
            
            return co_moving;
        }

        /// <summary>
        /// 协程函数，负责实际的移动逻辑，支持平滑或线性移动方式。
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="speed">移动速度</param>
        /// <param name="smooth">是否启用平滑插值</param>
        /// <returns>IEnumerator用于协程执行</returns>
        private IEnumerator MovingToPosition(Vector2 position, float speed, bool smooth)
        {
            // 计算目标锚点范围
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);
            // 保存当前锚点的尺寸（padding）
            Vector2 padding = root.anchorMax - root.anchorMin;
    
            // 保存初始位置（这是关键修改点）
            Vector2 initialMinAnchor = root.anchorMin;
            float elapsedTime = 0f;
    
            // 如果启用平滑模式
            if (smooth)
            {
                // 计算总移动距离，用于确定所需总时间
                float totalDistance = Vector2.Distance(initialMinAnchor, minAnchorTarget);
                float totalTime = totalDistance / (speed * 0.35f); // 调整与MoveTowards相似的速度
        
                // 随着时间推移线性增加t值
                while (elapsedTime < totalTime)
                {
                    // t值随时间线性增长，从0到1
                    float t = elapsedTime / totalTime;
            
                    // 使用初始位置和目标位置进行插值，而不是当前位置
                    root.anchorMin = Vector2.Lerp(initialMinAnchor, minAnchorTarget, t);
                    root.anchorMax = root.anchorMin + padding;
            
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
        
                // 确保最终精确到达目标位置
                root.anchorMin = minAnchorTarget;
                root.anchorMax = maxAnchorTarget;
            }
            else
            {
                // 非平滑模式使用MoveTowards（保持原有逻辑）
                while (root.anchorMin != minAnchorTarget || root.anchorMax != maxAnchorTarget)
                {
                    root.anchorMin = Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed * Time.deltaTime * 0.35f);
                    root.anchorMax = root.anchorMin + padding;
                    yield return null;
                }
            }
    
            // 移动完成日志输出
            Debug.Log("Done moving.");
            co_moving = null;
        }

        /// <summary>
        /// 将UI目标位置转换为相对于角色的锚点目标范围。
        /// </summary>
        /// <param name="position">输入的UI位置（Vector2）</param>
        /// <returns>包含最小锚点和最大锚点的元组</returns>
        protected (Vector2, Vector2) ConvertUITargetPositionToRelativeCharacterAnchorTargets(Vector2 position)
        {
            // 获取当前锚点的尺寸（padding）
            Vector2 padding = root.anchorMax - root.anchorMin;
            
            // 计算可移动的最大比例值
            float maxX = 1f - padding.x;
            float maxY = 1f - padding.y;

            // 计算目标锚点最小值
            Vector2 minAnchorTarget = new Vector2(maxX * position.x, maxY * position.y);
            // 计算目标锚点最大值
            Vector2 maxAnchorTarget = minAnchorTarget + padding;
            
            return (minAnchorTarget, maxAnchorTarget);
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
