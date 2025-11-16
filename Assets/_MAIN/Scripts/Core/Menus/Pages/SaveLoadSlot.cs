using System.IO;
using ADVENTUREGAME;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 存档/读档槽位UI控制类，用于显示和管理单个存档槽位的信息。
/// 包括预览图、标题、以及保存、加载、删除等功能按钮。
/// </summary>
public class SaveLoadSlot : MonoBehaviour
{
    public GameObject root;
    public RawImage previewImage;
    public TextMeshProUGUI titleText;
    public Button deleteButton;
    public Button saveButton;
    public Button loadButton;

    [HideInInspector] public int fileNumber = 0;
    [HideInInspector] public string filePath = "";
    
    private UIConfirmationMenu uiChoiceMenu => UIConfirmationMenu.instance;

    /// <summary>
    /// 根据当前菜单功能（保存或加载）填充该槽位的详细信息。
    /// 如果文件存在，则加载并显示存档信息；否则显示为空文件。
    /// </summary>
    /// <param name="function">当前菜单的功能类型（保存或加载）</param>
    public void PopulateDetails(SaveAndLoadMenu.MenuFunction function)
    {
        if (File.Exists(filePath))
        {
            AVGGameSave file = AVGGameSave.Load(filePath);
            PopulateDetailsFromFile(function, file);
        }
        else
        {
            PopulateDetailsFromFile(function, null);
        }
    }

    /// <summary>
    /// 根据传入的存档数据填充UI元素内容。
    /// 若存档为空，则显示为“空文件”状态；否则显示实际存档信息。
    /// </summary>
    /// <param name="function">当前菜单的功能类型（保存或加载）</param>
    /// <param name="file">要显示的存档对象，若为null则表示空文件</param>
    private void PopulateDetailsFromFile(SaveAndLoadMenu.MenuFunction function, AVGGameSave file)
    {
        // 显示空文件状态
        if (file == null)
        {
            titleText.text = $"{fileNumber}. Empty File";
            deleteButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.save);
            previewImage.texture = SaveAndLoadMenu.Instance.emptyFileImage;
        }
        // 显示已有存档状态
        else
        {
            titleText.text = $"{fileNumber}. {file.timestamp}";
            deleteButton.gameObject.SetActive(true);
            loadButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.load);
            saveButton.gameObject.SetActive(function == SaveAndLoadMenu.MenuFunction.save);
            
            byte[] data = File.ReadAllBytes(file.screenshotPath);
            Texture2D screenshotPreview = new Texture2D(1, 1);
            ImageConversion.LoadImage(screenshotPreview, data);
            previewImage.texture = screenshotPreview;
        }
    }

    /// <summary>
    /// 显示删除文件确认对话框，需要用户进行二次确认后执行删除操作
    /// </summary>
    public void Delete()
    {
        // 显示第一次确认对话框，询问是否要删除文件
        uiChoiceMenu.Show(
            // Title
            "Delete this file? (<i>This cannot be undone!</i>)",
            //Choice 1
            new UIConfirmationMenu.ConfirmationButtopn("Yes", () =>
                {
                    // 显示第二次确认对话框，进一步确认删除操作
                    uiChoiceMenu.Show(
                        "Are you sure?",
                        new UIConfirmationMenu.ConfirmationButtopn("I am sure", OnConfirmDelete),
                        new UIConfirmationMenu.ConfirmationButtopn("Never", null));
                },
                autoCloseOnQuit: false
            ),
            //Choice 2
            new UIConfirmationMenu.ConfirmationButtopn("No", null));
    }

    /// <summary>
    /// 执行文件删除操作的回调函数，删除指定文件并刷新界面显示
    /// </summary>
    private void OnConfirmDelete()
    {
        File.Delete(filePath);
        PopulateDetails(SaveAndLoadMenu.Instance.menuFunction);
    }
    
    /// <summary>
    /// 加载当前槽位对应的存档文件，并关闭所有菜单界面。
    /// </summary>
    public void Load()
    {
        // 加载指定路径的存档文件
        AVGGameSave file = AVGGameSave.Load(filePath, false);
        // 关闭存档加载菜单界面
        SaveAndLoadMenu.Instance.Close(closeAllMenus: true);
        
        // 根据当前场景决定加载方式
        if (SceneManager.GetActiveScene().name == MainMenu.MAIN_MENU_SCENE)
        {
            // 在主菜单场景中加载游戏
            MainMenu.instance.LoadGame(file);
        }
        else
        {
            // 在其他场景中激活存档数据
            file.Activate();
        }
    }
    
    /// <summary>
    /// 将当前游戏状态保存到该槽位，并更新界面显示。
    /// </summary>
    public void Save()
    {
        var activeSave = AVGGameSave.activeFile;
        activeSave.slotNumber = fileNumber;
        
        activeSave.Save();
        
        PopulateDetailsFromFile(SaveAndLoadMenu.Instance.menuFunction, activeSave);
    }
}