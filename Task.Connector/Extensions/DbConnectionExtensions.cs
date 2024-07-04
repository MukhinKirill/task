using System.Data;
using System.Data.Common;
using Task.Connector.Domain;

namespace Task.Connector.Extensions;

internal static class DbConnectionExtensions
{
    internal static void ExecuteCommand(this IDbConnection connection, params Instruction[] commands)
    {
        connection.Open();

        using var transaction = connection.BeginTransaction();
        using var dbCommand = connection.CreateCommand();

        dbCommand.Transaction = transaction;

        try
        {
            foreach (var command in commands)
            {
                dbCommand.CommandText = command.Text;
                if (command.Parameters.Any() is true)
                    foreach (var parameter in command.Parameters)
                    {
                        var parm = dbCommand.CreateParameter();

                        parm.ParameterName = parameter.Key;
                        parm.Value = parameter.Value;

                        dbCommand.Parameters.Add(parm);
                    }

                dbCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    internal static TResult ExecuteQuery<TResult>(this IDbConnection connection, Instruction query, Func<DbDataReader, TResult> mapper)
    {
        connection.Open();

        using var dbCommand = connection.CreateCommand();

        try
        {
            dbCommand.CommandText = query.Text;
            if (query.Parameters.Any() is true)
                foreach (var parameter in query.Parameters)
                {
                    var parm = dbCommand.CreateParameter();

                    parm.ParameterName = parameter.Key;
                    parm.Value = parameter.Value;

                    dbCommand.Parameters.Add(parm);
                }

            using var reader = dbCommand.ExecuteReader();

            return mapper((DbDataReader)reader);
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}