当用户要求创建/重构 MonoBehaviour 组件时：
- 仅生成需要的生命周期方法；按需添加 [DisallowMultipleComponent]/[RequireComponent].
- 字段：private + [SerializeField]；对外只读属性；重要字段加 [Tooltip]/[Header].
- 在 Awake/OnEnable 缓存组件引用；Update 中不做重复查找/分配。
- 使用 Time.deltaTime 处理随帧变化的量；物理放 FixedUpdate；相机/跟随放 LateUpdate。
- 事件与订阅：在 OnEnable 订阅，在 OnDisable 退订；在 OnDestroy 释放引用/句柄（协程、Addressables 等）。

模板示例：
```csharp
using UnityEngine;

namespace Company.Product.Feature
{
    [DisallowMultipleComponent]
    // [RequireComponent(typeof(Rigidbody))]
    public sealed class ExampleMover : MonoBehaviour
    {
        [Header("Move Settings")]
        [SerializeField, Tooltip("移动速度（单位/秒）")]
        private float speed = 5f;

        [SerializeField, Tooltip("水平移动输入（-1 ~ 1）")]
        private float horizontal;

        // 缓存引用
        private Transform _tf;

        private void Awake()
        {
            _tf = transform;
        }

        private void Update()
        {
            // 非物理移动：使用 deltaTime
            if (Mathf.Approximately(horizontal, 0f)) return;
            var delta = horizontal * speed * Time.deltaTime;
            _tf.position += new Vector3(delta, 0f, 0f);
        }

        // 提供输入写入接口（由输入层调用）
        public void SetHorizontal(float value) => horizontal = Mathf.Clamp(value, -1f, 1f);
    }
}
```