// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Azure.DataApiBuilder.Core.Services.MetadataProviders;

/// <summary>
/// Utility class for working with SQL Server VECTOR data types.
/// Provides dimension extraction and type detection for VECTOR(N) columns.
/// </summary>
public static class SqlVectorTypeHelper
{
    /// <summary>
    /// Regular expression to match VECTOR type declarations and extract dimensions.
    /// Matches: "vector(1536)", "VECTOR(768)", "vector ( 3072 )" etc.
    /// Captures the dimension value (N) from "vector(N)".
    /// </summary>
    private static readonly Regex VectorDimensionRegex = new(@"vector\s*\(\s*(\d+)\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Determines if a SQL type name represents a VECTOR type.
    /// </summary>
    /// <param name="sqlTypeName">The SQL type name (e.g., "vector(1536)", "nvarchar", "int")</param>
    /// <returns>True if the type is a VECTOR type, false otherwise</returns>
    public static bool IsVectorType(string sqlTypeName)
    {
        if (string.IsNullOrWhiteSpace(sqlTypeName))
        {
            return false;
        }

        return VectorDimensionRegex.IsMatch(sqlTypeName);
    }

    /// <summary>
    /// Extracts the dimension value from a VECTOR type declaration.
    /// </summary>
    /// <param name="sqlTypeName">The SQL type name (e.g., "vector(1536)", "VECTOR(768)")</param>
    /// <returns>
    /// The dimension value if the type is a valid VECTOR type (e.g., 1536 for "vector(1536)"),
    /// or null if the type is not a VECTOR type or the dimension cannot be parsed.
    /// </returns>
    /// <example>
    /// <code>
    /// int? dim = SqlVectorTypeHelper.ExtractVectorDimension("vector(1536)");
    /// // dim == 1536
    /// 
    /// int? dim2 = SqlVectorTypeHelper.ExtractVectorDimension("nvarchar(50)");
    /// // dim2 == null
    /// </code>
    /// </example>
    public static int? ExtractVectorDimension(string sqlTypeName)
    {
        if (string.IsNullOrWhiteSpace(sqlTypeName))
        {
            return null;
        }

        Match match = VectorDimensionRegex.Match(sqlTypeName);
        if (match.Success && match.Groups.Count > 1)
        {
            string dimensionStr = match.Groups[1].Value;
            if (int.TryParse(dimensionStr, out int dimension))
            {
                return dimension;
            }
        }

        return null;
    }

    /// <summary>
    /// Validates that a vector dimension is within SQL Server's supported range.
    /// SQL Server 2025 supports VECTOR dimensions from 1 to 8000.
    /// </summary>
    /// <param name="dimension">The dimension value to validate</param>
    /// <returns>True if the dimension is valid, false otherwise</returns>
    public static bool IsValidVectorDimension(int dimension)
    {
        return dimension >= 1 && dimension <= 8000;
    }

    /// <summary>
    /// Validates a vector array for insertion/update operations.
    /// Checks dimension match and valid float values.
    /// </summary>
    /// <param name="vectorArray">The vector array to validate</param>
    /// <param name="expectedDimension">The expected dimension from column metadata</param>
    /// <param name="errorMessage">Output error message if validation fails</param>
    /// <returns>True if validation passes, false otherwise</returns>
    public static bool ValidateVectorArray(float[]? vectorArray, int? expectedDimension, out string? errorMessage)
    {
        errorMessage = null;

        // NULL vectors are valid if column is nullable
        if (vectorArray == null)
        {
            return true;
        }

        // Check dimension match
        if (expectedDimension.HasValue && vectorArray.Length != expectedDimension.Value)
        {
            errorMessage = $"Vector dimension mismatch: expected {expectedDimension.Value} but received {vectorArray.Length}";
            return false;
        }

        // Check for invalid float values (NaN, Infinity not supported by SQL Server VECTOR)
        for (int i = 0; i < vectorArray.Length; i++)
        {
            if (float.IsNaN(vectorArray[i]) || float.IsInfinity(vectorArray[i]))
            {
                errorMessage = $"Vector contains invalid float value at index {i}: {vectorArray[i]}";
                return false;
            }
        }

        return true;
    }
}
