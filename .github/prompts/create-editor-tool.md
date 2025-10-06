创建一个 Unity 编辑器工具：
上下文
- 目标：{批量重命名/生成材质/校验资源/统计引用/...}
- 作用范围：{选中资源/整个工程/指定目录}
- 撤销与安全：{需要 Undo/操作前确认}

要求
- 生成 EditorWindow（IMGUI 或 UIElements，二选一）
- 提供菜单入口、最小窗口尺寸
- 对资源改动使用 Undo.RecordObject/AssetDatabase.SaveAssets
- 日志清晰、失败可回滚