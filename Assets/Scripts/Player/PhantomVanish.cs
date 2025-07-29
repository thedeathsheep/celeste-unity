using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 幻影消失控制器 - 控制冲刺幻影的淡出效果
/// 实现冲刺时留下的幻影逐渐消失的视觉效果
/// </summary>
public class PhantomVanish : MonoBehaviour
{
    private SpriteRenderer sprite; // 精灵渲染器组件

    [SerializeField] private int lifeTime = 15; // 幻影的生命周期（帧数）
    private int countdown; // 倒计时计数器

    public bool facingLeft; // 是否面向左侧

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>(); // 获取精灵渲染器组件
        countdown = lifeTime; // 初始化倒计时

        // 根据朝向翻转幻影
        if (facingLeft) // 如果面向左侧
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 180, this.transform.eulerAngles.z); // 旋转180度
        }
        else // 如果面向右侧
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 0, this.transform.eulerAngles.z); // 保持0度
        }
    }

    /// <summary>
    /// 固定更新方法，处理幻影的淡出效果
    /// </summary>
    void FixedUpdate()
    {
        if (countdown > 0) // 如果倒计时还未结束
        {
            // 使用平方函数计算透明度，实现平滑的淡出效果
            // 透明度从1逐渐减少到0
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Pow((float)countdown / (float)lifeTime, 2));

            countdown--; // 倒计时递减
        }
        else // 如果倒计时结束
        {
            Destroy(this.gameObject); // 销毁幻影对象
        }
    }
}
