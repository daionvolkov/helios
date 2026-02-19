-- 0) Database (optional)
-- CREATE DATABASE helios;

-- 1) Extensions (run inside helios DB)
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 2) Tenants
CREATE TABLE IF NOT EXISTS tenants (
    tenant_id  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name       text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

-- 3) Users
CREATE TABLE IF NOT EXISTS users (
    user_id       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    email         text NOT NULL,
    password_hash text NOT NULL,
    is_active     boolean NOT NULL DEFAULT true,
    created_at    timestamptz NOT NULL DEFAULT now()
);

-- unique email per tenant
CREATE UNIQUE INDEX IF NOT EXISTS ux_users_tenant_email
ON users (tenant_id, lower(email));

CREATE INDEX IF NOT EXISTS ix_users_tenant
ON users (tenant_id);

-- 4) Roles
CREATE TABLE IF NOT EXISTS roles (
    role_id    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id  uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    code       text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_roles_tenant_code
ON roles (tenant_id, lower(code));

CREATE INDEX IF NOT EXISTS ix_roles_tenant
ON roles (tenant_id);

-- 5) UserRoles (many-to-many)
CREATE TABLE IF NOT EXISTS user_roles (
    tenant_id uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    user_id   uuid NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    role_id   uuid NOT NULL REFERENCES roles(role_id) ON DELETE CASCADE,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT pk_user_roles PRIMARY KEY (tenant_id, user_id, role_id)
);

CREATE INDEX IF NOT EXISTS ix_user_roles_user
ON user_roles (tenant_id, user_id);

CREATE INDEX IF NOT EXISTS ix_user_roles_role
ON user_roles (tenant_id, role_id);

-- 6) Projects
CREATE TABLE IF NOT EXISTS projects (
    project_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id  uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    name       text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_projects_tenant_name
ON projects (tenant_id, lower(name));

-- 7) Environments (optional but recommended to add now)
CREATE TABLE IF NOT EXISTS environments (
    environment_id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    project_id     uuid NOT NULL REFERENCES projects(project_id) ON DELETE CASCADE,
    name           text NOT NULL,
    created_at     timestamptz NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_envs_project_name
ON environments (tenant_id, project_id, lower(name));

CREATE INDEX IF NOT EXISTS ix_envs_project
ON environments (tenant_id, project_id);

-- 8) Servers
CREATE TABLE IF NOT EXISTS servers (
    server_id      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id      uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    project_id     uuid NULL REFERENCES projects(project_id) ON DELETE SET NULL,
    environment_id uuid NULL REFERENCES environments(environment_id) ON DELETE SET NULL,
    name           text NOT NULL,
    hostname       text NULL,
    tags           jsonb NULL,
    created_at     timestamptz NOT NULL DEFAULT now(),
    updated_at     timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_servers_tenant
ON servers (tenant_id);

CREATE INDEX IF NOT EXISTS ix_servers_project
ON servers (tenant_id, project_id);

CREATE INDEX IF NOT EXISTS ix_servers_environment
ON servers (tenant_id, environment_id);

-- 9) Agent enrollment tokens
CREATE TABLE IF NOT EXISTS agent_enrollment_tokens (
    token_id    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id   uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    server_id   uuid NOT NULL REFERENCES servers(server_id) ON DELETE CASCADE,
    token_hash  bytea NOT NULL,
    expires_at  timestamptz NOT NULL,
    used_at     timestamptz NULL,
    created_by  uuid NULL REFERENCES users(user_id) ON DELETE SET NULL,
    created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_enroll_tokens_server
ON agent_enrollment_tokens (tenant_id, server_id);

CREATE INDEX IF NOT EXISTS ix_enroll_tokens_expires
ON agent_enrollment_tokens (tenant_id, expires_at);

-- prevent duplicates (optional)
CREATE UNIQUE INDEX IF NOT EXISTS ux_enroll_tokens_hash
ON agent_enrollment_tokens (tenant_id, token_hash);

-- 10) Agents
CREATE TABLE IF NOT EXISTS agents (
    agent_id      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id     uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    server_id     uuid NOT NULL REFERENCES servers(server_id) ON DELETE CASCADE,
    display_name  text NOT NULL,
    agent_version text NOT NULL,
    os            text NOT NULL,
    arch          text NOT NULL,
    capabilities  jsonb NULL,
    status        text NOT NULL,
    last_seen_at  timestamptz NULL,
    created_at    timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT ck_agents_status CHECK (status IN ('Active', 'Disconnected', 'Revoked'))
);

CREATE INDEX IF NOT EXISTS ix_agents_server
ON agents (tenant_id, server_id);

CREATE INDEX IF NOT EXISTS ix_agents_status
ON agents (tenant_id, status);

-- 11) Agent credentials (access-key)
CREATE TABLE IF NOT EXISTS agent_credentials (
    tenant_id       uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    agent_id        uuid NOT NULL REFERENCES agents(agent_id) ON DELETE CASCADE,
    access_key_id   text NOT NULL,
    access_key_hash bytea NOT NULL,
    issued_at       timestamptz NOT NULL DEFAULT now(),
    revoked_at      timestamptz NULL,
    CONSTRAINT pk_agent_credentials PRIMARY KEY (tenant_id, agent_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_agent_credentials_keyid
ON agent_credentials (tenant_id, access_key_id);

-- 12) Agent heartbeats (history)
CREATE TABLE IF NOT EXISTS agent_heartbeats (
    tenant_id   uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    agent_id    uuid NOT NULL REFERENCES agents(agent_id) ON DELETE CASCADE,
    server_id   uuid NOT NULL REFERENCES servers(server_id) ON DELETE CASCADE,
    received_at timestamptz NOT NULL DEFAULT now(),
    payload     jsonb NULL
);

CREATE INDEX IF NOT EXISTS ix_heartbeats_agent_time
ON agent_heartbeats (tenant_id, agent_id, received_at DESC);

-- 13) Commands
CREATE TABLE IF NOT EXISTS commands (
    command_id      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    server_id       uuid NOT NULL REFERENCES servers(server_id) ON DELETE CASCADE,
    agent_id        uuid NULL REFERENCES agents(agent_id) ON DELETE SET NULL,
    type            text NOT NULL,
    payload         jsonb NULL,
    status          text NOT NULL,
    correlation_id  uuid NOT NULL DEFAULT gen_random_uuid(),
    idempotency_key text NULL,
    created_by      uuid NULL REFERENCES users(user_id) ON DELETE SET NULL,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    expires_at      timestamptz NULL,

    CONSTRAINT ck_commands_status CHECK (status IN (
        'Queued','Dispatched','Acked','Running','Succeeded','Failed','TimedOut','Canceled'
    ))
);

CREATE INDEX IF NOT EXISTS ix_commands_server_status
ON commands (tenant_id, server_id, status);

CREATE INDEX IF NOT EXISTS ix_commands_agent_status
ON commands (tenant_id, agent_id, status);

CREATE INDEX IF NOT EXISTS ix_commands_created_at
ON commands (tenant_id, created_at DESC);

-- optional idempotency uniqueness per tenant+server+key
CREATE UNIQUE INDEX IF NOT EXISTS ux_commands_idempotency
ON commands (tenant_id, server_id, idempotency_key)
WHERE idempotency_key IS NOT NULL;

-- 14) Command results (1:1)
CREATE TABLE IF NOT EXISTS command_results (
    command_id  uuid PRIMARY KEY REFERENCES commands(command_id) ON DELETE CASCADE,
    tenant_id   uuid NOT NULL REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    status      text NOT NULL,
    exit_code   int NULL,
    stdout      text NULL,
    stderr      text NULL,
    started_at  timestamptz NULL,
    finished_at timestamptz NULL,

    CONSTRAINT ck_command_results_status CHECK (status IN ('Succeeded','Failed'))
);

CREATE INDEX IF NOT EXISTS ix_command_results_tenant
ON command_results (tenant_id);