using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冲刺颜色控制器 - 根据玩家的冲刺状态改变精灵颜色
/// 用于视觉反馈，显示玩家当前的冲刺能力状态
/// </summary>
public class DashColor : MonoBehaviour
{
    [SerializeField] private GameObject player; // 玩家对象引用（可在Inspector中设置）
    private PlayerMovement playerMovement; // 玩家移动组件引用
    private SpriteRenderer sprite; // 当前对象的精灵渲染器组件

    private int dashLeft; // 判断冲刺次数
    private bool isDashing; // 是否正在冲刺

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>(); // 获取当前对象的精灵渲染器组件
        playerMovement = player.GetComponent<PlayerMovement>(); // 获取玩家的移动组件
    }

    /// <summary>
    /// 每帧更新方法，根据冲刺状态更新颜色
    /// </summary>
    void Update()
    {
        // 从玩家移动组件获取当前的冲刺状态
        dashLeft = playerMovement.dashLeft; // 获取剩余冲刺次数
        isDashing = playerMovement.isDashing; // 获取是否正在冲刺

        // 根据剩余冲刺次数设置不同的颜色
        if (dashLeft == 0) // 没有进行过冲刺次数：蓝色
        {
            sprite.color = new Color(67 / 255f, 163 / 255f, 245 / 255f);
        }
        else if (dashLeft == 1) // 进行过1次冲刺：红色
        {
            sprite.color = new Color(172 / 255f, 32 / 255f, 32 / 255f);
        }
        else // 进行过多次冲刺：绿色
        {
            sprite.color = Color.green;
        }
    }
}
