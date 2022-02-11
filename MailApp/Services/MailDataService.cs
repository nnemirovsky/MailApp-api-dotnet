using System.Text;
using MailApp.Filters;
using MailApp.Models;
using Npgsql;
using NpgsqlTypes;

namespace MailApp.Services;

public class MailDataService: IMailDataService
{
    private readonly string _dbConnectionString;
    private const NpgsqlDbType ArrayNpgsqlType = NpgsqlDbType.Array | NpgsqlDbType.Text;
    
    public MailDataService(string dbConnectionString)
    {
        _dbConnectionString = dbConnectionString;
    }
    
    public (IEnumerable<Letter> letters, long fullCount) GetLetters(PaginationFilter pagination, string? sender,
        string? recipient, string? tag, DateFilter dateRange)
    {
        using var command = new NpgsqlCommand();
        command.CommandText = @"SELECT id, subject, created_at, array_to_string(recipients, ',') AS recipients, 
                        sender, array_to_string(tags, ',') AS tags, body, COUNT(*) OVER() AS full_count FROM letters";
        var query = new StringBuilder();
        if (sender is not null || recipient is not null || tag is not null || dateRange.Start.HasValue ||
            dateRange.End.HasValue)
        {
            query.Append(" WHERE ");
            var hasPrevious = false;

            if (sender is not null)
            {
                hasPrevious = true;
                query.Append("sender = @sender");
                command.Parameters.Add(new NpgsqlParameter("sender", sender));
            }

            if (recipient is not null)
            {
                if (hasPrevious)
                    query.Append(" AND ");
                hasPrevious = true;
                query.Append("@recipient = ANY(recipients)");
                command.Parameters.Add(new NpgsqlParameter("recipient", recipient));
            }

            if (tag is not null)
            {
                if (hasPrevious)
                    query.Append(" AND ");
                hasPrevious = true;
                query.Append("@tag = ANY(tags)");
                command.Parameters.Add(new NpgsqlParameter("tag", tag));
            }

            if (dateRange.Start.HasValue && dateRange.End.HasValue)
            {
                if (hasPrevious)
                    query.Append(" AND ");
                query.Append("created_at BETWEEN @start AND @end");
                command.Parameters.Add(new NpgsqlParameter("start", dateRange.Start.Value));
                command.Parameters.Add(new NpgsqlParameter("end", dateRange.End.Value));
            }
            else if (dateRange.Start.HasValue)
            {
                if (hasPrevious)
                    query.Append(" AND ");
                query.Append("created_at >= @start");
                command.Parameters.Add(new NpgsqlParameter("start", dateRange.Start.Value));
            }
            else if (dateRange.End.HasValue)
            {
                if (hasPrevious)
                    query.Append(" AND ");
                query.Append("created_at <= @end");
                command.Parameters.Add(new NpgsqlParameter("end", dateRange.End.Value));
            }
        }

        var lettersToSkip = (pagination.PageNumber - 1) * pagination.PageSize;
        command.CommandText += query + $" ORDER BY id OFFSET {lettersToSkip} LIMIT {pagination.PageSize}";

        using var connection = new NpgsqlConnection(_dbConnectionString);
        connection.Open();
        command.Connection = connection;

        using var reader = command.ExecuteReader();

        var letters = new List<Letter>();

        if (!reader.Read())
        {
            reader.Close();
            command.CommandText = "SELECT COUNT(*) FROM letters " + query;
            return (letters, (long) (command.ExecuteScalar() ?? 0));
        }

        var fullCount = (long) reader["full_count"];
        do
        {
            letters.Add(new Letter
            {
                Id = (int) reader["id"],
                Subject = reader["subject"].ToString()!,
                CreatedAt = (DateTime) reader["created_at"],
                Recipients = reader["recipients"].ToString()!.Split(','),
                Sender = reader["sender"].ToString()!,
                Tags = reader["tags"].ToString()!.Split(','),
                Body = reader["body"].ToString()!
            });
        } while (reader.Read());

        return (letters, fullCount);
    }
    
    public IndexLetterDto GetLetterById(int id)
    {
        using var connection = new NpgsqlConnection(_dbConnectionString);
        connection.Open();
        var query = @$"SELECT subject, created_at, array_to_string(recipients, ',') AS recipients, sender, 
            array_to_string(tags, ',') AS tags, body FROM letters WHERE id = {id}";
        using var command = new NpgsqlCommand(query, connection);
        using var reader = command.ExecuteReader();
        if (!reader.HasRows)
            throw new KeyNotFoundException();
        reader.Read();
        var letter = new IndexLetterDto
        {
            Subject = reader["subject"].ToString()!,
            CreatedAt = (DateTime) reader["created_at"],
            Recipients = reader["recipients"].ToString()!.Split(','),
            Sender = reader["sender"].ToString()!,
            Tags = reader["tags"].ToString()!.Split(','),
            Body = reader["body"].ToString()!
        };
        return letter;
    }
    
    public int CreateLetter(CreateLetterDto letter)
    {
        using var connection = new NpgsqlConnection(_dbConnectionString);
        connection.Open();
        const string query = @"INSERT INTO letters (subject, recipients, sender, tags, body) 
                                VALUES(@subject, @recipients, @sender, @tags, @body) RETURNING id";
        using var command = new NpgsqlCommand(query, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("subject", letter.Subject),
                new NpgsqlParameter
                {
                    ParameterName = "recipients", Value = letter.Recipients, NpgsqlDbType = ArrayNpgsqlType
                },
                new NpgsqlParameter("sender", letter.Sender),
                new NpgsqlParameter {ParameterName = "tags", Value = letter.Tags, NpgsqlDbType = ArrayNpgsqlType},
                new NpgsqlParameter("body", letter.Body),
            }
        };
        return (int) command.ExecuteScalar()!;
    }

    public IndexLetterDto UpdateLetter(int id, UpdateLetterDto updateLetterParams)
    {
        var letter = GetLetterById(id);
        if (updateLetterParams.Subject is not null)
            letter.Subject = updateLetterParams.Subject;
        if (updateLetterParams.Recipients is not null)
            letter.Recipients = updateLetterParams.Recipients;
        if (updateLetterParams.Sender is not null)
            letter.Sender = updateLetterParams.Sender;
        if (updateLetterParams.Tags is not null)
            letter.Tags = updateLetterParams.Tags;
        if (updateLetterParams.CreatedAt is not null)
            letter.CreatedAt = updateLetterParams.CreatedAt.Value;
        if (updateLetterParams.Body is not null)
            letter.Body = updateLetterParams.Body;

        using var connection = new NpgsqlConnection(_dbConnectionString);
        connection.Open();
        string query = @$"UPDATE letters SET subject = @subject, recipients = @recipients, sender = @sender, 
                            tags = @tags, created_at = @created_at, body = @body WHERE id = {id}";
        using var command = new NpgsqlCommand(query, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("subject", letter.Subject),
                new NpgsqlParameter
                {
                    ParameterName = "recipients", Value = letter.Recipients, NpgsqlDbType = ArrayNpgsqlType
                },
                new NpgsqlParameter("sender", letter.Sender),
                new NpgsqlParameter {ParameterName = "tags", Value = letter.Tags, NpgsqlDbType = ArrayNpgsqlType},
                new NpgsqlParameter("created_at", letter.CreatedAt),
                new NpgsqlParameter("body", letter.Body)
            }
        };
        command.ExecuteNonQuery();
        return letter;
    }
    
    public void RemoveLetter(int id)
    {
        using var connection = new NpgsqlConnection(_dbConnectionString);
        connection.Open();

        string query = @$"DELETE FROM letters WHERE id = {id}";
        using var command = new NpgsqlCommand(query, connection);
        if (command.ExecuteNonQuery() == -1)
            throw new KeyNotFoundException();
    }
}
