using Server.Domain;
using Server.Repository;

namespace Server.Service;


public class UserService : AbstractService<long, User>
{
    private const int BcryptRounds = 12;
    private readonly OfficeService _officeService;
    public User? LoggedInUser { get; private set; } = null;

    public UserService(UserRepository repository, OfficeService officeService) : base(repository)
    {
        _officeService = officeService;
    }

    public static string HashPassword(string plainPassword)
    {
        return BC.HashPassword(plainPassword, BcryptRounds);
    }

    public static bool CheckPassword(string plainPassword, string hashed)
    {
        try
        {
            return BC.Verify(plainPassword, hashed);
        }
        catch
        {
            return false;
        }
    }

    public User Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Username and password are required.");
        }

        var f = new Filter();
        f.AddFilter("username", username);
        
        var matches = Repository.Filter(f);
        if (!matches.Any())
        {
            throw new UnauthorizedAccessException("Incorrect credentials.");
        }

        var user = matches[0];
        string stored = user.Password;
        bool isValid;

        if (stored.StartsWith("$2"))
        {
            isValid = CheckPassword(password, stored);
        }
        else
        {
            isValid = stored == password;
            if (isValid)
            {
                user.Password = HashPassword(password);
                Repository.Update(user);
            }
        }

        if (!isValid)
        {
            throw new UnauthorizedAccessException("Incorrect credentials.");
        }
        var office = _officeService.FindById(user.OfficeId);
        user.OfficeName = office?.Address ?? "Unknown Office";

        LoggedInUser = user;
        return user;
    }

    public void Logout()
    {
        LoggedInUser = null;
    }
}