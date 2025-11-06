# Quickstart: SQL Server Vector Support

**Feature**: SQL Server VECTOR type support in Data API Builder  
**Version**: 1.0  
**Target**: Developers integrating vector embeddings with DAB

---

## Prerequisites

1. **SQL Server 2025** (Preview or later with VECTOR support)
2. **Data API Builder** v0.14.0+ (with vector support)
3. **OpenAI API key** (optional, for generating embeddings)

---

## Overview

SQL Server 2025 introduces the `VECTOR` data type for storing embeddings. DAB now supports:
- ✅ **Reading vector data** via GraphQL and REST
- ✅ **Writing vector data** (INSERT, UPDATE) with validation
- ✅ **Omitting vectors** from API surface when not needed

**Time to first query**: ~5 minutes 🚀

---

## Quick Start

### Step 1: Create Table with Vector Column

```sql
-- Create table with embedding column (1536 dimensions for OpenAI ada-002)
CREATE TABLE Product (
    ProductId INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10,2),
    Embedding VECTOR(1536) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Insert sample data
INSERT INTO Product (Name, Description, Price, Embedding)
VALUES 
    ('Wireless Headphones', 'Premium noise-cancelling headphones', 299.99, NULL),
    ('Smart Watch', 'Fitness tracking smartwatch', 399.99, NULL);
```

### Step 2: Configure DAB (Default - Vectors Included)

```json
{
  "$schema": "https://github.com/Azure/data-api-builder/releases/download/v1.2.10/dab.draft.schema.json",
  "data-source": {
    "database-type": "mssql",
    "connection-string": "@env('SQL_CONNECTION_STRING')"
  },
  "entities": {
    "Product": {
      "source": "dbo.Product",
      "permissions": [
        {
          "actions": ["create", "read", "update", "delete"],
          "role": "anonymous"
        }
      ]
    }
  }
}
```

### Step 3: Query via GraphQL

**Query** (fetch product with embedding):
```graphql
query GetProduct {
  product_by_pk(ProductId: 1) {
    ProductId
    Name
    Description
    Price
    Embedding
  }
}
```

**Response** (NULL embedding):
```json
{
  "data": {
    "product_by_pk": {
      "ProductId": 1,
      "Name": "Wireless Headphones",
      "Description": "Premium noise-cancelling headphones",
      "Price": 299.99,
      "Embedding": null
    }
  }
}
```

### Step 4: Insert Vector Data

**Insert vector** (GraphQL mutation):
```graphql
mutation CreateProductWithEmbedding {
  createProduct(
    item: {
      Name: "USB Cable"
      Price: 9.99
      Embedding: [
        0.023, -0.145, 0.892, 0.034, -0.567, 0.213, 
        # ... 1530 more floats ...
        0.445, -0.789, 0.123
      ]
    }
  ) {
    ProductId
    Name
    Embedding
  }
}
```

**Response**:
```json
{
  "data": {
    "createProduct": {
      "ProductId": 3,
      "Name": "USB Cable",
      "Embedding": [0.023, -0.145, 0.892, ...]
    }
  }
}
```

---

## Configuration Options

### Option 1: Include Vectors (Default)

**Use case**: AI/ML applications requiring vector similarity search

```json
{
  "entities": {
    "Product": {
      "source": "dbo.Product",
      "permissions": [...]
      // omit-vector-columns: false (default)
    }
  }
}
```

**GraphQL Schema** (vectors exposed):
```graphql
type Product {
  ProductId: Int!
  Name: String!
  Description: String
  Price: Decimal
  Embedding: [Float!]  # <-- VECTOR(1536) exposed
  CreatedAt: DateTime!
}
```

**REST Response**:
```json
{
  "value": {
    "ProductId": 1,
    "Name": "Wireless Headphones",
    "Embedding": [0.1, 0.2, ...],  // <-- 6 KB for VECTOR(1536)
    "Price": 299.99
  }
}
```

### Option 2: Omit Vectors from API

**Use case**: Traditional CRUD where embeddings are managed separately

```json
{
  "entities": {
    "Product": {
      "source": "dbo.Product",
      "omit-vector-columns": true,  // <-- NEW PROPERTY
      "permissions": [...]
    }
  }
}
```

**GraphQL Schema** (vectors hidden):
```graphql
type Product {
  ProductId: Int!
  Name: String!
  Description: String
  Price: Decimal
  CreatedAt: DateTime!
  # Embedding field NOT present
}
```

**REST Response**:
```json
{
  "value": {
    "ProductId": 1,
    "Name": "Wireless Headphones",
    "Price": 299.99
    // Embedding field NOT present
  }
}
```

**Benefits**:
- ✅ Unblocks CRUD on tables with existing vector columns
- ✅ Reduces response payload size (~6 KB per VECTOR(1536))
- ✅ Prevents accidental exposure of embeddings

---

## Common Patterns

### Pattern 1: Generate Embeddings with OpenAI

**C# example** (using Azure.AI.OpenAI):
```csharp
using Azure.AI.OpenAI;

var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

// Generate embedding for product description
var description = "Premium noise-cancelling headphones";
var embeddingResponse = await client.GetEmbeddingsAsync(
    new EmbeddingsOptions("text-embedding-ada-002", new[] { description })
);

float[] embedding = embeddingResponse.Value.Data[0].Embedding.ToArray();
// embedding.Length == 1536 for ada-002

// Insert via DAB GraphQL mutation
var mutation = @"
mutation CreateProduct($name: String!, $embedding: [Float!]) {
  createProduct(item: { Name: $name, Embedding: $embedding }) {
    ProductId
  }
}";

// Send mutation with embedding as JSON array
```

### Pattern 2: Bulk Insert with Vectors

**SQL approach** (insert 1000 products):
```sql
-- Generate embeddings externally, then bulk insert
INSERT INTO Product (Name, Description, Embedding)
SELECT 
    p.Name,
    p.Description,
    CAST(p.EmbeddingJson AS VECTOR(1536))
FROM StagingProducts p;

-- Query via DAB REST API
GET /api/Product?$select=ProductId,Name,Embedding&$top=1000
```

**Performance**: ~6 MB for 1000 products with VECTOR(1536)

### Pattern 3: Update Embeddings After Insert

**GraphQL mutation**:
```graphql
mutation UpdateEmbedding($id: Int!, $embedding: [Float!]!) {
  updateProduct(
    ProductId: $id
    item: { Embedding: $embedding }
  ) {
    ProductId
    Embedding
  }
}
```

**REST PATCH**:
```http
PATCH /api/Product/id/1
Content-Type: application/json

{
  "Embedding": [0.1, 0.2, 0.3, ...]
}
```

### Pattern 4: Field Selection for Performance

**GraphQL** (exclude embedding from list queries):
```graphql
query ListProducts {
  products {
    items {
      ProductId
      Name
      Price
      # Embedding field NOT requested
    }
  }
}
```

**REST** (use `$select`):
```http
GET /api/Product?$select=ProductId,Name,Price
```

**Performance gain**: ~97% smaller payload (20 KB vs 600 KB for 100 products)

---

## Error Handling

### Error 1: Dimension Mismatch

**Mutation** (wrong dimension):
```graphql
mutation {
  createProduct(item: {
    Name: "Test"
    Embedding: [0.1, 0.2, 0.3]  # Only 3 elements, expected 1536
  }) {
    ProductId
  }
}
```

**Error Response**:
```json
{
  "errors": [
    {
      "message": "Vector column 'Embedding' expects 1536 dimensions but received 3",
      "extensions": {
        "code": "VECTOR_DIMENSION_MISMATCH",
        "column": "Embedding",
        "expected": 1536,
        "actual": 3
      }
    }
  ]
}
```

**Fix**: Provide array with exactly 1536 elements.

### Error 2: Invalid Type

**Mutation** (string instead of array):
```graphql
mutation {
  createProduct(item: {
    Name: "Test"
    Embedding: "not-an-array"
  }) {
    ProductId
  }
}
```

**Error Response**:
```json
{
  "errors": [
    {
      "message": "Vector column 'Embedding' requires an array of floats",
      "extensions": {
        "code": "VECTOR_INVALID_TYPE"
      }
    }
  ]
}
```

**Fix**: Use JSON array: `[0.1, 0.2, ...]`

### Error 3: Non-Float Elements

**REST POST** (invalid element):
```json
{
  "Name": "Test",
  "Embedding": [0.1, "invalid", 0.3]
}
```

**Error Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Vector element 'invalid' cannot be converted to float"
}
```

**Fix**: All elements must be numbers.

---

## Advanced Scenarios

### Multi-Vector Columns

**Table** (multiple embeddings):
```sql
CREATE TABLE Document (
    DocumentId INT PRIMARY KEY,
    Title NVARCHAR(500),
    Content NVARCHAR(MAX),
    TitleEmbedding VECTOR(1536),    -- From title text
    ContentEmbedding VECTOR(3072),  -- Larger model
    MetadataEmbedding VECTOR(768)   -- Smaller model
);
```

**GraphQL Query**:
```graphql
query GetDocument {
  document_by_pk(DocumentId: 1) {
    DocumentId
    Title
    TitleEmbedding      # [Float!]
    ContentEmbedding    # [Float!]
    MetadataEmbedding   # [Float!]
  }
}
```

**Selective omission** (not supported - omit affects ALL vectors):
```json
{
  "entities": {
    "Document": {
      "source": "dbo.Document",
      "omit-vector-columns": true  // Omits ALL vector columns
    }
  }
}
```

### NULL Handling

**Insert NULL**:
```graphql
mutation {
  createProduct(item: {
    Name: "Product without embedding"
    Embedding: null  # <-- Explicitly NULL
  }) {
    ProductId
    Embedding
  }
}
```

**Response**:
```json
{
  "data": {
    "createProduct": {
      "ProductId": 4,
      "Embedding": null
    }
  }
}
```

**Update to NULL** (clear embedding):
```graphql
mutation {
  updateProduct(
    ProductId: 1
    item: { Embedding: null }
  ) {
    ProductId
    Embedding
  }
}
```

### Configuration Validation

**Invalid config**:
```json
{
  "entities": {
    "Product": {
      "source": "dbo.Product",
      "omit-vector-columns": "yes"  // ERROR: Must be boolean
    }
  }
}
```

**DAB Startup Error**:
```
Configuration error: 'omit-vector-columns' must be a boolean value (true/false)
```

---

## Testing Checklist

Use this checklist to verify vector support:

### Basic Operations
- [ ] **Read NULL vector**: `GET /api/Product/id/1` returns `"Embedding": null`
- [ ] **Read vector data**: Insert vector via SQL, query via GraphQL returns float array
- [ ] **Create with vector**: GraphQL mutation with 1536-element array succeeds
- [ ] **Update vector**: PATCH with new embedding array updates successfully
- [ ] **Create with NULL**: Mutation with `Embedding: null` succeeds

### Configuration
- [ ] **Default behavior**: Vector columns exposed in schema when `omit-vector-columns` not set
- [ ] **Omit vectors**: Schema excludes vector fields when `omit-vector-columns: true`
- [ ] **Validation**: Invalid `omit-vector-columns` value causes clear startup error

### Error Handling
- [ ] **Dimension mismatch**: Creating with wrong dimension returns error with expected/actual counts
- [ ] **Invalid type**: Passing string instead of array returns clear error message
- [ ] **Non-float element**: Array with non-numeric element returns conversion error

### Performance
- [ ] **Field selection**: `$select` without vector field returns smaller payload
- [ ] **Pagination**: Large result sets with vectors paginate correctly

---

## Troubleshooting

### Issue: "Type 'vector' is not supported"

**Symptom**: DAB fails to start with type error

**Cause**: Running SQL Server version without VECTOR support

**Fix**:
1. Upgrade to SQL Server 2025 Preview or later
2. Or configure `omit-vector-columns: true` to bypass vectors

### Issue: "Vector column not in schema"

**Symptom**: GraphQL query returns error for `Embedding` field

**Cause**: `omit-vector-columns: true` is configured

**Fix**: Remove or set to `false` in dab-config.json

### Issue: Response payload too large

**Symptom**: Slow queries, large network traffic

**Cause**: Fetching vectors for all records in list queries

**Fix**: Use `$select` to exclude embeddings:
```http
GET /api/Product?$select=ProductId,Name,Price
```

### Issue: Dimension validation failing

**Symptom**: Mutations rejected with dimension mismatch

**Cause**: Embedding array length doesn't match SQL column dimension

**Fix**: Ensure array length matches:
- VECTOR(1536) → 1536 elements
- VECTOR(3072) → 3072 elements

---

## Next Steps

1. **Vector Similarity Search**: SQL Server 2025 supports `VECTOR_DISTANCE()` for similarity queries (not yet exposed via DAB)
2. **Indexing**: Create vector indexes for performance optimization
3. **Batch Operations**: Use DAB's `/$batch` endpoint for bulk inserts

---

## Configuration Reference

### Entity-Level Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `omit-vector-columns` | boolean | `false` | Exclude all VECTOR columns from API surface |

### Example Configurations

**Full vector support**:
```json
{
  "entities": {
    "Product": {
      "source": "dbo.Product",
      "permissions": [...]
    }
  }
}
```

**Omit all vectors**:
```json
{
  "entities": {
    "Product": {
      "source": "dbo.Product",
      "omit-vector-columns": true,
      "permissions": [...]
    }
  }
}
```

---

## Summary

**Time to first query**: ~5 minutes  
**Configuration**: 1 line (optional)  
**Breaking changes**: None (feature is additive)

Vector support is production-ready and follows DAB's existing patterns for type handling, validation, and error reporting.
