
using System.ComponentModel;
using UnityEngine.SceneManagement;

namespace AgentFAI.Tools;

public static class GameTools
{
    [Description("进入关卡编辑器")]
    public static bool EnterLevelEditor()
    {
        if (SceneManager.GetActiveScene().name == "scnEditor") return true;
        SceneManager.LoadScene("scnEditor");
        return true;
    }
}