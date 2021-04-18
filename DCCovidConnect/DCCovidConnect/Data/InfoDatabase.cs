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
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Globalization;
using DCCovidConnect.Views;

namespace DCCovidConnect.Data
{
    /// <summary>
    /// This class handles the database for the application
    /// </summary>
    public class InfoDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public static Dictionary<int, Task> DataTasks = new Dictionary<int, Task>();

        public InfoDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        /// <summary>
        /// This method initializes the database.
        /// </summary>
        /// <returns></returns>
        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(InfoItem).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(InfoItem)).ConfigureAwait(false);
                }

                //if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(USCasesModel).Name))
                //{
                //    await Database.CreateTablesAsync(CreateFlags.None, typeof(USCasesModel)).ConfigureAwait(false);
                //}
                //if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(StateCasesModel).Name))
                //{
                //    await Database.CreateTablesAsync(CreateFlags.None, typeof(StateCasesModel)).ConfigureAwait(false);
                //}
                //if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(CountyCasesModel).Name))
                //{
                //    await Database.CreateTablesAsync(CreateFlags.None, typeof(CountyCasesModel)).ConfigureAwait(false);
                //}
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(VersionInfo).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(VersionInfo)).ConfigureAwait(false);
                }

                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(SearchableItem).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(SearchableItem)).ConfigureAwait(false);
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
            return Database.Table<InfoItem>().Where(x => x.Title.ToLower().Contains(title.ToLower()))
                .FirstOrDefaultAsync();
        }

        public Task<InfoItem> GetInfoItemAsync(InfoItem item) => this.GetInfoItemAsync(item.Title);

        /// <summary>
        /// This method updates the <c>InfoItem</c> in the database or inserts it if its not present.
        /// </summary>
        /// <param name="item">The item to put into the database.</param>
        /// <returns></returns>
        public Task<int> SaveInfoItemAsync(InfoItem item)
        {
#if DEBUG
            if (GetInfoItemAsync(item.ID).Result != null)
            {
                Console.WriteLine($"============UPDATING {item.ID}");
            }
            else
            {
                Console.WriteLine($"============INSERTING {item.ID}");
            }
#endif
            if (GetInfoItemAsync(item.ID).Result != null)
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

        public Task<List<USCasesModel>> GetUSCasesItemsAsync() => Database.Table<USCasesModel>().ToListAsync();
        public Task<List<StateCasesItem>> GetStateCasesItemsAsync() => Database.Table<StateCasesItem>().ToListAsync();

        public Task<StateCasesItem> GetStateCasesItemAsync(String state) =>
            Database.Table<StateCasesItem>().Where(i => i.State == state).FirstOrDefaultAsync();

        public Task<List<CountyCasesItem>> GetCountyCasesItemsAsync() =>
            Database.Table<CountyCasesItem>().ToListAsync();

        public Task<List<CountyCasesItem>> GetCountyCasesItemByStateAsync(String state) => Database
            .Table<CountyCasesItem>().Where(i => i.State.ToLower() == state.ToLower()).ToListAsync();

        public Task<List<CountyCasesItem>> GetCountyCasesItemAsync(String county) => Database.Table<CountyCasesItem>()
            .Where(i => i.County.ToLower() == county.ToLower()).ToListAsync();

        public Task<VersionInfo> GetVersionItemAsync() => Database.Table<VersionInfo>().FirstOrDefaultAsync();

        /// <summary>
        /// This method updates the <c>VersionInfo</c> or inserts it if not present.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <returns></returns>
        public Task<int> SaveVersionItemAsync(VersionInfo item)
        {
            if (Database.Table<VersionInfo>().Where(i => i.ID == item.ID).FirstOrDefaultAsync().Result != null)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        private Task _updateCovidStatsTask;
        private Task _updateInfoTask;

        public Task UpdateCovidStatsTask
        {
            get
            {
                if (_updateCovidStatsTask == null)
                    _updateCovidStatsTask = UpdateCovidStats();
                return _updateCovidStatsTask;
            }
        }

        public Task UpdateInfoTask
        {
            get
            {
                if (_updateInfoTask == null)
                    _updateInfoTask = UpdateInfo();
                return _updateInfoTask;
            }
        }

        /// <summary>
        /// This method is used to update the database.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateDatabase()
        {
            await (_updateCovidStatsTask ??= UpdateCovidStats());
            await (_updateInfoTask ??= UpdateInfo());
            Console.WriteLine("Database Updated!");
        }

        /// <summary>
        /// This method updates the covid case data using the GitHub API and updates the version of the database.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateCovidStats()
        {
            JObject us_cases, state_cases, county_cases;
            us_cases = JObject.Parse(
                await GetCallAPI("https://api.github.com/repos/nytimes/covid-19-data/contents/live/us.csv"));
            state_cases =
                JObject.Parse(
                    await GetCallAPI("https://api.github.com/repos/nytimes/covid-19-data/contents/live/us-states.csv"));
            county_cases =
                JObject.Parse(
                    await GetCallAPI(
                        "https://api.github.com/repos/nytimes/covid-19-data/contents/live/us-counties.csv"));
            // VersionInfo version = new VersionInfo { ID = 0 };
            VersionInfo version = await GetVersionItemAsync() ?? new VersionInfo();

            string date_format = "yyyy-MM-dd";

            // This creates a new table and inputs all the data retieved from the API.
            if (!String.Equals(version.US_CASES_SHA, us_cases["sha"].ToString()))
            {
                version.US_CASES_SHA = us_cases["sha"].ToString();
                await Database.DropTableAsync<USCasesModel>();
                await Database.CreateTablesAsync(CreateFlags.None, typeof(USCasesModel)).ConfigureAwait(false);
                byte[] data = Convert.FromBase64String(us_cases["content"].ToString());
                string[] content = Encoding.UTF8.GetString(data).Split('\n');
                for (int i = 1; i < content.Length; i++)
                {
                    string[] values = content[i].Split(',');
                    await Database.InsertAsync(new USCasesModel
                    {
                        Date = DateTime.ParseExact(values[0], date_format, CultureInfo.InvariantCulture),
                        Cases = String.IsNullOrWhiteSpace(values[1]) ? -1 : Int32.Parse(values[1]),
                        Deaths = String.IsNullOrWhiteSpace(values[2]) ? -1 : Int32.Parse(values[2]),
                        ConfirmedCases = String.IsNullOrWhiteSpace(values[3]) ? -1 : Int32.Parse(values[3]),
                        ConfirmedDeaths = String.IsNullOrWhiteSpace(values[4]) ? -1 : Int32.Parse(values[4]),
                        ProbableCases = String.IsNullOrWhiteSpace(values[5]) ? -1 : Int32.Parse(values[5]),
                        ProbableDeaths = String.IsNullOrWhiteSpace(values[6]) ? -1 : Int32.Parse(values[6])
                    });
                }
            }

            if (!String.Equals(version.STATE_CASES_SHA, state_cases["sha"].ToString()))
            {
                version.STATE_CASES_SHA = state_cases["sha"].ToString();
                await Database.DropTableAsync<StateCasesItem>();
                await Database.CreateTablesAsync(CreateFlags.None, typeof(StateCasesItem)).ConfigureAwait(false);
                byte[] data = Convert.FromBase64String(state_cases["content"].ToString());
                string[] content = Encoding.UTF8.GetString(data).Split('\n');
                for (int i = 1; i < content.Length; i++)
                {
                    string[] values = content[i].Split(',');
                    await Database.InsertAsync(new StateCasesItem
                    {
                        Date = DateTime.ParseExact(values[0], date_format, CultureInfo.InvariantCulture),
                        State = values[1],
                        FIPS = String.IsNullOrWhiteSpace(values[2]) ? -1 : Int32.Parse(values[2]),
                        Cases = String.IsNullOrWhiteSpace(values[3]) ? -1 : Int32.Parse(values[3]),
                        Deaths = String.IsNullOrWhiteSpace(values[4]) ? -1 : Int32.Parse(values[4]),
                        ConfirmedCases = String.IsNullOrWhiteSpace(values[5]) ? -1 : Int32.Parse(values[5]),
                        ConfirmedDeaths = String.IsNullOrWhiteSpace(values[6]) ? -1 : Int32.Parse(values[6]),
                        ProbableCases = String.IsNullOrWhiteSpace(values[7]) ? -1 : Int32.Parse(values[7]),
                        ProbableDeaths = String.IsNullOrWhiteSpace(values[8]) ? -1 : Int32.Parse(values[8])
                    });
                }
            }

            if (!String.Equals(version.COUNTY_CASES_SHA, county_cases["sha"].ToString()))
            {
                version.COUNTY_CASES_SHA = county_cases["sha"].ToString();
                await Database.DropTableAsync<CountyCasesItem>();
                await Database.CreateTablesAsync(CreateFlags.None, typeof(CountyCasesItem)).ConfigureAwait(false);
                byte[] data = Convert.FromBase64String(county_cases["content"].ToString());
                string[] content = Encoding.UTF8.GetString(data).Split('\n');
                for (int i = 1; i < content.Length; i++)
                {
                    string[] values = content[i].Split(',');
                    await Database.InsertAsync(new CountyCasesItem
                    {
                        Date = DateTime.ParseExact(values[0], date_format, CultureInfo.InvariantCulture),
                        County = values[1],
                        State = values[2],
                        FIPS = String.IsNullOrWhiteSpace(values[3]) ? -1 : Int32.Parse(values[3]),
                        Cases = String.IsNullOrWhiteSpace(values[4]) ? -1 : Int32.Parse(values[4]),
                        Deaths = String.IsNullOrWhiteSpace(values[5]) ? -1 : Int32.Parse(values[5]),
                        ConfirmedCases = String.IsNullOrWhiteSpace(values[6]) ? -1 : Int32.Parse(values[6]),
                        ConfirmedDeaths = String.IsNullOrWhiteSpace(values[7]) ? -1 : Int32.Parse(values[7]),
                        ProbableCases = String.IsNullOrWhiteSpace(values[8]) ? -1 : Int32.Parse(values[8]),
                        ProbableDeaths = String.IsNullOrWhiteSpace(values[9]) ? -1 : Int32.Parse(values[9])
                    });
                }
            }

            // Save the version
            await SaveVersionItemAsync(version);
        }

        /// <summary>
        /// This method gets the response json from an API call.
        /// </summary>
        /// <param name="url">Url of the API call.</param>
        /// <returns>Returns a task of the response object.</returns>
        private async Task<string> GetCallAPI(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return await client.GetStringAsync(url);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            return null;
        }

        /// <summary>
        /// This method updates the COVID19 information from a WordPress database and parses the pages into a usuable JSON object.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateInfo()
        {
            bool searchNeedsReindexing = false;
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
                    string query = @"SELECT ID, post_title, MAX(post_date_gmt) AS post_date, post_content
                                        FROM db_a65a18_covid.covidco_wp_posts
                                        WHERE NULLIF(post_content, '') IS NOT NULL AND NULLIF(post_title, '') IS NOT NULL AND (post_title LIKE '%-%' OR post_title = 'FAQs')
                                        GROUP BY post_title
                                        ORDER BY post_title ASC";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string title = reader.GetString(1);
                                InfoType type = InfoType.NONE;

                                /// check if the title is accounted for in the <c>InfoMenuPage</c>
                                foreach (InfoType t in Enum.GetValues(typeof(InfoType)))
                                {
                                    if (title.Contains(t.ToString().Replace('_', ' '),
                                        StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        type = t;
                                        break;
                                    }
                                }

                                // Inserts item into the database if its a valid type.
                                if (type != InfoType.NONE)
                                {
                                    int id = reader.GetInt32(0);
                                    DateTime date = reader.GetDateTime(2);
                                    if ((await GetInfoItemAsync(id))?.Date == date)
                                    {
#if DEBUG
                                        Console.WriteLine($"Item ID: {id} already up to date!");
#endif
                                        DataTasks.Add(id, null);
                                    }
                                    else
                                    {
                                        string content = reader.GetString(3);
                                        InfoItem saved = await GetInfoItemAsync(id);
                                        (saved ??= new InfoItem {ID = id, Title = title}).Date = date;
                                        saved.Type = type;
                                        await SaveInfoItemAsync(saved);
                                        // Parse the HTML source in the background and check the other objects
                                        DataTasks.Add(id, Task.Run(() => { UpdateItem(saved, content, type); }));
                                    }
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

            // Delete Items that aren't present in the main database anymore.
            foreach (InfoItem item in await this.GetInfoItemsAsync())
            {
                if (!DataTasks.ContainsKey(item.ID))
                {
                    // reindex search tree as items are deleted
                    searchNeedsReindexing = true;
                    await this.DeleteInfoItemAsync(item);
                }
            }
        }

        private Task<int> SaveSearchableItemAsync(SearchableItem item)
        {
            if (Database.Table<SearchableItem>().Where(i => i.Name == item.Name).FirstOrDefaultAsync().Result != null)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<List<SearchableItem>> GetSearchableItemsAsync() =>
            Database.Table<SearchableItem>().ToListAsync();

        public Task<List<SearchableItem>> GetSearchableItemsByPathAsync(string route) => Database
            .Table<SearchableItem>().Where(i => i.Path == route).ToListAsync();
        
        public Task<List<SearchableItem>> GetSearchableItemsByNameAsync(string name) => Database
            .Table<SearchableItem>().Where(i => i.Name.Contains(name)).ToListAsync();

        private async void UpdateItem(InfoItem saved, string content, InfoType type)
        {
            Parser parser = new Parser(content);
            parser.Parse();
            saved.Content = parser.Output;
            await SaveSearchableItemAsync(new SearchableItem()
            {
                Path = $"{nameof(InfoDetailPage)}?id={saved.ID}",
                Name = saved.Title,
                Priority = 1,
            });
            foreach (var item in parser.ItemList)
            {
                await SaveSearchableItemAsync(new SearchableItem()
                {
                    Path = $"{nameof(InfoDetailPage)}?id={saved.ID}",
                    Name = item,
                    Priority = 0,
                });
            }

            await SaveInfoItemAsync(saved);
        }
    }
}