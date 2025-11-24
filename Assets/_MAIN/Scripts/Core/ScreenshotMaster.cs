using System.IO;
using UnityEngine;

/// <summary>
/// 截图管理器类，提供屏幕截图和保存功能
/// </summary>
public class ScreenshotMaster : MonoBehaviour
{
    /// <summary>
    /// 捕获屏幕截图的便捷方法，使用主摄像机进行截图
    /// </summary>
    /// <param name="width">截图宽度</param>
    /// <param name="height">截图高度</param>
    /// <param name="supersize">超采样倍数，用于提高截图质量</param>
    /// <param name="filePath">保存文件路径，如果为空则不保存到文件</param>
    /// <returns>返回捕获的Texture2D纹理</returns>
    public static Texture2D CaptureScreenshot(int width, int height, float supersize = 1, string filePath = "") => CaptureScreenshot(Camera.main, width, height, supersize, filePath);

    /// <summary>
    /// 使用指定摄像机捕获屏幕截图
    /// </summary>
    /// <param name="cam">用于渲染截图的摄像机</param>
    /// <param name="width">截图宽度</param>
    /// <param name="height">截图高度</param>
    /// <param name="supersize">超采样倍数，用于提高截图质量</param>
    /// <param name="filePath">保存文件路径，如果为空则不保存到文件</param>
    /// <returns>返回捕获的Texture2D纹理</returns>
    public static Texture2D CaptureScreenshot(Camera cam, int width, int height, float supersize = 1,
        string filePath = "")
    {
        // 根据超采样倍数调整截图尺寸
        if (supersize != 1)
        {
            width = Mathf.RoundToInt(width * supersize);
            height = Mathf.RoundToInt(height * supersize);
        }
        
        // 创建临时渲染纹理并设置摄像机目标纹理
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 32);
        cam.targetTexture = rt;
        
        // 创建Texture2D用于存储截图数据
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
        
        // 渲染摄像机视图到目标纹理
        cam.Render();
        
        // 激活渲染纹理并读取像素数据
        RenderTexture.active = rt;
        
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        
        // 清理资源
        cam.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        // 如果指定了文件路径，则保存截图到文件
        if (filePath != "")
            SaveScreenshotToFile(screenshot, filePath);
        
        return screenshot;
    }
    
    /// <summary>
    /// 图片文件类型枚举
    /// </summary>
    public enum ImageType { PNG, JPG }

    /// <summary>
    /// 将截图保存到指定文件路径
    /// </summary>
    /// <param name="screenshot">要保存的Texture2D纹理</param>
    /// <param name="filePath">保存文件路径</param>
    /// <param name="fileType">图片文件类型（PNG或JPG）</param>
    public static void SaveScreenshotToFile(Texture2D screenshot, string filePath, ImageType fileType = ImageType.PNG)
    {
        // 根据文件类型编码纹理数据
        byte[] bytes = new byte[0];
        string extension = "";
        switch (fileType)
        {
            case ImageType.PNG:
                bytes = screenshot.EncodeToPNG();
                extension = ".png";
                break;
            case ImageType.JPG:
                bytes = screenshot.EncodeToJPG();
                extension = ".jpg";
                break;
        }
        
        // 如果文件路径不包含扩展名，则自动添加
        if (!filePath.Contains('.'))
            filePath += extension;
        
        FileManager.TryCreateDirectoryFromPath(filePath);
        
        // 将字节数据写入文件
        File.WriteAllBytes(filePath, bytes);
    }
}