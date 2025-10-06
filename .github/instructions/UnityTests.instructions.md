当用户需要测试：
- EditMode：测试纯逻辑（不依赖场景/帧循环）。
- PlayMode：涉及 MonoBehaviour/协程/场景；使用 [UnityTest] 返回 IEnumerator 以跨帧断言。
- 不访问网络/文件系统；对时间敏感逻辑用虚拟时间或可注入时钟。

模板示例（EditMode + PlayMode）：
```csharp
// EditMode 示例
using NUnit.Framework;

public class MathUtilTests
{
    [Test]
    public void Clamp_ShouldLimitValue_WhenOutOfRange()
    {
        Assert.AreEqual(10, Mathf.Clamp(12, 0, 10));
    }
}
```

```csharp
// PlayMode 示例
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MoverPlayModeTests
{
    [UnityTest]
    public IEnumerator Mover_ShouldMoveRight_WhenPositiveInput()
    {
        var go = new GameObject("mover");
        var mover = go.AddComponent<Company.Product.Feature.ExampleMover>();
        mover.SetHorizontal(1f);

        var start = go.transform.position.x;
        yield return null; // 等待一帧
        Assert.Greater(go.transform.position.x, start);
    }
}
```