﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.SemanticKernel;
using RepoUtils;

// ReSharper disable once InconsistentNaming
public static class Example05_InlineFunctionDefinition
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Inline Function Definition ========");

        string openAIModelId = TestConfiguration.OpenAI.ChatModelId;

        /*
         * Example: normally you would place prompt templates in a folder to separate
         *          C# code from natural language code, but you can also define a semantic
         *          function inline if you like.
         */

        var builder = new KernelBuilder();

        var azureEndpoint = "https://eastus-shared-prd-cs.openai.azure.com";

        var model = "gpt-35-turbo1-1";
        //"gpt-4";
        //"gpt-35-turbo";

        IKernel kernel = new KernelBuilder()
            .WithLoggerFactory(ConsoleLogger.LoggerFactory)
            .WithAzureChatCompletionService(model, azureEndpoint, new DefaultAzureCredential())
            .Build();

        // Function defined using few-shot design pattern
        const string FunctionDefinition = @"
            Generate a creative reason or excuse for the given event.
            Be creative and be funny. Let your imagination run wild.

            Event: I am running late.
            Excuse: I was being held ransom by giraffe gangsters.

            Event: I haven't been to the gym for a year
            Excuse: I've been too busy training my pet dragon.

            Event: {{$input}}
            ";

        var excuseFunction = kernel.CreateSemanticFunction(FunctionDefinition, maxTokens: 100, temperature: 0.4, topP: 1);

        var result = await excuseFunction.InvokeAsync("I missed the F1 final race");
        Console.WriteLine(result);

        result = await excuseFunction.InvokeAsync("sorry I forgot your birthday");
        Console.WriteLine(result);

        var fixedFunction = kernel.CreateSemanticFunction($"Translate this date {DateTimeOffset.Now:f} to French format", maxTokens: 100);

        result = await fixedFunction.InvokeAsync();
        Console.WriteLine(result);
    }
}
