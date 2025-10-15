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
        public const bool ENABLE_ON_START = false;

        /// <summary>
        /// 未高亮状态下的暗化强度常量值
        /// </summary>
        private const float UNHIGHLIGHTED_DARKEN_STRENGTH = 0.65f;
        
        /// <summary>
        /// 默认方向是否朝左的常量定义
        /// </summary>
        public const bool DEFAULT_ORIENTATION_IS_FACING_LEFT = true;
        
        /// <summary>
        /// 动画刷新触发器的常量定义，用于控制动画状态机中的刷新操作
        /// </summary>
        public const string ANIMATION_REFRESH_TRIGGER = "Refresh";
        
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
        /// 获取或设置颜色属性
        /// </summary>
        public Color color { get; private set; } = Color.white;
        /// <summary>
        /// 获取当前显示的颜色，根据高亮状态返回对应的高亮或非高亮颜色
        /// </summary>
        protected Color displayColor => highlighted ? highlightedColor : unhighlightedColor;

        /// <summary>
        /// 获取高亮状态下的颜色
        /// </summary>
        protected Color highlightedColor => color;
        
        /// <summary>
        /// 获取非高亮状态下的颜色，通过对原始颜色的RGB分量进行暗化处理得到
        /// </summary>
        protected Color unhighlightedColor => new Color(color.r * UNHIGHLIGHTED_DARKEN_STRENGTH, color.g * UNHIGHLIGHTED_DARKEN_STRENGTH, color.b * UNHIGHLIGHTED_DARKEN_STRENGTH, color.a);
        
        /// <summary>
        /// 获取或设置当前是否处于高亮状态
        /// </summary>
        public bool highlighted { get; protected set; } = true;
        
        /// <summary>
        /// 面向左侧的默认方向
        /// </summary>
        protected bool facingLeft = DEFAULT_ORIENTATION_IS_FACING_LEFT;
        
        /// <summary>
        /// 获取或设置优先级属性
        /// </summary>
        public int priority { get; protected set; }

        
        /// <summary>
        /// 获取角色管理器实例的引用
        /// </summary>
        protected CharacterManager characterManager => CharacterManager.instance;
        
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
        /// 协程变量，用于控制颜色变化的协程执行
        /// </summary>
        protected Coroutine co_changingColor;
        
        /// <summary>
        /// 协程变量，用于控制高亮效果的协程执行
        /// </summary>
        protected Coroutine co_highlighting;

        /// <summary>
        /// 协程对象，用于控制翻转动画或操作的执行流程
        /// </summary>
        protected Coroutine co_flipping;

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
        /// 获取一个布尔值，指示是否正在执行颜色变化操作
        /// </summary>
        public bool isChangingColor => co_changingColor != null;

        /// <summary>
        /// 获取当前是否处于高亮状态的标识
        /// </summary>
        public bool isHighlighting => (highlighted && co_highlighting != null);

        /// <summary>
        /// 获取当前是否处于取消高亮状态的标识
        /// </summary>
        public bool isUnHighlighting => (!highlighted && co_highlighting != null);


        /// <summary>
        /// 获取或设置一个值，该值指示当前对象是否可见。
        /// </summary>
        public virtual bool isVisible { get; set; }

        /// <summary>
        /// 获取角色是否面向左方
        /// </summary>
        public bool isFacingLeft => facingLeft;
        
        /// <summary>
        /// 获取角色是否面向右方
        /// </summary>
        public bool isFacingRight => !facingLeft;
        
        /// <summary>
        /// 获取角色是否正在翻转
        /// </summary>
        public bool isFlipping => co_flipping != null;

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
                // 根据角色类型确定父面板
                // Transform parentPanel = (config.characterType == CharacterType.Live2D ? characterManager.characterPanelLive2D : characterManager.characterPanel);
                Transform parentPanel = null;
                
                switch (config.characterType)
                {
                    case CharacterType.Sprite:
                    case CharacterType.SpriteSheet:
                        parentPanel = characterManager.characterPanel;
                        break;
                    case CharacterType.Live2D:
                        parentPanel = characterManager.characterPanelLive2D;
                        break;
                    case CharacterType.Model3D:
                        parentPanel = characterManager.characterPanelModel3D;
                        break;
                }
                
                // 实例化角色预制体，并设置其父节点为角色面板
                GameObject ob = Object.Instantiate(prefab, parentPanel);
                // 设置角色名称
                ob.name = characterManager.FormatCharacterPath(characterManager.characterPrefabNameFormat, name);
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
        /// <param name="speedMultiplier">显示速度的倍数，默认值为1f</param>
        /// <returns>返回表示显示操作的协程</returns>
        public virtual Coroutine Show(float speedMultiplier = 1f)
        {
            // 如果正在显示中，则直接返回当前显示协程
            if (isRevealing)
                return co_revealing;
            
            // 如果正在隐藏中，则停止隐藏协程
            if (isHiding)
                characterManager.StopCoroutine(co_hiding);
            
            // 启动显示协程并保存引用
            co_revealing = characterManager.StartCoroutine(ShowingOrHiding(true, speedMultiplier));
            
            return co_revealing;
        }
        
        /// <summary>
        /// 隐藏角色
        /// </summary>
        /// <param name="speedMultiplier">隐藏速度的倍数，默认为1f</param>
        /// <returns>返回表示隐藏操作的协程</returns>
        public virtual Coroutine Hide(float speedMultiplier = 1f)
        {
            // 如果正在隐藏，则直接返回当前隐藏协程
            if (isHiding)
                return co_hiding;
            
            // 如果正在显示，则停止显示协程
            if (isRevealing)
                characterManager.StopCoroutine(co_revealing);
            
            // 启动隐藏协程并保存引用
            co_hiding = characterManager.StartCoroutine(ShowingOrHiding(false, speedMultiplier));
            
            return co_hiding;
        }

        /// <summary>
        /// 控制角色显示或隐藏的核心协程方法，需由派生类实现具体逻辑 
        /// </summary>
        /// <param name="show">true 表示显示角色；false 表示隐藏角色</param>
        /// <param name="speedMultiplier">速度倍数，用于控制显示/隐藏动画的速度</param>
        /// <returns>IEnumerator 类型，供协程使用</returns>
        public virtual IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            // 记录基础角色类型无法执行显示/隐藏操作的日志信息
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
                characterManager.StopCoroutine(co_moving);
            
            // 启动新的移动协程
            co_moving = characterManager.StartCoroutine(MovingToPosition(position, speed, smooth));
            
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
        /// 设置颜色
        /// </summary>
        /// <param name="color">要设置的颜色</param>
        public virtual void SetColor(Color color)
        {
            this.color = color;
        }

        /// <summary>
        /// 过渡颜色变化
        /// </summary>
        /// <param name="color">目标颜色</param>
        /// <param name="speed">颜色变化速度，默认为1f</param>
        /// <returns>颜色变化协程</returns>
        public Coroutine TransitionColor(Color color, float speed = 1f)
        {
            this.color = color;
            
            // 如果正在变色，则停止之前的协程
            if (isChangingColor)
                characterManager.StopCoroutine(co_changingColor);
            
            // 启动新的变色协程
            co_changingColor = characterManager.StartCoroutine(ChangingColor(speed));
            
            return co_changingColor;
        }
        
        /// <summary>
        /// 颜色变化协程
        /// </summary>
        /// <param name="speed">变化速度</param>
        /// <returns>枚举器</returns>
        public virtual IEnumerator ChangingColor(float speed)
        {
            Debug.Log("Color changing is not applicable on this character type!");
            yield return null;
        }
        
        /// <summary>
        /// 启动高亮显示效果的协程
        /// </summary>
        /// <param name="speed">高亮变化的速度倍数，默认为1f</param>
        /// <param name="immediate">是否立即执行高亮效果，默认为false</param>
        /// <returns>返回正在执行的高亮协程对象</returns>
        public Coroutine Highlight(float speed = 1f, bool immediate = false)
        {
            // 如果正在高亮过程中，则直接返回当前协程
            if (isHighlighting)
                return co_highlighting;
            
            // 如果正在取消高亮，则停止当前协程
            if (isUnHighlighting)
                characterManager.StopCoroutine(co_highlighting);

            // 设置高亮状态并启动高亮协程
            highlighted = true;
            co_highlighting = characterManager.StartCoroutine(Highlighting(speed, immediate));
            
            return co_highlighting;
        }
        
        /// <summary>
        /// 启动取消高亮显示效果的协程
        /// </summary>
        /// <param name="speed">取消高亮变化的速度倍数，默认为1f</param>
        /// <param name="immediate">是否立即取消高亮效果，默认为false</param>
        /// <returns>返回正在执行的取消高亮协程对象</returns>
        public Coroutine UnHighlight(float speed = 1f, bool immediate = false)
        {
            // 如果正在取消高亮过程中，则直接返回当前协程
            if (isUnHighlighting)
                return co_highlighting;
            
            // 如果正在高亮，则停止当前协程
            if (isHighlighting)
                characterManager.StopCoroutine(co_highlighting);

            highlighted = false;
            co_highlighting = characterManager.StartCoroutine(Highlighting(speed, immediate));
            
            return co_highlighting;
        }
        
        /// <summary>
        /// 高亮效果的核心实现协程（虚方法，需要子类重写具体实现）
        /// </summary>
        /// <param name="speedMultiplier">速度倍数</param>
        /// <param name="immediate">是否立即执行</param>
        /// <returns>协程迭代器</returns>
        public virtual IEnumerator Highlighting(float speedMultiplier, bool immediate = false)
        {
            // 输出警告信息，提示当前角色类型不支持高亮效果
            Debug.Log("Highlighting is not available on this character type!");
            // 返回空的协程迭代器
            yield return null;
        }
        
        /// <summary>
        /// 翻转角色面向方向的公共接口方法
        /// </summary>
        /// <param name="speed">翻转速度倍数，默认为1</param>
        /// <param name="immediate">是否立即翻转，默认为false</param>
        /// <returns>控制翻转过程的协程对象</returns>
        public Coroutine Flip(float speed = 1, bool immediate = false)
        {
            if (isFacingLeft)
                return FaceRight(speed, immediate);
            else
                return FaceLeft(speed, immediate);
        }

        /// <summary>
        /// 使角色面向左侧
        /// </summary>
        /// <param name="speed">翻转速度倍数，默认为1</param>
        /// <param name="immediate">是否立即翻转，默认为false</param>
        /// <returns>控制翻转过程的协程对象</returns>
        public Coroutine FaceLeft(float speed = 1, bool immediate = false)
        {
            // 如果正在翻转中，则停止当前翻转协程
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);
            
            facingLeft = true;
            // 启动新的面向方向协程
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));
            
            return co_flipping;
        }
        
        /// <summary>
        /// 使角色面向右侧
        /// </summary>
        /// <param name="speed">翻转速度倍数，默认为1</param>
        /// <param name="immediate">是否立即翻转，默认为false</param>
        /// <returns>控制翻转过程的协程对象</returns>
        public Coroutine FaceRight(float speed = 1, bool immediate = false)
        {
            // 如果正在翻转中，则停止当前翻转协程
            if (isFlipping)
                characterManager.StopCoroutine(co_flipping);
            
            facingLeft = false;
            // 启动新的面向方向协程
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));
            
            return co_flipping;
        }

        /// <summary>
        /// 面向指定方向的虚方法，子类需要重写此方法来实现具体的翻转逻辑
        /// </summary>
        /// <param name="faceLeft">是否面向左侧</param>
        /// <param name="speedMultiplier">翻转速度倍数</param>
        /// <param name="immediate">是否立即翻转</param>
        /// <returns>控制翻转过程的枚举器</returns>
        public virtual IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            Debug.Log("Cannot flip a character of this type!");
            yield return null;
        }

        /// <summary>
        /// 设置优先级，并可选择是否自动对UI上的角色进行排序
        /// </summary>
        /// <param name="priority">优先级数值</param>
        /// <param name="autoSortCharactersOnUI">是否自动对UI上的角色进行排序，默认为true</param>
        public void SetPriority(int priority, bool autoSortCharactersOnUI = true)
        {
            this.priority = priority;
            
            // 如果需要自动排序，则调用角色管理器的排序方法
            if (autoSortCharactersOnUI)
                characterManager.SortCharacters();
        }

        /// <summary>
        /// 触发动画触发器
        /// </summary>
        /// <param name="animation">动画触发器的名称</param>
        public void Animate(string animation)
        {
            animator.SetTrigger(animation);
        }
        
        /// <summary>
        /// 设置动画布尔状态并刷新动画
        /// </summary>
        /// <param name="animation">动画布尔参数的名称</param>
        /// <param name="state">布尔状态值</param>
        public void Animate(string animation, bool state)
        {
            animator.SetBool(animation, state);
            animator.SetTrigger(ANIMATION_REFRESH_TRIGGER);
        }

        /// <summary>
        /// 当执行排序操作时触发的虚方法，可在派生类中重写以实现自定义排序逻辑
        /// </summary>
        /// <param name="sortingIndex">排序索引，用于指定排序的列或字段</param>
        public virtual void OnSort(int sortingIndex)
        {
            return;
        }

        /// <summary>
        /// 当接收到投射表达式时触发的回调方法
        /// </summary>
        /// <param name="layer">投射表达式所在的层级</param>
        /// <param name="expression">投射表达式的字符串内容</param>
        public virtual void OnReceiveCastingExpression(int layer, string expression)
        {
            return;
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
