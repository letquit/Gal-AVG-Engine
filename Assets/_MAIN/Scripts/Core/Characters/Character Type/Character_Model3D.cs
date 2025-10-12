using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        public const string CHARACTER_RENDER_GROUP_PREFAB_NAME_FORMAT = "RenderGroup - [{0}]";

        /// <summary>
        /// 渲染纹理资源名称格式化字符串。
        /// </summary>
        public const string CHARACTER_RENDER_TEXTURE_NAME_FORMAT = "RenderTexture";

        /// <summary>
        /// 角色在场景中的堆叠深度间隔，用于防止模型重叠。
        /// </summary>
        public const int CHARACTER_STACKING_DEPTH = 15;

        /// <summary>
        /// 表达式过渡速度常量，用于控制表情或状态切换的动画速度
        /// </summary>
        public const float EXPRESSION_TRANSITION_SPEED = 100f;
        
        private GameObject renderGroup;
        private Camera camera;
        private Transform modelContainer, model;
        private Animator modelAnimator;
        private SkinnedMeshRenderer modelExpressionController;

        private RawImage renderer;
        
        private Dictionary<string, Coroutine> expressionCoroutines = new Dictionary<string, Coroutine>();
        
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
    }
}
