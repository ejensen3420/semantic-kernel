// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.TemplateEngine.Prompt;
using RepoUtils;

// ReSharper disable once InconsistentNaming
public static class Example06_TemplateLanguage
{
    /// <summary>
    /// Show how to invoke a Native Function written in C#
    /// from a Semantic Function written in natural language
    /// </summary>
    public static async Task RunAsync()
    {
        Console.WriteLine("======== TemplateLanguage ========");

        //string openAIModelId = TestConfiguration.OpenAI.ChatModelId;
        //string openAIApiKey = TestConfiguration.OpenAI.ApiKey;

        var azureEndpoint = "https://eastus-shared-prd-cs.openai.azure.com";

        var model = "gpt-35-turbo";
        //"gpt-4";
        //"gpt-35-turbo";

        var embeding_model = "text-embedding-ada-002";

        //if (openAIModelId == null || openAIApiKey == null)
        //{
        //    Console.WriteLine("OpenAI credentials not found. Skipping example.");
        //    return;
        //}

        IKernel kernel = Kernel.Builder
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithAzureChatCompletionService(model, azureEndpoint, new DefaultAzureCredential())
            .Build();

        // Load native skill into the kernel skill collection, sharing its functions with prompt templates
        // Functions loaded here are available as "time.*"
        kernel.ImportSkill(new TimeSkill(), "time");

        // Semantic Function invoking time.Date and time.Time native functions
        const string FunctionDefinition = @"
Today is: {{time.Date}}
Current time is: {{time.Time}}

Answer to the following questions using JSON syntax, including the data used.
Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
Is it weekend time (weekend/not weekend)?
";

        // This allows to see the prompt before it's sent to OpenAI
        Console.WriteLine("--- Rendered Prompt");
        var promptRenderer = new PromptTemplateEngine();
        var renderedPrompt = await promptRenderer.RenderAsync(FunctionDefinition, kernel.CreateNewContext());
        Console.WriteLine(renderedPrompt);

        // Run the prompt / semantic function
        var kindOfDay = kernel.CreateSemanticFunction(FunctionDefinition, maxTokens: 150);

        // Show the result
        Console.WriteLine("--- Semantic Function result");
        var result = await kernel.RunAsync(kindOfDay);
        Console.WriteLine(result);

        /* OUTPUT:

            --- Rendered Prompt

            Today is: Friday, April 28, 2023
            Current time is: 11:04:30 PM

            Answer to the following questions using JSON syntax, including the data used.
            Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
            Is it weekend time (weekend/not weekend)?

            --- Semantic Function result

            {
                "date": "Friday, April 28, 2023",
                "time": "11:04:30 PM",
                "period": "night",
                "weekend": "weekend"
            }
         */
    }
}
