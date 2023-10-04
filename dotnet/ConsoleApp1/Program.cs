using System.Text;
using Microsoft.Extensions.Configuration;

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
      ""ServiceId"": ""gpt-3.5-turbo"",
      ""ModelId"": ""gpt-3.5-turbo"",
    //  ""ApiKey"": """"
  },
  ""AzureOpenAI"": {
    ""ServiceId"": ""gpt-35-turbo"",
    ""deploymentName"": ""gpt-35-turbo"",
    ""chatDeploymentName"": ""gpt-35-turbo"",
    ""modelId"": ""gpt-3.5-turbo"",
    ""Endpoint"": ""https://eastus-shared-prd-cs.openai.azure.com""
    //  ""ApiKey"": """"
  }
}";

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configJson)))
    .Build();
TestConfiguration.Initialize(configRoot);
