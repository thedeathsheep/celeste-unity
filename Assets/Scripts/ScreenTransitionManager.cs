using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 屏幕转换管理器 - 控制屏幕之间的转换逻辑
/// 处理玩家在不同屏幕间的移动和摄像机切换
/// </summary>
public class ScreenTransitionManager : MonoBehaviour
{
    public GameObject virtualCamera; // 虚拟摄像机对象
    [SerializeField] private GameObject player; // 玩家对象引用（可在Inspector中设置）
    private ScreenManager screenManager; // 屏幕管理器组件

    /// <summary>
    /// 碰撞进入检测方法，处理屏幕转换
    /// </summary>
    /// <param name="coll">碰撞对象</param>
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (virtualCamera.activeInHierarchy == false && coll.CompareTag("Player") && !coll.isTrigger) // 如果虚拟摄像机未激活且碰撞对象是玩家且不是触发器
        {
            if (coll.gameObject.GetComponent<Rigidbody2D>().velocity != Vector2.zero) // 如果玩家正在移动（转换屏幕）
            {
                screenManager = transform.parent.gameObject.GetComponent<ScreenManager>(); // 获取屏幕管理器

                bool upperTransition; // 是否为向上转换
                upperTransition = screenManager.currentCamera.transform.position.y < transform.position.y; // 如果当前摄像机位置低于转换器位置，则为向上转换

                virtualCamera.SetActive(true); // 激活虚拟摄像机
                screenManager.currentCamera = virtualCamera; // 设置当前摄像机
                player.GetComponent<StopObject>().Stop(0.4f, upperTransition, virtualCamera); // 停止玩家0.4秒

                RefreshWingedStrawberry(virtualCamera.transform.parent.gameObject); // 刷新飞行草莓
            }
        }
    }

    /// <summary>
    /// 碰撞退出检测方法，处理摄像机停用
    /// </summary>
    /// <param name="coll">碰撞对象</param>
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (virtualCamera.activeInHierarchy == true && coll.CompareTag("Player") && !coll.isTrigger) // 如果虚拟摄像机已激活且碰撞对象是玩家且不是触发器
        {
            if (coll.gameObject.GetComponent<Rigidbody2D>().velocity != Vector2.zero) // 如果玩家正在移动
            {
                virtualCamera.SetActive(false); // 停用虚拟摄像机
            }
        }
    }

    /// <summary>
    /// 刷新飞行草莓的方法
    /// </summary>
    /// <param name="screen">屏幕对象</param>
    private void RefreshWingedStrawberry(GameObject screen)
    {
        GameObject[] wingedBerries = GameObject.FindGameObjectsWithTag("Winged Strawberry"); // 查找所有飞行草莓
        foreach (GameObject berry in wingedBerries) // 遍历所有飞行草莓
        {
            if (GameObject.ReferenceEquals(berry.GetComponent<WingedStrawberry>().screen, screen)) // 如果草莓属于当前屏幕
            {
                berry.GetComponent<WingedStrawberry>().Refresh(); // 刷新草莓状态
            }
        }
    }
}
