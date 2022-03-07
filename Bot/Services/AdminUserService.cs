using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Bot.Models;
using Bot.Data;

namespace Bot.Services
{
    public class AdminUserService
    {
        private readonly Entities.ApplicationContext _context;
        private readonly IMapper _mapper;

        public AdminUserService(Entities.ApplicationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public bool IsUserAdmin(AdminUser admin) 
        {
            return admin.Role is UserRole.Admin or UserRole.Developer or UserRole.Owner;
        }

        public bool DoesUserExist(string userName) 
        {
            return _context.AdminUsers.Any(admin => admin.UserName == userName);
        }

        public bool DoesUserExist(long adminChatId)
        {
            return _context.AdminUsers.Any(admin => admin.AdminChatId == adminChatId);
        }

        public bool DoesUserNeedUpdate(string userName, AdminUser admin) 
        {
            return admin.UserName != userName;
        }

        public async Task FinishUserRegistration(AdminUser admin, long adminChatId) 
        {
            admin.AdminChatId = adminChatId;

            _context.AdminUsers.Update(_mapper.Map<Entities.AdminUser>(admin));

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(string userName, AdminUser admin) 
        {
            admin.UserName = userName;

            _context.AdminUsers.Update(_mapper.Map<Entities.AdminUser>(admin));

            await _context.SaveChangesAsync();
        }

        public async Task<AdminUser> GetAdminUser(string userName) 
        {
            return _mapper.Map<AdminUser>(await _context.AdminUsers
                .AsNoTracking()
                .FirstAsync(x => x.UserName == userName));
        }

        public async Task<AdminUser> GetAdminUser(long adminChatId)
        {
            return _mapper.Map<AdminUser>(await _context.AdminUsers
                .AsNoTracking()
                .FirstAsync(x => x.AdminChatId == adminChatId));
        }

        public async Task AddAdminUser(string userName, UserRole role) 
        {
            await _context.AdminUsers.AddAsync(new Entities.AdminUser
            {
                UserName = userName,
                Role = role
            });

            await _context.SaveChangesAsync();
        }
    }
}
