using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using Raven.Client;

namespace ToDoWebsite.Behaviors
{
    public class RavenSessionBehavior : BasicBehavior
    {
        private readonly IDocumentSession _documentSession;

        public RavenSessionBehavior(IDocumentSession documentSession) : base(PartialBehavior.Ignored)
        {
            _documentSession = documentSession;
        }

        protected override void afterInsideBehavior()
        {
            _documentSession.SaveChanges();
        }
    }

    public class RavenSessionPolicy : Policy
    {
        public RavenSessionPolicy()
        {
            Where.RespondsToHttpMethod("POST");
            Wrap.WithBehavior<RavenSessionBehavior>();
        }
    }
}