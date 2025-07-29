using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 摄像机设置控制器 - 控制摄像机的跟随和缩放功能
/// 用于实现2D游戏中的摄像机行为
/// </summary>
public class CameraSettings : MonoBehaviour
{
    [SerializeField] private bool enableFollowingCamera = false; // 是否启用摄像机跟随功能（可在Inspector中调整）

    [SerializeField] private Transform player; // 玩家对象的Transform组件（可在Inspector中设置）

    [SerializeField] private float cameraSize = 1f; // 摄像机缩放大小（可在Inspector中调整，默认值为1）
    private Camera cam; // 摄像机组件引用

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    private void Start()
    {
        cam = GetComponent<Camera>(); // 获取当前GameObject上的Camera组件
    }

    /// <summary>
    /// 每帧更新方法，处理摄像机的跟随和缩放逻辑
    /// </summary>
    private void Update()
    {
        if (enableFollowingCamera) // 如果启用了摄像机跟随功能
        {
            // 将摄像机位置设置为玩家位置，但保持摄像机的Z轴位置不变
            // 这样可以实现2D游戏中摄像机跟随玩家的效果
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);

            // 设置摄像机的正交大小（orthographicSize）
            // 使用 1 / cameraSize 的公式，这样cameraSize越大，摄像机视野越小（放大效果）
            // cameraSize越小，摄像机视野越大（缩小效果）
            cam.orthographicSize = 1 / cameraSize;
        }
    }
}
