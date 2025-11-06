// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Azure.DataApiBuilder.Service.Tests.SqlTests
{
    /// <summary>
    /// Integration tests for SQL Server VECTOR type support with omit-vector-columns configuration.
    /// Tests verify that tables with VECTOR columns can be accessed through DAB when vectors are omitted.
    /// </summary>
    [TestClass, TestCategory(TestCategory.MSSQL)]
    public class VectorOmissionTests : SqlTestBase
    {
        #region Test Fixture Setup

        /// <summary>
        /// Set the database engine for the tests
        /// </summary>
        [ClassInitialize]
        public static async Task SetupAsync(TestContext context)
        {
            DatabaseEngine = TestCategory.MSSQL;
            await InitializeTestFixture();
        }

        /// <summary>
        /// Runs after every test to reset the database state
        /// </summary>
        [TestCleanup]
        public async Task TestCleanup()
        {
            await ResetDbStateAsync();
        }

        #endregion

        #region T012: GraphQL Read with Omitted Vectors

        /// <summary>
        /// <code>Test: </code> T012 [US1] Query table with omit-vector-columns=true via GraphQL
        /// <code>Do: </code> Query a table that has VECTOR columns configured to omit vectors
        /// <code>Check: </code> Response contains non-vector fields only, vector fields excluded from schema
        /// <code>Spec: </code> FR-005: System excludes VECTOR columns from GraphQL schema when omit-vector-columns is true
        /// </summary>
        [TestMethod]
        public async Task Test_Table_With_Omitted_Vectors_Returns_NonVector_Fields_Via_GraphQL()
        {
            // Arrange
            string graphQLQuery = @"
                query {
                    product_by_pk(id: 1) {
                        id
                        name
                        price
                    }
                }
            ";

            // Act
            // This test should FAIL until implementation is complete
            // Expected: Query succeeds, returns non-vector fields
            // Actual (before implementation): GraphQL schema generation fails or vector fields appear in schema
            Assert.Fail("TEST NOT IMPLEMENTED: Implementation pending for T016-T019");

            // TODO after implementation:
            // JsonElement response = await ExecuteGraphQLRequestAsync(graphQLQuery, "product_by_pk", isAuthenticated: false);
            // Assert.IsNotNull(response);
            // Assert.IsTrue(response.TryGetProperty("id", out _), "Response should contain id field");
            // Assert.IsTrue(response.TryGetProperty("name", out _), "Response should contain name field");
            // Assert.IsTrue(response.TryGetProperty("price", out _), "Response should contain price field");
            // Assert.IsFalse(response.TryGetProperty("embedding", out _), "Response should NOT contain embedding field when omitted");
        }

        #endregion

        #region T013: REST API Read with Omitted Vectors

        /// <summary>
        /// <code>Test: </code> T013 [US1] Query table with omit-vector-columns=true via REST
        /// <code>Do: </code> GET request to table that has VECTOR columns configured to omit vectors
        /// <code>Check: </code> Response contains non-vector fields only, vector fields excluded
        /// <code>Spec: </code> FR-006: System excludes VECTOR columns from REST responses when omit-vector-columns is true
        /// </summary>
        [TestMethod]
        public async Task Test_Table_With_Omitted_Vectors_Returns_NonVector_Fields_Via_REST()
        {
            // Arrange
            string restPath = "/api/Product/id/1";

            // Act
            // This test should FAIL until implementation is complete
            // Expected: GET succeeds, returns non-vector fields only
            // Actual (before implementation): REST serialization fails or vector fields appear in response
            Assert.Fail("TEST NOT IMPLEMENTED: Implementation pending for T016-T020");

            // TODO after implementation:
            // JsonElement response = await ExecuteRestApiRequestAsync(restPath, HttpMethod.Get, isAuthenticated: false);
            // Assert.IsNotNull(response);
            // Assert.IsTrue(response.TryGetProperty("id", out _), "Response should contain id field");
            // Assert.IsTrue(response.TryGetProperty("name", out _), "Response should contain name field");
            // Assert.IsTrue(response.TryGetProperty("price", out _), "Response should contain price field");
            // Assert.IsFalse(response.TryGetProperty("embedding", out _), "Response should NOT contain embedding field when omitted");
        }

        #endregion

        #region T014: Mutations with Omitted Vectors

        /// <summary>
        /// <code>Test: </code> T014 [US1] Mutation on non-vector fields succeeds with omit-vector-columns=true
        /// <code>Do: </code> Insert/Update mutations on table with omitted vectors, only modifying non-vector fields
        /// <code>Check: </code> Mutation succeeds, non-vector fields updated correctly, vector columns untouched in DB
        /// <code>Spec: </code> FR-007: Users can perform mutations on non-VECTOR fields when omit-vector-columns is true
        /// </summary>
        [TestMethod]
        public async Task Test_Mutation_On_NonVector_Fields_Succeeds_With_Omitted_Vectors()
        {
            // Arrange
            string graphQLMutation = @"
                mutation {
                    createProduct(item: {
                        name: ""Test Product""
                        price: 99.99
                    }) {
                        id
                        name
                        price
                    }
                }
            ";

            // Act
            // This test should FAIL until implementation is complete
            // Expected: Mutation succeeds, inserts row with NULL embedding
            // Actual (before implementation): Mutation fails due to vector column presence
            Assert.Fail("TEST NOT IMPLEMENTED: Implementation pending for T016-T022");

            // TODO after implementation:
            // JsonElement response = await ExecuteGraphQLRequestAsync(graphQLMutation, "createProduct", isAuthenticated: false);
            // Assert.IsNotNull(response);
            // Assert.IsTrue(response.TryGetProperty("id", out _), "Response should contain created id");
            // Assert.AreEqual("Test Product", response.GetProperty("name").GetString());
            // Assert.AreEqual(99.99, response.GetProperty("price").GetDouble());
            // Assert.IsFalse(response.TryGetProperty("embedding", out _), "Response should NOT contain embedding field");
            //
            // // Verify DB state: embedding column should be NULL
            // string verifyQuery = $"SELECT embedding FROM Product WHERE id = {response.GetProperty(\"id\").GetInt32()}";
            // // Assert embedding is NULL in database
        }

        #endregion

        #region T015: Error Handling Without Omit Configuration

        /// <summary>
        /// <code>Test: </code> T015 [US1] Table with vectors but omit-vector-columns=false returns error
        /// <code>Do: </code> Attempt to query table with VECTOR columns when omit-vector-columns is false (default)
        /// <code>Check: </code> Clear error message indicating VECTOR type not supported without omit configuration
        /// <code>Spec: </code> FR-008: System provides clear error when VECTOR columns encountered without omit configuration
        /// </summary>
        [TestMethod]
        public async Task Test_Table_Without_Omit_Config_Returns_Error_When_Vectors_Present()
        {
            // Arrange
            // Table with VECTOR columns but omit-vector-columns NOT configured (defaults to false)

            // Act
            // This test should FAIL until implementation is complete
            // Expected: Startup or query fails with clear error message about vector support
            // Actual (before implementation): Generic error or incorrect behavior
            Assert.Fail("TEST NOT IMPLEMENTED: Implementation pending for T021-T022");

            // TODO after implementation:
            // Expected error patterns (either at startup or query time):
            // "VECTOR columns detected in table 'Product'. Set 'omit-vector-columns: true' to exclude vectors from API."
            // OR
            // "Unsupported column type 'vector(1536)' in table 'Product'. Configure 'omit-vector-columns' or upgrade SQL Server."
            //
            // Verify error message contains:
            // - Table name
            // - Column name or "VECTOR" type reference
            // - Configuration guidance (omit-vector-columns)
        }

        #endregion

        #region Additional Test Cases (Future)

        // TODO T011: Add more test cases after base implementation:
        // - Multiple vector columns in same table (all omitted)
        // - Table with both nullable and non-nullable vector columns
        // - Relationships between tables with omitted vectors
        // - Pagination with omitted vectors
        // - Filtering/ordering on non-vector fields with vectors omitted
        // - Configuration validation errors (invalid boolean value)

        #endregion
    }
}
