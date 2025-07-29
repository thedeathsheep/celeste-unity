using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 停止对象控制器 - 控制玩家在屏幕转换时的停止和恢复逻辑
/// 处理玩家在屏幕切换时的暂停和重生点更新
/// </summary>
public class StopObject : MonoBehaviour
{
    public bool stopped = false; // 是否处于停止状态
    private int frameDuration; // 停止持续时间（帧数）
    private int timer = 0; // 计时器

    private Vector2 storedVelocity; // 存储的速度

    private bool upperTransition; // 是否为向上转换
    private GameObject transitionCamera; // 转换摄像机

    private Rigidbody2D rb; // 刚体组件
    private PlayerMovement playerMovement; // 玩家移动组件
    private DeathAndRespawn deathResp; // 死亡重生组件

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取刚体组件
        playerMovement = GetComponent<PlayerMovement>(); // 获取玩家移动组件
        deathResp = GetComponent<DeathAndRespawn>(); // 获取死亡重生组件
    }

    /// <summary>
    /// 固定更新方法，处理停止状态的逻辑
    /// </summary>
    private void FixedUpdate()
    {
        if (stopped) // 如果处于停止状态
        {
            if (timer < frameDuration) // 如果计时器未达到持续时间
            {
                timer++; // 计时器递增
            }
            else // 如果停止时间结束
            {
                timer = 0; // 重置计时器
                stopped = false; // 停止停止状态

                if (upperTransition && transitionCamera.activeInHierarchy && deathResp.dead == false) // 如果是向上转换且摄像机激活且玩家未死亡
                {
                    storedVelocity = new Vector2(0f, 11f); // 设置向上的速度
                    playerMovement.ResetDashAndGrab(); // 重置冲刺和抓取状态
                }

                rb.velocity = storedVelocity; // 恢复存储的速度
                rb.gravityScale = playerMovement.gravityScale; // 恢复重力缩放
                playerMovement.dashLeft = playerMovement.dashNumber; // 恢复冲刺次数

                // 更新重生点
                List<GameObject> spawnpointsInCamera = new List<GameObject>(); // 摄像机内的重生点列表

                // 只选择摄像机内的重生点
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Respawn")) // 遍历所有重生点
                {
                    if (VisibleInCamera(obj)) // 如果重生点在摄像机内可见
                    {
                        spawnpointsInCamera.Add(obj); // 添加到列表
                    }
                }

                // 将最近的重生点设置为新的重生点
                deathResp.respawnPosition = deathResp.Nearest(spawnpointsInCamera.ToArray()); // 找到最近的重生点
            }
        }
    }

    /// <summary>
    /// 停止玩家的方法
    /// </summary>
    /// <param name="timeDuration">停止持续时间</param>
    /// <param name="up">是否为向上转换</param>
    /// <param name="camera">转换摄像机</param>
    public void Stop(float timeDuration, bool up, GameObject camera)
    {
        frameDuration = (int)(timeDuration / Time.deltaTime); // 计算停止帧数
        upperTransition = up; // 设置转换方向
        transitionCamera = camera; // 设置转换摄像机

        stopped = true; // 激活停止状态
        timer = 0; // 重置计时器

        storedVelocity = GetComponent<Rigidbody2D>().velocity; // 存储当前速度
        GetComponent<Rigidbody2D>().velocity = Vector2.zero; // 停止移动
        GetComponent<Rigidbody2D>().gravityScale = 0f; // 停止重力
    }

    /// <summary>
    /// 检查对象是否在摄像机内可见
    /// </summary>
    /// <param name="gameObject">要检查的游戏对象</param>
    /// <returns>如果对象在摄像机内可见则返回true</returns>
    private bool VisibleInCamera(GameObject gameObject)
    {
        if (GameObject.ReferenceEquals(gameObject.GetComponent<SpawnpointInitialization>().screen, transitionCamera.transform.parent.gameObject)) // 检查对象的屏幕是否与转换摄像机相同
        {
            return true; // 在摄像机内可见
        }
        else
        {
            return false; // 不在摄像机内可见
        }
    }
}
