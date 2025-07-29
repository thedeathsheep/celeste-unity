using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 死亡与重生控制器 - 处理玩家的死亡动画和重生逻辑
/// 包括死亡效果、重生位置计算和状态重置
/// </summary>
public class DeathAndRespawn : MonoBehaviour
{
    private GameObject[] spawnPoints; // 所有重生点对象的数组
    [SerializeField] private GameObject Ball; // 死亡动画球体预制体（可在Inspector中设置）
    private Rigidbody2D rb; // 玩家的刚体组件
    public Vector2 respawnPosition = Vector2.zero; // 重生位置
    [SerializeField] private float deadSpeed = 5f; // 死亡时的弹飞速度

    public bool dead = false; // 死亡状态标志
    private int deathAnimationTimer = 0; // 死亡动画计时器

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取刚体组件
        // 查找所有重生点并找到最近的作为初始重生位置
        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        respawnPosition = Nearest(spawnPoints);
    }

    /// <summary>
    /// 固定更新方法，处理死亡动画和重生逻辑
    /// </summary>
    private void FixedUpdate()
    {
        if (dead) // 如果玩家处于死亡状态
        {
            rb.velocity *= 0.9f; // 逐渐减速，使动画更流畅

            // 死亡动画：创建死亡球体效果
            if (deathAnimationTimer == 6) // 在第6帧创建死亡球体
            {
                for (int i = 0; i < 8; i++) // 创建8个球体，均匀分布在圆周上
                {
                    GameObject ball = Instantiate(Ball, this.transform.position, Quaternion.identity);
                    ball.GetComponent<BallAnimation>().angle = (float)i * Mathf.PI / 4; // 设置每个球体的角度
                }
            }

            // 在球体动画结束后隐藏精灵和头发
            if (deathAnimationTimer > 15)
            {
                foreach (Transform transf in GetComponentsInChildren<Transform>())
                {
                    transf.gameObject.GetComponent<SpriteRenderer>().enabled = false; // 隐藏所有子对象的精灵渲染器
                }
            }
            else
            {
                foreach (Transform transf in GetComponentsInChildren<Transform>())
                {
                    transf.gameObject.GetComponent<SpriteRenderer>().enabled = true; // 显示所有子对象的精灵渲染器
                }
            }

            // 重生逻辑
            if (deathAnimationTimer == 30) // 在第30帧执行重生
            {
                transform.position = respawnPosition; // 将玩家传送到重生点
                transform.localScale = 1f * Vector3.one; // 恢复正常大小
                rb.velocity = Vector2.zero; // 停止所有速度
                GetComponent<PlayerMovement>().dashLeft = GetComponent<PlayerMovement>().dashNumber; // 重置冲刺次数
                GetComponent<PlayerMovement>().staminaLeft = GetComponent<PlayerMovement>().maxStamina; // 重置体力
                GetComponent<PlayerMovement>().ResetDashAndGrab(); // 重置冲刺和抓取状态
                GetComponent<Animator>().SetBool("dead", false); // 关闭死亡动画状态
                GetComponent<BoxCollider2D>().enabled = true; // 重新激活碰撞体

                // 创建反向球体效果（向玩家聚集）
                for (int i = 0; i < 8; i++)
                {
                    GameObject ball = Instantiate(Ball, this.transform.position, Quaternion.identity);
                    ball.GetComponent<BallAnimation>().angle = (float)i * Mathf.PI / 4;
                    ball.GetComponent<BallAnimation>().reverse = true; // 设置为反向模式
                }

                // 刷新所有飞行草莓（如果它们飞走了）
                foreach (GameObject wingedStrawberry in GameObject.FindGameObjectsWithTag("Winged Strawberry"))
                {
                    wingedStrawberry.GetComponent<WingedStrawberry>().Refresh();
                }

                // 重置摄像机
                GetComponent<InitializeActiveCamera>().Start();
            }

            // 计时器和死亡结束
            if (deathAnimationTimer < 50) // 死亡动画总时长50帧
            {
                deathAnimationTimer++; // 计时器递增
            }
            else // 死亡动画结束
            {
                dead = false; // 重置死亡状态
                deathAnimationTimer = 0; // 重置计时器
                GetComponent<PlayerMovement>().ResetDashAndGrab(); // 停止冲刺和抓取状态
                // 重新激活Madeline和头发的渲染
                foreach (Transform transf in GetComponentsInChildren<Transform>())
                {
                    transf.gameObject.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// 碰撞检测方法，处理与尖刺的碰撞
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spike")) // 检查是否与尖刺碰撞
        {
            dead = true; // 触发死亡状态
            rb.gravityScale = 0f; // 停止重力影响

            // 设置死亡时的弹飞速度和方向
            rb.velocity = deadSpeed * Vector2.one; // 初始弹飞速度
            if (collision.transform.position.y > transform.position.y) // 如果尖刺在玩家上方，向下弹飞
            {
                rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
            }
            if (!GetComponent<PlayerMovement>().facingLeft) // 根据玩家朝向调整水平弹飞方向
            {
                rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
            }
            GetComponent<Collider2D>().enabled = false; // 禁用碰撞体
            GetComponent<Animator>().SetBool("dead", true); // 激活死亡动画状态
        }
    }

    /// <summary>
    /// 查找距离玩家最近的重生点
    /// </summary>
    /// <param name="gameObjectList">重生点对象数组</param>
    /// <returns>最近重生点的位置</returns>
    public Vector2 Nearest(GameObject[] gameObjectList)
    {
        int index = 0; // 最近对象的索引
        float minDist = Mathf.Infinity; // 最小距离，初始化为无穷大

        for (int i = 0; i < gameObjectList.Length; i++) // 遍历所有重生点
        {
            float dist = Vector2.Distance(transform.position, gameObjectList[i].transform.position); // 计算距离

            if (dist < minDist) // 如果找到更近的重生点
            {
                index = i; // 更新索引
                minDist = dist; // 更新最小距离
            }
        }

        return gameObjectList[index].transform.position; // 返回最近重生点的位置
    }
}
