# Data Model: SQL Server Vector Data Type Support

**Feature**: SQL Server VECTOR type support for Data API Builder  
**Date**: 2025-11-05  
**Purpose**: Define entities and their relationships for vector support implementation

---

## Entity Definitions

### 1. VECTOR Column Metadata

**Purpose**: Represents metadata about VECTOR columns retrieved during schema introspection

**Properties**:
| Property | Type | Description | Source |
|----------|------|-------------|--------|
| `ColumnName` | `string` | Name of the vector column | `sys.columns.name` |
| `VectorDimensions` | `int?` | Number of dimensions (e.g., 1536 for embeddings) | Parsed from type name via regex |
| `IsNullable` | `bool` | Whether the column allows NULL values | `sys.columns.is_nullable` |
| `SystemType` | `Type` | CLR type mapping (always `typeof(float[])` for vectors) | Computed in SqlToCLRType() |
| `SqlTypeName` | `string` | SQL Server type name (e.g., "vector(1536)") | `TYPE_NAME(system_type_id)` |

**Relationships**:
- **Belongs to**: `ColumnDefinition` (extends existing class)
- **Used by**: `MsSqlMetadataProvider` during schema introspection
- **Consumed by**: `GraphQLSchemaCreator`, REST API serializers

**Storage**: In-memory as part of `ColumnDefinition` objects in metadata cache

**Lifecycle**: Loaded during application startup or hot reload, cached for performance

---

### 2. Vector Data

**Purpose**: Represents actual vector embeddings as arrays of float values

**Properties**:
| Property | Type | Description | Format |
|----------|------|-------------|--------|
| `Values` | `float[]` | Array of floating-point values | IEEE 754 float32 |
| `Dimension` | `int` | Number of elements in the array | Derived from array length |
| `IsNull` | `bool` | Whether the vector is NULL | Checked during deserialization |

**Wire Formats**:

1. **SQL Server (Binary)**:
   ```
   byte[] (little-endian float32 values)
   Example: VECTOR(3) with [1.0, 2.0, 3.0]
   → [0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x40, 0x40]
   ```

2. **GraphQL (JSON Array)**:
   ```graphql
   {
     embedding: [1.0, 2.0, 3.0]
   }
   ```

3. **REST API (JSON Array)**:
   ```json
   {
     "embedding": [1.0, 2.0, 3.0]
   }
   ```

**Validation Rules**:
- All elements must be valid float32 values
- Array length must match declared dimension from schema
- NULL is valid if column allows NULL
- Empty array is invalid (dimension mismatch)
- Special float values (NaN, Infinity, -Infinity) are technically valid but may need documentation

**Relationships**:
- **Serialized by**: `SqlResponseHelpers.DeserializeVector()`
- **Deserialized by**: `SqlMutationEngine` serialization logic
- **Validated by**: `VectorValidationService`

---

### 3. Entity Configuration

**Purpose**: Configuration settings for entities with vector columns

**Properties**:
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `OmitVectorColumns` | `bool` | `false` | When true, all VECTOR columns are excluded from schema and responses |

**Configuration File Example**:
```json
{
  "entities": {
    "Product": {
      "source": {
        "object": "products",
        "type": "table"
      },
      "omit-vector-columns": true,
      "permissions": [
        {
          "role": "anonymous",
          "actions": ["create", "read", "update", "delete"]
        }
      ]
    },
    "Document": {
      "source": {
        "object": "documents",
        "type": "table"
      },
      "omit-vector-columns": false,
      "permissions": [
        {
          "role": "authenticated",
          "actions": ["read"]
        }
      ]
    }
  }
}
```

**Behavior**:
- **When `true`**: 
  - Vector columns excluded from GraphQL schema
  - Vector columns excluded from REST API responses
  - Queries on non-vector columns succeed
  - Mutations on non-vector columns succeed
  - Attempts to query/mutate vector columns result in validation errors

- **When `false`** (default):
  - Vector columns included in GraphQL schema (if full support implemented)
  - Vector columns included in REST API responses (if full support implemented)
  - Full CRUD operations on vector columns enabled

**Validation**:
- Must be a boolean value
- Validated during config file loading
- Invalid values result in configuration error with clear message

**Relationships**:
- **Part of**: `Entity` class in `Config/ObjectModel/Entity.cs`
- **Read by**: `GraphQLSchemaCreator`, REST serializers, `MsSqlMetadataProvider`
- **Configured via**: CLI `dab init` and `dab add` commands (future enhancement)

---

## Entity Relationships Diagram

```
┌──────────────────────────────────┐
│   Entity Configuration           │
│   (dab-config.json)              │
│                                  │
│   - OmitVectorColumns: bool      │
└─────────────┬────────────────────┘
              │ configures
              ▼
┌──────────────────────────────────┐
│   ColumnDefinition               │
│   (Metadata)                     │
│                                  │
│   - ColumnName: string           │
│   - VectorDimensions: int?  ◄────┼─── Extended for vectors
│   - IsNullable: bool             │
│   - SystemType: Type             │
└─────────────┬────────────────────┘
              │ describes
              ▼
┌──────────────────────────────────┐
│   Vector Data                    │
│   (Runtime)                      │
│                                  │
│   - Values: float[]              │
│   - Dimension: int               │
│   - IsNull: bool                 │
└──────────────────────────────────┘
```

---

## Data Flow

### 1. Schema Introspection (Read Metadata)

```
SQL Server           MsSqlMetadataProvider         ColumnDefinition
    │                        │                           │
    │  sys.types query       │                           │
    │◄───────────────────────│                           │
    │                        │                           │
    │  "vector(1536)"        │                           │
    │───────────────────────►│                           │
    │                        │  Extract dimension        │
    │                        │  (regex parse)            │
    │                        │                           │
    │                        │  Create/Update            │
    │                        │──────────────────────────►│
    │                        │  VectorDimensions = 1536  │
```

### 2. Query Execution (Read Data)

```
Client              DAB GraphQL/REST         SqlResponseHelpers        SQL Server
   │                      │                         │                      │
   │  Query embedding     │                         │                      │
   │─────────────────────►│                         │                      │
   │                      │  SELECT embedding       │                      │
   │                      │───────────────────────────────────────────────►│
   │                      │                         │                      │
   │                      │         byte[]          │                      │
   │                      │◄───────────────────────────────────────────────│
   │                      │                         │                      │
   │                      │  DeserializeVector()    │                      │
   │                      │────────────────────────►│                      │
   │                      │                         │                      │
   │                      │  float[]                │                      │
   │                      │◄────────────────────────│                      │
   │                      │                         │                      │
   │  [1.0, 2.0, ...]     │                         │                      │
   │◄─────────────────────│                         │                      │
```

### 3. Mutation Execution (Write Data)

```
Client          DAB GraphQL/REST      VectorValidationService    SqlMutationEngine      SQL Server
   │                  │                        │                        │                    │
   │  Create record   │                        │                        │                    │
   │  {embedding:[]}  │                        │                        │                    │
   │─────────────────►│                        │                        │                    │
   │                  │  Validate dimensions   │                        │                    │
   │                  │───────────────────────►│                        │                    │
   │                  │                        │  Dimension check       │                    │
   │                  │                        │  Type check            │                    │
   │                  │◄───────────────────────│                        │                    │
   │                  │  Serialize to byte[]   │                        │                    │
   │                  │──────────────────────────────────────────────►  │                    │
   │                  │                        │                        │  INSERT with       │
   │                  │                        │                        │  binary param      │
   │                  │                        │                        │───────────────────►│
   │                  │                        │                        │◄───────────────────│
   │                  │◄──────────────────────────────────────────────  │                    │
   │  Success         │                        │                        │                    │
   │◄─────────────────│                        │                        │                    │
```

---

## Type Mappings

### SQL Server → CLR → GraphQL

| SQL Server Type | CLR Type | GraphQL Type | JSON Type |
|-----------------|----------|--------------|-----------|
| `VECTOR(N)` | `float[]` | `[Float!]` | `number[]` |
| `VECTOR(N) NULL` | `float[]?` | `[Float!]` (nullable) | `number[] \| null` |

### Dimension Examples

| Dimension | Use Case | Common In |
|-----------|----------|-----------|
| 384 | Sentence embeddings (small models) | BERT-based models |
| 768 | Token embeddings | GPT-2, BERT-large |
| 1536 | OpenAI text-embedding-ada-002 | OpenAI API |
| 3072 | OpenAI text-embedding-3-large | OpenAI API |
| 4096 | Large language model embeddings | Custom models |

---

## Implementation Notes

### Performance Considerations

1. **Binary Serialization**:
   - `Buffer.BlockCopy` is most efficient for float[] ↔ byte[] conversion
   - Avoids per-element iteration and boxing

2. **Memory Allocation**:
   - For VECTOR(1536): 6,144 bytes per vector
   - Batch queries should consider memory for multiple vectors
   - Consider streaming for large result sets

3. **Validation Cost**:
   - Dimension checking is O(1) (array.Length comparison)
   - Type validation is O(N) where N = dimension
   - Consider validation caching for repeated mutations

### Security Considerations

1. **Input Validation**:
   - Validate dimension match BEFORE SQL execution (fail fast)
   - Prevent buffer overflow by validating array length
   - SQL injection prevention via parameterized queries (byte[] parameter)

2. **Error Messages**:
   - Do not expose internal structure in error messages
   - Provide actionable guidance ("expected 1536 dimensions, received 1024")
   - Log full details for debugging while sanitizing user-facing errors

---

## Extension Points

### Future Enhancements

1. **Custom Scalar Type** (if needed):
   ```graphql
   scalar Vector1536  # Dimension-specific type
   
   type Product {
     embedding: Vector1536
   }
   ```

2. **Multiple Database Support**:
   - PostgreSQL: `pgvector` extension uses different format
   - Azure Cosmos DB: May require different serialization
   - Abstract via `IVectorSerializer` interface

3. **Vector Operations**:
   - Similarity search: `VECTOR_DISTANCE(embedding, @query)`
   - Filtering by similarity threshold
   - K-nearest neighbors queries

---

## Summary

This data model defines three core entities:

1. **VECTOR Column Metadata**: Schema-level information (dimension, nullability)
2. **Vector Data**: Runtime representation (float arrays)
3. **Entity Configuration**: User-configurable omission settings

The model supports the two-phase implementation:
- **Phase 1 (P1)**: Entity Configuration enables omission (unblock tables)
- **Phase 2 (P2-P3)**: Full CRUD via Vector Data serialization/validation

All entities integrate with existing DAB architecture (ColumnDefinition, Entity, SqlMutationEngine) following the constitution's pattern consistency principle.
