using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(findparts.Startup))]
namespace findparts
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
