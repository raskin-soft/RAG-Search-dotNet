using System.Diagnostics;
using System.Globalization;

namespace RAG_Search.Services
{
    public class OllamaEmbeddingService
    {
        private readonly string _modelName = "nomic/embedding-text";

        /// <summary>
        /// Get embedding vector for a given text.
        /// Automatically ensures the model is installed.
        /// </summary>
        //public float[] GetEmbedding(string text)
        //{
        //    // Step 1: Ensure model exists
        //    EnsureModelExists();

        //    // Step 2: Run Ollama CLI to get embedding
        //    var psi = new ProcessStartInfo
        //    {
        //        FileName = "ollama",
        //        Arguments = $"run {_modelName} \"{text}\"",
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        UseShellExecute = false
        //    };

        //    using var process = Process.Start(psi);
        //    string output = process.StandardOutput.ReadToEnd();
        //    string error = process.StandardError.ReadToEnd();
        //    process.WaitForExit();

        //    if (process.ExitCode != 0)
        //        throw new Exception($"Ollama embed failed: {error}");

        //    // Step 3: Parse output into float array
        //    output = output.Trim().TrimStart('[').TrimEnd(']');
        //    var embedding = output.Split(',', StringSplitOptions.RemoveEmptyEntries)
        //        .Select(s => float.Parse(s, CultureInfo.InvariantCulture))
        //        .ToArray();

        //    return embedding;
        //}

        /// <summary>
        /// Checks if the model exists locally. Pulls it automatically if missing.
        /// </summary>
        //private void EnsureModelExists()
        //{
        //    // Run 'ollama list' to check if model exists
        //    var psiList = new ProcessStartInfo
        //    {
        //        FileName = "ollama",
        //        Arguments = "list",
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        UseShellExecute = false
        //    };

        //    using var processList = Process.Start(psiList);
        //    string outputList = processList.StandardOutput.ReadToEnd();
        //    processList.WaitForExit();

        //    if (!outputList.Contains(_modelName))
        //    {
        //        // Model not found, pull it
        //        var psiPull = new ProcessStartInfo
        //        {
        //            FileName = "ollama",
        //            Arguments = $"pull {_modelName}",
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            UseShellExecute = false
        //        };

        //        using var processPull = Process.Start(psiPull);
        //        string pullOutput = processPull.StandardOutput.ReadToEnd();
        //        string pullError = processPull.StandardError.ReadToEnd();
        //        processPull.WaitForExit();

        //        if (processPull.ExitCode != 0)
        //            throw new Exception($"Failed to pull Ollama model '{_modelName}': {pullError}");
        //    }
        //}
    }



}
