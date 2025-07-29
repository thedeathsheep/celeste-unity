# Celeste Unity 重制版 - 代码逻辑文档

这是一个基于Unity的Celeste重制版，专注于演示如何实现复杂的2D平台游戏机制。

## 项目架构概览

### 核心系统设计
项目采用模块化设计，每个游戏机制都有独立的脚本组件：

```
Assets/Scripts/
├── Player/                 # 玩家核心系统
│   ├── PlayerMovement.cs   # 玩家移动控制器（核心）
│   ├── DeathAndRespawn.cs  # 死亡和重生系统
│   ├── UpdateAnimation.cs  # 动画状态管理
│   └── PlayerCollectables.cs # 收集品系统
├── Platform Systems/       # 平台系统
│   ├── Moving Platform/    # 移动平台
│   ├── Collapsing Platform/ # 坍塌平台
│   └── One Way Platform/   # 单向平台
└── Game Systems/          # 游戏系统
    ├── ScreenTransitionManager.cs # 屏幕转换
    ├── StrawberryCollect.cs      # 草莓收集
    └── CrystalActivation.cs      # 水晶激活
```

## 核心代码逻辑详情

### 1. 玩家移动系统 (`PlayerMovement.cs`)

#### 输入处理机制
```csharp
// 自定义输入状态枚举
private enum KeyState { Off, Held, Up, Down }

// 输入检测系统
private KeyState UpdateKeyState(string keyName)
{
    return Input.GetButton(keyName) ? KeyState.Held : KeyState.Off;
}
```

#### 状态管理
- **地面状态检测**：使用Physics2D.BoxCast检测地面接触
- **空中控制**：空中移动具有额外的惯性系统
- **冲刺系统**：包括冲刺次数限制和方向控制
- **攀爬系统**：体力消耗和墙壁抓取逻辑

#### 物理系统
```csharp
// 移动速度控制
if (IsGrounded()) {
    rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
} else {
    // 空中移动具有惯性限制
    float horizontalVelocity = rb.velocity.x + dirX * moveSpeed / 8;
    rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
}
```

### 2. 死亡和重生系统 (`DeathAndRespawn.cs`)

#### 死亡检测
- **挤压检测**：当玩家被墙壁挤压时触发死亡
- **死亡动画**：粒子效果和视觉反馈
- **状态重置**：重生时重置所有玩家状态

#### 重生逻辑
```csharp
// 自动找到最近的检查点
spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
respawnPosition = Nearest(spawnPoints);

// 重生时重置状态
transform.position = respawnPosition;
GetComponent<PlayerMovement>().ResetDashAndGrab();
```

### 3. 屏幕转换系统 (`ScreenTransitionManager.cs`)

#### 摄像机切换机制
- **虚拟摄像机系统**：使用Cinemachine实现平滑摄像机转换
- **触发器检测**：当玩家进入特定区域时激活新摄像机
- **状态保持**：确保屏幕转换期间玩家状态一致性

#### 转换逻辑
```csharp
private void OnTriggerEnter2D(Collider2D coll)
{
    if (virtualCamera.activeInHierarchy == false && coll.CompareTag("Player")) {
        virtualCamera.SetActive(true);
        screenManager.currentCamera = virtualCamera;
        player.GetComponent<StopObject>().Stop(0.4f, upperTransition, virtualCamera);
    }
}
```

### 4. 平台系统设计

#### 移动平台 (`Moving Platform/`)
- **路径系统**：支持各种移动路径（线性、圆形、自定义）
- **速度控制**：可调节的移动速度和加速度
- **玩家交互**：平台移动影响玩家速度

#### 坍塌平台 (`Collapsing Platform/`)
- **触发机制**：当玩家踩上时开始坍塌倒计时
- **视觉反馈**：坍塌过程中的动画效果
- **重置系统**：玩家死亡后平台自动重置

#### 单向平台 (`One Way Platform/`)
- **碰撞检测**：只允许从下方通过
- **跳跃穿透**：按下跳跃键时允许平台穿透

### 5. 收集品系统

#### 草莓收集 (`StrawberryCollect.cs`)
- **收集检测**：碰撞检测和收集动画
- **状态持久化**：收集品状态持久化
- **视觉效果**：收集时的粒子效果

#### 飞行草莓 (`WingedStrawberry.cs`)
- **AI行为**：自动飞行和玩家躲避
- **状态管理**：飞行、收集、重置状态
- **难度调整**：可调节的飞行速度和反应时间

## 技术实现详情

### 物理系统
- **Rigidbody2D**：用于玩家物理模拟
- **BoxCollider2D**：精确的碰撞检测
- **LayerMask**：分层碰撞系统

### 动画系统
- **Animator Controller**：状态机管理
- **Animation Events**：关键帧触发的游戏逻辑
- **Blend Animations**：平滑的动画过渡

### 性能优化
- **对象池**：重用频繁创建的对象（如粒子效果）
- **事件系统**：减少不必要的Update调用
- **缓存引用**：避免频繁的GetComponent调用

## 代码设计模式

### 1. 组件模式
每个功能都是独立的MonoBehaviour组件，便于维护和扩展。

### 2. 状态机模式
玩家状态通过枚举和状态机管理，确保一致的状态转换。

### 3. 观察者模式
使用Unity的事件系统实现组件间的松耦合。

### 4. 工厂模式
用于创建和管理游戏对象（如粒子效果、收集品）。

## 可扩展性设计

### 添加新机制
- **新平台类型**：继承基础平台类并实现特定行为
- **新收集品**：实现ICollectable接口
- **新移动能力**：在PlayerMovement中添加新的移动状态

### 配置系统
- **ScriptableObject**：游戏数据配置
- **Inspector参数**：运行时可调节的游戏参数
- **保存系统**：玩家进度和设置持久化

## 开发指南

### 代码标准
- 使用有意义的变量和方法名
- 为复杂逻辑添加适当的注释
- 遵循Unity命名约定

### 调试工具
- 使用Debug.Log输出关键状态信息
- 在Scene视图中显示碰撞体和触发器区域
- 使用Unity Profiler进行性能监控

### 测试策略
- 单元测试关键游戏逻辑
- 集成测试玩家-环境交互
- 性能测试确保流畅运行

## 项目状态

✅ **已完成**
- 核心移动系统
- 死亡和重生机制
- 屏幕转换系统
- 基础平台系统
- 收集品系统

🔄 **进行中**
- 关卡设计优化
- 性能优化
- 代码重构

📋 **计划中**
- 音频系统
- UI系统
- 保存系统
- 成就系统

---

*本项目仅供学习和研究目的。Celeste的原始概念、音效和图像属于其各自的所有者。*
