using Microsoft.SemanticKernel.Connectors.OpenAI;
using Sample;

namespace Phase
{
    public class Ai
    {
        internal float[] GetVectors(string query)
        {
#pragma warning disable 
            var embeddingGenerator = new OpenAITextEmbeddingGenerationService(AppServicesHelper.AppConfiguration.Settings["EMBEDDINGS_MODEL"], AppServicesHelper.AppConfiguration.Settings["EMBEDDINGS_KEY"]);
#pragma warning restore
            var runTask = Task.Run(() => embeddingGenerator.GenerateEmbeddingsAsync(new List<string> { query }));
            runTask.Wait();
            return runTask.Result.First().ToArray();

        }
    }
}
