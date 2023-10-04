using SQLite;

namespace TikTalk.Models
{
    [Table("people")]
    public class Person
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public DateTime scheduledNotification { get; set; }
    }
}
