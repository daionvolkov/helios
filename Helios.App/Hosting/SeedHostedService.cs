using Helios.Identity.Options;
using Helios.Persistence.Context;
using Helios.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Helios.App.Hosting
{
    public sealed class SeedHostedService : IHostedService
    {
        private readonly IServiceProvider _sp;
        private readonly SeedOptions _seed;

        public SeedHostedService(IServiceProvider sp, IOptions<SeedOptions> seed)
        {
            _sp = sp;
            _seed = seed.Value;
        }


        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HeliosDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<User>>();

            await db.Database.MigrateAsync(ct);

            // MVP policy: if any tenant exists -> already seeded
            if (await db.Tenants.AnyAsync(ct))
                return;

            var now = DateTimeOffset.UtcNow;

            var tenant = new Tenant
            {
                TenantId = Guid.NewGuid(),
                Name = _seed.TenantName,
                CreatedAt = now
            };

            var ownerRole = new Role { RoleId = Guid.NewGuid(), TenantId = tenant.TenantId, Code = "Owner", CreatedAt = now };
            var adminRole = new Role { RoleId = Guid.NewGuid(), TenantId = tenant.TenantId, Code = "Admin", CreatedAt = now };
            var viewerRole = new Role { RoleId = Guid.NewGuid(), TenantId = tenant.TenantId, Code = "Viewer", CreatedAt = now };

            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                TenantId = tenant.TenantId,
                Email = _seed.AdminEmail.Trim(),
                IsActive = true,
                CreatedAt = now,
                PasswordHash = "" // set below
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, _seed.AdminPassword);

            db.Tenants.Add(tenant);
            db.Roles.AddRange(ownerRole, adminRole, viewerRole);
            db.Users.Add(adminUser);

            db.UserRoles.AddRange(
                new UserRole { TenantId = tenant.TenantId, UserId = adminUser.UserId, RoleId = ownerRole.RoleId, CreatedAt = now },
                new UserRole { TenantId = tenant.TenantId, UserId = adminUser.UserId, RoleId = adminRole.RoleId, CreatedAt = now }
            );

            await db.SaveChangesAsync(ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
