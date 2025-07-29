using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 草莓收集控制器 - 控制草莓的收集、跟随和消失逻辑
/// 管理草莓的状态变化和跟随玩家的行为
/// </summary>
public class StrawberryCollect : MonoBehaviour
{
    public int state = 0; // 草莓状态：0=未收集，1=已收集跟随，2=消失动画，-1=返回初始位置
    [HideInInspector] public Vector2 initialPosition; // 初始位置（隐藏于Inspector）
    private Vector2 relativeOffset; // 相对偏移量
    private List<GameObject> strawberries; // 草莓列表引用

    [SerializeField] private GameObject player; // 玩家对象引用（可在Inspector中设置）
    [SerializeField] private Vector2 offset; // 跟随偏移量（可在Inspector中调整）
    [SerializeField] private float lerpSpeed = 15f; // 插值速度（可在Inspector中调整）
    private Animator anim; // 动画控制器组件

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    private void Start()
    {
        strawberries = player.GetComponent<PlayerCollectables>().strawberries; // 获取玩家的草莓列表
        anim = GetComponent<Animator>(); // 获取动画控制器组件
        initialPosition = transform.position; // 保存初始位置
    }

    /// <summary>
    /// 碰撞检测方法，处理与玩家的碰撞
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == 0 && collision.gameObject.CompareTag("Player")) // 如果草莓未收集且碰撞对象是玩家
        {
            state = 1; // 设置为已收集状态
            player.GetComponent<PlayerCollectables>().strawberries.Add(this.gameObject); // 将草莓添加到玩家收集列表
        }
    }

    /// <summary>
    /// 固定更新方法，处理草莓的移动逻辑
    /// </summary>
    void FixedUpdate()
    {
        if (state == -1) // 返回初始位置
        {
            if (Vector2.Distance(transform.position, initialPosition) > .1f) // 检查草莓是否离初始位置太远
            {
                transform.position = Vector2.Lerp(transform.position, initialPosition, Time.deltaTime * lerpSpeed); // 使用插值平滑移动到目标位置
            }
            else // 确保草莓正确放置
            {
                GetComponent<VerticalOscillation>().timer = 0; // 重置垂直振荡
                transform.position = initialPosition; // 设置到初始位置
                state = 0; // 重置状态为未收集
            }
        }
        else if (state == 1) // 跟随目标
        {
            strawberries = player.GetComponent<PlayerCollectables>().strawberries; // 更新草莓列表

            int index = strawberries.IndexOf(this.gameObject); // 获取当前草莓在列表中的索引
            GameObject target = strawberries[index - 1]; // 获取跟随目标（前一个草莓或玩家）

            if (player.GetComponent<PlayerMovement>().facingLeft) // 如果玩家面向左侧
            {
                relativeOffset = new Vector2(offset.x, offset.y); // 使用原始偏移
            }
            else // 如果玩家面向右侧
            {
                relativeOffset = new Vector2(-offset.x, offset.y); // 水平翻转偏移
            }

            Vector2 targetPosition = (Vector2)target.transform.position + relativeOffset; // 计算目标位置
            Vector2 newPositionLerp = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed); // 使用插值计算新位置

            transform.position = newPositionLerp; // 更新位置

            if (Vector2.Distance(transform.position, targetPosition) < .2f && player.GetComponent<PlayerMovement>().IsVeryGrounded() && target == player) // 如果接近目标、玩家稳定着地且目标是玩家
            {
                state = 2; // 触发消失动画
                anim.SetBool("Vanish Animation", true); // 激活消失动画
            }
        }
    }

    /// <summary>
    /// 销毁草莓的方法
    /// </summary>
    public void DestroyStrawberry()
    {
        if (transform.parent != null) // 如果有父对象
        {
            //Destroy(transform.parent.gameObject); // 销毁父对象（已注释）
        }
        strawberries.Remove(this.gameObject); // 从草莓列表中移除
        Destroy(this.gameObject); // 销毁当前对象
    }
}
