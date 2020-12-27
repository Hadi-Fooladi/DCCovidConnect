using DCCovidConnect.Models;
using SQLite;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using DCCovidConnect.Services;
using static DCCovidConnect.Models.InfoItem;

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

        public static Dictionary<string, Task> DataTasks = new Dictionary<string, Task>();
        public InfoDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(InfoItem).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(InfoItem)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }
        public Task<List<InfoItem>> GetInfoItemsAsync()
        {
            return Database.Table<InfoItem>().ToListAsync();
        }

        public Task<List<InfoItem>> GetInfoItemsAsync(InfoType type)
        {
            return Database.Table<InfoItem>().Where(i => i.Type == type).ToListAsync();
        }

        public Task<InfoItem> GetInfoItemAsync(int id)
        {
            return Database.Table<InfoItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<InfoItem> GetInfoItemAsync(string title)
        {
            return Database.Table<InfoItem>().Where(x => x.Title.ToLower().Contains(title.ToLower())).FirstOrDefaultAsync();
        }

        public Task<int> SaveInfoItemAsync(InfoItem item)
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

        public Task<int> DeleteInfoItemAsync(InfoItem item)
        {
            return Database.DeleteAsync(item);
        }

        public async Task UpdateDatabase()
        {
            try
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();

                builder.Server = "MYSQL5044.site4now.net";
                builder.Database = "db_a65a18_covid";
                builder.UserID = "a65a18_covid";
                builder.Password = "NLGB6e337kSC2zA";
                using (MySqlConnection connection = new MySqlConnection(builder.ToString()))
                {
                    connection.Open();
                    string query = @"SELECT post_title, MAX(post_date_gmt) AS post_date, post_content
                                        FROM db_a65a18_covid.covidco_wp_posts
                                        WHERE NULLIF(post_content, '') IS NOT NULL AND NULLIF(post_title, '') IS NOT NULL AND post_title LIKE '%-%'
                                        GROUP BY post_title
                                        ORDER BY post_title ASC";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string title = reader.GetString(0);
                                InfoType type = InfoType.NONE;

                                foreach (InfoType t in Enum.GetValues(typeof(InfoType)))
                                {
                                    if (title.Contains(t.ToString().Replace('_', ' '), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        type = t;
                                        break;
                                    }
                                }

                                if (type != InfoType.NONE)
                                {

                                    string date = reader.GetDateTime(1).ToString();
                                    string content = reader.GetString(2);

                                    InfoItem saved = await GetInfoItemAsync(title);
                                    (saved ??= new InfoItem { Title = title }).Date = date;
                                    saved.Type = type;
                                    DataTasks.Add(title, Task.Run(() =>
                                    {
                                        UpdateItem(saved, content, type);
                                    }));

                                    await SaveInfoItemAsync(saved);
                                }
                            }
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
        private async void UpdateItem(InfoItem saved, string content, InfoType type)
        {
            Parser parser = new Parser(content);
            parser.Parse();
            saved.Content = parser.Output;
            await SaveInfoItemAsync(saved);
        }
    }
}
