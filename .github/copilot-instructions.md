# data-api-builder Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-05

## Active Technologies

- C# / .NET 8, SQL Server 2025 (VECTOR type), HotChocolate GraphQL, Microsoft.Data.SqlClient (001-vector-support)

## Project Structure

```text
src/
├── Core/                   # Domain models, metadata providers
│   ├── Models/            # ColumnDefinition, Entity models
│   ├── Services/          # MetadataProviders (MsSqlMetadataProvider)
│   └── Constants/         # SqlConstants
├── Config/                 # Configuration (Entity.cs, dab-config.json schema)
├── Service/                # API layer (GraphQL, REST)
│   ├── GraphQLSchemaCreator.cs
│   └── Serialization/     # Response formatting
├── Cli/                    # CLI commands (dab init, configure)
└── Service.Tests/          # xUnit tests
    ├── SqlTests/          # [TestCategory("MsSql")]
    └── UnitTests/
```

## Commands

dotnet build src/Azure.DataApiBuilder.sln; dotnet test --filter TestCategory=MsSql

## Code Style

C# - Follow existing patterns (nullable reference types, SOLID, XML docs), warnings as errors

## Recent Changes

- 001-vector-support: Added SQL Server VECTOR data type support with GraphQL [Float!] mapping, entity-level omit-vector-columns configuration, Buffer.BlockCopy serialization, centralized validation in SqlMutationEngine

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
