namespace AutoExpense.Android.Models
{
    public class SMS
    {
        public SMS(string id, string address, long date, string body, string threadId, string person, string type)
        {
            Id = id;
            Address = address;
            Date = date;
            Body = body;
            ThreadId = threadId;
            Person = person;
            Type = type;
        }

        public string Id { get; set; }
        public string Address { get; set; }
        public long Date { get; set; }
        public string Body { get; set; }
        public string ThreadId { get; set; }
        public string Person { get; set; }
        public string Type { get; set; }

    }
}