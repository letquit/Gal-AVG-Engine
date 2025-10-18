using System;
using System.Collections;
using CHARACTERS;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话框容器类，用于存储和管理对话系统中的UI组件引用
    /// 该类包含对话框的根对象以及用于显示角色名称和对话内容的文本组件
    /// </summary>
    [Serializable]
    public class DialogueContainer
    {
        private const float DEFAULT_FADE_SPEED = 3f;
        
        /// <summary>
        /// 对话框的根游戏对象，用于控制整个对话框的显示和隐藏
        /// </summary>
        public GameObject root;
        
        /// <summary>
        /// 角色名称容器，用于存储和管理角色名称相关的UI组件引用
        /// </summary>
        public NameContainer nameContainer;

        /// <summary>
        /// 显示对话内容的TextMeshProUGUI文本组件
        /// </summary>
        public TextMeshProUGUI dialogueText;
        
        /// <summary>
        /// 获取对话框根对象上的CanvasGroup组件，用于控制透明度和交互状态
        /// </summary>
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();

        private Coroutine co_showing = null;
        private Coroutine co_hiding = null;
        
        /// <summary>
        /// 指示当前是否正在执行显示动画
        /// </summary>
        public bool isShowing => co_showing != null;
        
        /// <summary>
        /// 指示当前是否正在执行隐藏动画
        /// </summary>
        public bool isHiding => co_hiding != null;
        
        /// <summary>
        /// 指示当前是否正在进行淡入或淡出操作
        /// </summary>
        public bool isFading => isShowing || isHiding;
        
        /// <summary>
        /// 判断对话框当前是否可见（包括正在显示的状态）
        /// </summary>
        public bool isVisible => co_showing != null || rootCG.alpha > 0;
        
        /// <summary>
        /// 设置对话文本的颜色
        /// </summary>
        /// <param name="color">要设置的颜色值</param>
        public void SetDialogueColor(Color color) => dialogueText.color = color;
        
        /// <summary>
        /// 设置对话文本的字体
        /// </summary>
        /// <param name="font">要设置的字体资源</param>
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        
        /// <summary>
        /// 设置对话文本的字体大小
        /// </summary>
        /// <param name="size">要设置的字体大小</param>
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;
        
        /// <summary>
        /// 根据角色配置数据设置对话文本的样式
        /// </summary>
        /// <param name="config">包含对话文本颜色和字体配置的角色配置数据</param>
        public void SetConfig(CharacterConfigData config)
        {
            // 应用角色配置中的对话文本颜色
            SetDialogueColor(config.dialogueColor);
            
            // 应用角色配置中的对话文本字体
            SetDialogueFont(config.dialogueFont);
            
            // 应用角色配置中的对话文本字体大小，并考虑缩放因子
            SetDialogueFontSize(config.dialogueFontSize * config.dialogueFontScale);
        }

        /// <summary>
        /// 显示对话框并启动淡入动画。如果已经在显示则直接返回当前协程；如果正在隐藏，则先停止隐藏协程。
        /// </summary>
        /// <returns>表示淡入过程的协程引用</returns>
        public Coroutine Show()
        {
            if (isShowing)
                return co_showing;
            else if (isHiding)
            {
                DialogueSystem.instance.StopCoroutine(co_hiding);
                co_hiding = null;
            }
            
            co_showing = DialogueSystem.instance.StartCoroutine(Fading(1));
            
            return co_showing;
        }
        
        /// <summary>
        /// 隐藏对话框并启动淡出动画。如果已经在隐藏则直接返回当前协程；如果正在显示，则先停止显示协程。
        /// </summary>
        /// <returns>表示淡出过程的协程引用</returns>
        public Coroutine Hide()
        {
            if (isHiding)
                return co_hiding;
            else if (isShowing)
            {
                DialogueSystem.instance.StopCoroutine(co_showing);
                co_showing = null;
            }
            
            co_hiding = DialogueSystem.instance.StartCoroutine(Fading(0));
            
            return co_hiding;
        }

        /// <summary>
        /// 执行淡入/淡出的核心逻辑，通过修改CanvasGroup的alpha值实现渐变效果
        /// </summary>
        /// <param name="alpha">目标alpha值：1表示完全显示，0表示完全隐藏</param>
        /// <returns>可枚举的协程迭代器</returns>
        private IEnumerator Fading(float alpha)
        {
            CanvasGroup cg = rootCG;

            while (cg.alpha != alpha)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * DEFAULT_FADE_SPEED);
                yield return null;
            }
            
            co_showing = null;
            co_hiding = null;
        }
    }
}