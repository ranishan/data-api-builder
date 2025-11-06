# Implementation Plan: SQL Server Vector Data Type Support# Implementation Plan: [FEATURE]



**Branch**: `001-vector-support` | **Date**: 2025-11-05 | **Spec**: [spec.md](spec.md)**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]

**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

## Summary

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

Add SQL Server VECTOR data type support to Data API Builder enabling CRUD operations on vector columns via GraphQL and REST APIs. Implementation follows a two-phase approach: Phase 1 provides entity-level configuration to omit vector columns, unblocking tables with vectors; Phase 2 implements full vector support including read/write operations with proper type mapping and validation.

## Summary

## Technical Context

[Extract from feature spec: primary requirement + technical approach from research]

**Language/Version**: C# / .NET 8 (as per global.json)  

**Primary Dependencies**: Microsoft.Data.SqlClient, HotChocolate (GraphQL), ASP.NET Core  ## Technical Context

**Storage**: SQL Server 2025 preview or later (with VECTOR data type support)  

**Testing**: xUnit with existing test patterns (unit, integration, contract tests per database type)  <!--

**Target Platform**: Cross-platform (.NET 8, containerized for production)    ACTION REQUIRED: Replace the content in this section with the technical details

**Project Type**: Multi-project solution (Core, Config, Service, CLI)    for the project. The structure here is presented in advisory capacity to guide

**Performance Goals**: Vector query overhead ≤20% vs non-vector queries for typical 1536-dimension embeddings    the iteration process.

**Constraints**: -->

- SQL Server only (no cross-database support in this feature)

- Maximum vector dimension: 8000 (SQL Server limit, configurable)**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]  

- No vector similarity search (VECTOR_DISTANCE) in initial implementation**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]  

- No filtering/ordering by vector columns**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]  

**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]  

**Scale/Scope**: Production-grade feature supporting enterprise AI/ML workloads with embedding dimensions 128-4096**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]

**Project Type**: [single/web/mobile - determines source structure]  

## Constitution Check**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]  

**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]  

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.***Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]



### I. Production-Grade Quality## Constitution Check

- ✅ **Breaking Change Assessment**: Feature is additive; existing functionality unaffected

- ✅ **Performance**: Success criteria SC-004 defines ≤20% overhead target*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ **Security**: Vector data validation (type, dimension) prevents injection attacks

[Gates determined based on constitution file]

### II. C# Coding Standards & SOLID Principles

- ✅ **Pattern Alignment**: Will follow existing `SqlToCLRType`, `PopulateColumnDefinitionWithHasDefaultAndDbType` patterns in `MsSqlMetadataProvider`## Project Structure

- ✅ **SOLID**: Single Responsibility (separate type mapping, validation, serialization concerns)

- ✅ **Nullable**: Existing nullable reference types enabled project-wide### Documentation (this feature)



### III. Pattern Consistency```text

- ✅ **Existing Patterns**: Uses established `TypeHelper`, `ColumnDefinition`, metadata provider architecturespecs/[###-feature]/

- ✅ **Validation**: Follows existing validation patterns in `SqlMutationEngine`├── plan.md              # This file (/speckit.plan command output)

- ✅ **Error Handling**: Uses existing exception types (`DataApiBuilderException`)├── research.md          # Phase 0 output (/speckit.plan command)

├── data-model.md        # Phase 1 output (/speckit.plan command)

### IV. Comprehensive Testing├── quickstart.md        # Phase 1 output (/speckit.plan command)

- ✅ **Test Organization**: Will add tests to `Service.Tests` with `[TestCategory("MsSql")]`├── contracts/           # Phase 1 output (/speckit.plan command)

- ✅ **Integration Tests**: CRUD tests against SQL Server 2025 with VECTOR columns└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)

- ✅ **Unit Tests**: Type mapping, validation logic, schema generation```

- ✅ **Contract Tests**: GraphQL schema generation for vector types

### Source Code (repository root)

### V. Documentation & Code Comments<!--

- ✅ **Code Comments**: Complex logic (dimension extraction, serialization) will have explanatory comments  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout

- ✅ **XML Docs**: Public APIs will have XML documentation  for this feature. Delete unused options and expand the chosen structure with

- ✅ **User Docs**: FR-021 requires configuration samples and documentation updates  real paths (e.g., apps/admin, packages/something). The delivered plan must

  not include Option labels.

### VI. Breaking Change Management-->

- ✅ **No Breaking Changes**: Additive feature with explicit configuration opt-in

- ✅ **Backward Compatibility**: Tables without vectors unaffected; omit-vector config is optional```text

# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)

### VII. Multi-Database Supportsrc/

- ⚠️ **SQL Server Only**: Explicitly scoped to SQL Server in constraints section├── models/

- ✅ **Future Extensibility**: Design allows future PostgreSQL pgvector support├── services/

├── cli/

### VIII. Security-First Design└── lib/

- ✅ **Input Validation**: FR-011, FR-012 require type and dimension validation

- ✅ **SQL Injection**: Parameterized queries prevent injection via vector datatests/

- ✅ **Error Messages**: FR-022 requires clear validation errors (no sensitive data exposure)├── contract/

├── integration/

**Gate Status**: ✅ **PASSED** - All constitution requirements addressed└── unit/



## Project Structure# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)

backend/

### Documentation (this feature)├── src/

│   ├── models/

```text│   ├── services/

specs/001-vector-support/│   └── api/

├── plan.md              # This file└── tests/

├── research.md          # Phase 0 output

├── data-model.md        # Phase 1 outputfrontend/

├── quickstart.md        # Phase 1 output├── src/

├── contracts/           # Phase 1 output│   ├── components/

│   ├── graphql-schema.graphql│   ├── pages/

│   └── rest-api-examples.md│   └── services/

└── checklists/└── tests/

    └── requirements.md  # Quality validation (complete)

```# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)

api/

### Source Code (repository root)└── [same as backend above]



```textios/ or android/

src/└── [platform-specific structure: feature modules, UI flows, platform tests]

├── Config/```

│   └── ObjectModel/

│       └── Entity.cs                    # Add OmitVectorColumns property**Structure Decision**: [Document the selected structure and reference the real

├── Core/directories captured above]

│   ├── Models/

│   │   └── ColumnDefinition.cs          # Add VectorDimensions property## Complexity Tracking

│   ├── Services/

│   │   └── MetadataProviders/> **Fill ONLY if Constitution Check has violations that must be justified**

│   │       ├── SqlMetadataProvider.cs   # Abstract vector type handling

│   │       └── MsSqlMetadataProvider.cs # SQL Server vector implementation| Violation | Why Needed | Simpler Alternative Rejected Because |

│   ├── Resolvers/|-----------|------------|-------------------------------------|

│   │   ├── MsSqlQueryBuilder.cs         # Vector column introspection query| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |

│   │   ├── SqlMutationEngine.cs         # Vector data validation| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

│   │   └── SqlResponseHelpers.cs        # Vector serialization/deserialization
│   └── Services/
│       └── GraphQLSchemaCreator.cs      # Vector GraphQL type mapping
└── Service/
    └── GraphQLBuilder/
        └── GraphQLTypes/
            └── (Vector type handling)

tests/
└── Service.Tests/
    ├── SqlTests/
    │   ├── GraphQLMutationTests/
    │   │   └── VectorMutationTests.cs   # Vector CRUD via GraphQL
    │   ├── GraphQLQueryTests/
    │   │   └── VectorQueryTests.cs      # Vector read tests
    │   └── RestApiTests/
    │       └── VectorApiTests.cs        # Vector REST API tests
    └── UnitTests/
        ├── VectorTypeMappingTests.cs     # SqlToCLRType vector handling
        └── VectorValidationTests.cs      # Dimension/type validation
```

**Structure Decision**: Standard multi-project structure maintained. Vector support implemented across Core (type system), Service (GraphQL schema), Config (entity configuration). Tests organized by database type (MsSql) following existing patterns.

## Complexity Tracking

> No violations - feature aligns with existing architecture

---

## Phase 0: Research

**Goal**: Answer critical unknowns before design decisions. Output document: `research.md`

### R1: SQL Server Vector Type Introspection

**Question**: How does SQL Server represent VECTOR types in `sys.types` and `sys.columns`? What is the exact value of `TYPE_NAME(system_type_id)` for VECTOR columns, and how do we extract the dimension parameter?

**Why Critical**: Current code uses `TYPE_NAME(system_type_id)` in `MsSqlMetadataProvider.PopulateColumnDefinitionWithHasDefaultAndDbType()`. We need the exact type string format to detect vector columns during schema introspection.

**Research Approach**:
```sql
-- Test against SQL Server 2025 with VECTOR table
CREATE TABLE TestVectors (id INT, embedding VECTOR(1536));
SELECT c.name, TYPE_NAME(c.system_type_id), c.max_length, c.precision, c.scale
FROM sys.columns c WHERE c.object_id = OBJECT_ID('TestVectors');
```

**Expected Output**: Type name format (e.g., `"vector"` or `"VECTOR"`), dimension extraction strategy (from max_length, precision, or parsing type name).

### R2: ADO.NET VECTOR Data Wire Format

**Question**: What is the serialization format for VECTOR data between SQL Server and Microsoft.Data.SqlClient? Does `SqlDataReader` return vectors as byte arrays, strings, or custom types?

**Why Critical**: Determines implementation of `SqlResponseHelpers` deserialization logic and `SqlMutationEngine` parameter binding for INSERT/UPDATE.

**Research Approach**:
```csharp
// Test with Microsoft.Data.SqlClient
using (var reader = await cmd.ExecuteReaderAsync()) {
    while (await reader.ReadAsync()) {
        var vectorValue = reader.GetValue(columnIndex);
        Console.WriteLine($"Type: {vectorValue.GetType()}, Value: {vectorValue}");
    }
}
```

**Expected Output**: .NET type returned (byte[], string, float[]), binary format if byte[], JSON format if string.

### R3: GraphQL Vector Type Representation

**Question**: Should vector columns map to GraphQL list type `[Float!]` (simple approach) or a custom scalar `Vector` (type-safe approach)? What are the HotChocolate implications for query parsing and serialization?

**Why Critical**: Affects `GraphQLSchemaCreator` type mapping, client query syntax, and validation logic. List type is simpler but allows dimension mismatches; custom scalar is safer but requires more infrastructure.

**Research Approach**:
- Review HotChocolate documentation for custom scalar types
- Analyze existing custom type implementations in codebase (if any)
- Prototype both approaches with sample GraphQL schema

**Expected Output**: Recommendation with trade-offs, sample schema for chosen approach.

### R4: Entity Configuration Schema Extension

**Question**: What is the exact configuration syntax for `OmitVectorColumns` in `dab-config.json`? Should it be a boolean property under `entity.source` or a separate `vector-options` section?

**Why Critical**: Determines `Entity.cs` property structure, JSON schema updates (dab.draft.schema.json), and ConfigGenerator logic. Must follow existing configuration patterns (e.g., `key-fields`, `mappings`).

**Research Approach**:
- Analyze existing entity configuration properties in `Config/ObjectModel/Entity.cs`
- Review JSON schema structure in `schemas/dab.draft.schema.json`
- Propose syntax following existing conventions

**Expected Output**: 
```json
{
  "entities": {
    "Product": {
      "source": "products",
      "omit-vector-columns": true
    }
  }
}
```

### R5: Validation Error Handling Pattern

**Question**: Where should vector validation occur: in `SqlMutationEngine` before parameter binding, in `ODataParser` for REST API, or in GraphQL resolver pipeline? What existing validation patterns should be followed?

**Why Critical**: Ensures consistent error messages across GraphQL and REST APIs (FR-022 requirement). Must integrate with existing validation infrastructure.

**Research Approach**:
- Trace existing validation flow for INSERT operations in `SqlMutationEngine.cs`
- Identify where `DataApiBuilderException` is thrown for validation errors
- Analyze how REST and GraphQL converge on same validation logic

**Expected Output**: Validation insertion point (likely `SqlMutationEngine.ExecuteAsync`), error code constants for dimension/type mismatch.

---

**Phase 0 Exit Criteria**: All 5 research questions answered in `research.md`, with code samples and concrete recommendations. Proceed to Phase 1 design only after research complete.

**NOTE**: Phase 1 design artifacts will be generated after Phase 0 research is completed.
