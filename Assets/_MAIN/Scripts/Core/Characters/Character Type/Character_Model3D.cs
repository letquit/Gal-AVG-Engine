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
        
        private GameObject renderGroup;
        private Camera camera;
        private Transform modelContainer, model;
        private Animator modelAnimator;
        private SkinnedMeshRenderer modelExpressionController;

        private RawImage renderer;
        
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
    }
}
