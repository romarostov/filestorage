using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseUri = "http://localhost:8080";

            Console.WriteLine("Starting web Server...");
            WebApp.Start<Startup>(baseUri);
            Console.WriteLine("Server running at {0} - press Enter to quit. ", baseUri);
            Console.ReadLine();
        }
    }

    public class Startup
    {
        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            var webApiConfiguration = ConfigureWebApi();

            // Настройка контекста базы данных, диспетчера пользователей и диспетчера входа для использования одного экземпляра на запрос
            //app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ImMemoryUserStore>(ImMemoryUserStore.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Включение использования файла cookie, 
            //в котором приложение может хранить информацию для пользователя, выполнившего вход,
            // и использование файла cookie для временного хранения информации
            //о входах пользователя с помощью стороннего поставщика входа
            // Настройка файла cookie для входа
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                //LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {

                    // Позволяет приложению проверять метку безопасности при входе пользователя.
                    // Эта функция безопасности используется,
                    //когда вы меняете пароль или добавляете внешнее имя входа в свою учетную запись.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, CustomUser>(validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                },
                ExpireTimeSpan = TimeSpan.FromSeconds(30)
            });


            // Use the extension method provided by the WebApi.Owin library:
            app.UseWebApi(webApiConfiguration);
        }


        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{id}",
                new { id = RouteParameter.Optional });
            return config;
        }
    }
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Authorize]
    public class CompaniesController : ApiController
    {
        // Mock a data store:
        private static List<Company> _Db = new List<Company>
            {
                new Company { Id = 1, Name = "Microsoft" },
                new Company { Id = 2, Name = "Google" },
                new Company { Id = 3, Name = "Apple" }
            };


        public IEnumerable<Company> Get()
        {
            return _Db;
        }


        public Company Get(int id)
        {
            var company = _Db.FirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                throw new HttpResponseException(
                    System.Net.HttpStatusCode.NotFound);
            }
            return company;
        }


        public IHttpActionResult Post(Company company)
        {
            if (company == null)
            {
                return BadRequest("Argument Null");
            }
            var companyExists = _Db.Any(c => c.Id == company.Id);

            if (companyExists)
            {
                return BadRequest("Exists");
            }

            _Db.Add(company);
            return Ok();
        }


        public IHttpActionResult Put(Company company)
        {
            if (company == null)
            {
                return BadRequest("Argument Null");
            }
            var existing = _Db.FirstOrDefault(c => c.Id == company.Id);

            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = company.Name;
            return Ok();
        }


        public IHttpActionResult Delete(int id)
        {
            var company = _Db.FirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            _Db.Remove(company);
            return Ok();
        }
    }
}
