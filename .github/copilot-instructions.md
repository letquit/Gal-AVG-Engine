项目背景
- Unity 版本：Unity 6000.1.17f1
- C# 版本：与项目 Player 设置一致；不要使用项目未启用的语言特性。
- 输入系统：优先“新输入系统（Input System）”；如项目使用旧 Input，请先澄清再生成。
- Addressables：若涉及资源加载，优先使用 Addressables；未配置时降级为 Resources 但需提示风险。
- 脚本运行时：.NET 4.x 兼容；如为 .NET Standard 2.1 请避免不兼容 API。

代码结构与约定
- 命名空间：Company.Product.Feature（若仓库已有命名空间，请延续）。
- 目录：Runtime/、Editor/、Tests/（PlayMode/、EditMode/）；对应创建 asmdef，限制引用范围，避免循环依赖。
- 组件脚本模板：仅保留必要生命周期方法；字段使用 [SerializeField] + [Tooltip]/[Header]；必要时 [DisallowMultipleComponent]/[RequireComponent].
- 资源加载：在热路径外预加载并缓存句柄；场景切换释放/卸载。
- 异步：可使用协程；如使用 async/await，确保不阻塞主线程并考虑 PlayerLoop 上下文。
- 反射/动态生成：编辑器阶段可用；运行时慎用。
- 生成代码时，如果存在歧义（如旧/新输入系统、地址加载策略、平台分支）先提出 1–3 个澄清问题，再给实现。

测试策略
- Runtime 纯逻辑：NUnit 单测（EditMode）。
- 与 Unity 生命周期耦合：UnityTest（PlayMode），必要时用 TestScene 或 Test Prefab。
- Mock 策略：对外部依赖抽象接口后用替身；不引入第三方 mocking 框架时，手写 stub/fake。

提交与评审
- PR 中附带变更说明、测试覆盖点、对性能/内存的影响说明；大改动优先分小步可回滚的提交。