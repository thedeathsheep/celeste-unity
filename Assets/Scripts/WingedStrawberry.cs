using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 飞行草莓控制器 - 控制飞行草莓的行为和动画
/// 当玩家冲刺时，飞行草莓会飞离屏幕
/// </summary>
public class WingedStrawberry : MonoBehaviour
{
    private bool screenFound = false; // 是否找到屏幕对象
    public GameObject screen; // 屏幕对象引用
    private Vector2 initialPosition; // 初始位置
    private bool fly = false; // 是否正在飞行
    [SerializeField] private GameObject player; // 玩家对象引用（可在Inspector中设置）
    private Rigidbody2D rb; // 刚体组件
    private int timer; // 计时器
    private bool pickedUp = false; // 是否被收集
    private GameObject strawberry; // 草莓子对象

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        initialPosition = transform.position; // 保存初始位置
        rb = GetComponent<Rigidbody2D>(); // 获取刚体组件

        strawberry = transform.GetChild(0).gameObject; // 获取草莓子对象
    }

    /// <summary>
    /// 固定更新方法，处理飞行草莓的逻辑
    /// </summary>
    void FixedUpdate()
    {
        if (pickedUp == false) // 如果未被收集
        {
            if (fly == false) // 如果未开始飞行
            {
                // 玩家正在冲刺
                if (player.GetComponent<PlayerMovement>().isDashing) // 检查玩家是否在冲刺
                {
                    fly = true; // 开始飞行
                    timer = 0; // 重置计时器
                }
            }
            else // 飞离屏幕
            {
                if (timer < 15) // 前15帧的飞行动画
                {
                    // 稍微向上移动
                    transform.position = Vector2.Lerp(transform.position, initialPosition + 1.75f * Vector2.up, 12f * Time.deltaTime);

                    // 摆动动画
                    transform.localEulerAngles = new Vector3(0f, 0f, -30f * Mathf.Cos(Mathf.PI / 7 * timer));

                    timer++; // 计时器递增
                }
                else if (transform.position.y < screen.transform.position.y + 6f) // 如果还未飞出屏幕
                {
                    transform.localEulerAngles = Vector3.zero; // 停止摆动
                    rb.velocity += new Vector2(0f, .3f); // 继续向上移动
                }
                else // 如果已飞出屏幕
                {
                    rb.velocity = Vector2.zero; // 停止移动
                    GetComponent<SpriteRenderer>().enabled = false; // 隐藏精灵
                }
            }
        }
        else if (strawberry.GetComponent<StrawberryCollect>().state == 0) // 如果已被收集且草莓状态为0
        {
            Refresh(); // 刷新飞行草莓

            strawberry.GetComponent<SpriteRenderer>().enabled = false; // 隐藏草莓精灵
            strawberry.GetComponent<BoxCollider2D>().enabled = false; // 禁用草莓碰撞体
        }
    }

    /// <summary>
    /// 碰撞检测方法
    /// </summary>
    /// <param name="coll">碰撞对象</param>
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!screenFound && coll.gameObject.CompareTag("Screen")) // 如果未找到屏幕且碰撞对象是屏幕
        {
            screen = coll.gameObject; // 设置屏幕对象
            screenFound = true; // 标记已找到屏幕
        }
        // 被收集
        else if (strawberry.GetComponent<StrawberryCollect>().state == 0 && coll.gameObject.CompareTag("Player")) // 如果草莓状态为0且碰撞对象是玩家
        {
            // 更新子对象
            strawberry.GetComponent<SpriteRenderer>().enabled = true; // 显示草莓精灵
            strawberry.GetComponent<BoxCollider2D>().enabled = true; // 启用草莓碰撞体
            strawberry.GetComponent<StrawberryCollect>().state = 1; // 设置草莓状态为1（已收集）
            strawberry.transform.position = transform.position; // 设置草莓位置
            player.GetComponent<PlayerCollectables>().strawberries.Add(strawberry); // 将草莓添加到玩家收集列表

            // 更新飞行草莓
            pickedUp = true; // 标记已被收集
            GetComponent<SpriteRenderer>().enabled = false; // 隐藏飞行草莓精灵
            GetComponent<BoxCollider2D>().enabled = false; // 禁用飞行草莓碰撞体

            transform.localEulerAngles = Vector3.zero; // 重置旋转
            rb.velocity = Vector2.zero; // 停止移动
        }
    }

    /// <summary>
    /// 刷新飞行草莓状态的方法
    /// </summary>
    public void Refresh()
    {
        if (strawberry.GetComponent<StrawberryCollect>().state == 0) // 如果草莓状态为0
        {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            transform.position = initialPosition;
            fly = false;
            pickedUp = false;
            timer = 0;
        }
    }
}
