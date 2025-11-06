# Feature Specification: SQL Server Vector Data Type Support

**Feature Branch**: `001-vector-support`  
**Created**: 2025-11-05  
**Status**: Draft  
**Input**: User description: "Add vector support for Data API builder specifically for SQL Server data source. The user should be able to use GraphQL to perform CRUD on SQL Server objects that have vectors as the datatype."

## Clarifications

### Session 2025-11-05

- Q: Should MCP endpoint support be included in this feature? → A: No, MCP endpoint vector support is not a requirement for this feature and has been removed from scope.
- Q: At what level should the omit-vector configuration be applied? → A: Per-entity/table configuration, allowing all vector columns for that entity to be omitted together.
- Q: How should the system respond when users attempt unsupported vector operations (filtering, ordering)? → A: Return validation error with clear message before query execution.
- Q: Should there be a configurable maximum vector dimension limit? → A: Default limit matching SQL Server max (8000) with configuration option to support what SQL Server offers.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Omit Vector Columns (Priority: P1) 🎯 MVP

API consumers need to access tables that contain vector columns without receiving errors, even when they don't need the vector data itself. Currently, tables with VECTOR columns are inaccessible through DAB, blocking all operations on those tables.

**Why this priority**: This is the foundational capability that unblocks customers from using DAB with tables containing vector columns. It provides immediate value by allowing read/write operations on non-vector columns while gracefully omitting vector data.

**Independent Test**: Can be fully tested by configuring a table with vector columns to omit those columns, then performing standard CRUD operations on the non-vector fields. Success means the table is accessible and all non-vector operations work without errors.

**Acceptance Scenarios**:

1. **Given** a SQL Server table with both standard columns and VECTOR columns, **When** the table is configured with vector omission enabled, **Then** GraphQL queries return all non-vector fields successfully without errors
2. **Given** a table configured to omit vector columns, **When** a REST GET request is made, **Then** the response includes all non-vector fields and excludes vector fields
3. **Given** a table with omitted vector columns, **When** a mutation is performed on non-vector fields, **Then** the mutation succeeds and vector columns remain unchanged
4. **Given** a table with vector columns not configured for omission, **When** a query attempts to access the table, **Then** the system returns a clear error message indicating vector columns are not supported without omission configuration

---

### User Story 2 - Read Vector Data via GraphQL (Priority: P2)

API consumers need to query vector embeddings stored in SQL Server VECTOR columns through GraphQL queries to retrieve machine learning model embeddings for similarity searches, recommendations, and AI applications.

**Why this priority**: Once tables with vectors are accessible (P1), this enables read operations on vector data, which is the primary use case for embeddings in production applications. This allows integration with AI/ML workflows.

**Independent Test**: Can be tested by creating a table with VECTOR columns, inserting vector data, and querying via GraphQL to retrieve the vector arrays. Success means vectors are returned as arrays of floats matching the stored values.

**Acceptance Scenarios**:

1. **Given** a SQL Server table with a VECTOR(1536) column containing embeddings, **When** a GraphQL query selects that vector field, **Then** the response includes the vector as an array of 1536 float values
2. **Given** a table with multiple VECTOR columns of different dimensions, **When** queried via GraphQL, **Then** each vector is returned with its correct dimension and values
3. **Given** a table with NULL vector values, **When** queried via GraphQL, **Then** the vector field returns null in the response
4. **Given** a REST GET request for a record with a vector column, **When** the request specifies $select including the vector field, **Then** the response includes the vector as a JSON array of floats

---

### User Story 3 - Write Vector Data via GraphQL (Priority: P3)

API consumers need to insert and update vector embeddings through GraphQL mutations to store machine learning model outputs, allowing applications to persist embeddings generated from text, images, or other data.

**Why this priority**: Completes the full CRUD capability for vector columns. While reading vectors is more common, write operations are essential for applications that generate and store embeddings.

**Independent Test**: Can be tested by performing GraphQL mutations (create, update) with vector data as arrays of floats, then verifying the data is correctly stored in SQL Server. Success means vectors can be written and subsequently read back with identical values.

**Acceptance Scenarios**:

1. **Given** a GraphQL mutation to create a record, **When** the mutation includes a vector field with an array of float values, **Then** the record is created with the vector stored correctly in the VECTOR column
2. **Given** an existing record with a vector column, **When** a GraphQL mutation updates the vector field with a new array of floats, **Then** the vector is updated successfully
3. **Given** a mutation with a vector array that doesn't match the declared dimension, **When** the mutation is executed, **Then** the system returns a validation error indicating dimension mismatch
4. **Given** a mutation with a vector array containing non-float values, **When** the mutation is executed, **Then** the system returns a validation error indicating invalid data type
5. **Given** a REST POST request to create a record, **When** the request body includes a vector field as a JSON array of floats, **Then** the record is created with the vector stored correctly

---

### Edge Cases

- The system supports vector dimensions up to SQL Server's maximum (8000) with a configurable limit option for performance tuning
- How does the system handle partial vector data updates (updating some dimensions but not all)?
- What happens when a table has multiple VECTOR columns with different dimensions?
- How does the system respond when SQL Server returns vector data in an unexpected format?
- What happens during schema introspection when VECTOR columns are encountered but not yet supported?
- How are vector columns handled in relationships (foreign keys, joins)?
- When filtering or ordering by vector columns is attempted, the system returns a validation error with a clear message before query execution
- How does the system handle vector data in stored procedures and views?

## Requirements *(mandatory)*

### Functional Requirements

#### Phase 1: Omit Vector Feature (P1)

- **FR-001**: System MUST provide a per-entity/table configuration option to omit all VECTOR columns from query results for that entity by default
- **FR-002**: System MUST detect VECTOR data type columns during schema introspection by querying sys.types and sys.columns
- **FR-003**: System MUST allow tables with VECTOR columns to be accessible for CRUD operations on non-vector fields when omission is enabled
- **FR-004**: System MUST omit vector fields from GraphQL schema generation when configured to omit vectors
- **FR-005**: System MUST omit vector fields from REST API responses when configured to omit vectors
- **FR-006**: System MUST validate configuration schema to include the new omit-vector property
- **FR-008**: System MUST update JSON schema files to document the omit-vector configuration option
- **FR-009**: System MUST provide clear error messages when tables with non-omitted VECTOR columns are accessed and vector support is not fully enabled

#### Phase 2: Full Vector Support (P2-P3)

- **FR-010**: System MUST map SQL Server VECTOR data type to GraphQL list of floats ([Float!])
- **FR-011**: System MUST validate that vector input data is an array of float values during mutations
- **FR-012**: System MUST validate that vector array length matches the declared dimension from SQL Server schema
- **FR-013**: System MUST serialize vector data from SQL Server VECTOR format to JSON arrays of floats for GraphQL/REST responses
- **FR-014**: System MUST deserialize JSON arrays of floats to SQL Server VECTOR format for insert/update operations
- **FR-015**: System MUST handle NULL vector values correctly in queries and mutations
- **FR-016**: System MUST support vector columns in SELECT queries with proper $select filtering in REST API
- **FR-017**: System MUST support vector columns in INSERT operations via GraphQL mutations and REST POST
- **FR-018**: System MUST support vector columns in UPDATE operations via GraphQL mutations and REST PATCH/PUT
- **FR-019**: System MUST retrieve vector dimension metadata from SQL Server schema (sys.columns.vector_dimensions)
- **FR-020**: System MUST generate accurate GraphQL schema documentation for vector fields indicating they are arrays of floats
- **FR-021**: System MUST document vector column support in configuration samples and documentation
- **FR-022**: System MUST return clear validation errors before query execution when users attempt to filter or order by vector columns
- **FR-023**: System MUST support vector dimensions up to SQL Server's maximum limit (8000) with a configurable dimension limit option

### Assumptions

- Vector similarity search operations (e.g., VECTOR_DISTANCE, vector indexing) are out of scope for this feature and will be addressed separately
- Vector columns are not used in primary keys or foreign key relationships
- Filtering and ordering by vector columns are not supported in this initial implementation
- Vector columns in stored procedures will be handled as opaque types initially
- The SQL Server VECTOR data type follows the preview specification as documented in the GitHub issue
- Performance optimization for large vectors will be addressed in future iterations if needed
- Vector data validation is limited to type (float array) and dimension checking; value range validation is not included

### Key Entities

- **VECTOR Column Metadata**: Represents metadata about VECTOR columns including column name, declared dimension (from vector_dimensions), and nullable status. Retrieved from sys.columns and sys.types during schema introspection.
- **Vector Data**: Represents the actual vector embeddings as arrays of float32 values, with length matching the declared dimension. Serialized as JSON arrays in API responses and deserialized from JSON arrays in mutations.
- **Entity Configuration**: Represents DAB entity/table configuration with new omit-vector property that controls whether all VECTOR columns for that specific entity are excluded from queries and schema generation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can successfully configure tables with vector columns to be accessible through DAB within 5 minutes of reading documentation
- **SC-002**: Tables with VECTOR columns return query results for non-vector fields without errors when omission is configured
- **SC-003**: Vector data roundtrips correctly (insert then read) with 100% accuracy for all float values and dimensions up to 4096
- **SC-004**: GraphQL queries retrieving vector data complete within acceptable performance boundaries (no more than 20% overhead compared to equivalent non-vector queries for typical embedding dimensions of 1536)
- **SC-005**: Configuration validation catches invalid omit-vector settings and provides actionable error messages before runtime
- **SC-006**: 100% of vector mutations with invalid data (wrong dimension, wrong type) are rejected with clear validation error messages
- **SC-007**: Documentation and samples enable developers to implement vector support without requiring support escalation for common scenarios
- **SC-008**: Vector column support works consistently across GraphQL and REST endpoints with identical behavior for equivalent operations

### Constraints

- Initial implementation targets SQL Server exclusively; other database types are out of scope
- Vector similarity search functions are not included in this feature
- Maximum vector dimension supported defaults to SQL Server's limit (8000) with a configurable option to adjust for specific deployment needs
- Feature requires SQL Server version that supports VECTOR data type (SQL Server 2025 preview or later)
