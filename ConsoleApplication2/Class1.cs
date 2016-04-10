using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{

    public class CustomUser : IUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string PasswordHashKey { get; set; }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        //public CustomUser Clone()
        //{
        //    return new CustomUser()
        //    {
        //        Id = this.Id,
        //        UserName = UserName,
        //        PasswordHashKey = PasswordHashKey,
        //        Email=Email,
        //        IsEmailConfirmed=IsEmailConfirmed
        //    };
        //}

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<CustomUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
    }

    public class ImMemoryUserStore : IUserStore<CustomUser>, IUserPasswordStore<CustomUser>
        , IUserEmailStore<CustomUser>,
        IUserLockoutStore<CustomUser, string>,
         IUserTwoFactorStore<CustomUser, string>,
        IUserLoginStore<CustomUser, string>
    {
        private static ImMemoryUserStore _store = new ImMemoryUserStore();

        public static ImMemoryUserStore Create()
        {
            return _store;
        }

        private object _syncObject = new object();

        class XmlUser
        {
            public string Id { get; set; }

            public string UserName { get; set; }

            public string PasswordHashKey { get; set; }

            public string Email { get; set; }

            public bool IsEmailConfirmed { get; set; }

            public CustomUser Clone()
            {
                return new CustomUser()
                {
                    Id = this.Id,
                    UserName = UserName,
                    PasswordHashKey = PasswordHashKey,
                    Email = Email,
                    IsEmailConfirmed = IsEmailConfirmed
                };
            }

            public DateTimeOffset LockoutExpired { get; set; }

            public int CountFailedAccess { get; set; }
            public bool LockoutEnabled { get; internal set; }
        }

        Dictionary<string, XmlUser> _users = new Dictionary<string, XmlUser>();


        public Task CreateAsync(CustomUser user)
        {
            lock (_syncObject)
            {
                XmlUser new_user = new XmlUser()
                {
                    Id = Guid.NewGuid().ToString() + "local",
                    Email = user.Email,
                    IsEmailConfirmed = false,
                    PasswordHashKey = user.PasswordHashKey,
                    UserName = user.UserName
                };
                user.Id = new_user.Id;
                _users.Add(new_user.Id, new_user);
                return Task.FromResult(0);
            }
        }


        public Task DeleteAsync(CustomUser user)
        {
            return Task.Factory.StartNew(() =>
            {
                lock (_syncObject)
                {
                    _users.Remove(user.Id);
                }
            });
        }

        public void Dispose()
        {
        }

        public Task<CustomUser> FindByIdAsync(string userId)
        {

            lock (_syncObject)
            {
                CustomUser us = null;
                XmlUser ret;
                if (_users.TryGetValue(userId, out ret))
                {
                    us = ret.Clone();
                }
                return Task.FromResult<CustomUser>(us);
            }

        }

        public Task<CustomUser> FindByNameAsync(string userName)
        {
            lock (_syncObject)
            {
                XmlUser user = _users.Values.SingleOrDefault(x => x.UserName == userName);
                if (user != null)
                {
                    return Task.FromResult(user.Clone());
                }
                return Task.FromResult<CustomUser>(null);
            }

        }

        public Task UpdateAsync(CustomUser user)
        {

            lock (_syncObject)
            {
                return Task.FromResult(_users[user.Id].UserName = user.UserName);
            }

        }

        public Task SetPasswordHashAsync(CustomUser user, string passwordHash)
        {
            lock (_syncObject)
            {
                user.PasswordHashKey = passwordHash;
                //_users[user.Id].PasswordHashKey = passwordHash;
                return Task.FromResult(0);
            }
        }

        public Task<string> GetPasswordHashAsync(CustomUser user)
        {
            lock (_syncObject)
            {
                return Task.FromResult(_users[user.Id].PasswordHashKey);
            }
        }

        public Task<bool> HasPasswordAsync(CustomUser user)
        {
            return Task.FromResult<bool>(true);
        }

        public Task SetEmailAsync(CustomUser user, string email)
        {
            user.UserName = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(CustomUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(CustomUser user)
        {
            return Task.FromResult(user.IsEmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(CustomUser user, bool confirmed)
        {
            lock (_syncObject)
            {
                _users[user.Id].IsEmailConfirmed = confirmed;
                return Task.FromResult(0);
            }
        }

        public Task<CustomUser> FindByEmailAsync(string email)
        {
            lock (_syncObject)
            {
                XmlUser user = _users.Values.SingleOrDefault(x => x.Email == email);
                if (user != null)
                {
                    return Task.FromResult(user.Clone());
                }
                return Task.FromResult<CustomUser>(null);
            }
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(CustomUser send_user)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                return Task.FromResult(user.LockoutExpired);
            }

        }

        public Task SetLockoutEndDateAsync(CustomUser send_user, DateTimeOffset lockoutEnd)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                return Task.FromResult(user.LockoutExpired);
            }
        }

        public Task<int> IncrementAccessFailedCountAsync(CustomUser send_user)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                user.CountFailedAccess++;
                return Task.FromResult(user.CountFailedAccess);
            }
        }

        public Task ResetAccessFailedCountAsync(CustomUser send_user)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                user.CountFailedAccess = 0;
                return Task.FromResult(0);
            }
        }

        public Task<int> GetAccessFailedCountAsync(CustomUser send_user)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                return Task.FromResult(user.CountFailedAccess);
            }
        }

        public Task<bool> GetLockoutEnabledAsync(CustomUser send_user)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                return Task.FromResult(user.LockoutEnabled);
            }
        }

        public Task SetLockoutEnabledAsync(CustomUser send_user, bool enabled)
        {
            lock (_syncObject)
            {
                XmlUser user = _users[send_user.Id];
                user.LockoutEnabled = enabled;
                return Task.FromResult(0);
            }
        }

        public Task SetTwoFactorEnabledAsync(CustomUser user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetTwoFactorEnabledAsync(CustomUser user)
        {
            return Task.FromResult(false);
        }

        public Task AddLoginAsync(CustomUser user, UserLoginInfo login)
        {
            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(CustomUser user, UserLoginInfo login)
        {
            return Task.FromResult(0);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(CustomUser user)
        {
            return Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());
        }

        public Task<CustomUser> FindAsync(UserLoginInfo login)
        {
            return Task.FromResult<CustomUser>(null);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Подключите здесь службу SMS, чтобы отправить текстовое сообщение.
            return Task.FromResult(0);
        }
    }

    // Настройка диспетчера пользователей приложения. 
    //UserManager определяется в ASP.NET Identity и используется приложением.
    public class ApplicationUserManager : UserManager<CustomUser>
    {
        public ApplicationUserManager(IUserStore<CustomUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {

            //var manager = new ApplicationUserManager(new UserStore<User>(context.Get<ApplicationDbContext>()));
            var manager = new ApplicationUserManager(context.Get<ImMemoryUserStore>());

            // Настройка логики проверки имен пользователей
            manager.UserValidator = new UserValidator<CustomUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Настройка логики проверки паролей
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                //RequireNonLetterOrDigit = true,
                //RequireDigit = true,
                //RequireLowercase = true,
                //RequireUppercase = true,
            };

            // Настройка параметров блокировки по умолчанию
            //manager.UserLockoutEnabledByDefault = true;
            //manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Регистрация поставщиков двухфакторной проверки подлинности. Для получения кода проверки пользователя в данном приложении используется телефон и сообщения электронной почты
            // Здесь можно указать собственный поставщик и подключить его.
            //manager.RegisterTwoFactorProvider("Код, полученный по телефону", new PhoneNumberTokenProvider<User>
            //{
            //    MessageFormat = "Ваш код безопасности: {0}"
            //});
            //manager.RegisterTwoFactorProvider("Код из сообщения", new EmailTokenProvider<User>
            //{
            //    Subject = "Код безопасности",
            //    BodyFormat = "Ваш код безопасности: {0}"
            //});
            //manager.EmailService = new EmailService();
            //manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<CustomUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Настройка диспетчера входа для приложения.
    public class ApplicationSignInManager : SignInManager<CustomUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {

        }

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(CustomUser user)
        //{
        //    return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        //}

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
