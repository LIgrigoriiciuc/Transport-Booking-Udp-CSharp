using Microsoft.Data.Sqlite;

namespace Server.Service;


public class TransactionManager
{
    private readonly SqliteConnection _connection;
    public SqliteTransaction? Current { get; private set; }

    public TransactionManager(SqliteConnection connection)
    {
        _connection = connection;
    }

    public void Run(Action work)
    {
        using var transaction = _connection.BeginTransaction();
        Current = transaction;
        try
        {
            work();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception($"Transaction failed: {ex.Message}", ex);
        }
        finally { Current = null; }
    }

    public T RunWithResult<T>(Func<T> task)
    {
        using var transaction = _connection.BeginTransaction();
        Current = transaction;
        try
        {
            T result = task();
            transaction.Commit();
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception($"Transaction failed: {ex.Message}", ex);
        }
        finally { Current = null; }
    }
}