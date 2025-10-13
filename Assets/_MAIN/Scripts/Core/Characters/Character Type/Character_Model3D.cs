using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CHARACTERS
{
    /// <summary>
    /// 表示一个基于3D模型的角色类，继承自Character基类。
    /// 该类负责加载并显示3D角色模型，并支持动画播放和表情控制。
    /// </summary>
    public class Character_Model3D : Character
    {
        /// <summary>
        /// 渲染组预制体名称格式化字符串，用于构建资源路径。
        /// </summary>
        private const string CHARACTER_RENDER_GROUP_PREFAB_NAME_FORMAT = "RenderGroup - [{0}]";

        /// <summary>
        /// 渲染纹理资源名称格式化字符串。
        /// </summary>
        private const string CHARACTER_RENDER_TEXTURE_NAME_FORMAT = "RenderTexture";

        /// <summary>
        /// 角色在场景中的堆叠深度间隔，用于防止模型重叠。
        /// </summary>
        private const int CHARACTER_STACKING_DEPTH = 15;

        /// <summary>
        /// 表达式过渡速度常量，用于控制表情或状态切换的动画速度
        /// </summary>
        private const float EXPRESSION_TRANSITION_SPEED = 100f;
        
        /// <summary>
        /// 默认过渡速度常量，用于控制动画或状态切换的平滑过渡时间
        /// </summary>
        private const float DEFAULT_TRANSITION_SPEED = 3f;

        /// <summary>
        /// 默认面向方向值常量，用于表示角色或物体的默认朝向角度
        /// </summary>
        private const float DEFAULT_FACING_DIRECTION_VALUE = 25;
        
        private GameObject renderGroup;
        private Camera camera;
        private Transform modelContainer, model;
        private Animator modelAnimator;
        private SkinnedMeshRenderer modelExpressionController;

        private RawImage renderer;
        private CanvasGroup rendererCG => renderer.GetComponent<CanvasGroup>();
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();
        private Dictionary<string, Coroutine> expressionCoroutines = new Dictionary<string, Coroutine>();
        
        /// <summary>
        /// 获取或设置当前对象的可见状态
        /// </summary>
        /// <value>
        /// 如果对象正在显示(isRevealing为true)或根CanvasGroup的透明度大于0时返回true，否则返回false
        /// </value>
        public override bool isVisible
        {
            get => isRevealing || rootCG.alpha > 0;
            set => rootCG.alpha = value ? 1 : 0;
        }

        // 用于控制旧渲染器淡出过程的协程引用
        private Coroutine co_fadingOutOldRenderers = null;
        
        // 检查是否正在进行旧渲染器淡出操作
        private bool isFadingOutOldRenderers => co_fadingOutOldRenderers != null;
        
        // 旧渲染器淡出速度的倍数，默认使用DEFAULT_TRANSITION_SPEED
        private float oldRendererFadeOutSpeedMultiplier = DEFAULT_TRANSITION_SPEED;
        
        /// <summary>
        /// 用于存储旧渲染器相关信息的结构体
        /// 包含旧的CanvasGroup、RawImage组件和渲染器游戏对象的引用
        /// </summary>
        private struct OldRenderer
        {
            // 旧渲染器的CanvasGroup组件
            public CanvasGroup oldCG;
            
            // 旧渲染器的RawImage组件
            public RawImage oldImage;
            
            // 旧渲染器的游戏对象
            public GameObject oldRenderGroup;

            /// <summary>
            /// 初始化OldRenderer结构体
            /// </summary>
            /// <param name="oldCG">旧的CanvasGroup组件</param>
            /// <param name="oldImage">旧的RawImage组件</param>
            /// <param name="oldRenderGroup">旧渲染器的游戏对象</param>
            public OldRenderer(CanvasGroup oldCG, RawImage oldImage, GameObject oldRenderGroup)
            {
                this.oldCG = oldCG;
                this.oldImage = oldImage;
                this.oldRenderGroup = oldRenderGroup;
            }
        }
        
        // 存储所有旧渲染器的列表
        private List<OldRenderer> oldRenderers = new List<OldRenderer>();
        
        /// <summary>
        /// 构造函数，初始化3D角色对象。
        /// </summary>
        /// <param name="name">角色名称。</param>
        /// <param name="config">角色配置数据。</param>
        /// <param name="prefab">角色使用的预制体对象。</param>
        /// <param name="rootAssetsFolder">资源根目录路径。</param>
        public Character_Model3D(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {
            Debug.Log($"Created Model3D Character: '{name}'");

            // 设置UI层级
            SetupUIHierarchy();
            
            // 初始化3D渲染组件
            Initialize3DRendering(rootAssetsFolder, config.name);
        }

        /// <summary>
        /// 设置角色UI层级结构，确保角色显示在正确的Canvas层级中
        /// </summary>
        private void SetupUIHierarchy()
        {
            Transform charactersLayer = GameObject.Find("Main/Root/Canvas - Main/LAYERS/2 - Characters").transform;
            if (root != null && root.parent != charactersLayer)
            {
                root.SetParent(charactersLayer, false);
            }
        }

        /// <summary>
        /// 初始化3D渲染相关组件
        /// </summary>
        /// <param name="rootAssetsFolder">资源根目录路径</param>
        /// <param name="characterName">角色名称</param>
        private void Initialize3DRendering(string rootAssetsFolder, string characterName)
        {
            // 加载并设置渲染组
            if (!LoadAndSetupRenderGroup(rootAssetsFolder, characterName))
                return;

            // 初始化模型组件
            InitializeModelComponents();

            // 设置渲染纹理
            SetupRenderTexture(rootAssetsFolder);
        }

        /// <summary>
        /// 加载并设置渲染组预制体
        /// </summary>
        /// <param name="rootAssetsFolder">资源根目录路径</param>
        /// <param name="characterName">角色名称</param>
        /// <returns>加载是否成功</returns>
        private bool LoadAndSetupRenderGroup(string rootAssetsFolder, string characterName)
        {
            GameObject renderGroupPrefab = Resources.Load<GameObject>(rootAssetsFolder + '/' + 
                string.Format(CHARACTER_RENDER_GROUP_PREFAB_NAME_FORMAT, characterName));

            if (renderGroupPrefab == null)
            {
                Debug.LogError($"无法加载渲染组预制体: {rootAssetsFolder}/{string.Format(CHARACTER_RENDER_GROUP_PREFAB_NAME_FORMAT, characterName)}");
                return false;
            }

            renderGroup = Object.Instantiate(renderGroupPrefab, characterManager.characterPanelModel3D);
            renderGroup.name = string.Format(CHARACTER_RENDER_GROUP_PREFAB_NAME_FORMAT, name);
            renderGroup.SetActive(true);
            
            return true;
        }

        /// <summary>
        /// 初始化模型相关组件引用
        /// </summary>
        private void InitializeModelComponents()
        {
            camera = renderGroup.GetComponentInChildren<Camera>();
            modelContainer = camera.transform.GetChild(0);
            model = modelContainer.GetChild(0);
            modelAnimator = model.GetComponent<Animator>();
            modelExpressionController = model.GetComponentsInChildren<SkinnedMeshRenderer>()
                .FirstOrDefault(sm => sm.sharedMesh.blendShapeCount > 0);
        }

        /// <summary>
        /// 设置渲染纹理，将相机输出绑定到UI显示
        /// </summary>
        /// <param name="rootAssetsFolder">资源根目录路径</param>
        private void SetupRenderTexture(string rootAssetsFolder)
        {
            renderer = animator.GetComponentInChildren<RawImage>();
            RenderTexture renderTex = Resources.Load<RenderTexture>(rootAssetsFolder + '/' + 
                CHARACTER_RENDER_TEXTURE_NAME_FORMAT);
            
            if (renderTex != null)
            {
                RenderTexture newTex = new RenderTexture(renderTex);
                renderer.texture = newTex;
                camera.targetTexture = newTex;
            }
            else
            {
                Debug.LogError($"无法加载渲染纹理: {rootAssetsFolder}/{CHARACTER_RENDER_TEXTURE_NAME_FORMAT}");
            }

            int modelsInScene = characterManager.GetCharacterCountFromCharacterType(CharacterType.Model3D);
            renderGroup.transform.position += Vector3.down * (CHARACTER_STACKING_DEPTH * modelsInScene);
        }
        
        /// <summary>
        /// 播放指定名称的动画状态。
        /// </summary>
        /// <param name="motionName">要播放的动画状态名称。</param>
        public void SetMotion(string motionName)
        {
            if (modelAnimator != null)
            {
                modelAnimator.Play(motionName);
            }
        }

        /// <summary>
        /// 设置角色表情的混合形状权重，支持平滑过渡动画
        /// </summary>
        /// <param name="blendShapeName">混合形状的名称</param>
        /// <param name="weight">目标权重值，范围通常在0-100之间</param>
        /// <param name="speedMultiplier">过渡速度倍数，默认为1，值越大过渡越快</param>
        /// <param name="immediate">是否立即设置权重，true表示立即设置，false表示平滑过渡</param>
        public void SetExpression(string blendShapeName, float weight, float speedMultiplier = 1,
            bool immediate = false)
        {
            // 检查表情控制器是否存在
            if (modelExpressionController == null)
            {
                Debug.LogWarning($"Character {name} does not have an expression controller. Blend Shapes may be null. [{modelExpressionController.name}]");
                return;
            }
            
            // 如果该混合形状正在执行过渡动画，则停止之前的协程
            if(expressionCoroutines.ContainsKey(blendShapeName)) 
            {
                characterManager.StopCoroutine(expressionCoroutines[blendShapeName]);
                expressionCoroutines.Remove(blendShapeName);
            }

            // 启动新的表情过渡协程
            Coroutine expressionCoroutine = characterManager.StartCoroutine(ExpressionCoroutine(blendShapeName, weight, speedMultiplier, immediate));
            expressionCoroutines[blendShapeName] = expressionCoroutine;
        }

        /// <summary>
        /// 执行表情混合形状权重过渡的协程
        /// </summary>
        /// <param name="blendShapeName">混合形状名称</param>
        /// <param name="weight">目标权重值</param>
        /// <param name="speedMultiplier">过渡速度倍数</param>
        /// <param name="immediate">是否立即设置</param>
        /// <returns>IEnumerator协程对象</returns>
        private IEnumerator ExpressionCoroutine(string blendShapeName, float weight, float speedMultiplier = 1,
            bool immediate = false)
        {
            // 获取混合形状的索引
            int blendShapeIndex = modelExpressionController.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex == -1)
            {
                Debug.LogWarning($"Character {name} does not have a blend shape by the name of '{blendShapeName}' [{modelExpressionController.name}]");
                yield break; 
            }
            
            // 根据immediate参数决定是立即设置还是平滑过渡
            if (immediate)
                modelExpressionController.SetBlendShapeWeight(blendShapeIndex, weight);
            else
            {
                float currentValue = modelExpressionController.GetBlendShapeWeight(blendShapeIndex);
                // 使用MoveTowards方法逐步接近目标值，实现平滑过渡效果
                while (currentValue != weight)
                {
                    currentValue = Mathf.MoveTowards(currentValue, weight,
                        Time.deltaTime * EXPRESSION_TRANSITION_SPEED * speedMultiplier);
                    modelExpressionController.SetBlendShapeWeight(blendShapeIndex, currentValue);
                    yield return null;
                }
            }

            // 过渡完成后从字典中移除该混合形状的协程记录
            expressionCoroutines.Remove(blendShapeName);
        }

        /// <summary>
        /// 显示或隐藏UI元素的协程方法。通过平滑过渡透明度实现淡入/淡出效果。
        /// </summary>
        /// <param name="show">是否显示（true为显示，false为隐藏）</param>
        /// <returns>IEnumerator，用于协程执行</returns>
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
        /// 设置渲染器颜色，并同步更新旧渲染器的颜色。
        /// </summary>
        /// <param name="color">要设置的颜色</param>
        public override void SetColor(Color color)
        {
            base.SetColor(color);
            
            renderer.color = color;

            foreach (var or in oldRenderers)
                or.oldImage.color = color;
        }

        /// <summary>
        /// 改变渲染器颜色的协程方法。
        /// </summary>
        /// <param name="speedMultiplier">速度倍数，控制颜色变化的速度</param>
        /// <returns>IEnumerator，用于协程执行</returns>
        public override IEnumerator ChangingColor(float speedMultiplier)
        {
            yield return ChangingRendererColor(speedMultiplier);
            
            co_changingColor = null;
        }

        /// <summary>
        /// 高亮显示的协程方法。如果当前未在改变颜色，则执行颜色变化。
        /// </summary>
        /// <param name="speedMultiplier">速度倍数，控制高亮变化的速度</param>
        /// <returns>IEnumerator，用于协程执行</returns>
        public override IEnumerator Highlighting(float speedMultiplier)
        {
            if (!isChangingColor)
                yield return ChangingRendererColor(speedMultiplier);
            
            co_highlighting = null;
        }

        /// <summary>
        /// 改变渲染器颜色的核心协程逻辑。使用Lerp插值实现颜色渐变。
        /// </summary>
        /// <param name="speedMultiplier">速度倍数，控制颜色变化的速度</param>
        /// <returns>IEnumerator，用于协程执行</returns>
        private IEnumerator ChangingRendererColor(float speedMultiplier)
        {
            Color oldColor = renderer.color;

            float colorPrecent = 0;
            while (colorPrecent != 1)
            {
                colorPrecent += DEFAULT_TRANSITION_SPEED * speedMultiplier * Time.deltaTime;

                renderer.color = Color.Lerp(oldColor, displayColor, colorPrecent);

                foreach (var or in oldRenderers)
                    or.oldImage.color = renderer.color;
                
                yield return null;
            }
            
            co_changingColor = null;
        }

        /// <summary>
        /// 创建新的角色渲染实例。包括复制渲染组、相机、模型等组件，并设置新的渲染纹理。
        /// </summary>
        private void CreateNewCharacterRenderingInstance()
        {
            oldRenderers.Add(new OldRenderer(rendererCG, renderer, renderGroup));
            renderGroup = Object.Instantiate(renderGroup, renderGroup.transform.parent);
            renderGroup.name = string.Format(CHARACTER_RENDER_GROUP_PREFAB_NAME_FORMAT, name);
            
            camera = renderGroup.GetComponentInChildren<Camera>();
            modelContainer = camera.transform.GetChild(0);
            model = modelContainer.GetChild(0);
            modelAnimator = model.GetComponent<Animator>();
            modelExpressionController = model.GetComponentsInChildren<SkinnedMeshRenderer>()
                .FirstOrDefault(sm => sm.sharedMesh.blendShapeCount > 0);

            string rendererName = renderer.name;
            Texture oldRenderTexture = renderer.texture;
            renderer = Object.Instantiate(renderer.gameObject, renderer.transform.parent).GetComponent<RawImage>();
            renderer.name = rendererName;
            rendererCG.alpha = 0;
            RenderTexture newTex = new RenderTexture(oldRenderTexture as RenderTexture);
            renderer.texture = newTex;
            camera.targetTexture = newTex;

            for (int i = 0; i < oldRenderers.Count; i++)
                oldRenderers[i].oldRenderGroup.transform.localPosition = Vector3.zero + (Vector3.right * i);
            
            renderGroup.transform.position = Vector3.zero + (Vector3.right * (CHARACTER_STACKING_DEPTH * oldRenderers.Count));
        }
        
        /// <summary>
        /// 淡出旧渲染器的协程方法。逐步降低旧渲染器的透明度，最后销毁相关对象。
        /// </summary>
        /// <returns>IEnumerator，用于协程执行</returns>
        private IEnumerator FadingOutOldRenderers()
        {
            while (oldRenderers.Any(o => o.oldCG.alpha > 0))
            {
                float speed = DEFAULT_TRANSITION_SPEED * Time.deltaTime * oldRendererFadeOutSpeedMultiplier;
                foreach (var or in oldRenderers)
                    or.oldCG.alpha = Mathf.MoveTowards(or.oldCG.alpha, 0, speed);
                
                yield return null;
            }

            foreach (var or in oldRenderers)
            {
                Object.Destroy(or.oldRenderGroup);
                Object.Destroy(or.oldCG.gameObject);
            }
            
            oldRenderers.Clear();
            
            co_fadingOutOldRenderers = null;
        }

        /// <summary>
        /// 控制角色面向方向的协程方法。支持立即转向或带有过渡效果的转向。
        /// </summary>
        /// <param name="faceLeft">是否面向左侧（true为向左，false为向右）</param>
        /// <param name="speedMultiplier">速度倍数，控制转向过渡的速度</param>
        /// <param name="immediate">是否立即转向（true为立即，false为渐变）</param>
        /// <returns>IEnumerator，用于协程执行</returns>
        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            Vector3 facingAngle = new Vector3(0, (faceLeft ? DEFAULT_FACING_DIRECTION_VALUE : -DEFAULT_FACING_DIRECTION_VALUE), 0);

            if (immediate)
            {
                modelContainer.localEulerAngles = facingAngle;
            }
            else
            {
                CreateNewCharacterRenderingInstance();
                modelContainer.localEulerAngles = facingAngle;

                oldRendererFadeOutSpeedMultiplier = speedMultiplier;
                if (!isFadingOutOldRenderers)
                    co_fadingOutOldRenderers = characterManager.StartCoroutine(FadingOutOldRenderers());

                CanvasGroup newRenderer = rendererCG;
                while (newRenderer.alpha != 1)
                {
                    float speed = DEFAULT_TRANSITION_SPEED * Time.deltaTime * speedMultiplier;
                    newRenderer.alpha = Mathf.MoveTowards(newRenderer.alpha, 1, speed);
                    yield return null;
                }
            }

            co_flipping = null;
        }

        /// <summary>
        /// 当接收到表情表达式时触发的回调方法
        /// </summary>
        /// <param name="layer">表情图层索引</param>
        /// <param name="expression">表情表达式字符串</param>
        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            SetExpression(expression, 100);
        }
    }
}
