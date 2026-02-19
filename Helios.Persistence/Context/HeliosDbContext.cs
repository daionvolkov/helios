using Helios.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helios.Persistence.Context;

public sealed class HeliosDbContext : DbContext
{
    public HeliosDbContext(DbContextOptions<HeliosDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectEnvironment> Environments => Set<ProjectEnvironment>();
    public DbSet<Server> Servers => Set<Server>();

    public DbSet<AgentEnrollmentToken> AgentEnrollmentTokens => Set<AgentEnrollmentToken>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentCredential> AgentCredentials => Set<AgentCredential>();
    public DbSet<AgentHeartbeat> AgentHeartbeats => Set<AgentHeartbeat>();

    public DbSet<Command> Commands => Set<Command>();
    public DbSet<CommandResult> CommandResults => Set<CommandResult>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tenants
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(x => x.TenantId);
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // Users
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(x => x.UserId);
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.Email).HasColumnName("email").IsRequired();
            b.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            b.Property(x => x.IsActive).HasColumnName("is_active");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // unique (tenant_id, lower(email)) -> EF can't express lower() index cleanly;
            // create via migration SQL if you rely on migrations.
            b.HasIndex(x => new { x.TenantId, x.Email }).HasDatabaseName("ix_users_tenant_email");
        });

        // Roles
        modelBuilder.Entity<Role>(b =>
        {
            b.ToTable("roles");
            b.HasKey(x => x.RoleId);
            b.Property(x => x.RoleId).HasColumnName("role_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.Code).HasColumnName("code").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Roles)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.Code }).HasDatabaseName("ix_roles_tenant_code");
        });

        // UserRoles
        modelBuilder.Entity<UserRole>(b =>
        {
            b.ToTable("user_roles");
            b.HasKey(x => new { x.TenantId, x.UserId, x.RoleId });

            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.RoleId).HasColumnName("role_id");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Projects
        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("projects");
            b.HasKey(x => x.ProjectId);

            b.Property(x => x.ProjectId).HasColumnName("project_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Projects)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.Name }).HasDatabaseName("ix_projects_tenant_name");
        });

        // Environments
        modelBuilder.Entity<ProjectEnvironment>(b =>
        {
            b.ToTable("environments");
            b.HasKey(x => x.EnvironmentId);

            b.Property(x => x.EnvironmentId).HasColumnName("environment_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.ProjectId).HasColumnName("project_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired();
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Project)
                .WithMany(p => p.Environments)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.ProjectId, x.Name }).HasDatabaseName("ix_envs_project_name");
        });

        // Servers
        modelBuilder.Entity<Server>(b =>
        {
            b.ToTable("servers");
            b.HasKey(x => x.ServerId);

            b.Property(x => x.ServerId).HasColumnName("server_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.ProjectId).HasColumnName("project_id");
            b.Property(x => x.EnvironmentId).HasColumnName("environment_id");
            b.Property(x => x.Name).HasColumnName("name").IsRequired();
            b.Property(x => x.Hostname).HasColumnName("hostname");
            b.Property(x => x.TagsJson).HasColumnName("tags").HasColumnType("jsonb");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Servers)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Project)
                .WithMany(p => p.Servers)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.Environment)
                .WithMany(e => e.Servers)
                .HasForeignKey(x => x.EnvironmentId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.TenantId).HasDatabaseName("ix_servers_tenant");
            b.HasIndex(x => new { x.TenantId, x.ProjectId }).HasDatabaseName("ix_servers_project");
            b.HasIndex(x => new { x.TenantId, x.EnvironmentId }).HasDatabaseName("ix_servers_environment");
        });

        // Enrollment tokens
        modelBuilder.Entity<AgentEnrollmentToken>(b =>
        {
            b.ToTable("agent_enrollment_tokens");
            b.HasKey(x => x.TokenId);

            b.Property(x => x.TokenId).HasColumnName("token_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.ServerId).HasColumnName("server_id");
            b.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired();
            b.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            b.Property(x => x.UsedAt).HasColumnName("used_at");
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Server)
                .WithMany()
                .HasForeignKey(x => x.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Agents
        modelBuilder.Entity<Agent>(b =>
        {
            b.ToTable("agents");
            b.HasKey(x => x.AgentId);

            b.Property(x => x.AgentId).HasColumnName("agent_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.ServerId).HasColumnName("server_id");

            b.Property(x => x.DisplayName).HasColumnName("display_name").IsRequired();
            b.Property(x => x.AgentVersion).HasColumnName("agent_version").IsRequired();
            b.Property(x => x.Os).HasColumnName("os").IsRequired();
            b.Property(x => x.Arch).HasColumnName("arch").IsRequired();
            b.Property(x => x.CapabilitiesJson).HasColumnName("capabilities").HasColumnType("jsonb");

            b.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            b.Property(x => x.LastSeenAt).HasColumnName("last_seen_at");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Server)
                .WithMany(s => s.Agents)
                .HasForeignKey(x => x.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.ServerId }).HasDatabaseName("ix_agents_server");
            b.HasIndex(x => new { x.TenantId, x.Status }).HasDatabaseName("ix_agents_status");
        });

        // Agent credentials (1:1)
        modelBuilder.Entity<AgentCredential>(b =>
        {
            b.ToTable("agent_credentials");
            b.HasKey(x => new { x.TenantId, x.AgentId });

            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.AgentId).HasColumnName("agent_id");
            b.Property(x => x.AccessKeyId).HasColumnName("access_key_id").IsRequired();
            b.Property(x => x.AccessKeyHash).HasColumnName("access_key_hash").IsRequired();
            b.Property(x => x.IssuedAt).HasColumnName("issued_at");
            b.Property(x => x.RevokedAt).HasColumnName("revoked_at");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Agent)
                .WithOne(a => a.Credential)
                .HasForeignKey<AgentCredential>(x => x.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.AccessKeyId }).HasDatabaseName("ux_agent_credentials_keyid").IsUnique();
        });

        // Heartbeats
        modelBuilder.Entity<AgentHeartbeat>(b =>
        {
            b.ToTable("agent_heartbeats");
            b.HasNoKey(); // history table; alternatively add synthetic id. For v0.1, no key is OK for EF queries only.

            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.AgentId).HasColumnName("agent_id");
            b.Property(x => x.ServerId).HasColumnName("server_id");
            b.Property(x => x.ReceivedAt).HasColumnName("received_at");
            b.Property(x => x.PayloadJson).HasColumnName("payload").HasColumnType("jsonb");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Agent)
                .WithMany(a => a.Heartbeats)
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Server)
                .WithMany()
                .HasForeignKey(x => x.ServerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Commands
        modelBuilder.Entity<Command>(b =>
        {
            b.ToTable("commands");
            b.HasKey(x => x.CommandId);

            b.Property(x => x.CommandId).HasColumnName("command_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.ServerId).HasColumnName("server_id");
            b.Property(x => x.AgentId).HasColumnName("agent_id");

            b.Property(x => x.Type).HasColumnName("type").IsRequired();
            b.Property(x => x.PayloadJson).HasColumnName("payload").HasColumnType("jsonb");

            b.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            b.Property(x => x.CorrelationId).HasColumnName("correlation_id");
            b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key");
            b.Property(x => x.CreatedBy).HasColumnName("created_by");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            b.Property(x => x.ExpiresAt).HasColumnName("expires_at");

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Server)
                .WithMany()
                .HasForeignKey(x => x.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.TenantId, x.ServerId, x.Status }).HasDatabaseName("ix_commands_server_status");
            b.HasIndex(x => new { x.TenantId, x.AgentId, x.Status }).HasDatabaseName("ix_commands_agent_status");
        });

        // Command results (1:1)
        modelBuilder.Entity<CommandResult>(b =>
        {
            b.ToTable("command_results");
            b.HasKey(x => x.CommandId);

            b.Property(x => x.CommandId).HasColumnName("command_id");
            b.Property(x => x.TenantId).HasColumnName("tenant_id");
            b.Property(x => x.Status).HasColumnName("status").IsRequired();
            b.Property(x => x.ExitCode).HasColumnName("exit_code");
            b.Property(x => x.Stdout).HasColumnName("stdout");
            b.Property(x => x.Stderr).HasColumnName("stderr");
            b.Property(x => x.StartedAt).HasColumnName("started_at");
            b.Property(x => x.FinishedAt).HasColumnName("finished_at");

            b.HasOne(x => x.Command)
                .WithOne(c => c.Result)
                .HasForeignKey<CommandResult>(x => x.CommandId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
