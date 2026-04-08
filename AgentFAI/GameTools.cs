using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AgentFAI;

public class GameTools
{
    [Description("进入关卡编辑器")]
    public static void EnterEditor()
    {
        var load = SceneManager.LoadSceneAsync("scenes/scnEditor");
    }

    public static bool IsLevelEditor() => ADOBase.isLevelEditor;

    [Description("为关卡添加事件")]
    public static bool AddEvent(int floor,string @event)
    {
        return true;
    }

    public static List<string> GetAllEvents()
    {
        var events = GCS.levelEventsInfo;
        return events.Where(a => !a.Value.pro).Select(a => a.Key).ToList();
    }

    [Description("获取系统信息")]
    public static string GetSystemInfo()
    {
        var info = new Dictionary<string, string>
        {
            { "OS", SystemInfo.operatingSystem },
            { "CPU", SystemInfo.processorType },
            { "GPU", SystemInfo.graphicsDeviceName },
            { "GPU Type", SystemInfo.graphicsDeviceType.ToString() },
            { "GPU Shader", SystemInfo.graphicsShaderLevel.ToString() },
            { "GPU Memory", SystemInfo.graphicsMemorySize.ToString() },
            { "GPU Vendor", SystemInfo.graphicsDeviceVendor },
            { "GPU Version", SystemInfo.graphicsDeviceVersion },
        };
        return info.Aggregate("System:", (current, item) => current + "\n" + item.Key + ": " + item.Value);
    }
}