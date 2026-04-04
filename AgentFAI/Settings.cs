using System;
using System.ClientModel;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using UnityModManagerNet;
using UnityEngine;

namespace AgentFAI
{
    /// <summary>
    /// Mod settings class
    /// Mod 设置类
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        public string API_KEY = "";
        
        public string API_URL= "";
        
        public string Model = "";

        private string message;
        private AIAgent? agent;
        /// <summary>
        /// Draw mod GUI / 绘制 Mod GUI
        /// </summary>
        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            // Example: Draw settings UI / 示例：绘制设置界面
            GUILayout.Label("模型设置");
            GUILayout.BeginHorizontal();
            GUILayout.Label("API密钥:");
            API_KEY = GUILayout.TextField(API_KEY);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("API地址:");
            API_URL = GUILayout.TextField(API_URL);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("模型:");
            Model = GUILayout.TextField(Model);
            GUILayout.EndHorizontal();
 
            message = GUILayout.TextField(message);
            if (GUILayout.Button("发送"))
            {
                if (agent == null)
                {
                    var client = new OpenAIClient(new ApiKeyCredential(API_KEY),
                        new OpenAIClientOptions()
                        {
                            Endpoint = new Uri(API_URL)
                        });
                    agent = client.GetChatClient(Model)
                        .AsAIAgent(instructions: "你在C#搭建的Microsoft.Agents.AI环境中，接下来请回复用户问题",
                            tools: [AIFunctionFactory.Create(GameTools.GetSystemInfo)]);
                }
                SendMessage(message);
                message = "";
            }
        }

        public async Task SendMessage(string message)
        {
            var response = await agent.RunAsync(message);
            Main.Mod.Logger.Log(response.Text);
        }

        /// <summary>
        /// Called when saving GUI / 保存设置时调用
        /// </summary>
        public void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Save(modEntry);
        }

        /// <summary>
        /// Save settings / 保存设置
        /// </summary>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        /// <summary>
        /// Load settings / 加载设置
        /// </summary>
        public static Settings Load(UnityModManager.ModEntry modEntry)
        {
            return Load<Settings>(modEntry);
        }
    }
}