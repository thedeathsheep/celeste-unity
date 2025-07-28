using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 坍塌平台瓦片控制器
/// 负责管理平台坍塌的整个生命周期，包括检测玩家、计时坍塌、重置等
/// </summary>
public class CollapsingPlatformTiles : MonoBehaviour
{
    // 组件引用
    private SpriteRenderer sprite;        // 精灵渲染器组件
    private BoxCollider2D coll;           // 2D盒型碰撞器组件
    
    // 可在Inspector中设置的参数
    [SerializeField] private LayerMask playerMask;        // 玩家层级遮罩，用于检测玩家
    [SerializeField] private GameObject collapsingTile;   // 坍塌瓦片预制体
    public GameObject player;                             // 玩家对象引用
    public Sprite[] tiles;                               // 瓦片精灵数组
    [SerializeField] private Sprite collaspedSprite;     // 坍塌后的精灵图片
    
    // 状态和计时器
    public int collapsingState = 0;                      // 坍塌状态：0=正常，1=开始坍塌，2=已坍塌
    [SerializeField] private int timeBeforeCollapsing = 75;   // 坍塌前的等待时间
    [SerializeField] private int timeAfterCollapsing = 105;   // 坍塌后的重置等待时间
    
    private int timer = 0;                               // 计时器

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        // 获取组件引用
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        // 设置坍塌后的精灵图片
        sprite.sprite = collaspedSprite;

        // 根据平台宽度创建瓦片
        for (int i = 0; i < (int)(2 * sprite.size.x); i++)
        {
            // 实例化瓦片预制体
            GameObject tile = Instantiate(collapsingTile, transform.position, Quaternion.identity);

            // 将瓦片设置为当前平台的子对象
            tile.transform.parent = transform;
            
            // 计算瓦片位置，使其均匀分布在平台上
            tile.transform.position = new Vector2(
                tile.transform.position.x + ((float)i - sprite.size.x + 0.5f) / 2, 
                tile.transform.position.y
            );

            // 随机选择瓦片精灵
            int index = (int)Random.Range(0, 4);
            tile.GetComponent<InitializeCollapsingTile>().SetTileSprite(index);
        }

        // 隐藏主精灵，只显示子瓦片的精灵
        sprite.enabled = false;
    }

    /// <summary>
    /// 固定更新方法，用于物理检测和状态管理
    /// </summary>
    void FixedUpdate()
    {
        // 检测玩家是否站在平台上
        // 条件：1.检测到玩家在平台上 2.玩家没有离开平台 3.玩家在地面上 4.平台状态正常
        if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.up, .1f, playerMask) && 
            !Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.zero, .1f, playerMask) && 
            player.GetComponent<PlayerMovement>().IsGrounded() && 
            collapsingState == 0)
        {
            collapsingState = 1; // 开始坍塌过程
        }

        // 如果平台正在坍塌或已坍塌
        if (collapsingState > 0)
        {
            // 第一阶段：准备坍塌
            // 当计时器达到指定时间或玩家离开平台时，进入坍塌状态
            if (collapsingState == 1 && 
                (timer == timeBeforeCollapsing || 
                 !Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.up, .1f, playerMask)))
            {
                collapsingState = 2;  // 设置为已坍塌状态
                timer = 30;           // 重置计时器

                coll.enabled = false; // 禁用碰撞器，玩家可以穿过

                sprite.enabled = true; // 显示坍塌后的精灵
            }

            // 坍塌后的重置计时
            if (timer < timeBeforeCollapsing + timeAfterCollapsing)
            {
                timer++; // 增加计时器
            }
            else
            {
                // 重置平台状态
                timer = 0;
                collapsingState = 0;
                coll.enabled = true;  // 重新启用碰撞器
                Start();              // 重新初始化平台
            }
        }
    }
}
