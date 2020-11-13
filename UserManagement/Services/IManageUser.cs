using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Dtos;
using UserManagement.Entities;

namespace UserManagement.Services
{
   public interface IManageUser
    {
        User Authenticate(string username, string password);
        IEnumerable<User> Users();
        User GetUserById(int Id);
        CreatedResponseDto Create(User user, string password);
        void Delete(int id);
    }
}
