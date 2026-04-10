using System;
using System.ComponentModel;

namespace AgentFAI;

[AttributeUsage(AttributeTargets.Method)]
public class AgentTool(string description) : DescriptionAttribute(description)
{
    public readonly string description;
}
[AttributeUsage(AttributeTargets.Parameter)]
public class AgentToolParameter(string description) : AgentTool(description)
{ 
}