// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Reliability;

public static class Program
{
    // ReSharper disable once InconsistentNaming
    public static async Task Main(string[] args)
    {
        // Load configuration from environment variables or user secrets.
        LoadUserSecrets();

        // Execution canceled if the user presses Ctrl+C.
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken cancelToken = cancellationTokenSource.ConsoleCancellationToken();

        string? defaultFilter = null; // Modify to filter examples

        // Check if args[0] is provided
        string? filter = args.Length > 0 ? args[0] : defaultFilter;

        // Run examples based on the filter
        await RunExamplesAsync(filter, cancelToken);
    }

    private static async Task RunExamplesAsync(string? filter, CancellationToken cancellationToken)
    {
        var examples = (Assembly.GetExecutingAssembly().GetTypes())
            .Where(type => type.Name.StartsWith("Example", StringComparison.OrdinalIgnoreCase))
            .Select(type => type.Name).ToList();

        // Filter and run examples
        foreach (var example in examples)
        {
            if (string.IsNullOrEmpty(filter) || example.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Console.WriteLine($"Running {example}...");

                    var method = Assembly.GetExecutingAssembly().GetType(example)?.GetMethod("RunAsync");
                    if (method == null)
                    {
                        Console.WriteLine($"Example {example} not found");
                        continue;
                    }

                    bool hasCancellationToken = method.GetParameters().Any(param => param.ParameterType == typeof(CancellationToken));

                    var taskParameters = hasCancellationToken ? new object[] { cancellationToken } : null;
                    if (method.Invoke(null, taskParameters) is Task t)
                    {
                        await t.SafeWaitAsync(cancellationToken);
                    }
                    else
                    {
                        method.Invoke(null, null);
                    }
                }
                catch (ConfigurationNotFoundException ex)
                {
                    Console.WriteLine($"{ex.Message}. Skipping example {example}.");
                }
            }
        }
    }

    private static void LoadUserSecrets()
    {
        var configJson = @"{
  ""Logging"": {
    ""LogLevel"": { // No provider, LogLevel applies to all the enabled providers.
      ""Default"": ""Information"", // Default, application level if no other level applies
      ""Microsoft"": ""Warning"", // Log level for log category which starts with text 'Microsoft' (i.e. 'Microsoft.*')
      ""Microsoft.Graph.GraphServiceClient"": ""Information"",
      ""Microsoft.SemanticKernel.MsGraph.Skills"": ""Information""
    }
  },
  ""MsGraph"": {
    ""ClientId"": ""<Your App Client ID>"",
    ""TenantId"": ""<tenant ID>"", // MSA/Consumer/Personal tenant,  https://learn.microsoft.com/azure/active-directory/develop/accounts-overview
    ""Scopes"": [
      ""User.Read"",
      ""Files.ReadWrite"",
      ""Tasks.ReadWrite"",
      ""Mail.Send""
    ],
    ""RedirectUri"": ""http://localhost""
  },
  ""OneDrivePathToFile"": ""<path to a text file in your OneDrive>"", // e.g. ""Documents/MyFile.txt""
  ""DefaultCompletionServiceId"": ""gpt-35-turbo"", // ""gpt-3.5-turbo"" (note the '.' between 3 and 5) for OpenAI
  ""OpenAI"": {
      ""ServiceId"": ""gpt-3.5-turbo1-1"",
      ""ModelId"": ""gpt-3.5-turbo1-1"",
    ""chatModelId"": ""gpt-35-turbo1-1"",
    //  ""ApiKey"": """"
  },
  ""AzureOpenAI"": {
    ""ServiceId"": ""gpt-35-turbo1-1"",
    ""deploymentName"": ""gpt-35-turbo1-1"",
    ""chatDeploymentName"": ""gpt-35-turbo1-1"",
    ""modelId"": ""gpt-35-turbo1-1"",
    ""chatModelId"": ""gpt-35-turbo1-1"",
    ""Endpoint"": ""https://eastus-shared-prd-cs.openai.azure.com""
    //  ""ApiKey"": """"
  }
}";

        IConfigurationRoot configRoot = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configJson)))
            .Build();
        TestConfiguration.Initialize(configRoot);
    }

    private static CancellationToken ConsoleCancellationToken(this CancellationTokenSource tokenSource)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Canceling...");
            tokenSource.Cancel();
            e.Cancel = true;
        };

        return tokenSource.Token;
    }

    private static async Task SafeWaitAsync(this Task task,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await task.WaitAsync(cancellationToken);
            Console.WriteLine("== DONE ==");
        }
        catch (ConfigurationNotFoundException ex)
        {
            Console.WriteLine($"{ex.Message}. Skipping example.");
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}
