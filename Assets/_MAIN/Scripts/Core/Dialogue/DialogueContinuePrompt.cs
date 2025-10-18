using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 对话继续提示类，用于在对话文本末尾显示继续提示符号
/// </summary>
public class DialogueContinuePrompt : MonoBehaviour
{
    private RectTransform root;

    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI tmpro;

    /// <summary>
    /// 获取提示是否正在显示的状态
    /// </summary>
    public bool isShowing => anim.gameObject.activeSelf;
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Start()
    {
        root = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 显示继续提示符号
    /// 计算文本最后一个字符的位置，并将提示符号定位到该位置右侧
    /// </summary>
    public void Show()
    {
        // 如果文本为空，则隐藏提示符号并返回
        if (tmpro.text == string.Empty)
        {
            if (isShowing)
                Hide();
            
            return;
        }
        
        tmpro.ForceMeshUpdate();
        
        anim.gameObject.SetActive(true);
        root.transform.SetParent(tmpro.transform);
        
        // 计算最后一个字符的右下角位置，作为提示符号的定位点
        TMP_CharacterInfo finalCharacter = tmpro.textInfo.characterInfo[tmpro.textInfo.characterCount - 1];
        Vector3 targetPos = finalCharacter.bottomRight;
        float characterWidth = finalCharacter.pointSize * 0.5f;
        targetPos = new Vector3(targetPos.x + characterWidth, targetPos.y, 0);
        
        root.localPosition = targetPos;
    }
    
    /// <summary>
    /// 隐藏继续提示符号
    /// </summary>
    public void Hide()
    {
        anim.gameObject.SetActive(false);
    }
}