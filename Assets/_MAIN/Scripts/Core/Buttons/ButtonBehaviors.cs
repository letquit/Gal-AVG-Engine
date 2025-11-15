using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 按钮行为控制类，负责处理鼠标进入和退出按钮时的动画播放逻辑
/// 实现了Unity的事件系统接口来响应指针事件
/// </summary>
public class ButtonBehaviors : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static ButtonBehaviors selectedButton = null;
    public Animator anim;

    /// <summary>
    /// 当指针退出按钮区域时调用此方法
    /// 播放"Exit"动画状态
    /// </summary>
    /// <param name="eventData">包含事件相关信息的数据对象</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        anim.Play("Exit");
    }
    
    /// <summary>
    /// 当指针进入按钮区域时调用此方法
    /// 播放"Enter"动画状态，并处理与其他按钮的交互逻辑
    /// </summary>
    /// <param name="eventData">包含事件相关信息的数据对象</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 如果当前已有选中的按钮且不是当前按钮，则触发该按钮的退出事件
        if (selectedButton != null & selectedButton != this)
        {
            selectedButton.OnPointerExit(null);
        }
        
        anim.Play("Enter");
        selectedButton = this;
    }
}