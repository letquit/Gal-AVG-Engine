using System;
using System.Collections.Generic;
using CHARACTERS;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 用于保存角色状态数据的类，包括角色的基本信息、配置缓存以及可视状态等。
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        public string characterName;        // 角色名称（内部标识）
        public string displayName;          // 显示名称
        public bool enabled;                // 是否启用/可见
        public Color color;                 // 角色颜色
        public int priority;                // 渲染优先级
        public bool isHighLighted;          // 是否高亮显示
        public bool isFacingLeft;           // 是否面向左侧
        public Vector2 position;            // 当前位置
        public CharacterConfigCache characterConfig; // 角色配置缓存

        public string animationJSON;        // 动画数据JSON字符串
        public string dataJSON;             // 根据角色类型序列化的额外数据JSON字符串

        /// <summary>
        /// 缓存角色配置信息的嵌套类，用于存储与角色相关的字体、颜色和类型等配置。
        /// </summary>
        [Serializable]
        public class CharacterConfigCache
        {
            public string name;                     // 配置名称
            public string alias;                    // 别名
            public Character.CharacterType characterType; // 角色类型（Sprite/SpriteSheet/Live2D/Model3D）

            public Color nameColor;                 // 名称文本颜色
            public Color dialogueColor;             // 对话文本颜色

            public string nameFont;                 // 名称字体资源路径
            public string dialogueFont;             // 对话字体资源路径

            public float nameFontScale = 1f;        // 名称字体缩放比例
            public float dialogueFontScale = 1f;    // 对话字体缩放比例

            /// <summary>
            /// 使用 CharacterConfigData 初始化缓存配置。
            /// </summary>
            /// <param name="reference">原始角色配置数据</param>
            public CharacterConfigCache(CharacterConfigData reference)
            {
                name = reference.name;
                alias = reference.alias;
                characterType = reference.characterType;

                nameColor = reference.nameColor;
                dialogueColor = reference.dialogueColor;

                nameFont = FilePaths.resources_font + reference.nameFont.name;
                dialogueFont = FilePaths.resources_font + reference.dialogueFont.name;

                nameFontScale = reference.nameFontScale;
                dialogueFontScale = reference.dialogueFontScale;
            }
        }

        /// <summary>
        /// 捕获当前所有可见角色的状态并生成 CharacterData 列表。
        /// </summary>
        /// <returns>包含所有可见角色状态的 CharacterData 列表</returns>
        public static List<CharacterData> Capture()
        {
            List<CharacterData> characters = new List<CharacterData>();

            // 遍历所有角色，仅处理可见的角色
            foreach (var character in CharacterManager.instance.allCharacters)
            {
                if (!character.isVisible)
                    continue;

                CharacterData entry = new CharacterData();
                entry.characterName = character.name;
                entry.displayName = character.displayName;
                entry.enabled = character.isVisible;
                entry.color = character.color;
                entry.priority = character.priority;
                entry.isFacingLeft = character.isFacingLeft;
                entry.isHighLighted = character.highlighted;
                entry.position = character.targetPosition;
                entry.characterConfig = new CharacterConfigCache(character.config);
                // 保存动画数据
                entry.animationJSON = GetAnimationData(character);

                // 根据不同角色类型保存额外数据
                switch (character.config.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData sData = new SpriteData();
                        sData.layers = new List<SpriteData.LayerData>();

                        Character_Sprite sc = character as Character_Sprite;
                        foreach (var layer in sc.layers)
                        {
                            var layerData = new SpriteData.LayerData();
                            layerData.color = layer.renderer.color;
                            layerData.spriteName = layer.renderer.sprite.name;
                            sData.layers.Add(layerData);
                        }

                        entry.dataJSON = JsonUtility.ToJson(sData);
                        break;
                    case Character.CharacterType.Live2D:
                        Live2DData l2Data = new Live2DData();
                        Character_Live2D lc = character as Character_Live2D;

                        l2Data.expression = lc.activeExpression;
                        l2Data.motion = lc.activeMotion;

                        entry.dataJSON = JsonUtility.ToJson(l2Data);
                        break;
                    case Character.CharacterType.Model3D:
                        Model3DData m3Data = new Model3DData();
                        Character_Model3D mc = character as Character_Model3D;

                        m3Data.position = mc.model.position;
                        m3Data.rotation = mc.model.rotation;

                        entry.dataJSON = JsonUtility.ToJson(m3Data);
                        break;
                }

                characters.Add(entry);
            }

            return characters;
        }
        
        /// <summary>
        /// 应用一组角色数据到当前场景中的角色对象，包括显示名称、颜色、高亮状态、优先级、朝向、位置等属性。
        /// 同时根据角色类型（Sprite、Live2D、Model3D）更新其特有的表现数据。
        /// 最后隐藏未在数据列表中出现的角色。
        /// </summary>
        /// <param name="data">包含所有需要应用的角色数据的列表</param>
        public static void Apply(List<CharacterData> data)
        {
            // 缓存已处理角色的名称，用于后续判断哪些角色需要被隐藏
            List<string> cache = new List<string>();

            foreach (CharacterData characterData in data)
            {
                // 获取或创建对应名称的角色实例
                Character character =
                    CharacterManager.instance.GetCharacter(characterData.characterName, createIfDoesNotExist: true);
                
                // 设置基础属性
                character.displayName = characterData.displayName;
                character.SetColor(characterData.color);
                
                // 设置高亮状态
                if (characterData.isHighLighted)
                    character.Highlight(immediate: true);
                else
                    character.UnHighlight(immediate: true);
                
                // 设置优先级和朝向
                character.SetPriority(characterData.priority);

                // 设置朝向
                if (characterData.isFacingLeft)
                    character.FaceLeft(immediate: true);
                else
                    character.FaceRight(immediate: true);
                
                // 设置位置和可见性
                character.SetPosition(characterData.position);
                
                // 设置可见性
                character.isVisible = characterData.enabled;

                // 应用动画数据
                AnimationData animationData = JsonUtility.FromJson<AnimationData>(characterData.animationJSON);
                ApplyAnimationData(character, animationData);

                // 根据不同角色类型设置特定数据
                switch (character.config.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        // 处理 Sprite 类型角色：更新图层精灵
                        SpriteData sData = JsonUtility.FromJson<SpriteData>(characterData.dataJSON);
                        Character_Sprite sc = character as Character_Sprite;

                        for (int i = 0; i < sData.layers.Count; i++)
                        {
                            var layer = sData.layers[i];
                            if (sc.layers[i].renderer.sprite != null &&
                                sc.layers[i].renderer.sprite.name != layer.spriteName)
                            {
                                Sprite sprite = sc.GetSprite(layer.spriteName);
                                if (sprite != null)
                                    sc.SetSprite(sprite, i);
                                else 
                                    Debug.LogWarning($"History State: Could not load sprite '{layer.spriteName}");
                            }

                        }
                        break;
                    case Character.CharacterType.Live2D:
                        // 处理 Live2D 类型角色：更新表情和动作
                        Live2DData l2Data = JsonUtility.FromJson<Live2DData>(characterData.dataJSON);
                        Character_Live2D lc = (Character_Live2D)character;
                        if (lc.activeExpression != l2Data.expression)
                            lc.SetExpression(l2Data.expression);
                        if (lc.activeMotion != l2Data.motion)
                            lc.SetMotion(l2Data.motion);
                        break;
                    case Character.CharacterType.Model3D:
                        // 处理 3D 模型类型角色：更新模型的位置和旋转
                        Model3DData m3Data = JsonUtility.FromJson<Model3DData>(characterData.dataJSON);
                        Character_Model3D mc = (Character_Model3D)character;
                        mc.model.position = m3Data.position;
                        mc.model.rotation = m3Data.rotation;
                        break;
                }

                // 将已处理角色加入缓存
                cache.Add(character.name);
            }

            // 隐藏未在数据中指定的所有其他角色
            foreach (Character character in CharacterManager.instance.allCharacters)
            {
                if (!cache.Contains(character.name))
                    character.isVisible = false;
            }
        }

        /// <summary>
        /// 获取角色动画控制器的参数数据并序列化为JSON字符串
        /// </summary>
        /// <param name="character">要获取动画数据的角色对象</param>
        /// <returns>包含动画参数信息的JSON字符串</returns>
        private static string GetAnimationData(Character character)
        {
            Animator animator = character.animator;
            AnimationData data = new AnimationData();

            // 遍历动画控制器的所有参数，排除Trigger类型参数
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    continue;
                
                AnimationData.AnimationParameter pData = new AnimationData.AnimationParameter { name = param.name };

                // 根据参数类型获取对应的值并存储
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        pData.type = "Bool";
                        pData.value = animator.GetBool(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Float:
                        pData.type = "Float";
                        pData.value = animator.GetFloat(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Int:
                        pData.type = "Int";
                        pData.value = animator.GetInteger(param.name).ToString();
                        break;
                }
                
                data.parameters.Add(pData);
            }
            
            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// 将动画数据应用到角色的动画控制器中
        /// </summary>
        /// <param name="character">要应用动画数据的角色对象</param>
        /// <param name="data">包含动画参数信息的数据对象</param>
        private static void ApplyAnimationData(Character character, AnimationData data)
        {
            Animator animator = character.animator;
            
            // 遍历所有参数数据并设置到动画控制器中
            foreach (var param in data.parameters)
            {
                switch (param.type)
                {
                    case "Bool":
                        animator.SetBool(param.name, bool.Parse(param.value));
                        break;
                    case "Float":
                        animator.SetFloat(param.name, float.Parse(param.value));
                        break;
                    case "Int":
                        animator.SetInteger(param.name, int.Parse(param.value));
                        break;
                }
            }
            
            // 触发刷新动画的Trigger
            animator.SetTrigger(Character.ANIMATION_REFRESH_TRIGGER);
        }

        /// <summary>
        /// 动画数据容器类，用于存储和序列化动画参数信息
        /// </summary>
        [Serializable]
        public class AnimationData
        {
            public List<AnimationParameter> parameters = new List<AnimationParameter>();
            
            /// <summary>
            /// 动画参数数据类，存储单个动画参数的名称、类型和值
            /// </summary>
            [Serializable]
            public class AnimationParameter
            {
                public string name;
                public string type;
                public string value;
            }
        }

        /// <summary>
        /// 用于保存精灵角色图层数据的嵌套类。
        /// </summary>
        [Serializable]
        public class SpriteData
        {
            public List<LayerData> layers; // 图层列表

            /// <summary>
            /// 表示精灵图层的数据结构。
            /// </summary>
            [Serializable]
            public class LayerData
            {
                public string spriteName;   // 精灵名称
                public Color color;         // 图层颜色
            }
        }

        /// <summary>
        /// 用于保存Live2D角色状态数据的嵌套类。
        /// </summary>
        [Serializable]
        public class Live2DData
        {
            public string expression;   // 当前表情
            public string motion;       // 当前动作
        }

        /// <summary>
        /// 用于保存3D模型角色状态数据的嵌套类。
        /// </summary>
        [Serializable]
        public class Model3DData
        {
            public Vector3 position;     // 模型位置
            public Quaternion rotation;  // 模型旋转
        }
    }
}
