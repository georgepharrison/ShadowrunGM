-- Initialize PostgreSQL database with pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create the shadowrun user and database if they don't exist
-- (handled by environment variables in docker-compose.yml)

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE shadowrundb TO shadowrun;

-- Enable pgvector extension specifically for shadowrundb
\c shadowrundb;
CREATE EXTENSION IF NOT EXISTS vector;