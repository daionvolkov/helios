using Helios.Application.Abstractions;
using Helios.Platform.Agents;
using Helios.Platform.Agents.Intrefaces;
using Microsoft.Extensions.DependencyInjection;

namespace Helios.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHeliosApplication(this IServiceCollection services)
        {
            //Adapter: ITenantContext(from Helios.App)->ICurrentUserContext(application abstraction)
            services.AddScoped<Helios.Application.Abstractions.ICurrentUserContext, CurrentUserContextAdapter>();


            // Application services (use-cases)
            services.AddScoped<Helios.Application.Identity.IAuthAppService, Helios.Application.Identity.AuthAppService>();
            services.AddScoped<Helios.Application.Tenants.ITenantsAppService, Helios.Application.Tenants.TenantsAppService>();
            services.AddScoped<Helios.Application.Platform.Servers.IServersAppService, Helios.Application.Platform.Servers.ServersAppService>();

            // Platform managers (domain)
            services.AddScoped<Helios.Platform.Servers.IServerManager, Helios.Platform.Servers.ServerManager>();

            services.AddScoped<IAgentEnrollmentManager, Helios.Platform.Agents.AgentEnrollmentManager>();
            services.AddScoped<IAgentManager, Helios.Platform.Agents.AgentManager>();

            // Platform managers
            services.AddScoped<AgentEnrollmentManager, Helios.Platform.Agents.AgentEnrollmentManager>();
            services.AddScoped<IAgentManager, Helios.Platform.Agents.AgentManager>();

            // Application services
            services.AddScoped<Helios.Application.Agents.IEnrollmentAppService, Helios.Application.Agents.EnrollmentAppService>();
            services.AddScoped<Helios.Application.Agents.IAgentsAppService, Helios.Application.Agents.AgentsAppService>();
            return services;
        }
    }
}
