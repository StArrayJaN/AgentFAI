# 部署并运行

1. 首先，在IDE中构建一次项目  
2. 然后，打开你的Unity Hub，再打开`AgentFAI.UI`文件夹，此时会打开Unity编辑器  
3. 然后前往目录：`AgentFAI.UI/Assets/ThunderKitSettings/PipeLine`  
4. 双击 `Pipeline`，点击`Execute`，来构建资源包 
5. 完成后编辑文件：[AgentFAI/AgentFAI.csproj](https://github.com/StArrayJaN/AgentFAI/blob/main/AgentFAI/AgentFAI.csproj#L29)  
将：
```xml
<PropertyGroup>
    <GameExePath>D:\Softwares\Steam\steamapps\common\A Dance of Fire and Ice\A Dance of Fire and Ice.exe</GameExePath>
    <AutoLaunchGame>false</AutoLaunchGame>
</PropertyGroup>
```
中的`D:\Softwares\Steam\steamapps\common\A Dance of Fire and Ice\A Dance of Fire and Ice.exe`替换为游戏exe的路径  
再将`<AutoLaunchGame>false</AutoLaunchGame>`替换为`<AutoLaunchGame>true</AutoLaunchGame>`，如已为true则不需要替换  
6. 再次构建项目，游戏已经启动，即为部署成功  
之后如已修改代码，重复：1，3，4，6  
如未修改代码，重复：3，4，6
