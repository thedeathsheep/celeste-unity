using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 头发锚点控制器 - 控制头发部分的跟随和连接效果
/// 实现头发的物理模拟和流畅的跟随动画
/// </summary>
public class HairAnchor : MonoBehaviour
{
    public Vector2 partOffset = Vector2.zero; // 头发部分之间的偏移距离
    public float lerpSpeed = 20f; // 插值速度，控制头发跟随的平滑程度

    private Transform[] hairParts; // 所有头发部分的Transform数组
    private Transform hairAnchor; // 头发锚点的Transform组件

    /// <summary>
    /// 唤醒方法，在对象实例化时调用
    /// </summary>
    private void Awake()
    {
        hairAnchor = GetComponent<Transform>(); // 获取当前对象的Transform组件作为锚点
        hairParts = GetComponentsInChildren<Transform>(); // 获取所有子对象的Transform组件
    }

    /// <summary>
    /// 每帧更新方法，处理头发的跟随逻辑
    /// </summary>
    private void Update()
    {
        Transform pieceToFollow = hairAnchor; // 设置第一个跟随目标为头发锚点

        foreach (Transform hairPart in hairParts) // 遍历所有头发部分
        {
            if (!hairPart.Equals(hairAnchor)) // 检查是否为头发锚点本身
            {
                // 计算目标位置：跟随对象的位置 + 偏移量
                Vector2 targetPosition = (Vector2)pieceToFollow.position + partOffset;
                // 使用插值计算新的位置，实现平滑的跟随效果
                Vector2 newPositionLerp = Vector2.Lerp(hairPart.position, targetPosition, Time.deltaTime * lerpSpeed);

                hairPart.position = newPositionLerp; // 更新头发部分的位置

                pieceToFollow = hairPart; // 将当前头发部分设置为下一个部分的跟随目标
            }
        }
    }
}
