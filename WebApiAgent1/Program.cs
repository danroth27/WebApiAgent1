using System.ClientModel;
using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureOpenAIClient("openai")
    .AddChatClient("gpt-4o-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c => c.EnableSensitiveData = builder.Environment.IsDevelopment());

builder.AddAIAgent("writer", "You write short stories (100 words or less) about the specified topic.");

builder.AddAIAgent("editor", (sp, key) => new ChatClientAgent(
    sp.GetRequiredService<IChatClient>(),
    name: key,
    instructions: "You edit short stories to improve grammar and style, ensuring the stories are less than 100 words. Once finished editing, you select a title and format the story for publishing.",
    tools: [AIFunctionFactory.Create(FormatStory)]
));

builder.AddWorkflow("publisher", (sp, key) => AgentWorkflowBuilder.BuildSequential(
    workflowName: key,
    sp.GetRequiredKeyedService<AIAgent>("writer"),
    sp.GetRequiredKeyedService<AIAgent>("editor")
)).AddAsAIAgent();

// Register services for OpenAI responses and conversations
// This is also required for DevUI
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();
builder.Services.AddAGUI();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// Map endpoints for OpenAI responses and conversations
// This is also required for DevUI
app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapAGUI("ag-ui", app.Services.GetRequiredKeyedService<AIAgent>("publisher"));

if (builder.Environment.IsDevelopment())
{
    // Map DevUI endpoint to /devui
    app.MapDevUI();
}

app.Run();

[Description("Formats the story for publication, revealing its title.")]
string FormatStory(string title, string story) => $"""
    **Title**: {title}

    {story}
    """;
