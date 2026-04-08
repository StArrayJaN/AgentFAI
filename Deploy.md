# 部署并运行

1. 首先，打开你的Unity Hub，再打开`AgentFAI.UI`文件夹，此时会打开Unity编辑器  
2. 点击Tools/ThunderKit/Setting，点击ThunderKit Settings，在`Locate and Load game files for project`块，点击`Browser`，选择游戏exe以到导入游戏，之后按它的提示操作
3. 然后前往目录：`AgentFAI.UI/Assets/ThunderKitSettings/PipeLine`  
4. 双击 `Pipeline`，点击`Execute`，来构建资源包 
5. 完成后，打开IDE并编辑文件：[AgentFAI/AgentFAI.csproj](https://github.com/StArrayJaN/AgentFAI/blob/main/AgentFAI/AgentFAI.csproj#L29)  
将：
```xml
<PropertyGroup>
    <GameExePath>D:\Softwares\Steam\steamapps\common\A Dance of Fire and Ice\A Dance of Fire and Ice.exe</GameExePath>
    <AutoLaunchGame>false</AutoLaunchGame>
</PropertyGroup>
```
中的`D:\Softwares\Steam\steamapps\common\A Dance of Fire and Ice\A Dance of Fire and Ice.exe`替换为游戏exe的路径  
再将`<AutoLaunchGame>false</AutoLaunchGame>`替换为`<AutoLaunchGame>true</AutoLaunchGame>`，如已为true则不需要替换(可选，用于自动启动游戏)  
6. 构建项目，没有报错即为成功  
之后重复：4，6  
**注意：因为Unity项目依赖主项目代码，所以修改完成后一定要构建一次以更新代码引用**  


