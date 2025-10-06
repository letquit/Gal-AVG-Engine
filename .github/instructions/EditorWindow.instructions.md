当用户需要编辑器工具：
- 生成继承 EditorWindow 的工具窗口；提供 [MenuItem] 打开；设置最小尺寸；分离绘制与逻辑。
- 仅在 UNITY_EDITOR 区域内编译；不要依赖运行时资源路径。
- 对项目操作（批量重命名、生成资源等）前先确认，并支持撤消（Undo.RecordObject/AssetDatabase APIs）。

模板示例（IMGUI 简版）：
```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Company.Product.EditorTools
{
    public sealed class ExampleToolWindow : EditorWindow
    {
        private string _search = "";

        [MenuItem("Tools/Example Tool")]
        public static void ShowWindow()
        {
            var win = GetWindow<ExampleToolWindow>("Example Tool");
            win.minSize = new Vector2(360, 200);
            win.Show();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("批量工具", EditorStyles.boldLabel);
                _search = EditorGUILayout.TextField("Search", _search);

                if (GUILayout.Button("执行示例操作"))
                {
                    // TODO: 执行操作（带 Undo）
                    Debug.Log($"Run with keyword: {_search}");
                }
            }
        }
    }
}
#endif
```