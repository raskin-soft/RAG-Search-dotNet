using Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RAG_Search.Services
{


    public class QdrantService
    {
        private readonly QdrantClient _client;
        private readonly string _collectionName = "pdf_chunks";

        public QdrantService(IConfiguration config)
        {
            var host = config["Qdrant:Host"] ?? "localhost";
            var port = int.Parse(config["Qdrant:Port"] ?? "6334");

            _client = new QdrantClient(host, port);

            // Ensure collection exists (synchronous wait on startup is OK; in production consider async init)
            EnsureCollectionAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureCollectionAsync()
        {
            try
            {
                var collections = await _client.ListCollectionsAsync();

                // Case 1: SDK returns List<string>
                if (collections is IEnumerable<string> names)
                {
                    if (!names.Contains(_collectionName))
                    {
                        await CreateCollectionAsync();
                    }
                    return;
                }

                // Case 2: SDK returns an object with Collections property
                var collProp = collections.GetType().GetProperty("Collections");
                if (collProp != null)
                {
                    var collValue = collProp.GetValue(collections);
                    if (collValue is IEnumerable<object> collList)
                    {
                        var names2 = collList
                            .Select(c =>
                            {
                                var nameProp = c.GetType().GetProperty("Name");
                                return nameProp?.GetValue(c)?.ToString();
                            })
                            .Where(n => !string.IsNullOrEmpty(n));

                        if (!names2.Contains(_collectionName))
                        {
                            await CreateCollectionAsync();
                        }
                        return;
                    }
                }

                // Fallback: always try to create (will no-op or error if exists)
                await CreateCollectionAsync();
            }
            catch
            {
                // As a fallback, just try to create
                await CreateCollectionAsync();
            }
        }

        private async Task CreateCollectionAsync()
        {
            await _client.CreateCollectionAsync(
                _collectionName,
                new VectorParams
                {
                    Size = 1536, // adjust to your embedding dim
                    Distance = Distance.Cosine
                }
            );
        }


        public async Task UpsertChunkAsync(string id, float[] embedding, Dictionary<string, object> payload)
        {
            // Use the style that your SDK version supports:
            // PointId with Uuid property (works when id is a string)
            var pointId = Guid.NewGuid();  // new PointId { Uuid = id };

            var point = new PointStruct
            {
                Id = pointId,
                // NOTE: many SDK versions nest Vector -> Dense -> Data
                Vectors = new Vectors
                {
                    Vector = new Vector
                    {
                        Dense = new DenseVector { Data = { embedding } }
                    }
                }
            };

            // Payload values as string (or extend to handle numbers/objects as needed)
            foreach (var kv in payload)
            {
                point.Payload[kv.Key] = new Qdrant.Client.Grpc.Value { StringValue = kv.Value?.ToString() ?? "" };
            }

            await _client.UpsertAsync(_collectionName, new[] { point });
        }

        public async Task<List<(string Chunk, float Score, Dictionary<string, object> Payload)>> SearchAsync(float[] embedding, int topK = 5)
        {
            var results = await _client.SearchAsync(_collectionName, embedding, limit: (ulong)topK);

            return results
                .Select(r =>
                {
                    var preview = r.Payload.TryGetValue("preview", out var val) ? val.StringValue : "";
                    var payloadDict = r.Payload.ToDictionary(
                        kv => kv.Key,
                        kv => (object)kv.Value.StringValue
                    );
                    return (preview, r.Score, payloadDict);
                })
                .ToList();
        }


        public async Task<List<string>> GetAllUploadedDocumentsAsync()
        {
            // List all points in the collection
            var scrollResponse = await _client.ScrollAsync(_collectionName);

            var fileNames = scrollResponse.Result
                .SelectMany(p => p.Payload)
                .Where(kv => kv.Key == "fileName")
                .Select(kv => kv.Value.StringValue)
                .Distinct()
                .ToList();

            return fileNames;
        }


    }


}
