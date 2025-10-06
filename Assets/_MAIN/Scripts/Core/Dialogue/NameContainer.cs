using System;
using CHARACTERS;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 名称容器类，用于管理对话中角色名称的显示和隐藏
    /// </summary>
    [Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject root;
        [SerializeField] public TextMeshProUGUI nameText;

        /// <summary>
        /// 显示名称容器
        /// </summary>
        /// <param name="nameToShow">要显示的名称文本，如果为空则不更新文本内容</param>
        public void Show(string nameToShow = "")
        {
            // 激活根对象以显示名称容器
            root.SetActive(true);

            // 如果提供了新的名称文本，则更新显示内容
            if (nameToShow != string.Empty)
                nameText.text = nameToShow;
        }

        /// <summary>
        /// 隐藏名称容器
        /// </summary>
        public void Hide()
        {
            // 隐藏根对象以隐藏整个名称容器
            root.SetActive(false);
        }
        
        /// <summary>
        /// 设置名称文本的颜色
        /// </summary>
        /// <param name="color">要设置的颜色值</param>
        public void SetNameColor(Color color) => nameText.color = color;
        
        /// <summary>
        /// 设置名称文本的字体
        /// </summary>
        /// <param name="font">要设置的字体资源</param>
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
        
        
        /// <summary>
        /// 根据角色配置数据设置名称显示样式
        /// </summary>
        /// <param name="config">包含名称颜色和字体配置的数据对象</param>
        public void SetConfig(CharacterConfigData config)
        {
            SetNameColor(config.nameColor);
            SetNameFont(config.nameFont);
        }
    }
}

