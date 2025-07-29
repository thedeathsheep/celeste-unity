using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 头发中间点控制器 - 在两个头发部分之间创建中间连接点
/// 用于实现更自然的头发连接效果
/// </summary>
public class HairIntermediate : MonoBehaviour
{
    [SerializeField] private GameObject hairPart; // 头发部分对象（可在Inspector中设置）
    [SerializeField] private GameObject hairPartFollowed; // 被跟随的头发部分对象（可在Inspector中设置）

    /// <summary>
    /// 每帧更新方法，计算并设置中间点位置
    /// </summary>
    void Update()
    {
        // 计算两个头发部分之间的中点位置
        // 公式：被跟随对象位置 + (当前对象位置 - 被跟随对象位置) / 2
        this.transform.position = hairPartFollowed.transform.position + (hairPart.transform.position - hairPartFollowed.transform.position) / 2;
    }
}
