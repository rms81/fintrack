-- FinTrack Database Initialization
-- This script runs when the PostgreSQL container starts for the first time

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Note: uuidv7 is native in PostgreSQL 18, no extension needed

-- Create additional indexes or configurations here if needed
-- Most schema is managed by EF Core migrations

-- Example: Create read-only user for reporting (optional)
-- CREATE USER fintrack_readonly WITH PASSWORD 'readonly_password';
-- GRANT CONNECT ON DATABASE fintrack TO fintrack_readonly;
-- GRANT USAGE ON SCHEMA public TO fintrack_readonly;
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO fintrack_readonly;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO fintrack_readonly;
