namespace TG_Bot_Microcontrollers.UserManager
{
    public class User
    {
        public long Id { get; set; }

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Username { get; set; } = "";

        public DateTime RegisteredAt { get; set; }

        public bool IsApproved { get; set; }
    }
}
