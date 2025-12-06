using SQLite;
using LocationTracker.Models;
using System.Collections.ObjectModel;

namespace LocationTracker.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
    }

    private async Task Init()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<LocationPoint>();
    }

    public async Task SaveLocationAsync(LocationPoint location)
    {
        await Init();
        await _database!.InsertAsync(location);
    }

    public async Task<List<LocationPoint>> GetAllLocationsAsync()
    {
        await Init();
        return await _database!.Table<LocationPoint>().ToListAsync();
    }

    public async Task DeleteAllLocationsAsync()
    {
        await Init();
        await _database!.DeleteAllAsync<LocationPoint>();
    }

    public async Task<int> GetLocationCountAsync()
    {
        await Init();
        return await _database!.Table<LocationPoint>().CountAsync();
    }
}

