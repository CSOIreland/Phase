using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace PhaseHarvester.AIModels
{
    internal class AiReader
    {
        internal string GetAiSummary(string json)
        {
            string summary=String.Empty;
            string promptFile = ConfigurationManager.AppSettings["PROMPT_LOCATION"] + "SummaryPrompt.txt";
            string prompt=File.ReadAllText(promptFile);
            prompt=prompt + ":"+ json;
            var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
               modelId: ConfigurationManager.AppSettings["LLM_MODEL"],
               apiKey:  ConfigurationManager.AppSettings["LLM_KEY"]
               
                )
            .Build();

            var reply = kernel.InvokePromptAsync(prompt, new(new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.1,
                MaxTokens = 4000
            }));
            reply.Wait(120000);
            return reply.Result.ToString();
        }

        internal float[] GetVectors(string summary)
        {
#pragma warning disable 
            var embeddingGenerator = new OpenAITextEmbeddingGenerationService(ConfigurationManager.AppSettings["EMBEDDINGS_MODEL"], ConfigurationManager.AppSettings["EMBEDDINGS_KEY"]);
#pragma warning restore
            var runTask=Task.Run(()=>embeddingGenerator.GenerateEmbeddingsAsync(new List<string> { summary }));
            runTask.Wait();
            return runTask.Result.First().ToArray();
         
        }
    }

}
