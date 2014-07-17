using System;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Http.Cookies;
using FubuMVC.Core.Runtime;
using Raven.Client;

namespace ToDoWebsite.ToDo
{
    public class FailedLoginEndpoint
    {
        private readonly IOutputWriter _outputWriter;

        private const string AuthCookieName = "Username";

        public FailedLoginEndpoint(IDocumentSession session, IOutputWriter outputWriter, IEncryptionService encryptionService)
        {
            _outputWriter = outputWriter;
        }

        public FailedLoginViewModel get_Invalid(FailedLoginInputModel inputModel)
        {
            var cookie = new Cookie(AuthCookieName)
            {
                Expires = DateTime.Today.AddDays(-1)
            };
            _outputWriter.AppendCookie(cookie);

            return new FailedLoginViewModel();
        }

        public FubuContinuation post_Invalid(InvalidLoginPostInputModel inputModel)
        {
            return FubuContinuation.RedirectTo<LoginInputModel>();
        }
    }

    public class InvalidLoginPostInputModel
    {
    }

    public class FailedLoginViewModel
    {
    }

    public class FailedLoginInputModel
    {
    }
}