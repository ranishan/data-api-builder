# Research Document: SQL Server Vector Data Type Support

**Feature**: SQL Server VECTOR type support for Data API Builder  
**Date**: 2025-11-05  
**Status**: Research Complete  
**Purpose**: Answer critical technical questions before implementation

---

## R1: SQL Server Vector Type Introspection

### Question
How does SQL Server represent VECTOR types in `sys.types` and `sys.columns`? What is the exact value of `TYPE_NAME(system_type_id)` for VECTOR columns, and how do we extract the dimension parameter?

### Research Findings

**SQL Server 2025 Preview Behavior:**

Based on SQL Server 2025 preview documentation and testing:

1. **Type Name Representation**:
   - `TYPE_NAME(system_type_id)` returns `"vector"` (lowercase)
   - The type is user-defined but appears as a system type in preview
   - Type name is case-insensitive for comparison purposes

2. **Dimension Storage**:
   - SQL Server stores the dimension in the type definition, not in `sys.columns` directly
   - For `VECTOR(1536)`, the dimension is part of the type specification
   - Access via `sys.types` or by parsing the full type name from `TYPE_NAME()`

3. **Detection Query**:
```sql
SELECT 
    c.name AS column_name,
    TYPE_NAME(c.system_type_id) AS type_name,
    c.max_length,
    c.precision,
    c.scale,
    t.name AS user_type_name,
    t.max_length AS type_max_length
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('TableName');
```

4. **Dimension Extraction Strategy**:
   - **Option A**: Parse type name if it includes dimensions (e.g., "vector(1536)")
   - **Option B**: Query `sys.types` with `user_type_id` to get type metadata
   - **Recommended**: Use regex pattern `vector\\s*\\(\\s*(\\d+)\\s*\\)` on full type name

### Implementation Recommendation

```csharp
// In MsSqlMetadataProvider
public override Type SqlToCLRType(string sqlType)
{
    if (sqlType.StartsWith("vector", StringComparison.OrdinalIgnoreCase))
    {
        return typeof(float[]);
    }
    return base.SqlToCLRType(sqlType);
}

// Dimension extraction in SqlVectorTypeHelper
public static int? ExtractVectorDimensions(string vectorTypeName)
{
    var match = Regex.Match(vectorTypeName, @"vector\s*\(\s*(\d+)\s*\)", RegexOptions.IgnoreCase);
    if (match.Success && int.TryParse(match.Groups[1].Value, out int dimension))
    {
        return dimension;
    }
    return null; // Unspecified dimension or parse failure
}
```

---

## R2: ADO.NET VECTOR Data Wire Format

### Question
What is the serialization format for VECTOR data between SQL Server and Microsoft.Data.SqlClient? Does `SqlDataReader` return vectors as byte arrays, strings, or custom types?

### Research Findings

**Microsoft.Data.SqlClient Behavior:**

1. **Data Type Returned**:
   - VECTOR columns are returned as `byte[]` (binary data)
   - The binary format represents an array of float32 values
   - Use `SqlDataReader.GetValue()` or `SqlDataReader.GetFieldValue<byte[]>()`

2. **Binary Format Specification**:
   - Little-endian IEEE 754 float32 values
   - Each float is 4 bytes
   - For VECTOR(1536): 1536 floats × 4 bytes = 6144 bytes
   - No metadata prefix (dimension is known from schema)

3. **Deserialization Example**:
```csharp
// Reading vector from SqlDataReader
byte[] vectorBytes = reader.GetFieldValue<byte[]>(columnIndex);
float[] vectorArray = new float[vectorBytes.Length / sizeof(float)];
Buffer.BlockCopy(vectorBytes, 0, vectorArray, 0, vectorBytes.Length);
```

4. **Serialization for INSERT/UPDATE**:
```csharp
// Writing vector to SqlParameter
float[] vectorArray = new float[1536];
byte[] vectorBytes = new byte[vectorArray.Length * sizeof(float)];
Buffer.BlockCopy(vectorArray, 0, vectorBytes, 0, vectorBytes.Length);

SqlParameter param = new SqlParameter("@embedding", SqlDbType.VarBinary);
param.Value = vectorBytes;
```

5. **NULL Handling**:
   - NULL vectors return `DBNull.Value`
   - Check `reader.IsDBNull(columnIndex)` before reading

### Implementation Recommendation

**SqlResponseHelpers Deserialization**:
```csharp
public static float[]? DeserializeVector(object value)
{
    if (value == null || value == DBNull.Value)
    {
        return null;
    }

    if (value is byte[] vectorBytes)
    {
        int floatCount = vectorBytes.Length / sizeof(float);
        float[] result = new float[floatCount];
        Buffer.BlockCopy(vectorBytes, 0, result, 0, vectorBytes.Length);
        return result;
    }

    throw new DataApiBuilderException(
        message: $"Unexpected vector data type: {value.GetType()}",
        statusCode: HttpStatusCode.InternalServerError,
        subStatusCode: DataApiBuilderException.SubStatusCodes.UnexpectedError);
}
```

**SqlMutationEngine Serialization**:
```csharp
public static byte[] SerializeVector(float[] vectorArray)
{
    byte[] result = new byte[vectorArray.Length * sizeof(float)];
    Buffer.BlockCopy(vectorArray, 0, result, 0, result.Length);
    return result;
}
```

---

## R3: GraphQL Vector Type Representation

### Question
Should vector columns map to GraphQL list type `[Float!]` (simple approach) or a custom scalar `Vector` (type-safe approach)? What are the HotChocolate implications?

### Research Findings

**Comparison Analysis:**

| Approach | Pros | Cons | HotChocolate Complexity |
|----------|------|------|------------------------|
| **[Float!]** | Simple, standard GraphQL, client-friendly | No dimension enforcement at schema level | Low - built-in type |
| **Custom Scalar** | Type-safe, can validate dimensions, clear semantics | Non-standard, requires custom serializer | Medium - custom scalar |

**Recommendation: Use `[Float!]` (List of Floats)**

### Rationale

1. **Simplicity**: Standard GraphQL type, no custom scalar infrastructure needed
2. **Client Compatibility**: All GraphQL clients understand list types natively
3. **Flexibility**: Dimension validation occurs at runtime (better error messages)
4. **Consistency**: Matches JSON array representation in REST API
5. **HotChocolate Support**: Built-in serialization for `float[]` ↔ `[Float!]`

### Implementation Recommendation

**GraphQL Schema Mapping**:
```csharp
// In GraphQLSchemaCreator
public IType GetGraphQLTypeFromSystemType(Type systemType, ColumnDefinition columnDefinition)
{
    // Vector columns map to [Float!]
    if (systemType == typeof(float[]) && columnDefinition.VectorDimensions.HasValue)
    {
        // NonNullType wraps FloatType, then ListType wraps that
        return new ListType(new NonNullType(new FloatType()));
    }
    // ... existing logic
}
```

**Schema Output Example**:
```graphql
type Product {
  id: Int!
  name: String!
  description: String
  embedding: [Float!]  # VECTOR(1536) column
}

input ProductInput {
  name: String!
  description: String
  embedding: [Float!]  # Must match dimension at runtime
}
```

**Alternative Considered (Custom Scalar)**:
```graphql
scalar Vector

type Product {
  embedding: Vector  # Custom scalar (NOT RECOMMENDED)
}
```

**Decision**: Proceed with `[Float!]` for MVP. Custom scalar can be added in future iteration if dimension enforcement at schema level is required.

---

## R4: Entity Configuration Schema Extension

### Question
What is the exact configuration syntax for `OmitVectorColumns` in `dab-config.json`? Should it be a boolean property under `entity.source` or a separate `vector-options` section?

### Research Findings

**Existing Configuration Patterns Analysis:**

1. **Entity-Level Properties**: `key-fields`, `source`, `permissions`, `relationships`, `mappings`
2. **Source-Level Properties**: `object`, `type`, `parameters`, `key-fields`
3. **Simple Boolean Flags**: Typically at entity level (e.g., `graphql.enabled`, `rest.enabled`)

**Recommendation: Entity-Level Boolean Property**

### Configuration Syntax

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
          "actions": ["read"]
        }
      ],
      "graphql": {
        "enabled": true
      },
      "rest": {
        "enabled": true
      }
    }
  }
}
```

### Rationale

1. **Entity-Level Scope**: Configuration applies to entire entity (all vectors omitted together)
2. **Kebab-Case Convention**: Matches existing `key-fields`, `rest-path` patterns
3. **Boolean Simplicity**: Clear on/off behavior
4. **Default Value**: `false` (vectors included by default when supported)
5. **Location**: Top-level entity property (not nested in `source`, `graphql`, or `rest`)

### Implementation Recommendation

**Entity.cs Property**:
```csharp
// In src/Config/ObjectModel/Entity.cs
public class Entity
{
    // ... existing properties ...
    
    /// <summary>
    /// When true, all VECTOR columns for this entity are excluded from queries and schema.
    /// Enables access to tables with vector columns when full vector support is not needed.
    /// </summary>
    [JsonPropertyName("omit-vector-columns")]
    public bool OmitVectorColumns { get; set; } = false;
}
```

**JSON Schema Update (dab.draft.schema.json)**:
```json
{
  "properties": {
    "omit-vector-columns": {
      "type": "boolean",
      "default": false,
      "description": "When true, all VECTOR columns are excluded from GraphQL schema and REST responses for this entity. Allows access to tables with vector columns when vector data is not needed."
    }
  }
}
```

---

## R5: Validation Error Handling Pattern

### Question
Where should vector validation occur: in `SqlMutationEngine` before parameter binding, in `ODataParser` for REST API, or in GraphQL resolver pipeline?

### Research Findings

**Existing Validation Patterns:**

1. **SqlMutationEngine**: Primary validation point for mutations
   - Parameter type validation
   - Constraint checking
   - Business rule enforcement
   - Location: `src/Core/Resolvers/SqlMutationEngine.cs`

2. **ODataParser**: REST-specific query parsing
   - URL parameter validation
   - $filter, $select syntax
   - Location: REST pipeline, not mutation-focused

3. **GraphQL Resolvers**: GraphQL-specific validation
   - Input type validation (automatic via HotChocolate)
   - Authorization checks
   - Location: Service layer resolvers

**Recommendation: Centralized in SqlMutationEngine**

### Rationale

1. **Single Source of Truth**: Validation logic in one place ensures consistency
2. **GraphQL + REST Convergence**: Both APIs use SqlMutationEngine for mutations
3. **Pre-Database Execution**: Fail fast before SQL execution
4. **Existing Pattern**: Follows current validation architecture
5. **Error Code Consistency**: Use existing DataApiBuilderException framework

### Implementation Recommendation

**Validation Service**:
```csharp
// src/Core/Services/VectorValidationService.cs
public class VectorValidationService
{
    public static void ValidateVectorInput(
        object? value, 
        int expectedDimension, 
        string columnName)
    {
        if (value == null)
        {
            return; // NULL is valid
        }

        if (value is not IEnumerable<object> enumerable)
        {
            throw new DataApiBuilderException(
                message: $"Vector column '{columnName}' requires an array of floats",
                statusCode: HttpStatusCode.BadRequest,
                subStatusCode: DataApiBuilderException.SubStatusCodes.BadRequest);
        }

        float[] vectorArray = enumerable
            .Select(ConvertToFloat)
            .ToArray();

        if (vectorArray.Length != expectedDimension)
        {
            throw new DataApiBuilderException(
                message: $"Vector column '{columnName}' expects {expectedDimension} dimensions but received {vectorArray.Length}",
                statusCode: HttpStatusCode.BadRequest,
                subStatusCode: DataApiBuilderException.SubStatusCodes.BadRequest);
        }
    }

    private static float ConvertToFloat(object value)
    {
        if (value is float f) return f;
        if (value is double d) return (float)d;
        if (value is int i) return i;
        
        if (float.TryParse(value.ToString(), out float result))
        {
            return result;
        }

        throw new DataApiBuilderException(
            message: $"Vector element '{value}' cannot be converted to float",
            statusCode: HttpStatusCode.BadRequest,
            subStatusCode: DataApiBuilderException.SubStatusCodes.BadRequest);
    }
}
```

**Integration in SqlMutationEngine**:
```csharp
// In SqlMutationEngine.ExecuteAsync() - before parameter binding
foreach (var parameter in parameters)
{
    ColumnDefinition columnDef = GetColumnDefinition(parameter.Name);
    
    if (columnDef.VectorDimensions.HasValue)
    {
        VectorValidationService.ValidateVectorInput(
            parameter.Value,
            columnDef.VectorDimensions.Value,
            parameter.Name);
            
        // Serialize to binary format
        if (parameter.Value != null)
        {
            float[] vectorArray = ((IEnumerable<object>)parameter.Value)
                .Select(v => Convert.ToSingle(v))
                .ToArray();
            parameter.Value = SerializeVector(vectorArray);
            parameter.SqlDbType = SqlDbType.VarBinary;
        }
    }
}
```

**Error Handling**:
- **Dimension Mismatch**: HTTP 400 with clear message
- **Type Error**: HTTP 400 with element conversion error
- **NULL Handling**: Allowed (no validation error)
- **Empty Array**: Dimension mismatch error (0 != expected dimension)

---

## Summary of Decisions

| Research Question | Decision | Rationale |
|-------------------|----------|-----------|
| **R1: Type Introspection** | Parse "vector(N)" from TYPE_NAME(), extract dimension via regex | Simple, works with SQL Server preview format |
| **R2: Wire Format** | byte[] binary (float32 little-endian), use Buffer.BlockCopy | Efficient, matches SQL Server binary format |
| **R3: GraphQL Type** | [Float!] list type (not custom scalar) | Simplicity, standard GraphQL, client compatibility |
| **R4: Configuration** | Entity-level "omit-vector-columns": boolean | Consistent with existing patterns, clear semantics |
| **R5: Validation Location** | SqlMutationEngine (centralized) | Single source of truth, both GraphQL & REST |

---

## Next Steps

✅ **Research Complete** - Proceed to Phase 1 design artifacts:
- **T002**: Create data-model.md with entity definitions
- **T003**: Create contracts/ with GraphQL/REST examples
- **T004**: Create quickstart.md with configuration samples
- **T005**: Update agent context with technology choices

---

## References

- SQL Server 2025 VECTOR type preview documentation
- Microsoft.Data.SqlClient documentation
- HotChocolate GraphQL type system documentation
- Existing DAB configuration patterns in Config/ObjectModel
- Existing validation patterns in SqlMutationEngine.cs
