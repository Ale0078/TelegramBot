using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using Bot.Models;
using Bot.Data;
using Bot.Datas;

namespace Bot.Services
{
    public class AdminUserService
    {
        private readonly ConcurrentDictionary<long, ExecutingCommand> _executers;

        private readonly Entities.ApplicationContext _context;
        private readonly IMapper _mapper;

        public AdminUserService(Entities.ApplicationContext context, IMapper mapper)
        {
            _executers = new ConcurrentDictionary<long, ExecutingCommand>();

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

        public void StartExecutingCommand(long userChatId, ExecutingCommand command) 
        {
            _executers.TryAdd(userChatId, command);
        }

        public void StopExecutingCommand(long userChatId) 
        {
            _executers.Remove(userChatId, out _);
        }

        public bool TryGetExecutingCommand(long userChatId, out ExecutingCommand command) 
        {
            return _executers.TryGetValue(userChatId, out command);
        }

        public async Task AddUser(string userName, long userAddingChatId)
        {
            await _context.AdminUsers.AddAsync(new Entities.AdminUser
            {
                UserName = userName
            });

            StopExecutingCommand(userAddingChatId);

            await _context.SaveChangesAsync();
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

        public async Task<AdminUser> RemoveUser(string userName) 
        {
            try
            {
                return _mapper.Map<AdminUser>(_context.AdminUsers
                    .Remove(await _context.AdminUsers
                        .AsNoTracking()
                        .FirstAsync(x => x.UserName == userName))
                    .Entity);
            }
            finally
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
