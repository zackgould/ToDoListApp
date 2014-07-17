using System;
using System.Linq;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Runtime;
using Raven.Client;
using Raven.Client.Linq;
using Cookie = FubuMVC.Core.Http.Cookies.Cookie;

namespace ToDoWebsite.ToDo
{
    public class LoginEndpoint
    {
        private readonly IDocumentSession _session;
        private readonly IOutputWriter _outputWriter;
        private readonly IEncryptionService _encryptionService;

        private const string AuthCookieName = "Username";

        public LoginEndpoint(IDocumentSession session, IOutputWriter outputWriter, IEncryptionService encryptionService)
        {
            _session = session;
            _outputWriter = outputWriter;
            _encryptionService = encryptionService;
        }

        public LoginViewModel get_Login(LoginInputModel inputModel)
        {
            var cookie = new Cookie(AuthCookieName)
            {
                Expires = DateTime.Today.AddDays(-1)
            };
            _outputWriter.AppendCookie(cookie);

            return new LoginViewModel();
        }

        public FubuContinuation post_Login(LoginPostInputModel inputModel)
        {
            var name = inputModel.Username;

            if (name == "")
            {
                return FubuContinuation.RedirectTo<FailedLoginInputModel>();
            }

            var password = _encryptionService.GetStringSha1Hash(inputModel.Password);
            var user =
                _session.Query<Person>().Where(u => (u.Id == name || u.Name == name) && u.Password == password).ToList();

            if (user.Count == 0)
            {
                return FubuContinuation.RedirectTo<FailedLoginInputModel>();
            }

            var cookie = new Cookie(AuthCookieName, _encryptionService.EncryptUsername(name))
            {
                Expires = DateTime.Now.AddDays(1),
            };
            _outputWriter.AppendCookie(cookie);

            return FubuContinuation.RedirectTo<ToDoListInputModel>();
        }
    }

    public class LoginViewModel
    {
    }

    public class LoginInputModel
    {
    }

    public class LoginPostInputModel
    {
        public string Password { get; set; }
        public string Username { get; set; }
    }
}