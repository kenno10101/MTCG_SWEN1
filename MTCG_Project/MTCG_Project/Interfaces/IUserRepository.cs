using MTCG_Project.Models.User;

namespace MTCG_Project.Interfaces;

public interface IUserRepository
{
    Task Create(User user);
    Task<User> Get (string username);
    Task Update (User user, string username);
}
