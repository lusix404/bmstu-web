-- Read-only role for replica and mirror instances
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'guest') THEN
        CREATE ROLE guest LOGIN PASSWORD 'guest2025';
    END IF;
END$$;

GRANT CONNECT ON DATABASE web_course TO guest;
GRANT USAGE ON SCHEMA public TO guest;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO guest;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO guest;
