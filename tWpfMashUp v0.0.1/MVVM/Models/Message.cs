using System;

namespace tWpfMashUp_v0._0._1.MVVM.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}