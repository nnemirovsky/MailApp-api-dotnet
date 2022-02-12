using System.Data;
using Npgsql;

namespace MailApp.Helpers;

public class SqlHelper
{
    public static string DbConnectionString = string.Empty;

    private static NpgsqlConnection OpenConnection()
    {
        if (DbConnectionString == string.Empty)
            throw new ArgumentNullException("DbConnectionString must be initialize before open connection");
        var connection = new NpgsqlConnection(DbConnectionString);
        connection.Open();
        return connection;
    }

    public static TResult? GetValue<TResult>(NpgsqlCommand command)
    {
        var connection = OpenConnection();
        command.Connection = connection;
        var value = (TResult?) command.ExecuteScalar();
        connection.Close();
        return value;
    }

    public static DataSet GetData(NpgsqlCommand command)
    {
        var connection = OpenConnection();
        command.Connection = connection;
        var adapter = new NpgsqlDataAdapter(command);
        var dataSet = new DataSet();
        adapter.Fill(dataSet);
        return dataSet;
    }

    public static int Execute(NpgsqlCommand command)
    {
        var connection = OpenConnection();
        command.Connection = connection;
        return command.ExecuteNonQuery();
    }
}
