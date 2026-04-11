using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;

namespace AgentFAI;

public class AgentManager : IAsyncDisposable
{
    private readonly AIAgent _agent;
    private AgentSession? _session;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    public AgentManager(
        string apiKey,
        string apiUrl,
        string model,
        string presetPrompt = "",
        string agentName = nameof(AgentFAI),
        bool isReasoningModel = false,
        IEnumerable<AITool>? functions = null)
    {
        Main.Mod.Logger.Log("AgentManager Init");

        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(apiUrl) });

        var options = new ChatOptions
        {
            Instructions = presetPrompt,
            Tools = functions?.ToList() ?? []
        };

        if (isReasoningModel)
        {
            options.Reasoning = new ReasoningOptions
            {
                Effort = ReasoningEffort.ExtraHigh,
                Output = ReasoningOutput.Full
            };
        }

        _agent = client.GetChatClient(model)
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = agentName,
                ChatOptions = options
            });
    }

    // ---------------------------------------------------------------
    // Session 懒初始化（线程安全）
    // ---------------------------------------------------------------

    private async Task<AgentSession> GetOrCreateSessionAsync(CancellationToken ct = default)
    {
        if (_session != null) return _session;

        await _sessionLock.WaitAsync(ct);
        try
        {
            // double-check
            _session ??= await _agent.CreateSessionAsync(ct);
            return _session;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    // ---------------------------------------------------------------
    // 非流式
    // ---------------------------------------------------------------

    public async Task<AgentResponse> SendMessageAsync(
        string message,
        CancellationToken ct = default)
    {
        var session = await GetOrCreateSessionAsync(ct);
        return await _agent.RunAsync(message, session, cancellationToken:ct);
    }

    // ---------------------------------------------------------------
    // 流式：逐块返回 AgentResponse
    // ---------------------------------------------------------------

    public async IAsyncEnumerable<AgentResponseUpdate> SendMessageStreamingAsync(
        string message,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var session = await GetOrCreateSessionAsync(ct);

        await foreach (var chunk in _agent.RunStreamingAsync(message, session,cancellationToken: ct))
        {
            yield return chunk;
        }
    }

    // ---------------------------------------------------------------
    // 流式：聚合为完整文本（便捷重载）
    // ---------------------------------------------------------------

    public async Task<string> SendMessageStreamingFullAsync(
        string message,
        Action<string>? onChunk = null,
        CancellationToken ct = default)
    {
        var builder = new System.Text.StringBuilder();

        await foreach (var chunk in SendMessageStreamingAsync(message, ct))
        {
            var contents = chunk.Contents;
            foreach (var content in contents)
            {
                if (content is not TextContent textContent) continue;

                builder.Append(textContent.Text);
                onChunk?.Invoke(textContent.Text);
            }
        }

        return builder.ToString();
    }

    // ---------------------------------------------------------------
    // Session 管理
    // ---------------------------------------------------------------

    /// <summary>清除当前 session，下次发送时自动重建（相当于重置对话）</summary>
    public async Task ResetSessionAsync(CancellationToken ct = default)
    {
        await _sessionLock.WaitAsync(ct);
        try
        {
            if (_session != null)
            {
                _session = null;
            }
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    // ---------------------------------------------------------------
    // IAsyncDisposable
    // ---------------------------------------------------------------

    public async ValueTask DisposeAsync()
    {
        await ResetSessionAsync();
        _sessionLock.Dispose();
    }

    public void ExportSession(string path)
    {
        if (_session.TryGetInMemoryChatHistory(out var result))
        {
            var sb = new StringBuilder();

            foreach (var message in result)
            {
                sb.Append('[');
                sb.Append(message.Role.ToString().ToUpperInvariant());
                sb.AppendLine("]");

                foreach (var content in message.Contents)
                {
                    switch (content)
                    {
                        case TextContent text:
                            sb.AppendLine(text.Text);
                            break;

                        case TextReasoningContent reasoning:
                            sb.AppendLine("--- thinking ---");
                            sb.AppendLine(reasoning.Text);
                            sb.AppendLine("--- /thinking ---");
                            break;

                        case FunctionCallContent call:
                            sb.Append(">> ");
                            sb.Append(call.Name);
                            sb.Append("  (");
                            sb.Append(call.CallId);
                            sb.AppendLine(")");
                            if (call.Arguments is { Count: > 0 })
                            {
                                foreach (var kv in call.Arguments)
                                {
                                    sb.Append("   ");
                                    sb.Append(kv.Key);
                                    sb.Append(": ");
                                    sb.AppendLine(kv.Value?.ToString() ?? "null");
                                }
                            }
                            break;

                        case FunctionResultContent functionResultContent:
                            sb.Append("<< ");
                            sb.Append(functionResultContent.CallId);
                            sb.AppendLine();
                            sb.AppendLine(functionResultContent.Result?.ToString() ?? "null");
                            break;
                        
                        default:
                            sb.Append("[");
                            sb.Append(content.GetType().Name);
                            sb.AppendLine("]");
                            break;
                    }
                }

                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}