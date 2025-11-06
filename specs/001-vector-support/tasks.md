# Tasks: SQL Server Vector Data Type Support

**Input**: Design documents from `/specs/001-vector-support/`
**Prerequisites**: plan.md (complete), spec.md (complete with user stories)

**Tests**: Test tasks are included per constitution requirement for comprehensive testing coverage.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are based on existing Data API Builder multi-project structure:
- **Core logic**: `src/Core/`
- **Configuration**: `src/Config/`
- **Service layer**: `src/Service/`
- **Tests**: `src/Service.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and research completion

- [X] T001 Complete Phase 0 research document (research.md) answering 5 critical questions about SQL Server VECTOR introspection, ADO.NET wire format, GraphQL type mapping, configuration schema, and validation patterns
- [X] T002 [P] Create Phase 1 data model document (data-model.md) defining VECTOR Column Metadata, Vector Data, and Entity Configuration entities
- [X] T003 [P] Create Phase 1 contracts documentation in contracts/ directory with GraphQL schema examples and REST API samples
- [X] T004 [P] Create Phase 1 quickstart guide (quickstart.md) with configuration examples and usage scenarios
- [X] T005 Update agent context with technology choices using .specify/scripts/powershell/update-agent-context.ps1

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core type system and configuration infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T006 Add VectorDimensions nullable integer property to ColumnDefinition class in src/Core/Models/ColumnDefinition.cs
- [X] T007 Add OmitVectorColumns boolean property to Entity class in src/Config/ObjectModel/Entity.cs
- [X] T008 Update JSON schema (schemas/dab.draft.schema.json) to include omit-vector-columns configuration property with type boolean and description
- [X] T009 [P] Create SqlVectorTypeHelper utility class in src/Core/Services/MetadataProviders/SqlVectorTypeHelper.cs for dimension extraction and format parsing
- [X] T010 [P] Add vector type detection constants (type name patterns, dimension limits) to src/Core/Constants/SqlConstants.cs or appropriate constants file

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Omit Vector Columns (Priority: P1) 🎯 MVP

**Goal**: Enable tables with VECTOR columns to be accessible through DAB by providing entity-level configuration to omit vector columns, unblocking CRUD operations on non-vector fields

**Independent Test**: Configure a table with vector columns to omit vectors, then perform GraphQL queries and REST API calls on non-vector fields. Success means table is accessible and all non-vector operations work without errors.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T011 [P] [US1] Create VectorOmissionTests.cs in src/Service.Tests/SqlTests/ with [TestCategory("MsSql")] attribute for vector omission configuration validation
- [X] T012 [P] [US1] Add integration test in VectorOmissionTests.cs: Test_Table_With_Omitted_Vectors_Returns_NonVector_Fields_Via_GraphQL
- [X] T013 [P] [US1] Add integration test in VectorOmissionTests.cs: Test_Table_With_Omitted_Vectors_Returns_NonVector_Fields_Via_REST
- [X] T014 [P] [US1] Add integration test in VectorOmissionTests.cs: Test_Mutation_On_NonVector_Fields_Succeeds_With_Omitted_Vectors
- [X] T015 [P] [US1] Add integration test in VectorOmissionTests.cs: Test_Table_Without_Omit_Config_Returns_Error_When_Vectors_Present

### Implementation for User Story 1

- [X] T016 [US1] Extend MsSqlMetadataProvider.SqlToCLRType() method in src/Core/Services/MetadataProviders/MsSqlMetadataProvider.cs to detect vector type and return typeof(float[]) for vector columns
- [X] T017 [US1] Update MsSqlMetadataProvider.PopulateColumnDefinitionWithHasDefaultAndDbType() in src/Core/Services/MetadataProviders/MsSqlMetadataProvider.cs to extract vector dimensions using SqlVectorTypeHelper
#### User Story 1 Implementation (P1 - Core)
- [X] **T016**: Modify `MsSqlMetadataProvider.SqlToCLRType()` to detect VECTOR types → return `typeof(float[])`
- [X] **T017**: Modify `MsSqlMetadataProvider.PopulateColumnDefinitionWithHasDefaultAndDbType()` to extract dimension from "vector(N)" → set `VectorDimensions`
- [X] **T018**: Modify `MsSqlQueryBuilder.BuildStoredProcedureResultDetailsQuery()` to use `TYPE_NAME(system_type_id)` for introspection (already correct)
- [X] **T019**: Modify `GraphQLSchemaCreator` to exclude vector columns when `entity.OmitVectorColumns == true`
- [X] **T020**: Modify REST API serialization (`AuthorizationResolver`) to exclude vector columns when `entity.OmitVectorColumns == true`
- [X] **T021**: Add configuration validation ensuring `omit-vector-columns` boolean property (handled by JSON schema + Entity class)
- [X] **T022**: Add error handling when non-omitted VECTOR columns encountered → throw `DataApiBuilderException`
- [ ] T019 [US1] Update GraphQLSchemaCreator in src/Service/GraphQLSchemaCreator.cs to exclude vector columns from schema when entity.OmitVectorColumns is true
- [ ] T020 [US1] Update REST API response serialization in src/Service/ to exclude vector columns when entity.OmitVectorColumns is true
- [ ] T021 [US1] Add configuration validation in src/Config/ to validate omit-vector-columns property and provide clear error messages for invalid configurations
- [ ] T022 [US1] Add error handling in src/Core/Services/MetadataProviders/MsSqlMetadataProvider.cs to throw DataApiBuilderException with clear message when non-omitted VECTOR columns are encountered

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Tables with vectors are accessible when omit-vector-columns is configured.

---

## Phase 4: User Story 2 - Read Vector Data via GraphQL (Priority: P2)

**Goal**: Enable API consumers to query vector embeddings stored in SQL Server VECTOR columns through GraphQL and REST, returning vectors as arrays of floats

**Independent Test**: Create a table with VECTOR columns, insert vector data, query via GraphQL and REST to retrieve vectors. Success means vectors are returned as float arrays matching stored values.

### Tests for User Story 2

- [ ] T023 [P] [US2] Create VectorQueryTests.cs in src/Service.Tests/SqlTests/GraphQLQueryTests/ with [TestCategory("MsSql")] for vector read operations
- [ ] T024 [P] [US2] Add integration test in VectorQueryTests.cs: Test_Query_Returns_Vector_As_Float_Array_With_Correct_Dimensions
- [ ] T025 [P] [US2] Add integration test in VectorQueryTests.cs: Test_Query_Returns_Null_For_Null_Vector_Values
- [ ] T026 [P] [US2] Add integration test in VectorQueryTests.cs: Test_Query_Returns_Multiple_Vectors_With_Different_Dimensions
- [ ] T027 [P] [US2] Create VectorApiTests.cs in src/Service.Tests/SqlTests/RestApiTests/ for REST API vector read tests
- [ ] T028 [P] [US2] Add integration test in VectorApiTests.cs: Test_REST_GET_Returns_Vector_As_JSON_Array

### Implementation for User Story 2

- [ ] T029 [P] [US2] Update GraphQLSchemaCreator in src/Service/GraphQLSchemaCreator.cs to map VECTOR columns to [Float!] GraphQL list type when not omitted
- [ ] T030 [US2] Implement vector deserialization in SqlResponseHelpers in src/Core/Resolvers/SqlResponseHelpers.cs to convert SQL Server VECTOR format to float[] array
- [ ] T031 [US2] Update SqlQueryExecutor in src/Core/Resolvers/ to handle SqlDataReader vector column values using appropriate GetValue() or GetBytes() calls
- [ ] T032 [US2] Add null handling for vector columns in src/Core/Resolvers/SqlResponseHelpers.cs to return null when vector value is NULL
- [ ] T033 [US2] Update REST API JSON serialization in src/Service/ to serialize float[] arrays as JSON arrays for vector columns
- [ ] T034 [US2] Add GraphQL schema documentation generation in src/Service/GraphQLBuilder/ to document vector fields as arrays of floats with dimension information

### Unit Tests for User Story 2

- [ ] T035 [P] [US2] Create VectorTypeMappingTests.cs in src/Service.Tests/UnitTests/ to test SqlToCLRType vector type mapping
- [ ] T036 [P] [US2] Create VectorSerializationTests.cs in src/Service.Tests/UnitTests/ to test vector deserialization from various SQL Server formats

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Vector data can be read via GraphQL and REST.

---

## Phase 5: User Story 3 - Write Vector Data via GraphQL (Priority: P3)

**Goal**: Enable API consumers to insert and update vector embeddings through GraphQL mutations and REST API, accepting vectors as arrays of floats with validation

**Independent Test**: Perform GraphQL mutations and REST POST/PATCH with vector data as float arrays, verify data is stored correctly in SQL Server. Test dimension mismatch and type validation errors.

### Tests for User Story 3

- [ ] T037 [P] [US3] Create VectorMutationTests.cs in src/Service.Tests/SqlTests/GraphQLMutationTests/ with [TestCategory("MsSql")] for vector write operations
- [ ] T038 [P] [US3] Add integration test in VectorMutationTests.cs: Test_Create_Record_With_Vector_Succeeds
- [ ] T039 [P] [US3] Add integration test in VectorMutationTests.cs: Test_Update_Record_Vector_Succeeds
- [ ] T040 [P] [US3] Add integration test in VectorMutationTests.cs: Test_Create_With_Wrong_Dimension_Returns_Validation_Error
- [ ] T041 [P] [US3] Add integration test in VectorMutationTests.cs: Test_Create_With_Non_Float_Values_Returns_Validation_Error
- [ ] T042 [P] [US3] Add integration test in VectorMutationTests.cs: Test_Vector_Roundtrip_Accuracy_1536_Dimensions
- [ ] T043 [P] [US3] Add REST API write tests in VectorApiTests.cs: Test_REST_POST_With_Vector_Succeeds and Test_REST_PATCH_With_Vector_Succeeds

### Implementation for User Story 3

- [ ] T044 [P] [US3] Create VectorValidationService in src/Core/Services/ to validate vector input (type checking, dimension matching, null handling)
- [ ] T045 [US3] Implement vector serialization in SqlMutationEngine in src/Core/Resolvers/SqlMutationEngine.cs to convert float[] arrays to SQL Server VECTOR format for parameter binding
- [ ] T046 [US3] Add vector input validation in SqlMutationEngine.ExecuteAsync() in src/Core/Resolvers/SqlMutationEngine.cs before parameter binding
- [ ] T047 [US3] Update GraphQL mutation resolver in src/Service/ to accept [Float!] input type for vector fields and pass to mutation engine
- [ ] T048 [US3] Update REST API POST/PUT/PATCH handlers in src/Service/ to parse JSON array input and convert to float[] for vector columns
- [ ] T049 [US3] Add dimension validation in VectorValidationService comparing input array length against ColumnDefinition.VectorDimensions metadata
- [ ] T050 [US3] Add type validation in VectorValidationService ensuring all array elements are valid float values
- [ ] T051 [US3] Implement error handling in SqlMutationEngine to throw DataApiBuilderException with error codes for dimension mismatch (e.g., "VectorDimensionMismatch") and type errors (e.g., "VectorInvalidType")
- [ ] T052 [US3] Add support for NULL vector values in mutations (accepting null input and properly binding NULL parameters)

### Unit Tests for User Story 3

- [ ] T053 [P] [US3] Create VectorValidationTests.cs in src/Service.Tests/UnitTests/ to test dimension validation, type validation, and null handling
- [ ] T054 [P] [US3] Add unit tests in VectorValidationTests.cs for edge cases: empty arrays, extremely large dimensions, special float values (NaN, Infinity)

**Checkpoint**: All user stories should now be independently functional. Full CRUD operations on vector columns are supported.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories, documentation, and final validation

- [ ] T055 [P] Add XML documentation comments to all new public methods in SqlVectorTypeHelper, VectorValidationService, and updated MetadataProvider methods
- [ ] T056 [P] Add code comments explaining complex logic in dimension extraction (SqlVectorTypeHelper) and serialization/deserialization (SqlResponseHelpers, SqlMutationEngine)
- [ ] T057 Update user documentation in docs/ with vector support configuration examples, GraphQL query/mutation samples, and REST API examples
- [ ] T058 Add configuration samples showing omit-vector-columns usage to samples/ directory or update existing samples
- [ ] T059 [P] Add unit tests for SqlVectorTypeHelper dimension extraction logic in src/Service.Tests/UnitTests/
- [ ] T060 [P] Add contract tests validating GraphQL schema generation includes vector fields with correct types in src/Service.Tests/
- [ ] T061 Performance testing: Validate vector query overhead is ≤20% for 1536-dimension embeddings per success criteria SC-004
- [ ] T062 Run quickstart.md validation scenarios against SQL Server 2025 with VECTOR columns (once quickstart is created)
- [ ] T063 Security review: Verify vector input validation prevents SQL injection and validates all inputs per constitution security-first principle
- [ ] T064 Update CHANGELOG or release notes documenting new vector support feature with breaking change assessment (none expected)
- [ ] T065 Code cleanup: Remove placeholder comments in MsSqlMetadataProvider.SqlToCLRType() and PopulateColumnDefinitionWithHasDefaultAndDbType() after implementation
- [ ] T066 Final integration test: Create end-to-end test covering omit-vector config, read operations, and write operations in single test flow

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately. Research completion is critical before proceeding.
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories. Must complete T006-T010 before any story work begins.
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User Story 1 (P1) can start after Phase 2 - No dependencies on other stories
  - User Story 2 (P2) can start after Phase 2 - Independent but may reference US1 configuration patterns
  - User Story 3 (P3) can start after Phase 2 - Independent but builds on US2 read functionality for roundtrip tests
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories. This is the MVP.
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent implementation but shares type system from US1. Can run in parallel with US3 if team capacity allows.
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent implementation but uses read functionality (US2) for validation tests. Can run in parallel with US2 for core implementation.

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD approach per constitution)
- Type system and metadata (T016-T018) before schema generation (T019-T020) in US1
- Deserialization (T030-T031) before schema mapping (T029) in US2
- Validation service (T044) before mutation engine changes (T045-T051) in US3
- Unit tests can run in parallel with integration tests within each story
- Story complete and all tests passing before moving to next priority

### Parallel Opportunities

- **Phase 1**: Tasks T002, T003, T004 can run in parallel after T001 (research) completes
- **Phase 2**: Tasks T006-T008 (data model changes) can run in parallel with T009-T010 (utilities)
- **User Story 1 Tests**: T011-T015 can all be written in parallel
- **User Story 1 Implementation**: T016-T017 can run in parallel with T018-T020 once data model is ready
- **User Story 2 Tests**: T023-T028 can all be written in parallel
- **User Story 2 Implementation**: T029, T035-T036 can run in parallel after data model is ready
- **User Story 3 Tests**: T037-T043 can all be written in parallel
- **User Story 3 Implementation**: T044 must complete first, then T045-T052 and T053-T054 can proceed in parallel
- **Phase 6**: T055-T056 (documentation), T059-T060 (additional tests), and T063-T064 (reviews) can all run in parallel
- **Between Stories**: Once Phase 2 completes, different team members can work on US1, US2, and US3 in parallel

---

## Parallel Example: User Story 1

```bash
# Step 1: Launch all tests for User Story 1 together (write tests first):
Task T011: "Create VectorOmissionTests.cs in src/Service.Tests/SqlTests/"
Task T012: "Add test: Test_Table_With_Omitted_Vectors_Returns_NonVector_Fields_Via_GraphQL"
Task T013: "Add test: Test_Table_With_Omitted_Vectors_Returns_NonVector_Fields_Via_REST"
Task T014: "Add test: Test_Mutation_On_NonVector_Fields_Succeeds_With_Omitted_Vectors"
Task T015: "Add test: Test_Table_Without_Omit_Config_Returns_Error_When_Vectors_Present"

# Verify all tests FAIL (no implementation yet)

# Step 2: Launch foundational implementation in parallel:
Task T016: "Extend MsSqlMetadataProvider.SqlToCLRType() for vector detection"
Task T017: "Update PopulateColumnDefinitionWithHasDefaultAndDbType() for dimension extraction"
Task T018: "Modify MsSqlQueryBuilder schema introspection query"

# Step 3: Launch schema and API updates in parallel:
Task T019: "Update GraphQLSchemaCreator to exclude vectors when omitted"
Task T020: "Update REST API response serialization to exclude vectors"
Task T021: "Add configuration validation"
Task T022: "Add error handling for non-omitted vectors"

# Verify all tests now PASS
```

---

## Parallel Example: Foundational Phase

```bash
# Launch data model changes in parallel:
Task T006: "Add VectorDimensions property to ColumnDefinition.cs"
Task T007: "Add OmitVectorColumns property to Entity.cs"
Task T008: "Update JSON schema (dab.draft.schema.json)"

# Launch utilities in parallel:
Task T009: "Create SqlVectorTypeHelper utility class"
Task T010: "Add vector type detection constants"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only - P1)

1. Complete Phase 1: Setup (T001-T005) - Research and design documents
2. Complete Phase 2: Foundational (T006-T010) - CRITICAL blocking phase
3. Complete Phase 3: User Story 1 (T011-T022) - Omit vector columns feature
4. **STOP and VALIDATE**: Test User Story 1 independently with SQL Server 2025
5. Deploy/demo if ready - tables with vectors are now accessible!

**Estimated MVP Value**: Customers can immediately use DAB with tables containing vector columns by enabling omit-vector-columns configuration. This unblocks all other operations on those tables.

### Incremental Delivery

1. **Phase 1-2 (Setup + Foundation)** → Foundation ready, type system extended
2. **Add User Story 1 (P1)** → Test independently → Deploy/Demo (MVP!) - Omit vectors works
3. **Add User Story 2 (P2)** → Test independently → Deploy/Demo - Read vectors works
4. **Add User Story 3 (P3)** → Test independently → Deploy/Demo - Full CRUD on vectors works
5. **Phase 6 (Polish)** → Final testing, documentation, performance validation
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. **Team completes Setup + Foundational together** (T001-T010)
2. **Once Foundational is done, split work**:
   - Developer A: User Story 1 (T011-T022) - Omit vectors
   - Developer B: User Story 2 (T023-T036) - Read vectors
   - Developer C: User Story 3 (T037-T054) - Write vectors
3. Stories complete and integrate independently
4. **Team reconvenes for Phase 6** (T055-T066) - Polish together

### Testing Strategy

- **TDD Approach**: Write all tests for a user story first (marked with [P]), ensure they FAIL
- **Test Independence**: Each user story's tests should validate that story in isolation
- **Test Categories**: Use `[TestCategory("MsSql")]` to enable SQL Server-specific test runs
- **Integration Tests**: Test against real SQL Server 2025 instance with VECTOR columns
- **Unit Tests**: Test type mapping, validation logic, and utilities in isolation
- **Contract Tests**: Validate GraphQL schema generation and REST API contracts

---

## Notes

- **[P] tasks**: Different files, no dependencies - safe to parallelize
- **[Story] label**: Maps task to specific user story for traceability and independent delivery
- **Constitution Compliance**: All tasks follow C# coding standards, SOLID principles, and comprehensive testing requirements
- **SQL Server 2025**: All integration tests require SQL Server 2025 preview or later with VECTOR data type support
- **No Breaking Changes**: Feature is additive with explicit configuration opt-in (backward compatible)
- **Pattern Consistency**: Implementation follows existing metadata provider, type mapping, and validation patterns
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD per constitution)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently

---

## Task Summary

- **Total Tasks**: 66 tasks
- **Setup Phase**: 5 tasks (research, design documents, agent context)
- **Foundational Phase**: 5 tasks (CRITICAL - blocks all stories)
- **User Story 1 (P1 - MVP)**: 12 tasks (5 tests + 7 implementation)
- **User Story 2 (P2)**: 14 tasks (6 integration tests + 6 implementation + 2 unit tests)
- **User Story 3 (P3)**: 18 tasks (7 integration tests + 9 implementation + 2 unit tests)
- **Polish Phase**: 12 tasks (documentation, additional tests, validation)

**Parallel Opportunities**: 32 tasks marked [P] can be executed in parallel with other tasks in their phase

**MVP Scope**: Phases 1-3 (Tasks T001-T022) deliver omit-vector-columns feature enabling immediate customer value

**Independent Test Criteria**:
- **US1**: Configure omit-vector, perform CRUD on non-vector fields → Success if no errors
- **US2**: Query vector columns → Success if float arrays returned matching stored values  
- **US3**: Insert/update vectors → Success if data persists and roundtrips with 100% accuracy

**Suggested First Milestone**: Complete T001-T022 (MVP - User Story 1) for immediate customer unblocking
