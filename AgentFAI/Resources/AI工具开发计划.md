# ADOFAI AI工具开发计划

## 项目概述
基于 Microsoft Agent Framework 为 A Dance of Fire and Ice (ADOFAI) 游戏开发智能体工具，提供程序化的关卡编辑接口，用于辅助关卡创作、分析和优化。

## 技术栈
- **框架**: Microsoft Agent Framework (1.0.0)
- **目标平台**: Unity (C#)
- **部署方式**: Unity Mod (BepInEx)
- **存储位置**: `AIAgent/` 文件夹
- **NuGet包**: Microsoft.Agents.AI, Microsoft.Agents.AI.OpenAI
- **接口设计**: 基于工具方法的AI智能体集成

## Microsoft Agent Framework 简介

Microsoft Agent Framework 是微软推出的企业级AI智能体开发框架，结合了 Semantic Kernel 的企业功能和 AutoGen 的智能体抽象。框架提供：

### 核心功能
- **智能体 (Agents)**: 使用LLM处理输入、调用工具和MCP服务器、生成响应
- **工作流 (Workflows)**: 基于图的工作流，连接智能体和函数进行多步骤任务
- **工具集成**: 支持MCP (Model Context Protocol) 服务器和自定义工具
- **多模型支持**: Azure OpenAI、OpenAI、Anthropic、Ollama等

### 安装方式
```bash
# 核心包
dotnet add package Microsoft.Agents.AI --version 1.0.0

# OpenAI支持
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0

# 工作流支持
dotnet add package Microsoft.Agents.AI.Workflows --version 1.0.0
```

### 基本使用
```csharp
using Microsoft.Agents.AI;
using OpenAI;

// 创建智能体
OpenAIClient client = new OpenAIClient("your_api_key");
var chatClient = client.GetChatClient("gpt-4o-mini");
AIAgent agent = chatClient.AsAIAgent(
    instructions: "You are a helpful assistant for ADOFAI level editing."
);

// 调用智能体
var response = await agent.InvokeAsync("Create a new floor at position 5");
```

## 项目目标
### 1. 关卡编辑API智能体
- 支持地板的创建、删除、修改
- 支持事件的添加、删除、编辑
- 提供数据验证和错误处理

### 2. 关卡分析智能体
- 分析关卡难度和复杂度
- 检测节奏一致性和音乐同步
- 识别潜在的游戏性问题
- 提供数据驱动的优化建议

### 3. 关卡生成智能体
- 根据音乐特征自动生成地板序列
- 基于难度曲线智能放置事件
- 支持模板和风格的应用
- 生成符合音乐节奏的关卡

### 4. 关卡优化智能体
- 自动优化事件时间和参数
- 调整判定范围和难度平衡
- 优化视觉效果和性能
- 提供A/B测试功能

## 开发阶段

### 阶段1: 基础架构 (当前)
- [x] 分析ADOFAI源代码结构
- [x] 创建详细文档
- [x] 设计AI工具方法接口
- [ ] 搭建BepInEx Mod开发环境
- [ ] 创建基础项目结构

### 阶段2: 核心工具方法实现
- [x] 实现地板操作工具方法 (CreateFloor, DeleteFloor, ModifyFloorAngle)
- [x] 实现事件操作工具方法 (AddEvent, RemoveEvent, SetEventProperty)
- [x] 实现基于属性的事件操作 (AddEventWithProperties, RemoveEventsByProperty)
- [x] 实现关卡信息工具方法 (SetLevelInfo, SaveLevel, LoadLevel)
- [x] 实现数据验证和错误处理机制
- [ ] 创建工具方法注册系统

### 阶段3: 分析和查询工具
- [ ] 实现关卡分析工具方法 (AnalyzeDifficulty, AnalyzeRhythm)
- [ ] 实现数据查询工具方法 (FindFloorsWithEvent, GetUsedEventTypes)
- [ ] 实现统计信息计算 (GetLevelStatistics, CalculateAverageBPM)
- [ ] 实现问题检测算法 (DetectIssues)

### 阶段4: 生成和优化工具
- [ ] 实现关卡生成工具方法 (GenerateBasicPattern, AddSpeedVariations)
- [ ] 实现关卡优化工具方法 (OptimizeDifficultyCurve, SyncEventsToMusic)
- [ ] 实现装饰物和视觉效果生成
- [ ] 实现智能算法 (难度评估、节奏分析、模式识别)

### 阶段5: AI智能体集成
- [ ] 集成Microsoft Agent Framework
- [ ] 创建智能体工具调用接口
- [ ] 实现工具方法到智能体的映射
- [ ] 实现上下文感知和状态管理
- [ ] 创建自然语言交互界面

### 阶段6: 测试和优化
- [ ] 单元测试覆盖
- [ ] 集成测试和性能测试
- [ ] 用户界面和交互优化
- [ ] 文档和示例完善

## AI工具方法接口设计

基于 Microsoft Agent Framework 的工具集成模式，设计直观易用的工具方法接口。智能体通过工具调用来执行具体的关卡编辑操作：

### 1. 关卡编辑工具方法 (LevelEditingTools)

#### 地板操作
```csharp
[Description("在指定位置创建新地板")]
public static bool CreateFloor(
    [Description("插入位置的地板索引")] int position,
    [Description("地板角度(度)，0=右，90=上，180=左，270=下")] float angle)

[Description("删除指定位置的地板")]
public static bool DeleteFloor(
    [Description("要删除的地板索引")] int floorIndex)

[Description("修改地板的角度")]
public static bool ModifyFloorAngle(
    [Description("地板索引")] int floorIndex,
    [Description("新的角度(度)")] float newAngle)

[Description("获取关卡的地板总数")]
public static int GetFloorCount()

[Description("获取指定地板的角度")]
public static float GetFloorAngle(
    [Description("地板索引")] int floorIndex)
```

#### 基于属性的事件操作
```csharp
[Description("为关卡添加事件并设置属性")]
public static bool AddEventWithProperties(
    [Description("要添加事件的地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("事件属性字典")] Dictionary<string, object> properties)

[Description("为关卡添加事件并设置单个属性")]
public static bool AddEventWithProperty(
    [Description("要添加事件的地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性名称")] string propertyName,
    [Description("属性值")] object propertyValue)

[Description("删除指定地板上匹配属性条件的事件")]
public static int RemoveEventsByProperty(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性名称")] string propertyName,
    [Description("属性值")] object propertyValue)

[Description("删除指定地板上匹配多个属性条件的事件")]
public static int RemoveEventsByProperties(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性条件字典")] Dictionary<string, object> propertyConditions)

[Description("查找匹配属性条件的事件")]
public static EventInfo[] FindEventsByProperty(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性名称")] string propertyName,
    [Description("属性值")] object propertyValue)

[Description("查找匹配多个属性条件的事件")]
public static EventInfo[] FindEventsByProperties(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性条件字典")] Dictionary<string, object> propertyConditions)
```

#### 事件查询和管理
```csharp
#### 事件查询和管理
```csharp
[Description("为关卡添加事件，返回true为添加成功")]
public static bool AddEvent(
    [Description("要添加事件的地板索引")] int floor,
    [Description("事件类型名称")] string eventType)

[Description("删除指定地板上的事件")]
public static bool RemoveEvent(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("事件索引(同类型事件中的第几个，从0开始)")] int eventIndex = 0)

[Description("删除指定地板上指定类型的所有事件")]
public static int RemoveAllEvents(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType)

[Description("修改事件的属性值")]
public static bool SetEventProperty(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性名称")] string propertyName,
    [Description("属性值")] object value,
    [Description("事件索引(同类型事件中的第几个，从0开始)")] int eventIndex = 0)

[Description("批量修改同类型事件的属性值")]
public static int SetAllEventProperties(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性名称")] string propertyName,
    [Description("属性值")] object value)

[Description("获取事件的属性值")]
public static object GetEventProperty(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType,
    [Description("属性名称")] string propertyName,
    [Description("事件索引(同类型事件中的第几个，从0开始)")] int eventIndex = 0)

[Description("获取指定地板上的所有事件")]
public static string[] GetEventsOnFloor(
    [Description("地板索引")] int floor)

[Description("获取指定地板上的事件详细信息")]
public static EventInfo[] GetEventDetailsOnFloor(
    [Description("地板索引")] int floor)

[Description("获取指定类型事件的数量")]
public static int GetEventCount(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType)

[Description("获取指定地板上指定类型事件的所有属性")]
public static Dictionary<string, object>[] GetAllEventProperties(
    [Description("地板索引")] int floor,
    [Description("事件类型名称")] string eventType)
```
```

#### 关卡信息操作
```csharp
[Description("设置关卡基本信息")]
public static bool SetLevelInfo(
    [Description("艺术家名称")] string artist,
    [Description("歌曲名称")] string song,
    [Description("作者名称")] string author,
    [Description("BPM值")] float bpm)

[Description("获取关卡基本信息")]
public static LevelInfo GetLevelInfo()

[Description("保存当前关卡")]
public static bool SaveLevel(
    [Description("保存路径")] string filePath)

[Description("加载关卡文件")]
public static bool LoadLevel(
    [Description("关卡文件路径")] string filePath)
```

### 2. 关卡分析工具方法 (LevelAnalysisTools)

```csharp
[Description("分析关卡难度，返回0-10的难度评分")]
public static float AnalyzeDifficulty()

[Description("检测关卡的节奏一致性")]
public static RhythmAnalysis AnalyzeRhythm()

[Description("获取关卡统计信息")]
public static LevelStatistics GetLevelStatistics()

[Description("检测关卡中的潜在问题")]
public static string[] DetectIssues()

[Description("分析速度变化的频率和强度")]
public static SpeedAnalysis AnalyzeSpeedChanges()

[Description("计算关卡的平均BPM")]
public static float CalculateAverageBPM()
```

### 3. 关卡生成工具方法 (LevelGenerationTools)

```csharp
[Description("根据BPM和长度生成基础地板序列")]
public static bool GenerateBasicPattern(
    [Description("目标BPM")] float bpm,
    [Description("地板数量")] int floorCount,
    [Description("复杂度(0-1)")] float complexity)

[Description("在指定范围添加速度变化事件")]
public static bool AddSpeedVariations(
    [Description("开始地板索引")] int startFloor,
    [Description("结束地板索引")] int endFloor,
    [Description("速度变化强度(0-1)")] float intensity)

[Description("自动添加视觉效果事件")]
public static bool AddVisualEffects(
    [Description("效果密度(0-1)")] float density,
    [Description("效果类型偏好")] string[] preferredEffects)

[Description("生成装饰物")]
public static bool GenerateDecorations(
    [Description("装饰物密度(0-1)")] float density,
    [Description("装饰物类型")] string decorationType)
```

### 4. 关卡优化工具方法 (LevelOptimizationTools)

```csharp
[Description("优化关卡的难度曲线")]
public static bool OptimizeDifficultyCurve(
    [Description("目标难度(0-10)")] float targetDifficulty)

[Description("自动调整事件时间以匹配音乐节拍")]
public static bool SyncEventsToMusic()

[Description("优化视觉效果的性能")]
public static bool OptimizeVisualEffects()

[Description("平衡关卡的判定范围")]
public static bool BalanceHitMargins()

[Description("移除冗余或冲突的事件")]
public static int CleanupEvents()
```

### 5. 数据查询工具方法 (DataQueryTools)

```csharp
[Description("查找包含指定事件类型的所有地板")]
public static int[] FindFloorsWithEvent(
    [Description("事件类型名称")] string eventType)

[Description("查找指定角度范围内的地板")]
public static int[] FindFloorsInAngleRange(
    [Description("最小角度")] float minAngle,
    [Description("最大角度")] float maxAngle)

[Description("获取关卡中使用的所有事件类型")]
public static string[] GetUsedEventTypes()

[Description("计算两个地板之间的时间间隔")]
public static float CalculateTimeBetweenFloors(
    [Description("起始地板索引")] int startFloor,
    [Description("结束地板索引")] int endFloor)
```

## 数据模型

### AnalysisResult (分析结果)
```csharp
public class AnalysisResult
{
    public float DifficultyScore;
    public float RhythmConsistency;
    public List<string> Issues;
    public List<Suggestion> Suggestions;
    public Dictionary<string, float> Metrics;
}
```

### GenerationSettings (生成设置)
```csharp
public class GenerationSettings
{
    public float TargetDifficulty;
    public bool UseAdvancedEvents;
    public int MinFloors;
    public int MaxFloors;
    public List<LevelEventType> AllowedEventTypes;
}
```

## 技术挑战

### 1. 音乐分析
- 节拍检测 (BPM detection)
- 节奏模式识别
- 音乐结构分析 (intro, verse, chorus等)

### 2. 难度评估
- 速度变化频率
- 角度变化复杂度
- 事件密度
- 判定窗口大小

### 3. 关卡生成
- 保持节奏一致性
- 避免不可能的模式
- 平衡难度曲线
- 生成有趣的变化

### 4. Unity集成
- Mod加载机制
- 运行时代码注入
- 性能优化
- 兼容性处理

## 开发工具

### 必需工具
- Visual Studio 2022
- Unity Editor (对应游戏版本)
- UnityModManager (Unity Mod框架)
- Microsoft.Agents.AI SDK

### 推荐工具
- dnSpy (反编译工具)
- ILSpy (IL查看器)
- Harmony (运行时补丁库)

## 文件结构
```
AIAgent/
├── README.md                           # 项目说明
├── AI工具开发计划.md                   # 本文件
├── ADOFAI关卡与事件系统文档.md         # 系统文档
├── ADOFAI特效系统文档.md               # 特效文档
├── Plugin.cs                           # BepInEx插件入口
└── Tools/                              # AI工具方法实现
    └── LevelEditingTools.cs           # 关卡编辑工具方法
```

## 开发规范

### 代码规范
- 遵循C#命名约定
- 使用XML文档注释
- 保持代码简洁可读
- 编写单元测试

### 性能要求
- 关卡分析 < 1秒
- 关卡生成 < 5秒
- 实时监控开销 < 5%

### 兼容性
- 支持关卡格式版本 9-15
- 向后兼容旧版关卡
- 不破坏原游戏功能

## 下一步行动

1. **立即行动**
   - 安装BepInEx和开发工具
   - 创建基础Mod项目结构
   - 实现工具方法注册系统

2. **本周目标**
   - 实现核心地板操作工具方法 (CreateFloor, DeleteFloor, ModifyFloorAngle)
   - 实现基础事件操作工具方法 (AddEvent, RemoveEvent)
   - 创建数据验证和错误处理机制
   - 编写单元测试

3. **本月目标**
   - 完成所有基础工具方法实现
   - 集成Microsoft.Agents.AI框架
   - 实现自然语言到工具方法的映射
   - 创建简单的AI助手界面
