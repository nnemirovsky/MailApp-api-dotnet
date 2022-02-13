using System.Text;
using MailApp.Filters;
using MailApp.Helpers;
using MailApp.Models;
using Npgsql;
using NpgsqlTypes;

namespace MailApp.Services;

public class MailDataService : IMailDataService
{
    private const NpgsqlDbType ArrayNpgsqlType = NpgsqlDbType.Array | NpgsqlDbType.Text;
    private ILogger<MailDataService> _logger;

    public MailDataService(string dbConnectionString, ILogger<MailDataService> logger)
    {
        SqlHelper.DbConnectionString = dbConnectionString;
        _logger = logger;
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
        var rows = SqlHelper.GetData(command).Tables[0].Rows;
        var letters = new List<Letter>();
        if (rows.Count == 0)
        {
            command.CommandText = "SELECT COUNT(*) FROM letters " + query;
            return (letters, SqlHelper.GetValue<long>(command));
        }

        letters.Capacity = rows.Count;
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            letters.Add(new Letter
            {
                Id = (int) row["id"],
                Subject = row["subject"].ToString()!,
                CreatedAt = (DateTime) row["created_at"],
                Recipients = row["recipients"].ToString()!.Split(','),
                Sender = row["sender"].ToString()!,
                Tags = row["tags"].ToString()!.Split(','),
                Body = row["body"].ToString()!
            });
        }

        return (letters, (long) rows[0]["full_count"]);
    }

    public IndexLetterDto GetLetterById(int id)
    {
        var query = @$"SELECT subject, created_at, array_to_string(recipients, ',') AS recipients, sender, 
            array_to_string(tags, ',') AS tags, body FROM letters WHERE id = {id}";
        var command = new NpgsqlCommand(query);
        var dataSet = SqlHelper.GetData(command);
        if (dataSet.Tables[0].Rows.Count == 0)
            throw new KeyNotFoundException();
        var record = dataSet.Tables[0].Rows[0];
        var letter = new IndexLetterDto
        {
            Subject = record["subject"].ToString()!,
            CreatedAt = (DateTime) record["created_at"],
            Recipients = record["recipients"].ToString()!.Split(','),
            Sender = record["sender"].ToString()!,
            Tags = record["tags"].ToString()!.Split(','),
            Body = record["body"].ToString()!
        };
        return letter;
    }

    public int CreateLetter(CreateLetterDto letter)
    {
        const string query = @"INSERT INTO letters (subject, recipients, sender, tags, body) 
                                VALUES(@subject, @recipients, @sender, @tags, @body) RETURNING id";
        var command = new NpgsqlCommand(query)
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
        return SqlHelper.GetValue<int>(command);
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

        string query = @$"UPDATE letters SET subject = @subject, recipients = @recipients, sender = @sender, 
                            tags = @tags, created_at = @created_at, body = @body WHERE id = {id}";
        var command = new NpgsqlCommand(query)
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
        SqlHelper.Execute(command);
        return letter;
    }

    public void RemoveLetter(int id)
    {
        if (SqlHelper.Execute(new NpgsqlCommand($"DELETE FROM letters WHERE id = {id}")) == -1)
            throw new KeyNotFoundException();
    }
}
