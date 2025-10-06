using System;
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
    }
}
