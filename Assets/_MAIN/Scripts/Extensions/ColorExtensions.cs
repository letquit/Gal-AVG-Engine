using UnityEngine;

/// <summary>
/// 颜色扩展类，提供对Unity Color类型的扩展方法
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// 设置颜色的透明度(alpha)值，返回一个新的Color对象
    /// </summary>
    /// <param name="original">原始颜色对象</param>
    /// <param name="alpha">新的透明度值，范围为0-1，0表示完全透明，1表示完全不透明</param>
    /// <returns>返回一个新的Color对象，RGB值保持不变，alpha值被设置为指定值</returns>
    public static Color SetAlpha(this Color original, float alpha)
    {
        return new Color(original.r, original.g, original.b, alpha);
    }

    /// <summary>
    /// 根据颜色名称获取对应的Color对象
    /// </summary>
    /// <param name="original">扩展方法的目标颜色对象（实际未使用，仅用于扩展方法语法）</param>
    /// <param name="colorName">颜色名称字符串，支持常见颜色如red、green、blue等</param>
    /// <returns>返回与名称对应的颜色对象，如果名称无法识别则返回透明色(Color.clear)</returns>
    public static Color GetColorFromName(this Color original, string colorName)
    {
        // 根据颜色名称返回预定义的颜色值
        switch (colorName.ToLower())
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "yellow":
                return Color.yellow;
            case "white":
                return Color.white;
            case "black":
                return Color.black;
            case "gray":
                return Color.gray;
            case "cyan":
                return Color.cyan;
            case "magenta":
                return Color.magenta;
            case "orange":
                return new Color(1f, 0.5f, 0f);
            default:
                Debug.LogWarning("Unrecognized color name: " + colorName);
                return Color.clear;
        }
    }
}

