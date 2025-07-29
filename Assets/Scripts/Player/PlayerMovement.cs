using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家移动控制器 - 控制Madeline的所有移动、跳跃、冲刺和抓取功能
/// 这是游戏的核心脚本，包含所有玩家交互逻辑
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // 输入相关变量
    private enum KeyState { Off, Held, Up, Down } // 按键状态枚举：关闭、按住、抬起、按下
    private KeyState tempKeyJump; // 跳跃键的临时状态
    private KeyState keyJump; // 跳跃键的当前状态
    private KeyState tempKeyDash; // 冲刺键的临时状态
    private KeyState keyDash; // 冲刺键的当前状态
    private KeyState tempKeyGrab; // 抓取键的临时状态
    private KeyState keyGrab; // 抓取键的当前状态
    public float dirX; // 水平输入方向
    public float dirY; // 垂直输入方向

    // 杂项变量
    private Rigidbody2D rb; // 刚体组件
    [HideInInspector] public BoxCollider2D coll; // 碰撞体组件（隐藏于Inspector）
    private StopObject stop; // 停止对象组件
    public LayerMask wall; // 墙壁层级掩码
    public LayerMask spring; // 弹簧层级掩码

    private Vector2 hitboxCenter; // 碰撞体中心点
    private Vector2 hitboxSize; // 碰撞体大小
    private Vector2 halfBottomHitboxCenter = Vector2.zero; // 下半部分碰撞体中心
    private Vector2 halfBottomHitboxSize = Vector2.zero; // 下半部分碰撞体大小
    public Vector2 squishedOffset = Vector2.zero; // 挤压偏移量
    private Vector2 squishedLimit; // 挤压限制

    public bool facingLeft = false; // 是否面向左侧
    public bool isAirborne; // 是否在空中
    private int groundedFrames = 0; // 着地帧数计数
    private DeathAndRespawn deathResp; // 死亡重生组件

    // 速度相关变量
    [SerializeField] private float moveSpeed = 7f; // 移动速度（可在Inspector中调整）
    [SerializeField] private float jumpForce = 14f; // 跳跃力度（可在Inspector中调整）
    [SerializeField] private float maxVerticalSpeed = 20f; // 最大垂直速度（可在Inspector中调整）
    public bool boostedVelocity = false; // 是否处于加速状态
    public int boostedTimer = 0; // 加速计时器
    private bool keepVelocityAfterBoost = false; // 加速后是否保持速度
    [HideInInspector] public float maxBoostedHorizontalSpeed; // 最大加速水平速度（隐藏于Inspector）
    public float gravityScale; // 重力缩放

    // 冲刺相关变量
    [SerializeField] public int dashNumber = 1; // 冲刺次数（可在Inspector中调整）
    [SerializeField] private int dashDuration = 8; // 冲刺持续时间（可在Inspector中调整）
    [SerializeField] private float dashSpeed = 18f; // 冲刺速度（可在Inspector中调整）
    [SerializeField] private float waveDashFactor; // 波浪冲刺因子（可在Inspector中调整）
    [SerializeField] private bool isWaveDashing = false; // 是否正在波浪冲刺（可在Inspector中调整）
    [SerializeField] private GameObject phantomMadeline; // 幻影Madeline预制体（可在Inspector中设置）
    [HideInInspector] public Vector2 dashDirection = Vector2.zero; // 冲刺方向（隐藏于Inspector）
    public int dashState = 0; // 冲刺状态
    [HideInInspector] public int dashLeft; // 剩余冲刺次数（隐藏于Inspector）
    [HideInInspector] public bool isDashing = false; // 是否正在冲刺（隐藏于Inspector）
    private bool wallBouncing = false; // 是否正在墙壁反弹

    // 抓取相关变量
    [HideInInspector] public bool isGrabbing = false; // 是否正在抓取（隐藏于Inspector）
    [HideInInspector] public bool wallGrabbed = false; // 是否抓取墙壁（隐藏于Inspector）
    public float maxStamina = 180f; // 最大体力值
    public float staminaLeft; // 剩余体力
    public float climbSpeed = 4f; // 攀爬速度
    [HideInInspector] public int grabCooldownAfterJumpingFromWall = 0; // 从墙壁跳跃后的抓取冷却时间（隐藏于Inspector）
    [SerializeField] private bool nextToWall = false; // 是否靠近墙壁（可在Inspector中调整）
    private int nextToWallDirection = 0; // 靠近墙壁的方向
    public bool slidingOnWall = false; // 是否在墙壁上滑行
    private int canNeutralJumpTimer = 0; // 中性跳跃计时器（改变方向后，玩家不再滑行但有几帧可以执行墙壁反弹）
    [SerializeField] private int canNeutralJumpDuration = 10; // 中性跳跃持续时间（可在Inspector中调整）
    private bool neutralJumpFacingLeft; // 中性跳跃时的朝向

    /// <summary>
    /// 初始化方法，在游戏开始时调用
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取刚体组件
        coll = GetComponent<BoxCollider2D>(); // 获取碰撞体组件
        deathResp = GetComponent<DeathAndRespawn>(); // 获取死亡重生组件
        stop = GetComponent<StopObject>(); // 获取停止对象组件

        gravityScale = rb.gravityScale; // 保存初始重力缩放
        dashLeft = dashNumber; // 初始化剩余冲刺次数
        staminaLeft = maxStamina; // 初始化剩余体力

        // 计算挤压限制（碰撞体大小减去两个0.0625单位的边距）
        squishedLimit = new Vector2(coll.bounds.size.x - 2 * 0.0625f, coll.bounds.size.y - 2 * 0.0625f);
    }

    /// <summary>
    /// 每帧更新方法，处理输入检测
    /// </summary>
    void Update()
    {
        // 获取输入状态
        tempKeyJump = UpdateKeyState("Jump"); // 更新跳跃键状态
        tempKeyDash = UpdateKeyState("Dash"); // 更新冲刺键状态
        tempKeyGrab = UpdateKeyState("Grab"); // 更新抓取键状态
    }

    /// <summary>
    /// 固定更新方法，处理物理相关的移动逻辑
    /// </summary>
    void FixedUpdate()
    {
        // 获取输入

        // 水平输入检测
        dirX = Input.GetAxisRaw("Horizontal"); // 获取水平输入轴
        dirY = Input.GetAxisRaw("Vertical"); // 获取垂直输入轴

        // 将输入状态从Update传递到FixedUpdate
        keyJump = FixedUpdateKeyState(tempKeyJump, keyJump); // 更新跳跃键状态
        keyDash = FixedUpdateKeyState(tempKeyDash, keyDash); // 更新冲刺键状态
        keyGrab = FixedUpdateKeyState(tempKeyGrab, keyGrab); // 更新抓取键状态

        // 更新碰撞体引用
        halfBottomHitboxCenter = new Vector2(coll.bounds.center.x, coll.bounds.center.y + coll.bounds.size.y / 4); // 下半部分碰撞体中心
        halfBottomHitboxSize = new Vector2(coll.bounds.size.x, coll.bounds.size.y / 2); // 下半部分碰撞体大小

        hitboxCenter = coll.bounds.center; // 碰撞体中心
        hitboxSize = coll.bounds.size; // 碰撞体大小

        if (!deathResp.dead && !stop.stopped) // 如果玩家未死亡且未被停止
        {
            UpdateFacing(); // 更新朝向

            UpdateSliding(); // 更新滑行状态

            GrabCheck(); // 检查抓取

            DashCheck(); // 检查冲刺

            UpdateWaveDash(); // 更新波浪冲刺

            UpdateBoost(); // 更新加速状态

            UpdateGravity(); // 更新重力

            UpdateVelocity(); // 更新速度

            UpdateSquish(); // 更新挤压状态
        }

        isAirborne = !IsGrounded(); // 更新空中状态
    }
    /// <summary>
    /// 更新玩家速度的方法
    /// </summary>
    private void UpdateVelocity()
    {
        if (!isDashing && !isWaveDashing && !wallGrabbed && !slidingOnWall) // 如果不在冲刺、波浪冲刺、抓取墙壁或滑行状态
        {
            // 水平移动

            if (!boostedVelocity) // 正常移动
            {
                if (IsGrounded()) // 地面上的水平移动或高速移动
                {
                    rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
                }
                else if (Mathf.Abs(rb.velocity.x) < moveSpeed && dirX != 0) // 空中的水平移动
                {
                    float horizontalVelocity;
                    horizontalVelocity = rb.velocity.x + dirX * moveSpeed / 8; // 空中轻微漂移
                    if (Mathf.Abs(horizontalVelocity) > moveSpeed) // 限制水平速度
                    {
                        horizontalVelocity = dirX * moveSpeed;
                    }

                    rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y); // 空中的轻微漂移
                }
                else // 最大空中速度
                {
                    rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
                }
            }
            else if (boostedVelocity && boostedTimer == 0) // 玩家被加速时的水平移动（例如被移动平台）
            {
                if (IsGrounded() && !isWaveDashing)
                {
                    boostedVelocity = false; // 重置加速状态
                    rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
                }
                else if (boostedTimer == 0 && dirX != 0) // 空中的水平移动
                {
                    float horizontalVelocity;
                    horizontalVelocity = rb.velocity.x + dirX * moveSpeed / 10; // 加速状态下的空中移动

                    if (Mathf.Abs(horizontalVelocity) > maxBoostedHorizontalSpeed) // 限制最大加速水平速度
                    {
                        horizontalVelocity = dirX * maxBoostedHorizontalSpeed;
                    }

                    rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y); // 空中的轻微漂移

                    if (Mathf.Abs(rb.velocity.x) <= moveSpeed) // 返回正常移动
                    {
                        boostedVelocity = false; // 重置加速状态
                        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
                    }
                }
            }
        }

        if (keyJump == KeyState.Down && IsGrounded() && !isDashing && !wallGrabbed && !slidingOnWall) // 跳跃检测
        {
            if (transform.parent != null) // 如果玩家有父对象
            {
                GameObject obj = transform.parent.gameObject;
                if (obj.CompareTag("Moving Platform")) // 如果玩家在移动平台上 => 不跳跃而是被弹射
                {
                    StateUpdate state = obj.GetComponent<StateUpdate>();
                    if (state.EjectPlayer()) // 如果成功弹射玩家
                    {
                        state.playerJumped = true; // 标记玩家已跳跃
                    }
                    else // 正常跳跃
                    {
                        rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 设置跳跃速度

                        // 中断冲刺但保持水平速度
                        isDashing = false;
                        dashState = 0;
                        dashLeft = dashNumber;
                    }
                }
            }
            else // 正常跳跃
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 设置跳跃速度

                // 中断冲刺但保持水平速度
                isDashing = false;
                dashState = 0;
                dashLeft = dashNumber;
            }
        }

        if (rb.velocity.y < -maxVerticalSpeed) // 限制垂直速度
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxVerticalSpeed);
        }
    }

    /// <summary>
    /// 更新滑行状态的方法
    /// </summary>
    private void UpdateSliding()
    {
        // 条件：朝向墙壁且靠近墙壁
        Vector2 tempDir = Vector2.zero;
        if (facingLeft) // 如果面向左侧
        {
            tempDir = Vector2.left; // 设置临时方向为左
        }
        else // 如果面向右侧
        {
            tempDir = Vector2.right; // 设置临时方向为右
        }

        // 检查玩家是否可以滑行
        if (dirX == tempDir.x /* 输入方向相同 */ && Physics2D.BoxCast(halfBottomHitboxCenter, halfBottomHitboxSize, 0f, tempDir, .0625f, wall) && !IsGrounded() && rb.velocity.y < 0f)
        {
            slidingOnWall = true; // 激活滑行状态
        }
        else
        {
            slidingOnWall = false; // 停止滑行状态
        }

        // 检查玩家是否靠近墙壁
        if (Physics2D.BoxCast(halfBottomHitboxCenter, halfBottomHitboxSize, 0f, Vector2.left, .0625f, wall) && !IsGrounded() && !wallGrabbed) // 检查左侧墙壁
        {
            nextToWall = true; // 标记靠近墙壁
            nextToWallDirection = -1; // 设置墙壁方向为左
        }
        else if (Physics2D.BoxCast(halfBottomHitboxCenter, halfBottomHitboxSize, 0f, Vector2.right, .0625f, wall) && !IsGrounded() && !wallGrabbed) // 检查右侧墙壁
        {
            nextToWall = true; // 标记靠近墙壁
            nextToWallDirection = 1; // 设置墙壁方向为右
        }
        else // 如果不靠近墙壁
        {
            nextToWall = false; // 标记不靠近墙壁
            nextToWallDirection = 0; // 重置墙壁方向
        }

        // 滑行移动
        if (slidingOnWall) // 如果正在滑行
        {
            neutralJumpFacingLeft = facingLeft; // 记录中性跳跃时的朝向

            // 限制垂直速度
            if (keyJump != KeyState.Down || canNeutralJumpTimer == 0) // 如果没有按下跳跃键或中性跳跃计时器为0
            {
                if (rb.velocity.y < -maxVerticalSpeed / 2) // 如果垂直速度过快
                {
                    rb.velocity = new Vector2(rb.velocity.x, -maxVerticalSpeed / 2); // 限制垂直速度
                }
            }
        }
        if (nextToWall) // 如果靠近墙壁
        {
            neutralJumpFacingLeft = facingLeft; // 记录中性跳跃时的朝向
            // 更新墙壁跳跃计时器 - 滑行时达到最大值
            canNeutralJumpTimer = canNeutralJumpDuration;
        }
        else if (IsGrounded()) // 如果着地
        {
            canNeutralJumpTimer = 0; // 重置中性跳跃计时器
        }
        else // 如果在空中且不靠近墙壁
        {
            // 更新墙壁反弹计时器 - 不滑行时递减
            if (canNeutralJumpTimer > 0)
            {
                canNeutralJumpTimer--;
            }
        }

        // 中性跳跃（墙壁反弹）
        if (keyJump == KeyState.Down && canNeutralJumpTimer > 0) // 如果按下跳跃键且中性跳跃计时器有效
        {
            canNeutralJumpTimer = 0; // 重置计时器

            Vector2 newSpeed = Vector2.zero; // 新的速度向量

            if (slidingOnWall) // 如果正在滑行
            {
                if (neutralJumpFacingLeft) // 如果面向左侧
                {
                    newSpeed = new Vector2(1.6f * moveSpeed, 0.9f * jumpForce); // 向右反弹
                }
                else // 如果面向右侧
                {
                    newSpeed = new Vector2(-1.6f * moveSpeed, 0.9f * jumpForce); // 向左反弹
                }
                facingLeft = !neutralJumpFacingLeft; // 翻转朝向

                // 应用速度
                SetBoost(10, newSpeed, false);
            }
            else if (nextToWall) // 如果只是靠近墙壁
            {
                if (nextToWallDirection == -1) // 如果墙壁在左侧
                {
                    facingLeft = false; // 面向右侧
                    newSpeed = new Vector2(1.6f * moveSpeed, 0.9f * jumpForce); // 向右反弹
                }
                else if (nextToWallDirection == 1) // 如果墙壁在右侧
                {
                    facingLeft = true; // 面向左侧
                    newSpeed = new Vector2(-1.6f * moveSpeed, 0.9f * jumpForce); // 向左反弹
                }

                // 应用速度
                SetBoost(10, newSpeed, false);
            }

            slidingOnWall = false; // 停止滑行
            nextToWall = false; // 停止靠近墙壁
            nextToWallDirection = 0; // 重置墙壁方向
        }
    }

    /// <summary>
    /// 检查抓取状态的方法
    /// </summary>
    private void GrabCheck()
    {
        // 开始抓取
        if (!isDashing) // 冲刺时不能抓取
        {
            if (keyGrab == KeyState.Down || keyGrab == KeyState.Held) // 尝试抓取的条件
            {
                if (grabCooldownAfterJumpingFromWall > 0) // 从墙壁跳跃后不能立即抓取
                {
                    grabCooldownAfterJumpingFromWall--; // 减少冷却时间
                }
                else if (staminaLeft > 0f) // 没有体力不能抓取
                {
                    isGrabbing = true; // 激活抓取状态
                    slidingOnWall = false; // 停止滑行
                    nextToWall = false; // 停止靠近墙壁
                    canNeutralJumpTimer = 0; // 重置中性跳跃计时器
                }
            }
            else if (keyGrab == KeyState.Up) // 如果释放按键则停止抓取
            {
                isGrabbing = false; // 停止抓取状态
                wallGrabbed = false; // 停止墙壁抓取
                grabCooldownAfterJumpingFromWall = 0; // 重置抓取冷却时间
            }
        }

        // 检查可抓取的墙壁
        if (isGrabbing) // 只有在玩家尝试抓取时才能抓取墙壁
        {
            Vector2 grabDirection;

            if (facingLeft) // 可抓取的墙壁取决于朝向
            {
                grabDirection = Vector2.left;
            }
            else
            {
                grabDirection = Vector2.right;
            }

            if (Physics2D.BoxCast(halfBottomHitboxCenter, halfBottomHitboxSize, 0f, grabDirection, .0625f, wall)) // 检查是否有墙壁可以抓取
            {
                wallGrabbed = true; // 成功抓取墙壁

                if (Mathf.Abs(rb.velocity.y) > .1f)
                {
                    staminaLeft--; // 攀爬导致的体力损失
                }

                rb.velocity = Vector2.zero; // 停止任何动量以正确执行下面的移动

                if (keyJump == KeyState.Down) // 攀爬时跳跃
                {
                    if (transform.parent != null)
                    {
                        GameObject obj = transform.parent.gameObject;
                        if (obj.CompareTag("Moving Platform")) // 如果玩家在移动平台上 => 不跳跃而是被弹出
                        {
                            StateUpdate state = obj.GetComponent<StateUpdate>();
                            if (state.EjectPlayer())
                            {
                                state.playerJumped = true;
                            }
                            else
                            {
                                rb.velocity = new Vector2(rb.velocity.x, 0.85f * jumpForce);

                                wallGrabbed = false; // 跳跃停止玩家抓取墙壁
                                isGrabbing = false; // 跳跃结束抓取
                                grabCooldownAfterJumpingFromWall = 10; // 墙壁可以被抓取前的时间
                                staminaLeft -= 50f; // 跳跃导致的体力损失
                            }
                        }
                    }
                    else
                    {
                        rb.velocity = new Vector2(rb.velocity.x, 0.85f * jumpForce);

                        wallGrabbed = false; // 跳跃停止玩家抓取墙壁
                        isGrabbing = false; // 跳跃结束抓取
                        grabCooldownAfterJumpingFromWall = 10; // 墙壁可以被抓取前的时间
                        staminaLeft -= 50f; // 跳跃导致的体力损失
                    }
                }
                else if (Mathf.Abs(dirY) > .1f) // 检查是否向上或向下移动
                {
                    rb.velocity = new Vector2(rb.velocity.x, Mathf.Sign(dirY) * climbSpeed); // 根据向上或向下调整速度
                }

                if (staminaLeft <= 0f) // 如果没有体力则停止抓取
                {
                    staminaLeft = 0f; // 防止负体力（例如跳跃时）
                    wallGrabbed = false;
                    isGrabbing = false;
                }
            }
            else
            {
                wallGrabbed = false; // 由于不再靠近墙壁而结束抓取
            }
        }
        else
        {
            wallGrabbed = false; // 由于释放按键或跳跃/冲刺而结束抓取
        }

        if (IsGrounded()) // 接触地面时恢复体力
        {
            staminaLeft = maxStamina;
            grabCooldownAfterJumpingFromWall = 0;
        }
    }
    private void DashCheck()
    {
        if (keyDash == KeyState.Down && dashLeft > 0 && !isDashing && !Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, spring)) // 如果已经在冲刺或没有冲刺次数则不能冲刺
        {
            dashLeft--;
            isDashing = true;
            dashState = dashDuration;
            dashDirection = new Vector2(dirX, dirY);

            if (isGrabbing || wallGrabbed) // 抓取时冲刺 => 取消抓取
            {
                if (dirX > .1f) // 更新冲刺朝向
                {
                    facingLeft = false;
                }
                else if (dirX < -.1f)
                {
                    facingLeft = true;
                }

                // 重置抓取变量
                isGrabbing = false;
                wallGrabbed = false;
                staminaLeft = maxStamina;
                canNeutralJumpTimer = 0;
            }

            if (IsGrounded() && dashDirection.x != 0 && dashDirection.y == -1) // 如果在地面上试图向地面冲刺则修正冲刺
            {
                dashDirection = new Vector2(dashDirection.x, 0); // 更新冲刺方向以防止
            }

            if (dashDirection == Vector2.zero) // 没有方向输入 => 根据朝向执行中性冲刺
            {
                if (facingLeft)
                {
                    dashDirection = Vector2.left;
                }
                else
                {
                    dashDirection = Vector2.right;
                }
            }

            rb.velocity = dashDirection * dashSpeed; // 根据冲刺方向设置速度
        }

        if (isDashing) // 冲刺移动
        {
            if (dashState > 0) // 冲刺计时器检查
            {
                // 波冲检查
                if (IsGrounded() && keyJump == KeyState.Down && dashDirection.x != 0)
                {
                    float tempSpeedX;
                    if (facingLeft)
                    {
                        tempSpeedX = -waveDashFactor * moveSpeed;
                    }
                    else
                    {
                        tempSpeedX = waveDashFactor * moveSpeed;
                    }


                    // 检查波冲效率
                    float midDuration = dashDuration / 2;

                    if (dashDirection.y != 0 && Mathf.Abs(dashState - midDuration) <= 2) // 如果波冲效率高则恢复冲刺
                    {
                        dashLeft = dashNumber;
                    }

                    if (dashDirection.y == 0 && dashDuration - dashState > 5) // 在地面上执行时恢复冲刺的条件
                    {
                        dashLeft = dashNumber;
                    }

                    if (dashDirection.y == 0 && dashState < dashDuration - 2) // 水平速度效率
                    {
                        float reduceFactor;
                        reduceFactor = 0.6f + 0.4f * 1 / 3 * Mathf.Max(0, 5 - Mathf.Abs(dashDuration - midDuration));
                        tempSpeedX *= reduceFactor;
                    }

                    dashState = 0;

                    SetBoost(12, new Vector2(tempSpeedX, jumpForce), true);
                    isWaveDashing = true;
                }
                // 墙壁弹跳检查（左侧墙壁）- 仅在向上冲刺时
                else if (dashDirection == Vector2.up && keyJump == KeyState.Down && Physics2D.BoxCast(halfBottomHitboxCenter, halfBottomHitboxSize, 0f, Vector2.left, .5f, wall))
                {
                    isDashing = false;
                    dashState = 0;
                    facingLeft = false;
                    wallBouncing = true;

                    SetBoost(8, dashSpeed * new Vector2(0.53f, 1.2f), true);
                }
                // 墙壁弹跳检查（右侧墙壁）- 仅在向上冲刺时
                else if (dashDirection == Vector2.up && keyJump == KeyState.Down && Physics2D.BoxCast(halfBottomHitboxCenter, halfBottomHitboxSize, 0f, Vector2.right, .5f, wall))
                {
                    isDashing = false;
                    dashState = 0;
                    facingLeft = true;
                    wallBouncing = true;

                    SetBoost(8, dashSpeed * new Vector2(-0.53f, 1.2f), true);
                }
                // 如果撞到墙壁则更新冲刺方向
                else
                {
                    if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, dashDirection.x * Vector2.right, .1f, wall)) // 检查X方向是否有墙壁
                    {
                        rb.velocity = new Vector2(0f, rb.velocity.y); // 如果撞到墙壁则停止水平移动
                    }
                    if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, dashDirection.y * Vector2.up, .1f, wall)) // 检查Y方向是否有墙壁
                    {
                        rb.velocity = new Vector2(rb.velocity.x, 0f); // 如果撞到墙壁则停止垂直移动
                    }


                    if ((dashDuration - dashState) % 4 == 0) // 创建幻影复制品（炫酷！！）
                    {
                        GameObject phantom = Instantiate(phantomMadeline, transform.position, Quaternion.identity);

                        phantom.GetComponent<PhantomVanish>().facingLeft = facingLeft; // 复制品朝向根据玩家的朝向
                    }

                    dashState--; // 更新冲刺计时器
                }
            }
            else if (!isWaveDashing) // 结束冲刺
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
            }
        }
        else if (IsGrounded() && !isWaveDashing) // 在地面上时恢复冲刺
        {
            dashLeft = dashNumber;
        }

        if (wallBouncing)
        {
            // 撞到墙壁时停止墙壁弹跳方向
            if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, dashDirection.x * Vector2.right, .1f, wall)) // 检查X方向是否有墙壁
            {
                rb.velocity = new Vector2(0f, rb.velocity.y); // 如果撞到墙壁则停止水平移动
            }
            if (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, dashDirection.y * Vector2.up, .1f, wall)) // 检查Y方向是否有墙壁
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f); // 如果撞到墙壁则停止垂直移动
            }
        }

        // 墙壁弹跳和波冲的加速结束
        if (boostedTimer == 0 && (wallBouncing || isWaveDashing))
        {
            if (boostedVelocity)
            {
                rb.velocity = new Vector2(0.95f * rb.velocity.x, rb.velocity.y);

                if (Mathf.Abs(rb.velocity.x) < .0625f)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    boostedVelocity = false;
                }
            }
            else if (wallBouncing)
            {
                wallBouncing = false;
            }
            else if (isWaveDashing)
            {
                isWaveDashing = false;
            }
        }
    }
    private void UpdateWaveDash()
    {
        if (isWaveDashing)
        {
            if (boostedTimer == 0)
            {
                isWaveDashing = false;
                isDashing = false;
            }
        }
    }
    /// <summary>
    /// 更新加速状态的方法
    /// </summary>
    private void UpdateBoost()
    {
        if (boostedTimer > 0) // 玩家可以行动前的计时器
        {
            boostedTimer--; // 减少计时器
        }
        else // 如果计时器结束
        {
            boostedTimer = 0; // 重置计时器
            if (!keepVelocityAfterBoost) // 如果加速后不保持速度
            {
                boostedVelocity = false; // 停止加速状态
            }
        }
    }

    /// <summary>
    /// 更新朝向的方法
    /// </summary>
    private void UpdateFacing()
    {
        if (!isDashing && !wallGrabbed) // 冲刺或抓取时不能更新朝向
        {
            if (dirX > 0) // 如果向右移动
            {
                facingLeft = false; // 面向右侧
            }
            else if (dirX < 0) // 如果向左移动
            {
                facingLeft = true; // 面向左侧
            }
        }
    }
    /// <summary>
    /// 更新重力状态的方法
    /// </summary>
    private void UpdateGravity()
    {
        if ((isDashing && !isWaveDashing) || wallGrabbed) // 冲刺或抓取时停止重力
        {
            rb.gravityScale = 0f; // 设置重力为0
        }
        else if (slidingOnWall) // 如果在墙壁上滑行
        {
            rb.gravityScale = 1f; // 设置正常重力
        }
        else // 其他情况
        {
            rb.gravityScale = gravityScale; // 使用默认重力缩放
        }
    }

    /// <summary>
    /// 更新挤压状态的方法
    /// 如果墙壁靠近玩家，玩家会被挤压。如果挤压过度则杀死玩家
    /// </summary>
    private void UpdateSquish()
    {
        Vector2 tempUpdatedOffset = Vector2.zero; // 临时更新的偏移量，用于在所有4个检查中保持相同的挤压偏移

        // 水平检查
        if (Physics2D.BoxCast(hitboxCenter, hitboxSize - squishedOffset, 0f, Vector2.left, 0.0625f, wall) && Physics2D.BoxCast(hitboxCenter, hitboxSize - squishedOffset, 0f, Vector2.right, 0.0625f, wall)) // 检查左右两侧的墙壁
        {
            tempUpdatedOffset = Vector2.left; // 设置水平挤压
        }

        // 垂直检查
        if (Physics2D.BoxCast(hitboxCenter, hitboxSize - squishedOffset, 0f, Vector2.down, 0.0625f, wall) && Physics2D.BoxCast(hitboxCenter, hitboxSize - squishedOffset, 0f, Vector2.up, 0.0625f, wall)) // 检查上下两侧的墙壁
        {
            tempUpdatedOffset = Vector2.up; // 设置垂直挤压
        }

        // 更新挤压偏移量

        // 水平偏移
        if (tempUpdatedOffset.x == 0) // 如果没有偏移变化，减少偏移量
        {
            if (squishedOffset.x > 0) // 如果当前有水平偏移
            {
                squishedOffset = new Vector2(squishedOffset.x - 0.0625f, squishedOffset.y); // 减少水平偏移
            }
        }
        else // 如果有挤压
        {
            if (squishedOffset.x < squishedLimit.x) // 如果挤压未达到限制
            {
                squishedOffset = new Vector2(squishedOffset.x + 0.0625f, squishedOffset.y); // 增加水平挤压
            }
            else // 如果挤压过度
            {
                // 杀死玩家
                deathResp.dead = true; // 触发死亡
                rb.velocity = Vector2.zero; // 停止速度
                rb.gravityScale = 0f; // 停止重力
            }
        }

        // 垂直偏移
        if (tempUpdatedOffset.y == 0) // 如果没有偏移变化，减少偏移量
        {
            if (squishedOffset.y > 0) // 如果当前有垂直偏移
            {
                squishedOffset = new Vector2(squishedOffset.x, squishedOffset.y - 0.0625f); // 减少垂直偏移
            }
        }
        else // 如果有挤压
        {
            if (squishedOffset.y < squishedLimit.y) // 如果挤压未达到限制
            {
                squishedOffset = new Vector2(squishedOffset.x, squishedOffset.y + 0.0625f); // 增加垂直挤压
            }
            else // 如果挤压过度
            {
                // 杀死玩家
                deathResp.dead = true; // 触发死亡
                rb.velocity = Vector2.zero; // 停止速度
                rb.gravityScale = 0f; // 停止重力
            }
        }
    }

    /// <summary>
    /// 检查玩家是否着地
    /// </summary>
    /// <returns>如果玩家下方有墙壁则返回true</returns>
    public bool IsGrounded()
    {
        // 使用BoxCast检测玩家下方是否有墙壁，同时确保玩家不在墙壁内部
        return (Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .0625f, wall) && !Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.zero, .0625f, wall));
    }

    /// <summary>
    /// 检查玩家是否稳定着地（至少5帧）
    /// </summary>
    /// <returns>如果玩家稳定着地则返回true</returns>
    public bool IsVeryGrounded()
    {
        if (IsGrounded()) // 如果着地
        {
            if (groundedFrames < 5) // 如果着地帧数少于5
            {
                groundedFrames++; // 着地帧数递增
            }
        }
        else // 如果未着地
        {
            groundedFrames = 0; // 重置着地帧数
        }

        return groundedFrames == 5; // 返回是否稳定着地
    }

    /// <summary>
    /// 重置所有与冲刺和抓取相关的变量
    /// </summary>
    public void ResetDashAndGrab()
    {
        // 重置冲刺相关变量
        dashLeft = dashNumber; // 恢复冲刺次数
        isDashing = false; // 停止冲刺状态
        dashState = 0; // 重置冲刺状态

        // 重置抓取相关变量
        isGrabbing = false; // 停止抓取状态
        wallGrabbed = false; // 停止墙壁抓取
        staminaLeft = maxStamina; // 恢复体力
        grabCooldownAfterJumpingFromWall = 0; // 重置抓取冷却时间
    }

    /// <summary>
    /// 设置玩家加速状态
    /// </summary>
    /// <param name="boostDuration">加速持续时间</param>
    /// <param name="boostVector">加速向量</param>
    /// <param name="keep">加速后是否保持速度</param>
    public void SetBoost(int boostDuration, Vector2 boostVector, bool keep)
    {
        boostedVelocity = true; // 激活加速状态
        rb.velocity = boostVector; // 设置加速速度
        boostedTimer = boostDuration; // 设置加速计时器
        maxBoostedHorizontalSpeed = Mathf.Abs(rb.velocity.x); // 记录最大加速水平速度
        keepVelocityAfterBoost = keep; // 设置加速后是否保持速度
    }

    /// <summary>
    /// 更新按键状态
    /// </summary>
    /// <param name="keyName">按键名称</param>
    /// <returns>按键的当前状态</returns>
    private KeyState UpdateKeyState(string keyName)
    {
        KeyState key;

        if (Input.GetButton(keyName)) // 如果按键被按下
        {
            key = KeyState.Held; // 设置为按住状态
        }
        else // 如果按键未被按下
        {
            key = KeyState.Off; // 设置为关闭状态
        }

        return key;
    }

    /// <summary>
    /// 在FixedUpdate中更新按键状态
    /// 处理按键的按下和抬起事件
    /// </summary>
    /// <param name="tempKey">临时按键状态</param>
    /// <param name="key">当前按键状态</param>
    /// <returns>更新后的按键状态</returns>
    private KeyState FixedUpdateKeyState(KeyState tempKey, KeyState key)
    {
        /*
        一个inputFixedUpdate需要是'Down'（或'Up'）才能更新为'Held'（或'Off'）

        如果inputFixedUpdate是'Off'（或'Held'）而inputUpdate是'Held'（或'Off'），那么
        inputFixedUpdate更新为'Down'（或'Up'）
        */

        if (tempKey == KeyState.Held) // 如果临时状态是按住
        {
            if (key == KeyState.Down || key == KeyState.Held) // 如果当前状态是按下或按住
            {
                key = KeyState.Held; // 保持按住状态
            }
            else // 否则
            {
                key = KeyState.Down; // 设置为按下状态
            }
        }
        else if (tempKey == KeyState.Off) // 如果临时状态是关闭
        {
            if (key == KeyState.Up || key == KeyState.Off) // 如果当前状态是抬起或关闭
            {
                key = KeyState.Off; // 保持关闭状态
            }
            else // 否则
            {
                key = KeyState.Up; // 设置为抬起状态
            }
        }

        return key;
    }
}