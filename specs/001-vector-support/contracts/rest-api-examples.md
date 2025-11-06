# REST API Examples: Vector Support

**Feature**: SQL Server VECTOR type support  
**Date**: 2025-11-05  
**Purpose**: Define REST API contracts for vector column operations

---

## Base Configuration

**Base URL**: `https://localhost:5001/api`  
**Content-Type**: `application/json`  
**Authentication**: Bearer token (if configured)

---

## GET Operations

### Get Single Record with Vector

**Request**:
```http
GET /api/Product/id/1
Host: localhost:5001
Accept: application/json
```

**Response** (200 OK):
```json
{
  "value": {
    "id": 1,
    "name": "Wireless Headphones",
    "description": "Premium noise-cancelling headphones",
    "price": 299.99,
    "embedding": [
      0.023, -0.145, 0.892, 0.034, -0.567, ...
    ],
    "updatedAt": "2025-11-05T10:30:00Z"
  }
}
```

**Vector Format**:
- JSON array of numbers
- 1536 elements for OpenAI text-embedding-ada-002
- Float precision (up to 7 decimal places typical)

### Get Multiple Records

**Request**:
```http
GET /api/Product
Host: localhost:5001
Accept: application/json
```

**Response** (200 OK):
```json
{
  "value": [
    {
      "id": 1,
      "name": "Wireless Headphones",
      "embedding": [0.023, -0.145, ...]
    },
    {
      "id": 2,
      "name": "Smart Watch",
      "embedding": [0.156, 0.089, ...]
    }
  ]
}
```

### Field Selection with $select

**Request** (exclude embedding):
```http
GET /api/Product?$select=id,name,price
Host: localhost:5001
Accept: application/json
```

**Response** (200 OK - no embedding field):
```json
{
  "value": [
    {
      "id": 1,
      "name": "Wireless Headphones",
      "price": 299.99
    },
    {
      "id": 2,
      "name": "Smart Watch",
      "price": 399.99
    }
  ]
}
```

**Performance Note**: Use `$select` to exclude large vector columns when not needed (saves ~6 KB per VECTOR(1536) record).

### Field Selection with $select (include embedding)

**Request**:
```http
GET /api/Product?$select=id,embedding
Host: localhost:5001
Accept: application/json
```

**Response** (200 OK):
```json
{
  "value": [
    {
      "id": 1,
      "embedding": [0.023, -0.145, 0.892, ...]
    },
    {
      "id": 2,
      "embedding": [0.156, 0.089, -0.234, ...]
    }
  ]
}
```

### NULL Vector Handling

**Request**:
```http
GET /api/Document/id/5
Host: localhost:5001
Accept: application/json
```

**Response** (200 OK - embedding not yet generated):
```json
{
  "value": {
    "id": 5,
    "title": "New Document",
    "content": "Document content here",
    "contentEmbedding": null,
    "createdAt": "2025-11-05T09:00:00Z"
  }
}
```

### Pagination with $top and $skip

**Request**:
```http
GET /api/Product?$top=10&$skip=20&$select=id,name,embedding
Host: localhost:5001
Accept: application/json
```

**Response** (200 OK):
```json
{
  "value": [
    {
      "id": 21,
      "name": "Product 21",
      "embedding": [...]
    }
  ],
  "@odata.nextLink": "/api/Product?$top=10&$skip=30&$select=id,name,embedding"
}
```

---

## POST Operations (Create)

### Create Record with Vector

**Request**:
```http
POST /api/Product
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "name": "Smart Speaker",
  "description": "Voice-activated smart speaker",
  "price": 129.99,
  "embedding": [
    0.045, 0.123, -0.234, 0.567, -0.789, 0.012, ...
  ]
}
```

**Response** (201 Created):
```json
{
  "value": {
    "id": 3,
    "name": "Smart Speaker",
    "description": "Voice-activated smart speaker",
    "price": 129.99,
    "embedding": [0.045, 0.123, -0.234, ...],
    "updatedAt": "2025-11-05T11:00:00Z"
  }
}
```

**Response Headers**:
```http
HTTP/1.1 201 Created
Location: /api/Product/id/3
Content-Type: application/json
```

### Create with NULL Embedding

**Request**:
```http
POST /api/Product
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "name": "USB Cable",
  "price": 9.99,
  "embedding": null
}
```

**Response** (201 Created):
```json
{
  "value": {
    "id": 4,
    "name": "USB Cable",
    "price": 9.99,
    "embedding": null,
    "updatedAt": "2025-11-05T11:05:00Z"
  }
}
```

### Create without Embedding Field (Uses Default)

**Request**:
```http
POST /api/Product
Host: localhost:5001
Content-Type: application/json
```

**Body** (embedding field omitted):
```json
{
  "name": "HDMI Cable",
  "price": 14.99
}
```

**Response** (201 Created):
```json
{
  "value": {
    "id": 5,
    "name": "HDMI Cable",
    "price": 14.99,
    "embedding": null,
    "updatedAt": "2025-11-05T11:10:00Z"
  }
}
```

---

## PUT Operations (Replace)

### Replace Record (including Vector)

**Request**:
```http
PUT /api/Product/id/1
Host: localhost:5001
Content-Type: application/json
```

**Body** (full replacement):
```json
{
  "name": "Wireless Headphones Pro",
  "description": "Premium noise-cancelling headphones with updated features",
  "price": 349.99,
  "embedding": [
    0.789, -0.456, 0.123, 0.234, -0.890, ...
  ]
}
```

**Response** (200 OK):
```json
{
  "value": {
    "id": 1,
    "name": "Wireless Headphones Pro",
    "description": "Premium noise-cancelling headphones with updated features",
    "price": 349.99,
    "embedding": [0.789, -0.456, 0.123, ...],
    "updatedAt": "2025-11-05T11:30:00Z"
  }
}
```

**Note**: PUT requires all fields. Omitted fields may be set to NULL or default values.

---

## PATCH Operations (Partial Update)

### Update Only Embedding

**Request**:
```http
PATCH /api/Product/id/1
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "embedding": [
    0.111, 0.222, 0.333, 0.444, 0.555, ...
  ]
}
```

**Response** (200 OK):
```json
{
  "value": {
    "id": 1,
    "name": "Wireless Headphones",
    "description": "Premium noise-cancelling headphones",
    "price": 299.99,
    "embedding": [0.111, 0.222, 0.333, ...],
    "updatedAt": "2025-11-05T12:00:00Z"
  }
}
```

### Update Name and Embedding

**Request**:
```http
PATCH /api/Product/id/2
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "name": "Smart Watch v2",
  "embedding": [0.666, 0.777, 0.888, ...]
}
```

**Response** (200 OK):
```json
{
  "value": {
    "id": 2,
    "name": "Smart Watch v2",
    "description": "Fitness tracking smartwatch",
    "price": 399.99,
    "embedding": [0.666, 0.777, 0.888, ...],
    "updatedAt": "2025-11-05T12:05:00Z"
  }
}
```

### Set Embedding to NULL

**Request**:
```http
PATCH /api/Product/id/3
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "embedding": null
}
```

**Response** (200 OK):
```json
{
  "value": {
    "id": 3,
    "name": "Smart Speaker",
    "description": "Voice-activated smart speaker",
    "price": 129.99,
    "embedding": null,
    "updatedAt": "2025-11-05T12:10:00Z"
  }
}
```

---

## DELETE Operations

### Delete Record (with Vector)

**Request**:
```http
DELETE /api/Product/id/1
Host: localhost:5001
```

**Response** (204 No Content):
```http
HTTP/1.1 204 No Content
```

**Note**: Vector columns do not affect DELETE operations.

---

## Error Responses

### 400 Bad Request - Dimension Mismatch

**Request**:
```http
POST /api/Product
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "name": "Test Product",
  "embedding": [0.1, 0.2, 0.3]
}
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Vector column 'embedding' expects 1536 dimensions but received 3"
}
```

### 400 Bad Request - Invalid Type

**Request**:
```http
POST /api/Product
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "name": "Test Product",
  "embedding": "not-an-array"
}
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Vector column 'embedding' requires an array of floats"
}
```

### 400 Bad Request - Non-Float Element

**Request**:
```http
POST /api/Product
Host: localhost:5001
Content-Type: application/json
```

**Body**:
```json
{
  "name": "Test Product",
  "embedding": [0.1, "invalid", 0.3, ...]
}
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Vector element 'invalid' cannot be converted to float"
}
```

### 404 Not Found - Vector Field Not in Schema

**Request** (when `omit-vector-columns: true`):
```http
GET /api/Product?$select=id,embedding
Host: localhost:5001
```

**Response** (400 Bad Request - field selection error):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Field 'embedding' is not available for entity 'Product'"
}
```

---

## OData Query Options with Vectors

### Supported Operations

| OData Option | Supported with Vectors | Notes |
|--------------|------------------------|-------|
| `$select` | ✅ Yes | Can include/exclude vector fields |
| `$top` | ✅ Yes | Pagination works normally |
| `$skip` | ✅ Yes | Pagination works normally |
| `$orderby` | ❌ No | Cannot order by vector columns |
| `$filter` | ❌ No | Cannot filter by vector columns |
| `$expand` | ✅ Yes | Related entities can have vectors |

### $select with Vectors

**Include vector**:
```http
GET /api/Product?$select=id,name,embedding
```

**Exclude vector**:
```http
GET /api/Product?$select=id,name,price
```

### Attempting Unsupported Operations

**Request** (filter by vector - NOT SUPPORTED):
```http
GET /api/Product?$filter=embedding eq [0.1, 0.2]
Host: localhost:5001
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Filtering by vector column 'embedding' is not supported"
}
```

**Request** (order by vector - NOT SUPPORTED):
```http
GET /api/Product?$orderby=embedding
Host: localhost:5001
```

**Response** (400 Bad Request):
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Ordering by vector column 'embedding' is not supported"
}
```

---

## Configuration-Based Behavior

### With `omit-vector-columns: true`

**Request**:
```http
GET /api/Product/id/1
Host: localhost:5001
```

**Response** (200 OK - embedding excluded automatically):
```json
{
  "value": {
    "id": 1,
    "name": "Wireless Headphones",
    "description": "Premium noise-cancelling headphones",
    "price": 299.99,
    "updatedAt": "2025-11-05T10:30:00Z"
  }
}
```

**Note**: `embedding` field is NOT present in response, even if it exists in database.

### With `omit-vector-columns: false` (Default)

**Request**:
```http
GET /api/Product/id/1
Host: localhost:5001
```

**Response** (200 OK - embedding included):
```json
{
  "value": {
    "id": 1,
    "name": "Wireless Headphones",
    "description": "Premium noise-cancelling headphones",
    "price": 299.99,
    "embedding": [0.023, -0.145, ...],
    "updatedAt": "2025-11-05T10:30:00Z"
  }
}
```

---

## Performance Considerations

### Response Size

For VECTOR(1536) columns:
- **Per record**: ~6 KB (1536 floats × 4 bytes/float, plus JSON overhead)
- **100 records with vectors**: ~600 KB
- **100 records without vectors**: ~20 KB (typical non-vector data)

**Recommendation**: Use `$select` to exclude vectors when not needed, especially for list operations.

### Optimal Queries

**Good** (fetch vectors for detail view):
```http
GET /api/Product/id/1
```

**Good** (exclude vectors for list view):
```http
GET /api/Product?$select=id,name,price
```

**Avoid** (fetching vectors for all records in list):
```http
GET /api/Product?$top=100
```

---

## Batch Operations

DAB supports batch operations via POST to `/$batch`. Vectors work in batch requests:

**Request**:
```http
POST /api/$batch
Host: localhost:5001
Content-Type: multipart/mixed; boundary=batch_boundary
```

**Body**:
```http
--batch_boundary
Content-Type: application/http

POST /api/Product HTTP/1.1
Content-Type: application/json

{
  "name": "Product 1",
  "embedding": [0.1, 0.2, ...]
}
--batch_boundary
Content-Type: application/http

POST /api/Product HTTP/1.1
Content-Type: application/json

{
  "name": "Product 2",
  "embedding": [0.3, 0.4, ...]
}
--batch_boundary--
```

---

## Summary

### Supported Operations

| HTTP Method | Operation | Vector Support | Notes |
|-------------|-----------|----------------|-------|
| GET | Read single | ✅ Full | Returns float array |
| GET | Read list | ✅ Full | Use $select to optimize |
| POST | Create | ✅ Full | Dimension validation |
| PUT | Replace | ✅ Full | Full vector replacement |
| PATCH | Partial update | ✅ Full | Can update vector only |
| DELETE | Delete | ✅ Full | No special handling |

### Query Options

| OData Option | Supported | Notes |
|--------------|-----------|-------|
| $select | ✅ | Include/exclude vectors |
| $top, $skip | ✅ | Pagination |
| $filter | ❌ | Cannot filter by vectors |
| $orderby | ❌ | Cannot order by vectors |
| $expand | ✅ | Related entities with vectors |

### Error Codes

| Scenario | Status Code | Message Template |
|----------|-------------|------------------|
| Dimension mismatch | 400 | "Vector column '{name}' expects {expected} dimensions but received {actual}" |
| Invalid type | 400 | "Vector column '{name}' requires an array of floats" |
| Element conversion error | 400 | "Vector element '{value}' cannot be converted to float" |
| Unsupported filter | 400 | "Filtering by vector column '{name}' is not supported" |
| Unsupported orderby | 400 | "Ordering by vector column '{name}' is not supported" |
