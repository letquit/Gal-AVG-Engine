为 Unity 生成一个组件（MonoBehaviour）：
上下文
- 目标：{一句话描述功能}
- 输入来源：{新输入系统/旧输入系统/无输入}
- 依赖组件：{Rigidbody/CharacterController/Camera/...}
- 性能要求：{在 Update 低分配/可使用对象池/可接受少量分配}
- 测试：{需要 EditMode/PlayMode 样例测试 否/是}

要求
- 仅包含必要生命周期；在 Awake/OnEnable 缓存组件。
- 字段使用 private + [SerializeField]，提供只读属性。
- 对输入抽象为可注入接口（便于测试）。
- 提供最小使用示例或调用片段。