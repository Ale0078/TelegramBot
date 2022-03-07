using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Bot.Models;
using Bot.Data;

namespace Bot.Services
{
    public class AdminUserService
    {
        private readonly SynchronizedCollection<long> _uesrsAddingNewUser;

        private readonly Entities.ApplicationContext _context;
        private readonly IMapper _mapper;

        public AdminUserService(Entities.ApplicationContext context, IMapper mapper)
        {
            _uesrsAddingNewUser = new SynchronizedCollection<long>();

            _context = context;
            _mapper = mapper;
        }

        public bool IsUserAdmin(AdminUser admin) 
        {
            return admin.Role is UserRole.Admin or UserRole.Developer or UserRole.Owner;
        }

        public bool DoesUserExist(string userName) 
        {
            return _context.AdminUsers
                .AsNoTracking()
                .Any(admin => admin.UserName == userName);
        }

        public bool DoesUserExist(long adminChatId)
        {
            return _context.AdminUsers
                .AsNoTracking()
                .Any(admin => admin.AdminChatId == adminChatId);
        }

        public bool DoesUserNeedUpdate(string userName, AdminUser admin) 
        {
            return admin.UserName != userName;
        }

        public bool DoesUserNeedFinishRegistration(AdminUser admin) 
        {
            return admin.AdminChatId is 0L;
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

        public bool DoesUserStartAddingUser(long userChatId) 
        {
            return _uesrsAddingNewUser.Contains(userChatId);
        }

        public void StartAddingUser(long userAddingChatId) 
        {
            _uesrsAddingNewUser.Add(userAddingChatId);
        }

        public void BreakAddingUser(long userAddingChatId) 
        {
            _uesrsAddingNewUser.Remove(userAddingChatId);
        }

        public async Task AddUser(string userName, long userAddingChatId)
        {
            await _context.AdminUsers.AddAsync(new Entities.AdminUser
            {
                UserName = userName
            });

            _uesrsAddingNewUser.Remove(userAddingChatId);

            await _context.SaveChangesAsync();
        }
    }
}
