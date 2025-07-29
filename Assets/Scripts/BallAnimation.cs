using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 球体动画控制器 - 控制围绕玩家旋转的球体效果
/// 用于实现冲刺时的视觉效果
/// </summary>
public class BallAnimation : MonoBehaviour
{
    private GameObject player; // 玩家对象引用
    private SpriteRenderer sprite; // 球体的精灵渲染器
    [SerializeField] private float distanceSpeed; // 距离变化速度（可在Inspector中调整）
    [SerializeField] private float angularSpeed; // 角速度（旋转速度）
    private float maxDistance = 1.5f; // 球体与玩家的最大距离
    private float distance; // 当前球体与玩家的距离
    [HideInInspector] public float angle; // 球体相对于玩家的角度（隐藏于Inspector）
    private int timer = 0; // 计时器，控制球体的生命周期
    [HideInInspector] public bool reverse = false; // 是否反向运动（隐藏于Inspector）

    void Start()
    {
        player = GameObject.Find("Player"); // 查找场景中的玩家对象
        sprite = GetComponent<SpriteRenderer>(); // 获取球体的精灵渲染器组件

        transform.localScale *= 0.8f; // 将球体大小缩小到原来的80%
        if (reverse == false) // 如果不是反向模式：球体从玩家位置开始向外移动
        {
            distance = 0f; // 初始距离为0（从玩家位置开始）
        }
        else // 如果是反向模式：球体从远处向玩家移动
        {
            distance = 1f; // 初始距离为1
            transform.localScale *= Mathf.Pow(0.8f, 6); // 将球体进一步缩小（0.8的6次方）
        }
    }

    void FixedUpdate()
    {
        // 更新球体位置：玩家位置 + 根据距离和角度计算的位置偏移
        transform.position = (Vector2)player.transform.position + Rotation(distance * Vector2.right, angle);

        // 更新球体与玩家的距离
        if (reverse == false) // 正向模式：距离从0增加到最大距离
        {
            if (distance < maxDistance) // 检查距离是否未达到最大值
            {
                // 根据计时器和速度计算距离，使用平方增长
                distance = maxDistance * distanceSpeed * (float)timer / 12;
            }
            else // 如果距离已达到最大值
            {
                distance = maxDistance; // 保持最大距离
            }
        }
        else // 反向模式：距离从最大距离减少到0
        {
            if (distance > .1f) // 检查距离是否还足够大
            {
                // 根据计时器和速度计算距离，使用平方减少
                distance = maxDistance * distanceSpeed * (float)(12 - timer) / 12;
            }
            else // 如果距离已经很小
            {
                distance = 0; // 设置为0
            }
        }

        // 更新球体的旋转角度
        if (reverse == false) // 正向模式：顺时针旋转
        {
            angle += angularSpeed * Time.deltaTime;
        }
        else // 反向模式：逆时针旋转
        {
            angle -= angularSpeed * Time.deltaTime;
        }

        // 角度标准化：确保角度在[0, 2π)范围内
        if (angle > 2 * Mathf.PI) // 如果角度超过2π
        {
            angle = angle % (2 * Mathf.PI); // 取模运算，使角度属于[0, 2π)
        }

        // 根据玩家的冲刺状态改变球体颜色
        if (timer % 14 < 5) // 在特定时间段内改变颜色（每14帧的前5帧）
        {
            if (player.GetComponent<PlayerMovement>().dashLeft == 0) // 没有冲刺次数：蓝色
            {
                sprite.color = new Color(67 / 255f, 163 / 255f, 245 / 255f);
            }
            else if (player.GetComponent<PlayerMovement>().dashLeft == 1) // 还有1次冲刺：红色
            {
                sprite.color = new Color(172 / 255f, 32 / 255f, 32 / 255f);
            }
            else // 其他情况：绿色
            {
                sprite.color = Color.green;
            }
        }
        else // 其他时间段：恢复为白色
        {
            sprite.color = Color.white;
        }

        // 控制球体大小的变化
        if (reverse == false) // 正向模式：球体逐渐变小
        {
            if (timer >= 14) // 从第14帧开始缩小
            {
                transform.localScale *= 0.8f; // 每次缩小到原来的80%
            }
        }
        else // 反向模式：球体逐渐变大
        {
            if (timer <= 5) // 在前5帧内放大
            {
                transform.localScale /= 0.8f; // 每次放大到原来的125%（1/0.8）
            }
        }

        // 生命周期管理
        if (timer < 20) // 如果计时器未达到20
        {
            timer++; // 计时器递增
        }
        else // 如果计时器达到20
        {
            Destroy(this.gameObject); // 销毁球体对象
        }
    }

    /// <summary>
    /// 计算两个位置之间的方向向量
    /// </summary>
    /// <param name="position1">起始位置</param>
    /// <param name="position2">目标位置</param>
    /// <returns>单位方向向量</returns>
    private Vector2 VectorDirection(Vector2 position1, Vector2 position2)
    {
        return (position2 - position1) / Vector2.Distance(position1, position2);
    }

    /// <summary>
    /// 将向量绕原点旋转指定角度
    /// </summary>
    /// <param name="vector">要旋转的向量</param>
    /// <param name="angle">旋转角度（弧度）</param>
    /// <returns>旋转后的向量</returns>
    private Vector2 Rotation(Vector2 vector, float angle)
    {
        // 使用旋转矩阵公式计算旋转后的向量
        return new Vector2(
            Mathf.Cos(angle) * vector.x + Mathf.Sin(angle) * vector.y, 
            -Mathf.Sin(angle) * vector.x + Mathf.Cos(angle) * vector.y
        );
    }
}

