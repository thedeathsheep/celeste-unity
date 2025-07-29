using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弹射玩家控制器 - 控制弹簧等物体弹射玩家的逻辑
/// 处理玩家与弹射物体的碰撞和弹射动画
/// </summary>
public class LauchPlayer : MonoBehaviour
{
    [SerializeField] private float bounceSpeed; // 弹射速度（可在Inspector中调整）

    private Animator anim; // 动画控制器组件

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    private void Start()
    {
        anim = GetComponent<Animator>(); // 获取动画控制器组件
    }

    /// <summary>
    /// 碰撞检测方法，处理玩家与弹射物体的碰撞
    /// </summary>
    /// <param name="collision">碰撞对象</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GetComponent<Animator>().SetTrigger("Launched"); // 播放弹射动画

        GameObject player = collision.gameObject; // 获取玩家对象
        player.GetComponent<Rigidbody2D>().velocity = new Vector2(0, bounceSpeed); // 弹射玩家

        player.GetComponent<PlayerMovement>().ResetDashAndGrab(); // 重置玩家的冲刺和抓取状态
    }
}
