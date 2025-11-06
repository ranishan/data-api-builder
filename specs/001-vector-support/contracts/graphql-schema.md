# GraphQL Schema: Vector Support

**Feature**: SQL Server VECTOR type support  
**Date**: 2025-11-05  
**Purpose**: Define GraphQL schema for vector column operations

---

## Type Definitions

### Product Type (Example with Vector Column)

```graphql
"""
Product with ML embedding for similarity search
"""
type Product {
  """Primary key"""
  id: Int!
  
  """Product name"""
  name: String!
  
  """Product description"""
  description: String
  
  """Price in USD"""
  price: Float
  
  """
  Product embedding vector (1536 dimensions from OpenAI text-embedding-ada-002)
  NULL if embedding not yet generated
  """
  embedding: [Float!]
  
  """Timestamp of last update"""
  updatedAt: DateTime!
}
```

### Document Type (Example with Multiple Vectors)

```graphql
"""
Document with multiple embedding representations
"""
type Document {
  """Primary key"""
  id: Int!
  
  """Document title"""
  title: String!
  
  """Document content"""
  content: String!
  
  """
  Title embedding (384 dimensions from sentence-transformers)
  """
  titleEmbedding: [Float!]
  
  """
  Content embedding (768 dimensions from BERT)
  """
  contentEmbedding: [Float!]
  
  """Creation timestamp"""
  createdAt: DateTime!
}
```

---

## Query Operations

### Single Item Query

```graphql
query GetProduct {
  product_by_pk(id: 1) {
    id
    name
    description
    price
    embedding
    updatedAt
  }
}
```

**Response**:
```json
{
  "data": {
    "product_by_pk": {
      "id": 1,
      "name": "Wireless Headphones",
      "description": "Premium noise-cancelling headphones",
      "price": 299.99,
      "embedding": [0.023, -0.145, 0.892, ...],  // 1536 floats
      "updatedAt": "2025-11-05T10:30:00Z"
    }
  }
}
```

### List Query with Vector Selection

```graphql
query ListProducts {
  products(first: 10) {
    items {
      id
      name
      embedding
    }
  }
}
```

**Response**:
```json
{
  "data": {
    "products": {
      "items": [
        {
          "id": 1,
          "name": "Wireless Headphones",
          "embedding": [0.023, -0.145, 0.892, ...]
        },
        {
          "id": 2,
          "name": "Smart Watch",
          "embedding": [0.156, 0.089, -0.234, ...]
        }
      ]
    }
  }
}
```

### NULL Vector Handling

```graphql
query GetDocumentWithNullEmbedding {
  document_by_pk(id: 5) {
    id
    title
    contentEmbedding  # This document hasn't been embedded yet
  }
}
```

**Response**:
```json
{
  "data": {
    "document_by_pk": {
      "id": 5,
      "title": "New Document",
      "contentEmbedding": null
    }
  }
}
```

### Partial Field Selection (Excluding Vectors)

```graphql
query ListProductsWithoutEmbeddings {
  products {
    items {
      id
      name
      price
      # embedding field is omitted
    }
  }
}
```

**Response** (no embedding data transferred):
```json
{
  "data": {
    "products": {
      "items": [
        {
          "id": 1,
          "name": "Wireless Headphones",
          "price": 299.99
        }
      ]
    }
  }
}
```

---

## Mutation Operations

### Input Types

```graphql
"""
Input for creating a product with embedding
"""
input CreateProductInput {
  name: String!
  description: String
  price: Float
  embedding: [Float!]  # Must match dimension from schema (1536)
}

"""
Input for updating a product
"""
input UpdateProductInput {
  name: String
  description: String
  price: Float
  embedding: [Float!]  # Optional: update embedding
}
```

### Create Mutation

```graphql
mutation CreateProduct {
  createProduct(item: {
    name: "Smart Speaker"
    description: "Voice-activated smart speaker"
    price: 129.99
    embedding: [0.045, 0.123, -0.234, ...]  # 1536 floats
  }) {
    id
    name
    embedding
  }
}
```

**Response**:
```json
{
  "data": {
    "createProduct": {
      "id": 3,
      "name": "Smart Speaker",
      "embedding": [0.045, 0.123, -0.234, ...]
    }
  }
}
```

### Update Mutation

```graphql
mutation UpdateProductEmbedding {
  updateProduct(
    id: 1
    item: {
      embedding: [0.789, -0.456, 0.123, ...]  # New embedding
    }
  ) {
    id
    name
    embedding
    updatedAt
  }
}
```

**Response**:
```json
{
  "data": {
    "updateProduct": {
      "id": 1,
      "name": "Wireless Headphones",
      "embedding": [0.789, -0.456, 0.123, ...],
      "updatedAt": "2025-11-05T11:00:00Z"
    }
  }
}
```

### Create with NULL Embedding

```graphql
mutation CreateProductWithoutEmbedding {
  createProduct(item: {
    name: "USB Cable"
    price: 9.99
    embedding: null  # Explicitly NULL
  }) {
    id
    name
    embedding
  }
}
```

**Response**:
```json
{
  "data": {
    "createProduct": {
      "id": 4,
      "name": "USB Cable",
      "embedding": null
    }
  }
}
```

---

## Error Responses

### Dimension Mismatch Error

**Mutation**:
```graphql
mutation CreateWithWrongDimension {
  createProduct(item: {
    name: "Test Product"
    embedding: [0.1, 0.2, 0.3]  # Only 3 dimensions, expected 1536
  }) {
    id
  }
}
```

**Error Response**:
```json
{
  "errors": [
    {
      "message": "Vector column 'embedding' expects 1536 dimensions but received 3",
      "extensions": {
        "code": "BAD_REQUEST",
        "columnName": "embedding",
        "expectedDimensions": 1536,
        "receivedDimensions": 3
      }
    }
  ],
  "data": null
}
```

### Invalid Type Error

**Mutation**:
```graphql
mutation CreateWithInvalidType {
  createProduct(item: {
    name: "Test Product"
    embedding: "not-an-array"  # String instead of array
  }) {
    id
  }
}
```

**Error Response**:
```json
{
  "errors": [
    {
      "message": "Vector column 'embedding' requires an array of floats",
      "extensions": {
        "code": "BAD_REQUEST",
        "columnName": "embedding"
      }
    }
  ],
  "data": null
}
```

### Non-Float Element Error

**Mutation**:
```graphql
mutation CreateWithInvalidElement {
  createProduct(item: {
    name: "Test Product"
    embedding: [0.1, "invalid", 0.3, ...]  # String element
  }) {
    id
  }
}
```

**Error Response**:
```json
{
  "errors": [
    {
      "message": "Vector element 'invalid' cannot be converted to float",
      "extensions": {
        "code": "BAD_REQUEST",
        "columnName": "embedding"
      }
    }
  ],
  "data": null
}
```

---

## Configuration-Based Schema Variations

### With `omit-vector-columns: true`

**Schema** (vectors excluded):
```graphql
type Product {
  id: Int!
  name: String!
  description: String
  price: Float
  updatedAt: DateTime!
  # embedding field NOT present in schema
}
```

**Query** (attempting to access omitted field):
```graphql
query {
  product_by_pk(id: 1) {
    id
    name
    embedding  # This field doesn't exist in schema
  }
}
```

**Error Response**:
```json
{
  "errors": [
    {
      "message": "Cannot query field 'embedding' on type 'Product'",
      "extensions": {
        "code": "GRAPHQL_VALIDATION_FAILED"
      }
    }
  ]
}
```

### With `omit-vector-columns: false` (Default)

**Schema** (vectors included):
```graphql
type Product {
  id: Int!
  name: String!
  description: String
  price: Float
  embedding: [Float!]  # Field is present
  updatedAt: DateTime!
}
```

---

## Pagination with Vectors

### Forward Pagination

```graphql
query PaginatedProducts {
  products(first: 10, after: "cursor123") {
    items {
      id
      name
      embedding
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```

**Note**: Vector columns do NOT affect cursor-based pagination logic.

---

## GraphQL Introspection

### Field Introspection

```graphql
query IntrospectProductType {
  __type(name: "Product") {
    name
    fields {
      name
      type {
        name
        kind
        ofType {
          name
          kind
        }
      }
    }
  }
}
```

**Response** (showing embedding field):
```json
{
  "data": {
    "__type": {
      "name": "Product",
      "fields": [
        ...,
        {
          "name": "embedding",
          "type": {
            "name": null,
            "kind": "LIST",
            "ofType": {
              "name": "Float",
              "kind": "NON_NULL"
            }
          }
        }
      ]
    }
  }
}
```

---

## Performance Considerations

### Large Vector Queries

For VECTOR(1536) columns:
- Each float: 4 bytes
- Per record: 1536 × 4 = 6,144 bytes
- 100 records: ~614 KB

**Recommendation**: Use field selection to fetch vectors only when needed.

**Good**:
```graphql
query OptimizedList {
  products(first: 100) {
    items {
      id
      name
      # Don't fetch embedding unless needed
    }
  }
}
```

**Avoid** (if embeddings not needed):
```graphql
query UnoptimizedList {
  products(first: 100) {
    items {
      id
      name
      embedding  # Fetches 614 KB unnecessarily
    }
  }
}
```

---

## Summary

### Supported Operations

| Operation | Supported | Notes |
|-----------|-----------|-------|
| **Query single record** | ✅ | Returns float array |
| **Query multiple records** | ✅ | Pagination supported |
| **Create with vector** | ✅ | Dimension validation |
| **Update vector** | ✅ | Full vector replacement only |
| **Set vector to NULL** | ✅ | If column allows NULL |
| **Filter by vector** | ❌ | Not supported in initial implementation |
| **Order by vector** | ❌ | Not supported in initial implementation |
| **Partial vector updates** | ❌ | Must update all dimensions |

### Type System

- **GraphQL Type**: `[Float!]` (non-null list of non-null floats)
- **Nullable Vectors**: `[Float!]` (the list itself can be null)
- **Validation**: Runtime dimension checking (not schema-enforced)
- **Serialization**: JSON array of numbers

### Error Codes

| Error | HTTP Status | GraphQL Code | Trigger |
|-------|-------------|--------------|---------|
| Dimension mismatch | 400 | BAD_REQUEST | Array length ≠ expected |
| Invalid type | 400 | BAD_REQUEST | Non-array input |
| Element conversion | 400 | BAD_REQUEST | Non-numeric element |
| Field not in schema | N/A | GRAPHQL_VALIDATION_FAILED | Query omitted field |
