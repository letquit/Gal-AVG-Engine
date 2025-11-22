using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责管理图库菜单界面的显示与交互逻辑。
/// 包括图片预览按钮、分页导航以及大图展示等功能。
/// </summary>
public class GalleryMenu : MonoBehaviour
{
    /// <summary>
    /// 页面按钮限制常量，用于控制页面导航按钮的最大显示数量
    /// </summary>
    private const int PAGE_BUTTON_LIMIT = 2;

    /// <summary>
    /// 最大页面数，表示分页控件中的总页面数量
    /// </summary>
    private int maxPages = 0;

    /// <summary>
    /// 当前选中的页面索引，表示用户当前查看的页面位置
    /// </summary>
    private int selectedPage = 0;
    
    [SerializeField] private CanvasGroup root;
    private CanvasGroupController rootCG;

    [SerializeField] private Texture[] galleryImages;
    
    [SerializeField] private Button[] galleryPreviewButtons;
    [SerializeField] private Button panelSelectionButtonPrefab;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [SerializeField] private CanvasGroup previewPanel;
    private CanvasGroupController previewPanelCG;

    [SerializeField] private Button previewButton;

    private bool initialized = false;
    private int previewsPerPage => galleryPreviewButtons.Length;

    /// <summary>
    /// 初始化组件引用并加载图库资源。
    /// </summary>
    private void Start()
    {
        if (this == null) return;
        
        rootCG = new CanvasGroupController(this, root);
        previewPanelCG = new CanvasGroupController(this, previewPanel);
        
        GalleryConfig.Load();
        
        GetAllGalleryImages();
    }

    /// <summary>
    /// 打开图库菜单界面。
    /// 若尚未初始化则先进行初始化操作。
    /// </summary>
    public void Open()
    {
        if (!initialized)
            Initialize();
        
        rootCG.Show();
        rootCG.SetInteractableState(true);
    }

    /// <summary>
    /// 关闭图库菜单界面。
    /// </summary>
    public void Close()
    {
        rootCG?.Hide();
        rootCG.SetInteractableState(false);
    }
    
    /// <summary>
    /// 从Resources目录中加载所有图库图片资源。
    /// </summary>
    private void GetAllGalleryImages()
    {
        galleryImages = Resources.LoadAll<Texture>(FilePaths.resources_gallery);
    }

    /// <summary>
    /// 对图库菜单进行初始化设置，包括构建导航栏和加载第一页内容。
    /// </summary>
    private void Initialize()
    {
        initialized = true;
        
        ConstructNavBar();
        
        LoadPage(1);
    }

    /// <summary>
    /// 构建底部页面选择导航栏。
    /// 根据总图片数量计算所需页数，并为每一页创建一个对应的按钮。
    /// </summary>
    private void ConstructNavBar()
    {
        int totalImages = galleryImages.Length;

        // 计算总页数
        maxPages = (int)Mathf.Ceil((float)totalImages / previewsPerPage);
        // 确定实际要显示的按钮数量，不超过最大按钮限制或总页数
        int pagelimit = PAGE_BUTTON_LIMIT < maxPages ? PAGE_BUTTON_LIMIT : maxPages;

        for (int i = 1; i <= pagelimit; i++)
        {
            GameObject buttonOB = Instantiate(panelSelectionButtonPrefab.gameObject,
                panelSelectionButtonPrefab.transform.parent);
            buttonOB.SetActive(true);
            
            Button button = buttonOB.GetComponent<Button>();
            TextMeshProUGUI txt = buttonOB.GetComponentInChildren<TextMeshProUGUI>();

            buttonOB.name = i.ToString();
            int page = i;
            button.onClick.AddListener(() =>
            {
                LoadPage(page);
            });
            txt.text = i.ToString();
        }
        
        // 如果按钮数量小于总页数，则显示前后翻页按钮
        prevButton.gameObject.SetActive(pagelimit < maxPages);
        nextButton.gameObject.SetActive(pagelimit < maxPages);
        
        nextButton.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 加载指定页码的内容到预览按钮上。
    /// 每个按钮根据是否解锁决定其外观及点击事件。
    /// </summary>
    /// <param name="pageNumber">要加载的页码（从1开始）</param>
    private void LoadPage(int pageNumber)
    {
        int startingIndex = (pageNumber - 1) * previewsPerPage;

        for (int i = 0; i < previewsPerPage; i++)
        {
            int index = i + startingIndex;
            Button button = galleryPreviewButtons[i];
            
            button.onClick.RemoveAllListeners();

            if (index >= galleryImages.Length)
            {
                button.transform.parent.gameObject.SetActive(false);
                continue;
            }
            else
            {
                button.transform.parent.gameObject.SetActive(true);
                RawImage renderer = button.targetGraphic as RawImage;
                Texture previewImage = galleryImages[index];

                if (GalleryConfig.ImageIsUnlocked(previewImage.name))
                {
                    renderer.color = Color.white;
                    renderer.texture = previewImage;
                    button.onClick.AddListener(() =>
                    {
                        ShowPreviewImage(previewImage);
                    });
                }
                else
                {
                    renderer.color = Color.black;
                    renderer.texture = null;
                }
            }
        }

        // 记录当前页码
        selectedPage = pageNumber;
    }

    /// <summary>
    /// 显示给定纹理的大图预览面板。
    /// </summary>
    /// <param name="image">需要显示的纹理图像</param>
    private void ShowPreviewImage(Texture image)
    {
        RawImage renderer = previewButton.targetGraphic as RawImage;
        renderer.texture = image;
        previewPanelCG.Show();
        previewPanelCG.SetInteractableState(true);
    }

    /// <summary>
    /// 隐藏当前的大图预览面板。
    /// </summary>
    public void HidePreviewImage()
    {
        previewPanelCG.Hide();
        previewPanelCG.SetInteractableState(false);
    }

    /// <summary>
    /// 跳转到下一页
    /// </summary>
    public void ToNextPage()
    {
        if (selectedPage < maxPages)
            LoadPage(selectedPage + 1);
    }
    
    /// <summary>
    /// 跳转到上一页
    /// </summary>
    public void ToPreviousPage()
    {
        if (selectedPage > 1)
            LoadPage(selectedPage - 1);
    }
}