using DCCovidConnect.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace DCCovidConnect.Data
{
    public class InfoDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public InfoDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(InfoList).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(InfoList)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }
        public Task<List<InfoList>> GetInfoListsAsync()
        {
            return Database.Table<InfoList>().ToListAsync();
        }

        public Task<InfoList> GetInfoListAsync(int id)
        {
            return Database.Table<InfoList>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<InfoList> GetInfoListAsync(string title)
        {
            return Database.Table<InfoList>().Where(x => x.Title.ToLower().Contains(title.ToLower())).FirstOrDefaultAsync();
        }

        public Task<int> SaveInfoListAsync(InfoList item)
        {
            if (item.ID != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteInfoListAsync(InfoList item)
        {
            return Database.DeleteAsync(item);
        }
    }
}
