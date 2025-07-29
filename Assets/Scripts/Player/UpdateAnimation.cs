using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画更新控制器 - 控制Madeline的动画状态和头发动画
/// 根据玩家的移动状态更新动画和视觉效果
/// </summary>
public class UpdateAnimation : MonoBehaviour
{
    private Rigidbody2D rb; // 刚体组件
    private Animator anim; // 动画控制器组件
    private SpriteRenderer sprite; // 精灵渲染器组件

    private enum AnimationState { idle, walk, jump, fall, dash, climb }; // 动画状态枚举：待机、行走、跳跃、下落、冲刺、攀爬
    private AnimationState state; // 当前动画状态

    [SerializeField] private PlayerMovement playerMovement; // 玩家移动组件引用（可在Inspector中设置）

    private int lowStaminaTimer = 0; // 低体力闪烁计时器

    // 头发动画相关
    [SerializeField] private HairAnchor hairAnchor; // 头发锚点组件（可在Inspector中设置）

    [Header("Hair Animation Offsets")] // 头发动画偏移量设置
    [SerializeField] private Vector2 idleOffset; // 待机时的头发偏移（可在Inspector中调整）
    [SerializeField] private Vector2 walkOffset; // 行走时的头发偏移（可在Inspector中调整）
    [SerializeField] private Vector2 jumpOffset; // 跳跃时的头发偏移（可在Inspector中调整）
    [SerializeField] private Vector2 fallOffset; // 下落时的头发偏移（可在Inspector中调整）
    [SerializeField] private Vector2 dashOffset; // 冲刺时的头发偏移（可在Inspector中调整）

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取刚体组件
        anim = GetComponent<Animator>(); // 获取动画控制器组件
        sprite = GetComponent<SpriteRenderer>(); // 获取精灵渲染器组件
    }

    /// <summary>
    /// 固定更新方法，处理动画状态更新和头发动画
    /// </summary>
    void FixedUpdate()
    {
        // 朝向翻转
        if (playerMovement.facingLeft) // 如果面向左侧
        {
            transform.localScale = new Vector2(-1, transform.localScale.y); // 水平翻转
        }
        else // 如果面向右侧
        {
            transform.localScale = new Vector2(1, transform.localScale.y); // 正常朝向
        }

        // 更新动画状态
        if (playerMovement.wallGrabbed) // 如果正在抓取墙壁
        {
            state = AnimationState.climb; // 设置为攀爬状态

            if (Mathf.Abs(playerMovement.dirY) > .1f) // 如果正在攀爬（垂直输入）
            {
                anim.speed = 1f; // 正常播放动画
            }
            else // 如果静止不动
            {
                anim.speed = 0f; // 暂停动画
            }
        }
        else if (playerMovement.slidingOnWall) // 如果正在墙壁上滑行
        {
            state = AnimationState.climb; // 设置为攀爬状态
            anim.speed = 0f; // 暂停动画
        }
        else // 其他状态
        {
            anim.speed = 1f; // 正常播放动画

            if (playerMovement.isDashing) // 如果正在冲刺
            {
                state = AnimationState.dash; // 设置为冲刺状态
            }
            else if (rb.velocity.y > .1f) // 如果向上移动
            {
                state = AnimationState.jump; // 设置为跳跃状态
            }
            else if (rb.velocity.y < -.1f) // 如果向下移动
            {
                state = AnimationState.fall; // 设置为下落状态
            }
            else if (playerMovement.IsGrounded()) // 如果着地
            {
                if (rb.velocity.x != 0) // 如果有水平移动
                {
                    state = AnimationState.walk; // 设置为行走状态
                }
                else // 如果静止不动
                {
                    state = AnimationState.idle; // 设置为待机状态
                }
            }
        }

        anim.SetInteger("state", (int)state); // 将动画状态传递给动画控制器

        // 更新头发偏移量
        Vector2 currentOffset = Vector2.zero; // 当前偏移量

        if (state == AnimationState.idle) // 待机状态
        {
            currentOffset = idleOffset;
        }
        else if (state == AnimationState.walk) // 行走状态
        {
            currentOffset = walkOffset;
        }
        else if (state == AnimationState.jump) // 跳跃状态
        {
            currentOffset = jumpOffset;
        }
        else if (state == AnimationState.fall) // 下落状态
        {
            currentOffset = fallOffset;
        }
        else if (state == AnimationState.dash) // 冲刺状态
        {
            currentOffset = dashOffset;
        }
        else if (state == AnimationState.climb || playerMovement.slidingOnWall) // 攀爬或滑行状态的头发移动
        {
            if (rb.velocity.y > .1f) // 如果向上移动
            {
                currentOffset = jumpOffset; // 使用跳跃偏移
            }
            else if (rb.velocity.y < -.1f) // 如果向下移动
            {
                currentOffset = fallOffset; // 使用下落偏移
            }
        }
        else if (state == AnimationState.climb) // 攀爬状态
        {
            if (playerMovement.dirY > .1f) // 如果向上攀爬
            {
                currentOffset = jumpOffset; // 使用跳跃偏移
            }
            else if (playerMovement.dirY < -.1f) // 如果向下攀爬
            {
                currentOffset = fallOffset; // 使用下落偏移
            }
            else // 如果静止攀爬
            {
                currentOffset = idleOffset; // 使用待机偏移
            }
        }

        if (playerMovement.facingLeft) // 如果面向左侧
        {
            currentOffset = new Vector2(-currentOffset.x, currentOffset.y); // 水平翻转偏移量
        }

        hairAnchor.partOffset = currentOffset; // 设置头发锚点的偏移量

        // 低体力闪烁效果
        if (playerMovement.staminaLeft < 60f) // 如果体力低于60
        {
            if (lowStaminaTimer % (4 + (int)(playerMovement.staminaLeft / 2)) <= 1) // 体力越低闪烁越快
            {
                sprite.color = Color.red; // 显示红色
            }
            else
            {
                sprite.color = Color.white; // 显示白色
            }

            if (lowStaminaTimer < 60) // 闪烁计时器
            {
                lowStaminaTimer++; // 计时器递增
            }
            else
            {
                lowStaminaTimer = 0; // 重置计时器
            }
        }
        else // 如果体力充足
        {
            lowStaminaTimer = 0; // 重置计时器
            sprite.color = Color.white; // 显示白色
        }
    }
}
