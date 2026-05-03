using System.Transactions;
using Microsoft.Data.Sqlite;
using Serilog;
using Server.Domain;
using Server.Util;

namespace Server.Repository;

public abstract class GenericRepository<TId, TE> where TE : Entity<TId>
{
    private static readonly ILogger Logger = Log.ForContext(typeof(GenericRepository));
    private SqliteCommand CreateCommand(string sql, SqliteConnection conn)
    {
        var cmd = new SqliteCommand(sql, conn);
        cmd.Transaction = DatabaseConnection.GetActiveTransaction();
        return cmd;
    }
 
    public List<TE> Filter(Filter filter)
    {
        Logger.Debug("Filtering {TableName} with {Filter}", GetTableName(), filter);
        var entities = new List<TE>();
        string sql = $"SELECT * FROM {GetTableName()} {filter.BuildWhere()}";
 
        using var holder = DatabaseConnection.GetConnection();
        using var command = CreateCommand(sql, holder.Connection);
        filter.ApplyParameters(command);
 
        using var reader = command.ExecuteReader();
        while (reader.Read())
            entities.Add(MapResultSetToEntity(reader));
 
        return entities;
    }
 
    public List<TE> GetAll()
    {
        Logger.Debug("Getting all from {TableName}", GetTableName());
        var entities = new List<TE>();
        string sql = $"SELECT * FROM {GetTableName()}";
 
        using var holder = DatabaseConnection.GetConnection();
        using var command = CreateCommand(sql, holder.Connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            entities.Add(MapResultSetToEntity(reader));
 
        return entities;
    }
 
    public TE? FindById(TId id)
    {
        Logger.Debug("Finding {TableName} by id {Id}", GetTableName(), id);
        string sql = $"SELECT * FROM {GetTableName()} WHERE id = @id";
 
        using var holder = DatabaseConnection.GetConnection();
        using var command = CreateCommand(sql, holder.Connection);
        command.Parameters.AddWithValue("@id", id);
 
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapResultSetToEntity(reader) : null;
    }
 
    public bool Remove(TId id)
    {
        Logger.Debug("Removing {TableName} with id {Id}", GetTableName(), id);
        string sql = $"DELETE FROM {GetTableName()} WHERE id = @id";
 
        using var holder = DatabaseConnection.GetConnection();
        using var command = CreateCommand(sql, holder.Connection);
        command.Parameters.AddWithValue("@id", id);
        return command.ExecuteNonQuery() > 0;
    }
 
    public void Add(TE e)
    {
        Logger.Debug("Adding to {TableName}", GetTableName());
        string sql = $"{BuildInsertSql()}; SELECT last_insert_rowid();";
 
        using var holder = DatabaseConnection.GetConnection();
        using var command = CreateCommand(sql, holder.Connection);
        SetInsertParameters(command, e);
 
        var result = command.ExecuteScalar();
        if (result != null)
            e.Id = (TId)Convert.ChangeType(result, typeof(TId));
    }
 
    public bool Update(TE e)
    {
        Logger.Debug("Updating {TableName} with id {Id}", GetTableName(), e.Id);
        using var holder = DatabaseConnection.GetConnection();
        using var command = CreateCommand(BuildUpdateSql(), holder.Connection);
        SetUpdateParameters(command, e);
        return command.ExecuteNonQuery() > 0;
    }
 
    protected abstract string BuildInsertSql();
    protected abstract void SetInsertParameters(SqliteCommand command, TE e);
    protected abstract string BuildUpdateSql();
    protected abstract void SetUpdateParameters(SqliteCommand command, TE e);
    public abstract string GetTableName();
    protected abstract TE MapResultSetToEntity(SqliteDataReader reader);
}