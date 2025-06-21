using DormitoryManagement.Data;
using DormitoryManagement.Services;
using HostelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HostelManagement.Services
{
    internal class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DormitoryService> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<DormitoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            try
            {
                return await _context.Users
                    .AnyAsync(u => u.Username == username && u.Password == password);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Register(User user)
        {
            if (await UsernameExists(user.Username))
                return false;

            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExists(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }
    }
}
