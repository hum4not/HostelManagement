using HostelManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostelManagement.Services
{
    public interface IUserRepository
    {
        Task<bool> Authenticate(string username, string password);
        Task<bool> Register(User user);
        Task<User> GetUserByUsername(string username);
        Task<bool> UsernameExists(string username);
    }
}
