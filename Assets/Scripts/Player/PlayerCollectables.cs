using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家收集品控制器 - 管理玩家收集的草莓状态
/// 处理玩家死亡时收集品的重置逻辑
/// </summary>
public class PlayerCollectables : MonoBehaviour
{
    public List<GameObject> strawberries = new List<GameObject>(); // 玩家收集的草莓列表

    private DeathAndRespawn deathResp; // 死亡重生组件引用

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        strawberries.Add(this.gameObject); // 将玩家自身添加到草莓列表中
        deathResp = GetComponent<DeathAndRespawn>(); // 获取死亡重生组件
    }

    /// <summary>
    /// 每帧更新方法，处理死亡时的收集品重置
    /// </summary>
    private void Update()
    {
        if (deathResp.dead) // 如果玩家处于死亡状态
        {
            if (strawberries != new List<GameObject>()) // 检查草莓列表是否不为空
            {
                foreach (GameObject strawberry in strawberries) // 遍历所有收集的草莓
                {
                    if (!strawberry.CompareTag("Player")) // 如果不是玩家对象本身
                    {
                        // 将草莓状态设置为-1，使其回到原始位置
                        strawberry.GetComponent<StrawberryCollect>().state = -1;
                    }
                }
            }
            strawberries = new List<GameObject>(); // 清空草莓列表
            strawberries.Add(this.gameObject); // 只保留玩家自身在列表中
        }
    }
}
