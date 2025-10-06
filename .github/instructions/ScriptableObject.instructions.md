当用户需要数据资产/配置：
- 生成从 ScriptableObject 派生的不可变/可序列化数据容器。
- 提供 [CreateAssetMenu] 便于在 Project 视图创建；在 OnValidate 做基本校验与默认值修正。
- 不要包含场景/运行时状态；仅放可序列化配置。

模板示例：
```csharp
using UnityEngine;

namespace Company.Product.Config
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Configs/Enemy", order = 0)]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Min(1)] public int maxHp = 10;
        [Min(0f)] public float moveSpeed = 2f;
        [Tooltip("掉落物预制体")]
        public GameObject dropPrefab;

        private void OnValidate()
        {
            if (maxHp < 1) maxHp = 1;
            if (moveSpeed < 0f) moveSpeed = 0f;
        }
    }
}
```