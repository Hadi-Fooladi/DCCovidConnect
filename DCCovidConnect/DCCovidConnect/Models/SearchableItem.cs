using SQLite;

namespace DCCovidConnect.Models
{
    public class SearchableItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string BreadCrumbs { get; set; }
        public string Path { get; set; }
        public int Priority { get; set; }
    }
}