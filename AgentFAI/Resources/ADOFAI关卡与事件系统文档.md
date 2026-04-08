# ADOFAI 关卡与事件系统文档

## 概述
本文档描述了 A Dance of Fire and Ice (ADOFAI) 游戏的关卡结构和事件系统，用于AI工具开发参考。

## 核心类结构

### 1. LevelData (关卡数据)
位置: `src/ADOFAI/LevelData.cs`

关卡数据是整个关卡的核心容器，包含：

#### 关键属性
- `pathData`: 旧版本的路径字符串数据 (字符编码的角度)
- `angleData`: 新版本的角度数组 (浮点数数组)
- `levelEvents`: 关卡事件数组
- `decorations`: 装饰物数组
- `version`: 关卡格式版本号 (当前最新为15)

#### 设置对象
- `songSettings`: 音乐设置 (BPM, 音量, 音高, 偏移等)
- `levelSettings`: 关卡设置 (艺术家, 歌曲名, 作者等)
- `trackSettings`: 轨道设置 (颜色, 样式, 纹理等)
- `backgroundSettings`: 背景设置
- `cameraSettings`: 相机设置
- `miscSettings`: 杂项设置
- `eventSettings`: 事件设置
- `decorationSettings`: 装饰设置

#### 重要方法
- `LoadLevel(string levelPath, out LoadResult status)`: 加载关卡文件
- `Encode()`: 将关卡数据编码为JSON字符串
- `Decode(Dictionary<string, object> dict, out LoadResult status)`: 从字典解码关卡数据

### 2. LevelEvent (关卡事件)
位置: `src/ADOFAI/LevelEvent.cs`

关卡事件是游戏中所有动态行为的基础。

#### 核心属性
- `floor`: 事件触发的地板序号 (-1表示全局事件)
- `eventType`: 事件类型 (LevelEventType枚举)
- `data`: 事件数据字典 (键值对)
- `disabled`: 禁用属性字典
- `active`: 事件是否激活
- `visible`: 事件是否可见
- `locked`: 事件是否锁定

#### 事件类型 (LevelEventType)
位置: `src/ADOFAI/LevelEventType.cs`

主要事件类型包括：
- **速度控制**: `SetSpeed`, `Twirl`
- **检查点**: `Checkpoint`
- **相机**: `MoveCamera`, `ShakeScreen`
- **视觉效果**: `Flash`, `SetFilter`, `Bloom`, `HallOfMirrors`
- **轨道**: `ChangeTrack`, `ColorTrack`, `AnimateTrack`, `RecolorTrack`, `MoveTrack`
- **装饰**: `AddDecoration`, `AddText`, `SetText`, `MoveDecorations`
- **音效**: `SetHitsound`, `PlaySound`, `SetHoldSound`
- **游戏机制**: `Hold`, `MultiPlanet`, `FreeRoam`, `Pause`, `AutoPlayTiles`
- **高级**: `SetConditionalEvents`, `RepeatEvents`, `CallMethod`, `AddComponent`

### 3. scrFloor (地板/瓦片)
位置: `src/scrFloor.cs`

代表游戏中的单个地板瓦片。

#### 核心属性
- `seqID`: 地板序列ID
- `speed`: 速度倍数
- `entryangle`: 进入角度 (弧度)
- `exitangle`: 退出角度 (弧度)
- `angleLength`: 角度长度
- `entryTime`: 进入时间
- `isCCW`: 是否逆时针旋转
- `midSpin`: 是否为中旋
- `holdLength`: 长按长度 (-1表示无长按)
- `isportal`: 是否为传送门
- `floorIcon`: 地板图标类型
- `prevfloor`: 前一个地板引用
- `nextfloor`: 下一个地板引用

#### 特殊功能
- `freeroam`: 自由漫游模式
- `multiplanet`: 多行星模式
- `auto`: 自动播放
- `marginScale`: 判定范围缩放

### 4. scrLevelMaker (关卡生成器)
位置: `src/scrLevelMaker.cs`

负责从关卡数据生成实际的游戏对象。

#### 核心方法
- `MakeLevel()`: 生成整个关卡
- `InstantiateStringFloors()`: 从字符串数据实例化地板 (旧版)
- `InstantiateFloatFloors()`: 从浮点数组实例化地板 (新版)
- `CalculateFloorEntryTimes()`: 计算每个地板的进入时间
- `DrawFreeroam()`: 绘制自由漫游区域
- `DrawHolds()`: 绘制长按轨迹

#### 角度字符映射
```
'R' = 0°    (右)
'U' = 90°   (上)
'L' = 180°  (左)
'D' = 270°  (下)
'E' = 45°   (右上)
'Q' = 135°  (左上)
'Z' = 225°  (左下)
'C' = 315°  (右下)
'!' = 999°  (中旋标记)
```

### 5. scrController (游戏控制器)
位置: `src/scrController.cs`

游戏的主控制器，管理游戏状态和输入。

#### 核心属性
- `currentSeqID`: 当前地板序号
- `currFloor`: 当前地板引用
- `paused`: 是否暂停
- `speed`: 游戏速度
- `isCW`: 是否顺时针
- `noFail`: 无失败模式
- `mistakesManager`: 错误管理器

#### 游戏状态 (States)
- `Start`: 开始
- `PlayerControl`: 玩家控制
- `Fail`: 失败
- `Win`: 胜利

## 事件执行流程

### 1. 事件触发时机
- **LevelEventExecutionTime.OnBar**: 在节拍上触发
- **LevelEventExecutionTime.OnHit**: 在击打时触发
- **LevelEventExecutionTime.Instant**: 立即触发

### 2. 事件处理
事件通过 `ffxPlusBase` 基类及其派生类处理：
- `ffxCameraPlus`: 相机效果
- `ffxFlashPlus`: 闪光效果
- `ffxSetFilterPlus`: 滤镜效果
- `ffxMoveDecorationsPlus`: 移动装饰物
- 等等...

### 3. 条件事件
通过 `SetConditionalEvents` 可以设置条件触发的事件，根据判定结果执行不同的事件。

## 关卡文件格式

### JSON结构
```json
{
  "angleData": [0, 45, 90, ...],
  "settings": {
    "version": 15,
    "artist": "艺术家名",
    "song": "歌曲名",
    "bpm": 120,
    ...
  },
  "actions": [
    {
      "floor": 1,
      "eventType": "SetSpeed",
      "speedType": "Multiplier",
      "beatsPerMinute": 120,
      "bpmMultiplier": 2
    },
    ...
  ],
  "decorations": [...]
}
```

## 时间计算

### BPM与时间
- `crotchet`: 一拍的时间 = 60 / BPM
- `entryTime`: 地板进入时间，基于角度和速度计算
- 公式: `time = angle / (2π) * crotchet / speed`

### 角度计算
- 完整旋转 = 2π 弧度 = 360°
- 半旋转 = π 弧度 = 180°
- 角度移动通过 `scrMisc.GetAngleMoved()` 计算

## 判定系统

### HitMargin (判定等级)
- `Perfect`: 完美
- `EarlyPerfect`: 早完美
- `LatePerfect`: 晚完美
- `VeryEarly`: 很早
- `VeryLate`: 很晚
- `TooEarly`: 太早
- `TooLate`: 太晚
- `Loss`: 失误

### 判定范围
通过 `marginScale` 属性可以缩放判定范围。

## 特殊机制

### 1. 自由漫游 (FreeRoam)
- 允许玩家在网格上自由移动
- 通过 `freeroamDimensions` 定义网格大小
- 通过 `freeroamOffset` 定义偏移

### 2. 多行星 (MultiPlanet)
- 支持2个以上的行星
- 通过 `numPlanets` 属性设置

### 3. 长按 (Hold)
- 通过 `holdLength` 设置长按长度
- 支持长按音效设置

### 4. 中旋 (MidSpin)
- 特殊的旋转机制
- 角度标记为 999 或使用 '!' 字符

## 编辑器相关

### 关卡编辑器 (scnEditor)
- 提供可视化编辑界面
- 支持事件添加、修改、删除
- 支持装饰物放置和编辑

### 属性面板
- 通过 `PropertyInfo` 定义属性类型和默认值
- 支持多种属性类型: Int, Float, String, Bool, Enum, Vector2, Color等

## 注意事项

1. **版本兼容性**: 不同版本的关卡格式可能不兼容，需要处理版本迁移
2. **性能优化**: 大量事件和装饰物会影响性能
3. **时间同步**: 音频和视觉需要精确同步
4. **错误处理**: 需要处理关卡加载失败、事件执行错误等情况

## 事件和地板的创建、编辑、删除操作

### 1. 地板操作

#### 创建地板
```csharp
// 通过字符创建地板 (旧版本)
public void CreateFloorWithCharOrAngle(float angle, char chara, bool pulseFloorButtons = true, bool fullSpin = false)

// 核心创建方法
private void CreateFloor(char direction)  // 字符方向
private void CreateFloor(float angle)    // 角度方向

// 角度字符映射
'R' = 0°    (右)     'U' = 90°   (上)     'L' = 180°  (左)     'D' = 270°  (下)
'E' = 45°   (右上)   'Q' = 135°  (左上)   'Z' = 225°  (左下)   'C' = 315°  (右下)
'T' = 60°   'G' = 120°  'F' = 240°  'B' = 300°
'J' = 30°   'H' = 150°  'N' = 210°  'M' = 330°
'p' = 15°   'o' = 75°   'q' = 105°  'W' = 165°
'x' = 195°  'V' = 255°  'Y' = 285°  'A' = 345°
'!' = 999°  (中旋标记)
```

#### 删除地板
```csharp
// 删除单个选中的地板
public void DeleteSingleSelection(bool backspace = true)

// 删除多个选中的地板
public void DeleteMultiSelection(bool backspace = true)

// 核心删除方法
private bool DeleteFloor(int floorIndex, bool remakePath = true)
```

#### 地板数据结构
地板数据存储在 `LevelData` 中：
- **旧版本**: `pathData` (字符串，如 "RRUULLDD")
- **新版本**: `angleData` (浮点数组，如 [0, 0, 90, 90, 180, 180, 270, 270])

#### 地板属性修改
```csharp
// 修改地板角度
levelData.angleData[index] = newAngle;

// 修改地板字符 (旧版本)
levelData.pathData = levelData.pathData.Substring(0, index) + newChar + levelData.pathData.Substring(index + 1);

// 应用修改到游戏对象
ApplyEventsToFloors();
```

### 2. 事件操作

#### 创建事件
```csharp
// 在选中地板添加事件
public void AddEventAtSelected(LevelEventType eventType)

// 核心添加方法
private void AddEvent(int floorID, LevelEventType eventType)

// 手动创建事件
LevelEvent newEvent = new LevelEvent(floorID, eventType);
levelData.levelEvents.Add(newEvent);
```

#### 删除事件
```csharp
// 删除选中地板的指定类型事件
public void RemoveEventAtSelected(LevelEventType eventType)

// 删除单个事件
public void RemoveEvent(LevelEvent evnt, bool skipDecorationUpdate = false)

// 删除多个事件
public void RemoveEvents(List<LevelEvent> events)
```

#### 事件属性修改
```csharp
// 修改事件属性
levelEvent["propertyName"] = newValue;

// 启用/禁用事件
levelEvent.active = true/false;

// 显示/隐藏装饰事件
levelEvent.visible = true/false;

// 禁用特定属性
levelEvent.disabled["propertyName"] = true/false;
```

#### 事件数据验证
```csharp
// 属性值验证
PropertyInfo propertyInfo = levelEvent.info.propertiesInfo["propertyName"];
float validatedValue = propertyInfo.Validate(inputValue);
int validatedInt = propertyInfo.Validate(inputInt);
Vector2 validatedVector = propertyInfo.Validate(inputVector);
```

### 3. 编辑器操作流程

#### 地板编辑流程
1. **选择地板**: `SelectFloor(scrFloor floor)`
2. **修改属性**: 直接修改 `scrFloor` 对象属性
3. **更新数据**: 将修改同步到 `LevelData`
4. **重新生成**: 调用 `ApplyEventsToFloors()` 应用修改

#### 事件编辑流程
1. **选择地板**: 确定事件所在的地板
2. **添加事件**: `AddEventAtSelected(eventType)`
3. **编辑属性**: 通过属性面板修改事件数据
4. **验证数据**: 确保属性值符合要求
5. **应用修改**: 自动同步到关卡数据

#### 撤销/重做系统
```csharp
// 保存状态 (用于撤销)
public void SaveState(bool clearRedo = true, bool dataHasChanged = true)

// 撤销操作
public void Undo()

// 重做操作  
public void Redo()

// 状态作用域 (自动保存/恢复)
using (new SaveStateScope(editor))
{
    // 进行修改操作
}
```

### 4. 数据同步机制

#### 关卡数据 → 游戏对象
```csharp
// 重新生成所有地板
scrLevelMaker.instance.MakeLevel();

// 应用事件到地板
ApplyEventsToFloors();

// 更新装饰物
UpdateDecorationObjects();
```

#### 游戏对象 → 关卡数据
```csharp
// 从地板对象更新角度数据
for (int i = 0; i < floors.Count - 1; i++)
{
    levelData.angleData[i] = floors[i + 1].floatDirection;
}

// 事件数据自动同步 (通过引用)
```

### 5. 属性控件系统

#### 属性类型映射
```csharp
PropertyType.Int      → PropertyControl_Text (数字输入)
PropertyType.Float    → PropertyControl_Text (浮点输入)
PropertyType.String   → PropertyControl_Text (文本输入)
PropertyType.Bool     → PropertyControl_Bool (开关)
PropertyType.Enum     → PropertyControl_Toggle (下拉选择)
PropertyType.Color    → PropertyControl_Color (颜色选择器)
PropertyType.Vector2  → PropertyControl_Vector2 (二维向量)
PropertyType.File     → PropertyControl_File (文件浏览)
```

#### 属性验证和约束
```csharp
// 数值范围限制
propertyInfo.float_min / float_max
propertyInfo.int_min / int_max

// 条件显示/隐藏
propertyInfo.showIfVals / hideIfVals
propertyInfo.enableIfVals / disableIfVals

// 必填属性
propertyInfo.required = true
```

### 6. 实际操作示例

#### 创建一个简单的关卡
```csharp
// 1. 创建关卡数据
LevelData levelData = new LevelData();
levelData.Setup();

// 2. 设置基本信息
levelData.artist = "艺术家";
levelData.song = "歌曲名";
levelData.levelData.bpm = 120;

// 3. 创建地板序列 (右-上-左-下)
levelData.angleData = new List<float> { 0f, 90f, 180f, 270f };

// 4. 添加速度变化事件
LevelEvent speedEvent = new LevelEvent(1, LevelEventType.SetSpeed);
speedEvent["speedType"] = SpeedType.Multiplier;
speedEvent["beatsPerMinute"] = 120f;
speedEvent["bpmMultiplier"] = 2f;
levelData.levelEvents.Add(speedEvent);

// 5. 保存关卡
string json = levelData.Encode();
File.WriteAllText("level.adofai", json);
```

#### 批量修改事件属性
```csharp
// 将所有SetSpeed事件的速度倍数改为1.5
foreach (LevelEvent evt in levelData.levelEvents)
{
    if (evt.eventType == LevelEventType.SetSpeed)
    {
        evt["bpmMultiplier"] = 1.5f;
    }
}
```

#### 在指定位置插入地板
```csharp
// 在第3个位置插入一个45度地板
int insertIndex = 2;
float newAngle = 45f;

// 插入角度数据
levelData.angleData.Insert(insertIndex, newAngle);

// 更新所有事件的地板索引
foreach (LevelEvent evt in levelData.levelEvents)
{
    if (evt.floor >= insertIndex)
    {
        evt.floor++;
    }
}
```

### 7. 错误处理和验证

#### 常见错误类型
- **角度数据不匹配**: `angleData.Count != pathData.Length`
- **事件地板索引越界**: `event.floor >= floorCount`
- **属性值超出范围**: 需要使用 `PropertyInfo.Validate()`
- **必填属性缺失**: 检查 `PropertyInfo.required`

#### 数据完整性检查
```csharp
// 检查事件地板索引
foreach (LevelEvent evt in levelData.levelEvents)
{
    if (evt.floor < 0 || evt.floor >= levelData.angleData.Count)
    {
        Debug.LogError($"Event {evt.eventType} has invalid floor index: {evt.floor}");
    }
}

// 检查属性值有效性
foreach (var property in evt.info.propertiesInfo)
{
    if (property.Value.required && !evt.data.ContainsKey(property.Key))
    {
        Debug.LogError($"Required property {property.Key} is missing");
    }
}
```

## 相关文件
- `src/ADOFAI/`: ADOFAI核心命名空间
- `src/ADOFAI.Editor/`: 编辑器相关
- `src/ADOFAI.Editor.Actions/`: 编辑器操作动作
- `src/ffx*.cs`: 各种效果实现
- `src/scr*.cs`: 各种脚本组件
- `src/scnEditor.cs`: 关卡编辑器主控制器
