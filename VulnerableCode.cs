using System;
using System.Data.SqlClient;

public class SecureCode
{
    // ✅ Param reques
    public string GetUserData(string userId)
    {
        string query = "SELECT * FROM Users WHERE Id = @UserId";
        return ExecuteQuery(query, userId);
    }

    // ✅ Secret from environment variable
    private string _apiKey => Environment.GetEnvironmentVariable("API_KEY");
    
    // ✅ Command check before exec
    public void ExecuteUserCommand(string command)
    {
        var allowedCommands = new[] { "dir", "list", "check" };
        if (!Array.Exists(allowedCommands, element => element == command))
            throw new InvalidOperationException("Command not allowed");
        
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/c {command}";
        process.Start();
    }

    private string ExecuteQuery(string query, params object[] parameters)
    {
        using (SqlConnection conn = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION")))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", parameters[0]);
            return cmd.ExecuteScalar().ToString();
        }
    }
}