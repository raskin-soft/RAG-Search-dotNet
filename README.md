# RAG System with OpenAI Embeddings and Qdrant

A Retrieval-Augmented Generation (RAG) system built with .NET that enables semantic search and question-answering over PDF documents and SQL database content using OpenAI embeddings and Qdrant vector database.


## Features

- 📄 **PDF Document Processing**: Upload and process PDF files with automatic text extraction and chunking
- 🗄️ **SQL Database Integration**: Load and index data directly from SQL Server databases
- 🔍 **Semantic Search**: Vector-based similarity search using OpenAI embeddings
- 💬 **AI-Powered Q&A**: Generate contextual answers using OpenAI's GPT models
- 📊 **Document Management**: Track and manage all uploaded documents
- 🎯 **Chunk-based Retrieval**: Efficient text chunking with configurable overlap for better context preservation

## Architecture

The system consists of the following key components:

- **OpenAIEmbeddingService**: Generates vector embeddings using OpenAI's `text-embedding-3-small` model
- **QdrantService**: Manages vector storage and similarity search using Qdrant
- **PdfService**: Handles PDF text extraction and intelligent text chunking
- **SqlChunkLoaderService**: Loads data from SQL Server databases
- **RagQueryService**: Orchestrates the RAG pipeline for query answering
- **RagController**: MVC controller managing the web interface and form submissions

## Prerequisites

- .NET 6.0 or higher
- Qdrant vector database (running locally or remote)
- OpenAI API key
- SQL Server (for database integration feature)

## Dependencies

```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.1" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.9" />
<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="OpenAI" Version="2.5.0" />
<PackageReference Include="PdfPig" Version="0.1.11" />
<PackageReference Include="Qdrant.Client" Version="1.15.1" />
```

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
 "Qdrant": {
  "Url": "http://localhost:6334",
  "ApiKey": ""
},
"OpenAI": {
  "ApiKey": "provide your api key here",
  "Model": "gpt-4o-mini"
},
"Rag": {
  "ChunkSizeWords": 300,
  "ChunkOverlapWords": 50,
  "TopK": 5
},
"ConnectionStrings": {
  "DefaultConnection": "Data Source=;Initial Catalog=;Persist Security Info=True;User ID=;Password=;"
}
}
```

## Installation

1. Clone the repository:
```bash
git clone <[your-repository-url](https://github.com/raskin-soft/RAG-Search-dotNet.git)>
cd <repository-name>
```

2. Install dependencies:
```bash
dotnet restore
```

3. Set up Qdrant (using Docker):
```bash
docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

4. Update `appsettings.json` with your configuration values

5. Run the application:
```bash
dotnet run
```

## Usage

### Accessing the Application

Navigate to `/rag/index` in your browser to access the RAG Search Portal.

### Features Overview

The application provides a user-friendly web interface with the following capabilities:

#### 1. Upload PDF Documents

- Click on the "Upload PDF" card
- Select a PDF file from your computer
- Click "Upload & Index" to process the document

The system will:
- Extract text from the PDF
- Split text into configurable chunks (default: 300 words with 50-word overlap)
- Generate embeddings for each chunk
- Store vectors in Qdrant with metadata
- Display a success message with the number of chunks indexed

#### 2. Load Database Content

- Click "Load from Database" in the "Index SQL Data" card
- The system automatically queries the `AreaOfExpertises` table
- Processes and indexes the data into Qdrant for semantic search

#### 3. Semantic Search

Ask questions about your indexed content in the search box:
```
Example: "What are the main topics covered in the documents?"
```

The system:
1. Generates an embedding for your question
2. Performs similarity search in Qdrant (retrieves top 5 most relevant chunks)
3. Constructs a context-aware prompt
4. Uses OpenAI's GPT model to generate a comprehensive answer
5. Displays the answer with source document references

#### 4. View Indexed Sources

The left sidebar displays all documents that have been uploaded and indexed in the system, providing visibility into your knowledge base.

## How It Works

### 1. Document Processing Pipeline

```
PDF Upload → Text Extraction → Chunking → Embedding Generation → Vector Storage (Qdrant)
```

### 2. Query Pipeline

```
User Question → Query Embedding → Similarity Search → Context Building → LLM Generation → Answer
```

### 3. Text Chunking Strategy

- Configurable chunk size (default: 300 words)
- Overlapping chunks (default: 50 words) to maintain context continuity
- Preserves semantic meaning across chunk boundaries

## Application Interface

The RAG Search Portal provides an intuitive web interface featuring:

- **📚 Indexed Sources Sidebar**: Real-time view of all indexed documents and data sources
- **📄 PDF Upload Card**: Drag-and-drop or browse to upload PDF documents
- **🗃️ SQL Data Indexing Card**: One-click button to index database content
- **🔎 Semantic Search Box**: Natural language query interface with instant results
- **Source Attribution**: Displays which documents contributed to each answer

### Screenshot Features

- Clean, Bootstrap 5-based responsive design
- Real-time feedback messages for all operations
- Color-coded sections for easy navigation
- Mobile-friendly interface

## Vector Collection Schema

Each point in Qdrant contains:
- **Vector**: 1536-dimensional embedding (OpenAI text-embedding-3-small)
- **Payload**:
  - `documentId`: Unique document identifier
  - `chunkIndex`: Position of chunk within document
  - `preview`: Text preview (first 200 characters)
  - `fileName`: Source document name

## Customization

### Adjust Chunk Size

Modify in `appsettings.json`:
```json
"Rag": {
  "ChunkSizeWords": "500",
  "ChunkOverlapWords": "100"
}
```

### Change Embedding Model

Update in `OpenAIEmbeddingService`:
```csharp
_client = new EmbeddingClient("text-embedding-3-large", apiKey);
```

Remember to update vector dimensions in `QdrantService`:
```csharp
Size = 3072 // for text-embedding-3-large
```

### Modify LLM Model

Change in `appsettings.json`:
```json
"OpenAI": {
  "Model": "gpt-4o"
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[Specify your license here]

## Acknowledgments

- OpenAI for embeddings and language models
- Qdrant for vector database
- PdfPig for PDF processing
