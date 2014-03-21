using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlobStorageReplicator.Startup))]
namespace BlobStorageReplicator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
