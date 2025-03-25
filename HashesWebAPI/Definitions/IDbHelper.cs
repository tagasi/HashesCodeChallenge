public interface IDbHelper {
    Task<string> ExecuteStoredProcedureAsync(string procedureName, Dictionary<string, object>? parameters);
}