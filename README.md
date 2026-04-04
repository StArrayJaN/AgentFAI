# AgentFAI

一个为《A Dance of Fire and Ice》（冰与火之舞）开发的 Unity Mod Manager (UMM) Mod，集成了 AI 助手功能，帮助玩家进行关卡编辑和游戏增强。

## 功能特性

### 🎮 游戏工具
- **关卡编辑器集成** - 快速进入关卡编辑器
- **事件管理** - 查看和管理关卡事件
- **系统信息** - 获取游戏运行时的系统信息

### 🤖 AI 助手（开发中）
- **对话界面** - 与 AI 助手进行自然语言交互
- **智能关卡编辑** - 通过对话创建和修改关卡
- **自动化操作** - 使用 AI 辅助完成复杂的关卡设计

### 📦 资源管理
- **资源加载器** - 支持从 Resources 目录加载文本和其他资源文件
- **模块化设计** - 易于扩展和维护的代码结构

## 技术栈

- **.NET Framework 4.8.1** - 目标框架
- **Unity Mod Manager** - Mod 加载和管理
- **Harmony** - 运行时补丁和方法拦截
- **Microsoft Agents AI** - AI 代理框架
- **OpenAI SDK** - OpenAI API 集成
- **ILRepack** - DLL 合并工具，将所有依赖打包为单个 DLL

## 安装方法

### 前置要求
1. 已安装《A Dance of Fire and Ice》游戏
2. 已安装 [Unity Mod Manager](https://www.nexusmods.com/site/mods/21) (版本 0.27.0 或更高)

### 安装步骤
1. 下载最新版本的 AgentFAI Mod
2. 将 `out` 文件夹中的所有内容复制到游戏的 `Mods/AgentFAI/` 目录
3. 启动游戏并打开 Unity Mod Manager
4. 在 UMM 界面中启用 AgentFAI Mod

## 构建项目

### 环境要求
- .NET SDK (支持 .NET Framework 4.8.1)
- Rider 或 Visual Studio

### 构建步骤

```bash
# 克隆仓库
git clone <repository-url>
cd AgentFAI

# 还原依赖
dotnet restore

# 构建项目（Debug 模式）
dotnet build AgentFAI/AgentFAI.csproj -c Debug

# 构建项目（Release 模式）
dotnet build AgentFAI/AgentFAI.csproj -c Release
```

### 配置游戏路径

在项目文件中修改 `GameExePath` 属性：

```xml
<PropertyGroup>
    <GameExePath>D:\Your\Game\Path\A Dance of Fire and Ice.exe</GameExePath>
    <AutoLaunchGame>true</AutoLaunchGame>
</PropertyGroup>
```

构建完成后，Mod 会自动部署到游戏目录并可选择性地启动游戏。

### 项目结构

```
AgentFAI/
├── Main.cs              # Mod 主入口点
├── Settings.cs          # Mod 设置和 UI
├── Patches.cs           # Harmony 补丁
├── GameTools.cs         # 游戏工具函数
├── ResourceLoader.cs    # 资源加载器
├── Info.json            # UMM Mod 信息
├── Resources/           # 嵌入的资源文件
├── lib/                 # 额外的 DLL 库
└── AgentFAI.csproj      # 项目配置文件
```

### ILRepack 配置

项目使用 ILRepack 将所有 NuGet 包依赖合并到主 DLL 中，减少部署文件数量：

- **主 DLL**: `AgentFAI.dll` (包含所有依赖)
- **排除项**: 游戏本身的程序集（如 UnityEngine、Assembly-CSharp 等）不会被合并
- **自动执行**: 每次构建后自动执行 ILRepack

## 使用方法

### 启用 Mod
1. 在游戏中按 `Ctrl + F10` 打开 Unity Mod Manager
2. 找到 AgentFAI 并切换开关启用
3. Mod 会在启用时自动应用所有补丁

### 访问功能
- 在 UMM 界面中点击 AgentFAI 打开设置面板
- 使用内置的游戏工具函数
- （开发中）通过对话界面与 AI 助手交互

## 开发计划

查看 [TODO.md](TODO.md) 了解完整的功能路线图。

### 即将推出的功能
- [ ] 完整的关卡编辑器 AI 辅助
- [ ] 对话式关卡创建
- [ ] 关卡截图和预览
- [ ] 更多游戏自动化工具

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

本项目采用 GNU General Public License v3.0 (GPLv3) 许可证。详见 LICENSE 文件。

**重要说明**: 由于使用 GPLv3 许可证，任何基于此项目的衍生作品也必须采用相同的许可证开源。

## 致谢

- [Unity Mod Manager](https://github.com/newman55/unity-mod-manager) - Mod 加载框架
- [Harmony](https://github.com/pardeike/Harmony) - 运行时补丁库
- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) - AI 代理框架
- 《A Dance of Fire and Ice》开发团队 - 7th Beat Games

## 联系方式

- 作者: StArray
- 问题反馈: 请通过 GitHub Issues 提交

---

**注意**: 此 Mod 仍在积极开发中，某些功能可能不完整或发生变化。
