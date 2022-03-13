using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Bot.Models;
using Bot.Data;

namespace Bot.Services
{
    public class AdminUserService
    {
        private readonly SynchronizedCollection<long> _usersAddingNewUser;
        private readonly SynchronizedCollection<long> _usersSettingAdminUser;
        private readonly SynchronizedCollection<long> _usersRemovingAdminRole;

        private readonly Entities.ApplicationContext _context;
        private readonly IMapper _mapper;

        public AdminUserService(Entities.ApplicationContext context, IMapper mapper)
        {
            _usersAddingNewUser = new SynchronizedCollection<long>();
            _usersSettingAdminUser = new SynchronizedCollection<long>();
            _usersRemovingAdminRole = new SynchronizedCollection<long>();

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
            return _usersAddingNewUser.Contains(userChatId);
        }

        public bool DoesUserStartSettingAdminUser(long userChatId) 
        {
            return _usersSettingAdminUser.Contains(userChatId);
        }

        public void StartAddingUser(long userAddingChatId) 
        {
            _usersAddingNewUser.Add(userAddingChatId);
        }

        public void BreakAddingUser(long userAddingChatId) 
        {
            _usersAddingNewUser.Remove(userAddingChatId);
        }

        public async Task AddUser(string userName, long userAddingChatId)
        {
            await _context.AdminUsers.AddAsync(new Entities.AdminUser
            {
                UserName = userName
            });

            _usersAddingNewUser.Remove(userAddingChatId);

            await _context.SaveChangesAsync();
        }

        public void StartSettingAdminUser(long userSettingAdminUserChatId) 
        {
            _usersSettingAdminUser.Add(userSettingAdminUserChatId);
        }

        public void BreakSettingAdminUser(long userSettingAdminUserChatId) 
        {
            _usersSettingAdminUser.Remove(userSettingAdminUserChatId);
        }

        public void StartRemovingAdminRole(long userRemovingAdminRoleChatId) 
        {
            _usersRemovingAdminRole.Add(userRemovingAdminRoleChatId);
        }

        public void BreakRemovingAdminRole(long userRemovingAdminRoleChatId)
        {
            _usersRemovingAdminRole.Remove(userRemovingAdminRoleChatId);
        }

        public bool DoesUserStartRemovingAdminRole(long userRemovingAdminRoleChatId) 
        {
            return _usersRemovingAdminRole.Contains(userRemovingAdminRoleChatId);
        }

        public async Task SetRole(string uesrName, UserRole role) 
        {
            Entities.AdminUser adminUser = await _context.AdminUsers
                .FirstAsync(x => x.UserName == uesrName);

            adminUser.Role = role;

            await _context.SaveChangesAsync();
        }

        public IList<AdminUser> GetAdminByRole(UserRole role) 
        {
            return _mapper.Map<IList<AdminUser>>(_context.AdminUsers
                .AsNoTracking()
                .Where(x => x.Role == role)
                .ToList());
        }

        public bool HaveAnyNoAdminUsers() 
        {
            return _context.AdminUsers
                .AsNoTracking()
                .Any(x => x.Role == UserRole.Default);
        }

        public bool HaveAnyAdminUsers() 
        {
            return _context.AdminUsers
                .AsNoTracking()
                .Any(x => x.Role == UserRole.Admin);
        }
    }
}
