
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AgentFAI.Tools;

public static class GameTools
{
    [AgentTool("进入关卡编辑器")]
    public static bool EnterLevelEditor()
    {
        if (SceneManager.GetActiveScene().name == "scnEditor") return true;
        SceneManager.LoadScene("scnEditor");
        return true;
    }
    
    [AgentTool("进入官方关卡,不需要进入编辑器")]
    public static bool EnterLevel([Description("指定的关卡名，可通过GetInternalLevels获取")]string levelName)
    {
        if (SceneManager.GetActiveScene().name == levelName) return true;
        scrController.instance.EnterLevel(levelName);
        return true;
    }

    [AgentTool("获取关卡列表")]
    public static Dictionary<string,string[]> GetInternalLevels()
    {
        var worldDatas = GCNS.worldData;
        var worldLevels = new Dictionary<string, string[]>();
        GCNS.allWorlds.ForEach(a => worldLevels.Add(a,[""]));
        foreach (var worldData in worldDatas)
        {
            if (!worldLevels.ContainsKey(worldData.Key)) continue;
            var levels = new List<string>();
            for (int i = 1; i <= worldData.Value.levelCount; i++)
            {
                if (i + 1 > worldData.Value.levelCount)
                {
                    levels.Add($"{worldData.Key}-X");
                    break;
                }
                levels.Add($"{worldData.Key}-{i}");
            }
            worldLevels[worldData.Key] = levels.ToArray();
        }
        return worldLevels;
    }
    
    [AgentTool("设置星球颜色")]
    public static void SetPlanetColor([Description("索引，0为冰星球，1为火星球")]int index,
        [Description("颜色,范围为0x000000到0xFFFFFF")]uint color)
    {
        byte a = (byte)((color >> 24) & 0xFF);
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);
        var iceRenderer = scrController.instance.planetBlue.planetRenderer;
        var fireRenderer = scrController.instance.planetRed.planetRenderer;
        if (index == 0) iceRenderer.SetPlanetColor(new Color(r,g,b,a));
        else fireRenderer.SetPlanetColor(new Color(r,g,b,a));
    }
    
    [AgentTool("获取当前场景")]
    public static string GetCurrentScene()
    {
        return SceneManager.GetActiveScene().name;
    }
}