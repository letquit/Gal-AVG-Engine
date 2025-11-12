using System;
using ADVENTUREGAME;
using UnityEngine;

/// <summary>
/// 存档与读档菜单页面类，用于管理游戏存档和读档功能
/// 继承自MenuPage，提供分页显示存档槽位的功能
/// </summary>
public class SaveAndLoadMenu : MenuPage
{
    /// <summary>
    /// 单例实例，用于全局访问该菜单
    /// </summary>
    public static SaveAndLoadMenu Instance { get; private set; }
    
    /// <summary>
    /// 最大存档文件数量常量
    /// </summary>
    public const int MAX_FILES = 99;
    /// <summary>
    /// 存档文件存储路径
    /// </summary>
    private string savePath = FilePaths.gameSaves;
    
    /// <summary>
    /// 当前显示的页面编号
    /// </summary>
    private int currentPage = 1;
    /// <summary>
    /// 标记是否首次加载存档文件
    /// </summary>
    private bool loadedFilesForFirstTime = false;
    
    /// <summary>
    /// 菜单功能枚举，标识当前是存档模式还是读档模式
    /// </summary>
    public enum MenuFunction { save, load }
    /// <summary>
    /// 当前菜单功能（存档或读档）
    /// </summary>
    public MenuFunction menuFunction = MenuFunction.save;

    /// <summary>
    /// 存档槽位数组
    /// </summary>
    public SaveLoadSlot[] saveSlots;
    /// <summary>
    /// 每页槽位数量属性，返回saveSlots数组长度
    /// </summary>
    public int slotsPerPage => saveSlots.Length;

    /// <summary>
    /// 空存档文件显示图片
    /// </summary>
    public Texture emptyFileImage;

    /// <summary>
    /// Awake生命周期函数，在对象初始化时设置单例实例
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 打开菜单时的回调函数，继承自MenuPage
    /// 首次打开时会加载当前页面的存档槽位信息
    /// </summary>
    public override void Open()
    {
        base.Open();
        
        // 首次打开菜单时填充当前页面的存档槽位
        if (!loadedFilesForFirstTime)
            PopulateSaveSlotsForPage(currentPage);
    }

    /// <summary>
    /// 填充指定页面的存档槽位信息
    /// 根据页面编号计算起始和结束文件编号，并更新对应槽位的显示内容
    /// </summary>
    /// <param name="pageNumber">要填充的页面编号</param>
    public void PopulateSaveSlotsForPage(int pageNumber)
    {
        // 更新当前页面编号
        currentPage = pageNumber;   
        // 计算当前页面的起始文件编号
        int startingFile = ((currentPage - 1) * slotsPerPage) + 1;
        // 计算当前页面的结束文件编号
        int endingFile = startingFile + slotsPerPage - 1;

        // 遍历所有槽位并填充对应文件的信息
        for (int i = 0; i < slotsPerPage; i++)
        {
            int fileNum = startingFile + i;
            SaveLoadSlot slot = saveSlots[i];

            // 如果文件编号在有效范围内，则激活并填充槽位
            if (fileNum <= MAX_FILES)
            {
                slot.root.SetActive(true);
                // 构造完整的文件路径
                string filePath = $"{FilePaths.gameSaves}{fileNum}{AVGGameSave.FILE_TYPE}";
                slot.fileNumber = fileNum;
                slot.filePath = filePath;
                // 根据当前菜单功能填充槽位详细信息
                slot.PopulateDetails(menuFunction);
            }
            else
            {
                // 超出范围的槽位设为非激活状态
                slot.root.SetActive(false);
            }
        }
    }
}