using System;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Runtime;
using Raven.Client;
using ToDoWebsite.ToDo;

namespace ToDoWebsite.Behaviors
{
    public class AuthenticatedBehavior<TInputModel> : BasicBehavior where TInputModel : class, IRequireAuthentication
    {
        private readonly IFubuRequest _fubuRequest;
        private readonly IDocumentSession _session;
        private readonly IContinuationDirector _continuationDirector;
        private readonly IEncryptionService _encryptionService;

        public AuthenticatedBehavior(IFubuRequest fubuRequest, IDocumentSession session,
            IContinuationDirector continuationDirector, IEncryptionService encryptionService)
            : base(PartialBehavior.Ignored)
        {
            _fubuRequest = fubuRequest;
            _session = session;
            _continuationDirector = continuationDirector;
            _encryptionService = encryptionService;
        }

        protected override DoNext performInvoke()
        {
            var inputModel = _fubuRequest.Get<TInputModel>();

            if (string.IsNullOrEmpty(inputModel.Username))
            {
                _continuationDirector.RedirectTo<LoginInputModel>();
                return DoNext.Stop;
            }

            string decryptedUsername = _encryptionService.DecryptUsername(inputModel.Username);
            var user = _session.Load<Person>(decryptedUsername);

            if (user == null)
            {
                _continuationDirector.RedirectTo<LoginInputModel>();
                return DoNext.Stop;
            }

            inputModel.Username = decryptedUsername;

            return DoNext.Continue;
        }
    }

    public class AuthenticatedPolicy : Policy
    {
        public AuthenticatedPolicy()
        {
            Where.InputTypeImplements<IRequireAuthentication>();
            ModifyBy(chain => chain.FirstCall().AddBefore(new AuthenticationWrapper(chain.InputType())));
        }
    }

    public class AuthenticationWrapper : Wrapper
    {
        public AuthenticationWrapper(Type inputModel)
            : base(typeof(AuthenticatedBehavior<>).MakeGenericType(inputModel))
        {
            
        }
    }

    public static class ContinuationDirectorExtensions
    {
        public static void RedirectTo<TInputModel>(this IContinuationDirector continuationDirector) where TInputModel : new()
        {
            continuationDirector.RedirectTo(new TInputModel());
        }
    }
}