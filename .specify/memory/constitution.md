<!--
  SYNC IMPACT REPORT
  ==================
  Version Change: Initial → 1.0.0
  Modified Principles: All (initial creation)
  Added Sections: All core principles, architecture requirements, development workflow, governance
  Removed Sections: None (initial version)
  
  Template Updates:
  ✅ .specify/templates/plan-template.md - Constitution Check section aligns with all principles
  ✅ .specify/templates/spec-template.md - Requirements align with testing and documentation requirements
  ✅ .specify/templates/tasks-template.md - Task organization reflects testing-first approach
  ✅ .github/prompts/*.md - Agent guidance files reviewed for consistency
  
  Follow-up TODOs: None - all principles fully defined
-->

# Data API Builder Constitution

## Core Principles

### I. Production-Grade Quality (NON-NEGOTIABLE)

Data API builder is used in production by customers. All code changes MUST meet production-grade standards:

- Code MUST be production-ready, thoroughly tested, and follow existing patterns
- Code MUST be cautious about introducing breaking changes
- Breaking changes MUST be explicitly documented and justified in code comments
- Performance implications MUST be considered for high-scale scenarios
- Security considerations MUST be evaluated for all changes

**Rationale**: Production usage demands reliability and backward compatibility. Customer trust depends on stable, 
secure, and performant releases.

### II. C# Coding Standards & SOLID Principles (NON-NEGOTIABLE)

All code MUST adhere to C# best practices and architectural principles:

- Follow C# coding conventions and naming standards
- Apply SOLID principles: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, 
  Dependency Inversion
- Implement Clean Architecture patterns with clear separation of concerns
- Write Clean Code: readable, maintainable, and self-documenting
- Use nullable reference types appropriately (Nullable is enabled project-wide)
- Treat warnings as errors (TreatWarningsAsErrors is true)
- Run `dotnet format` before committing (enforced in CI)

**Rationale**: Consistent code quality ensures long-term maintainability, reduces bugs, and enables team scalability.

### III. Pattern Consistency (NON-NEGOTIABLE)

New code MUST follow existing patterns in the codebase:

- Study existing implementations before adding new features
- Reuse established patterns for similar functionality
- Maintain consistency in error handling, logging, and validation approaches
- Follow existing project structure conventions (Auth, Config, Core, Service, CLI, etc.)
- Align with existing dependency injection and service registration patterns

**Rationale**: Pattern consistency reduces cognitive load, makes code predictable, and simplifies onboarding 
for new contributors.

### IV. Comprehensive Testing (NON-NEGOTIABLE)

All code changes MUST include appropriate test coverage following existing testing patterns:

- **Unit Tests**: Required for business logic, utilities, and isolated components
- **Integration Tests**: Required for database operations, API endpoints, and cross-component interactions
- **Contract Tests**: Required when adding or modifying API contracts
- Tests MUST be organized by database type (MsSql, PostgreSql, MySql, CosmosDb_NoSql) where applicable
- Tests MUST use existing test infrastructure and helper methods
- Tests MUST cover edge cases, error scenarios, and boundary conditions
- Generated SQL queries in tests MUST be formatted for readability using standard SQL formatters

**Rationale**: Comprehensive testing prevents regressions, documents expected behavior, and enables confident refactoring.

### V. Documentation & Code Comments (NON-NEGOTIABLE)

Documentation MUST be maintained alongside code changes:

- Update official documentation on Microsoft Learn when user-facing behavior changes
- Complex logic MUST have explanatory comments
- File headers, classes, and public methods MUST have XML documentation comments following existing patterns
- Breaking changes MUST be documented in code with explicit comments
- Configuration changes MUST be reflected in schema files and samples
- README and contributing guidelines MUST be kept current

**Rationale**: Documentation ensures features are discoverable, usable, and maintainable. Complex systems require 
clear explanations for sustainability.

### VI. Breaking Change Management

Breaking changes MUST be handled with extreme care:

- Breaking changes MUST be explicitly identified in code comments using "breaking change" terminology
- Breaking changes MUST be justified with clear reasoning
- Alternatives to breaking changes MUST be explored first
- When unavoidable, breaking changes MUST include migration guidance
- Default values and public APIs MUST NOT be changed without explicit approval
- Deprecation warnings MUST be issued before removal of functionality

**Rationale**: Production users depend on stable APIs. Breaking changes cause customer pain and erode trust.

### VII. Multi-Database Support

Code MUST account for all supported database types:

- Microsoft SQL Server
- PostgreSQL  
- MySQL
- Azure Cosmos DB for NoSQL
- Azure SQL Data Warehouse

**Rationale**: DAB's value proposition includes multi-database support. Database-specific implementations must 
maintain feature parity where possible.

### VIII. Security-First Design

Security considerations MUST be embedded in all features:

- Authentication and authorization patterns MUST be followed
- Input validation MUST be comprehensive
- SQL injection prevention MUST be maintained
- Security-related configuration changes MUST be reviewed carefully
- Claims-based policies MUST be evaluated for authorization logic
- Security events MUST be logged appropriately

**Rationale**: DAB handles sensitive data and authentication. Security vulnerabilities have severe consequences.

## Architecture Requirements

### Clean Architecture Enforcement

The project structure reflects Clean Architecture principles:

- **Core**: Domain logic, authorization, resolvers (no external dependencies)
- **Config**: Configuration models and validation
- **Auth**: Authentication and authorization infrastructure  
- **Service**: API layer, GraphQL/REST endpoints
- **CLI**: Command-line interface and configuration generation
- Dependencies MUST flow inward (Service/CLI → Config/Auth → Core)

### Observability & Diagnostics

All features MUST support operational visibility:

- Structured logging using existing logging infrastructure
- OpenTelemetry integration for distributed tracing
- Health endpoints for monitoring
- Hot reload for dynamic log level changes
- Error messages MUST be actionable and clear

## Development Workflow

### Code Review Requirements

All pull requests MUST:

- Pass all automated tests (unit, integration, contract)
- Pass `dotnet format` validation
- Have zero build warnings (warnings treated as errors)
- Include test coverage for new/changed code
- Update documentation if user-facing behavior changes
- Follow existing patterns and architectural principles

### Integration Testing Workflow

Database-specific tests require setup:

1. Configure database with appropriate schema from `Service.Tests/DatabaseSchema-<engine>.sql`
2. Set connection string in configuration or use `@env()` syntax
3. Run tests with filter: `dotnet test --filter "TestCategory=<DatabaseType>"`
4. CosmosDB tests auto-create database/collections (no schema setup needed)

### Contribution Process

Contributors MUST:

1. Review CONTRIBUTING.md and existing code patterns
2. Agree to Contributor License Agreement (CLA)
3. Submit issues using appropriate templates
4. Include configuration files, logs, and hosting model in issue reports
5. Follow the existing project structure and conventions

## Governance

This constitution supersedes all other practices and serves as the definitive guide for development standards.

### Amendment Process

Constitution changes require:

1. Documentation of rationale for changes
2. Review of impact on existing code and patterns
3. Update of dependent templates and guidance documents
4. Version bump following semantic versioning rules
5. Communication to contributors

### Compliance Review

All code changes MUST be verified against these principles during:

- Development (developer self-check)
- Code review (reviewer verification)
- CI/CD pipeline (automated checks where possible)

### Versioning Policy

Constitution follows semantic versioning:

- **MAJOR**: Backward incompatible principle removals or redefinitions
- **MINOR**: New principles added or materially expanded guidance
- **PATCH**: Clarifications, wording improvements, non-semantic refinements

### Runtime Development Guidance

For agent-specific development guidance, refer to `.github/prompts/` command files. This constitution defines 
the "what" and "why"; prompt files define the "how" for automated agents.

**Version**: 1.0.0 | **Ratified**: 2025-11-05 | **Last Amended**: 2025-11-05
