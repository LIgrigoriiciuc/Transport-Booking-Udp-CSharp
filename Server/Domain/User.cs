namespace Server.Domain;

public class User : Entity<long>
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public long OfficeId { get; set; }

    public User(long id, string username, string password, string fullName, long officeId) 
        : base(id)
    {
        Username = username;
        Password = password;
        FullName = fullName;
        OfficeId = officeId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || GetType() != obj.GetType()) return false;
        if (!base.Equals(obj)) return false;

        var user = (User)obj;
        return Username == user.Username && 
               Password == user.Password && 
               FullName == user.FullName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Username, Password, FullName);
    }
}