为下述类/功能生成测试：
上下文
- 被测目标：{类/方法/组件}
- 测试类型：{EditMode/PlayMode}
- 关注点：{正确性/边界/异常/性能回归}

要求
- 使用 NUnit（必要时 PlayMode 用 [UnityTest]）。
- 覆盖正常/边界/异常路径；命名 Method_ShouldDoX_WhenY。
- 若依赖 MonoBehaviour，提供简单 Test Scene/临时对象构造；测试结束清理对象。