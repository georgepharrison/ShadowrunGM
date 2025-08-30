# Database Setup Guide

This guide covers setting up the PostgreSQL database with pgvector extension for the ShadowrunGM project.

## Prerequisites

- Docker and Docker Compose installed
- .NET 9.0 SDK for running migrations
- Git for cloning the repository

## Quick Start

### 1. Start PostgreSQL with Docker Compose

```bash
# From the project root directory
docker-compose up -d postgres

# Verify the container is running
docker ps | grep shadowrun-postgres
```

### 2. Verify pgvector Extension

```bash
# Connect to the database
docker exec -it shadowrun-postgres psql -U shadowrun -d shadowrundb

# Check pgvector is installed
shadowrundb=# \dx
                               List of installed extensions
   Name   | Version |   Schema   |                    Description                     
----------+---------+------------+----------------------------------------------------
 plpgsql  | 1.0     | pg_catalog | PL/pgSQL procedural language
 vector   | 0.5.1   | public     | vector data type and ivfflat access method

# Exit psql
shadowrundb=# \q
```

### 3. Apply Entity Framework Migrations

```bash
# From the project root
cd src/API
dotnet ef database update

# Verify tables were created
docker exec -it shadowrun-postgres psql -U shadowrun -d shadowrundb -c "\dt"
```

### 4. Verify Seed Data

```bash
# Check game items were seeded
docker exec -it shadowrun-postgres psql -U shadowrun -d shadowrundb -c "SELECT COUNT(*) FROM game_items;"

# Sample a few items
docker exec -it shadowrun-postgres psql -U shadowrun -d shadowrundb -c "SELECT name, item_type, category FROM game_items LIMIT 5;"
```

## Configuration Details

### Docker Compose Configuration

The `docker-compose.yml` includes:

```yaml
services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: shadowrun-postgres
    environment:
      POSTGRES_DB: shadowrundb
      POSTGRES_USER: shadowrun
      POSTGRES_PASSWORD: shadowrun123
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U shadowrun -d shadowrundb"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
    driver: local
```

**Key Features:**
- **pgvector/pgvector:pg16** - PostgreSQL 16 with pgvector extension pre-installed
- **Health checks** - Container monitoring for dependent services
- **Persistent storage** - Data survives container restarts
- **Initialization script** - Automatically sets up extensions and permissions

### Database Initialization Script

`scripts/init-db.sql` runs on first container startup:

```sql
-- Initialize PostgreSQL database with pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE shadowrundb TO shadowrun;

-- Enable pgvector extension specifically for shadowrundb
\c shadowrundb;
CREATE EXTENSION IF NOT EXISTS vector;
```

### Connection Strings

**Development** (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=shadowrundb;Username=shadowrun;Password=shadowrun123;Port=5432"
  }
}
```

**Production** (use environment variables):
```bash
export ConnectionStrings__DefaultConnection="Host=prod-db;Database=shadowrundb;Username=app_user;Password=secure_password;Port=5432"
```

## Database Schema

### Core Tables

#### Characters Table
```sql
-- Primary aggregate table
CREATE TABLE characters (
    id uuid PRIMARY KEY,
    name character varying(100) NOT NULL,
    created_at timestamp with time zone DEFAULT timezone('utc', now()),
    modified_at timestamp with time zone DEFAULT timezone('utc', now()),
    
    -- Attributes (owned value object)
    "Body" integer,
    "Agility" integer,
    "Reaction" integer,
    "Strength" integer,
    "Willpower" integer,
    "Logic" integer,
    "Intuition" integer,
    "Charisma" integer,
    
    -- Edge (owned value object) 
    "CurrentEdge" integer,
    "MaxEdge" integer,
    
    -- Health (owned value object)
    "PhysicalBoxes" integer,
    "PhysicalDamage" integer,
    "StunBoxes" integer,
    "StunDamage" integer
);

CREATE INDEX ix_characters_name ON characters (name);
CREATE INDEX ix_characters_created_at ON characters (created_at);
```

#### Game Content Tables
```sql
-- Imported sourcebook content
CREATE TABLE sourcebooks (
    id uuid PRIMARY KEY,
    code character varying(10) NOT NULL,
    title character varying(200) NOT NULL,
    edition character varying(10) NOT NULL,
    year integer,
    created_utc timestamp with time zone,
    updated_utc timestamp with time zone
);

-- Structured game items (equipment, weapons, etc.)
CREATE TABLE game_items (
    id uuid PRIMARY KEY,
    name character varying(200) NOT NULL,
    slug character varying(250) NOT NULL,
    item_type character varying(50) NOT NULL,
    category character varying(100),
    cost decimal(10,2),
    availability character varying(20),
    stats_json jsonb,
    sourcebook_id uuid REFERENCES sourcebooks(id),
    page integer,
    created_at timestamp with time zone,
    updated_at timestamp with time zone
);

-- Magic abilities and spells
CREATE TABLE magic_abilities (
    id uuid PRIMARY KEY,
    name character varying(200) NOT NULL,
    slug character varying(250) NOT NULL,
    ability_type character varying(50) NOT NULL,
    category character varying(100),
    drain_value character varying(10),
    stats_json jsonb,
    description text,
    sourcebook_id uuid REFERENCES sourcebooks(id),
    page integer,
    created_at timestamp with time zone,
    updated_at timestamp with time zone
);
```

## Common Operations

### Database Maintenance

```bash
# Stop the database
docker-compose stop postgres

# Remove container but keep data
docker-compose rm postgres

# Remove container and data (destructive!)
docker-compose down -v

# View logs
docker-compose logs postgres

# Execute arbitrary SQL
docker exec -it shadowrun-postgres psql -U shadowrun -d shadowrundb -c "YOUR_SQL_HERE"
```

### Development Workflow

```bash
# Reset database for clean state
docker-compose down -v
docker-compose up -d postgres

# Wait for container to be ready
docker-compose exec postgres sh -c 'until pg_isready -U shadowrun; do sleep 1; done'

# Apply migrations and seed data
cd src/API
dotnet ef database update
dotnet run  # This triggers seeding on startup
```

### Backup and Restore

```bash
# Create backup
docker exec shadowrun-postgres pg_dump -U shadowrun shadowrundb > backup.sql

# Restore from backup
cat backup.sql | docker exec -i shadowrun-postgres psql -U shadowrun -d shadowrundb
```

## Troubleshooting

### Common Issues

**Connection Refused:**
```bash
# Check if container is running
docker ps | grep postgres

# Check container logs
docker logs shadowrun-postgres

# Test connection
docker exec shadowrun-postgres pg_isready -U shadowrun
```

**Permission Denied:**
```bash
# Verify user and database exist
docker exec -it shadowrun-postgres psql -U postgres -c "\du"
docker exec -it shadowrun-postgres psql -U postgres -c "\l"
```

**Migration Failures:**
```bash
# Check EF connection string
dotnet ef dbcontext info --project src/API

# Reset migrations (destructive!)
cd src/API
rm -rf Infrastructure/Migrations/
dotnet ef migrations add InitialCreate --output-dir Infrastructure/Migrations
```

### Performance Issues

**Slow Queries:**
```sql
-- Enable query logging
ALTER SYSTEM SET log_statement = 'all';
SELECT pg_reload_conf();

-- Check indexes
SELECT * FROM pg_stat_user_indexes WHERE idx_tup_read = 0;

-- Analyze query performance
EXPLAIN ANALYZE SELECT * FROM characters WHERE name = 'TestName';
```

## Production Considerations

### Security
- Use strong passwords and non-default usernames
- Enable SSL/TLS connections
- Configure firewall rules appropriately
- Regular security updates for PostgreSQL

### Performance
- Configure appropriate connection pooling
- Monitor query performance and add indexes as needed
- Consider read replicas for heavy read workloads
- Set up proper backup and disaster recovery

### Monitoring
- Set up PostgreSQL metrics collection
- Monitor disk space usage
- Configure alerting for connection failures
- Track query performance over time