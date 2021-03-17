using AutoMapper;
using DAL;
using Findparts.App_Start;
using Findparts.Models;
using Findparts.Services.Interfaces;
using Findparts.Services.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Web;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Findparts
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();



            container.RegisterType<DbContext, ApplicationDbContext>(new HierarchicalLifetimeManager());                        
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<UserManager<ApplicationUser>>(new HierarchicalLifetimeManager());

            container.RegisterFactory<HttpContext>(c => HttpContext.Current);
            container.RegisterFactory<IAuthenticationManager>(c =>
                System.Web.HttpContext.Current.GetOwinContext().Authentication);

            container.RegisterType<FindPartsEntities>(new HierarchicalLifetimeManager());

            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(
                new InjectionConstructor(typeof(ApplicationDbContext))
            );

            // add automapper
            var config = new MapperConfiguration(cfg =>
            {   
                cfg.AddProfile<MappingProfile>();
            });

            IMapper mapper = config.CreateMapper();

            container.RegisterInstance(mapper);


            // add our self defined services

            container.RegisterType<IPartsSearchService, PartsSearchService>();
            container.RegisterType<IMembershipService, MembershipService>();
            container.RegisterType<IMailService, MailService>();
        }
    }
}