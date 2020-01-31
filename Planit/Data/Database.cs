using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using Planit.Models;
using System.Threading.Tasks;

namespace Planit.Data
{
    public class Database
    {
        readonly SQLiteAsyncConnection _database;

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Event>().Wait();
            _database.CreateTableAsync<Models.Task>().Wait();
            _database.CreateTableAsync<PlannedTask>().Wait();
        }

        public Task<List<Event>> GetEventsAsync()
        {
            return _database.Table<Event>().ToListAsync();
        }

        public Task<List<Models.Task>> GetTasksAsync()
        {
            return _database.Table<Models.Task>().ToListAsync();
        }

        public Task<List<PlannedTask>> GetPlannedAsync()
        {
            return _database.Table<PlannedTask>().ToListAsync();
        }

        public Task<Event> GetEventAsync(int id)
        {
            return _database.Table<Event>()
                            .Where(i => i.ID == id)
                            .FirstOrDefaultAsync();
        }

        public Task<Models.Task> GetTaskAsync(int id)
        {
            return _database.Table<Models.Task>()
                            .Where(i => i.ID == id)
                            .FirstOrDefaultAsync();
        }

        public Task<PlannedTask> GetPlannedAsync(int id)
        {
            return _database.Table<PlannedTask>()
                            .Where(i => i.ID == id)
                            .FirstOrDefaultAsync();
        }

        public Task<int> SaveEventAsync(Event eventToSave)
        {
            if(eventToSave.ID != 0)
            {
                return _database.UpdateAsync(eventToSave);
            }
            else
            {
                return _database.InsertAsync(eventToSave);
            }
        }

        public Task<int> SaveTaskAsync(Models.Task taskToSave)
        {
            if (taskToSave.ID != 0)
            {
                return _database.UpdateAsync(taskToSave);
            }
            else
            {
                return _database.InsertAsync(taskToSave);
            }
        }

        public Task<int> SavePlannedAsync(PlannedTask plannedToSave)
        {
            if (plannedToSave.ID != 0)
            {
                return _database.UpdateAsync(plannedToSave);
            }
            else
            {
                return _database.InsertAsync(plannedToSave);
            }
        }

        public Task<int> DeleteEventAsync(Event eventToDelete)
        {
            return _database.DeleteAsync(eventToDelete);
        }

        public Task<int> DeleteTaskAsync(Models.Task eventToDelete)
        {
            return _database.DeleteAsync(eventToDelete);
        }

        public Task<int> DeletePlannedAsync(PlannedTask plannedToDelete)
        {
            return _database.DeleteAsync(plannedToDelete);
        }

        public Task<int> DeleteAllPlannedAsync(bool deleteUserModified)
        {
            if (deleteUserModified)
            {
                return _database.DeleteAllAsync<PlannedTask>();
            }
            else
            {
                return _database.Table<PlannedTask>()
                                .Where(i => i.UserModified == false)
                                .DeleteAsync();
            }
            
        }
    }
}
