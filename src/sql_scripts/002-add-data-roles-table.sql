
INSERT INTO roles (id_role, name)
VALUES 
    (1, 'user'), 
    (2, 'moderator'),
    (3, 'administrator')
ON CONFLICT (id_role) DO NOTHING;