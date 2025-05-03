-- Database/init.sql

CREATE TABLE IF NOT EXISTS userrole (
    id SERIAL PRIMARY KEY UNIQUE NOT NULL,
    name VARCHAR(20) NOT NULL
);

INSERT INTO userrole (name) VALUES 
('user'),('admin');


CREATE TABLE IF NOT EXISTS userstate (
    id SMALLSERIAL PRIMARY KEY,
    name VARCHAR(20) NOT NULL UNIQUE
);

INSERT INTO userstate (name) VALUES
('active'),('inactive'),('deleted');


CREATE TABLE IF NOT EXISTS appuser (
    id SERIAL   PRIMARY KEY,
    username    VARCHAR(20) NOT NULL UNIQUE,
    password    VARCHAR(255) NOT NULL,
    stateid     SMALLINT NOT NULL DEFAULT 1,
    lastonline  TIMESTAMP,
    created     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO appuser (username, password) VALUES
('test123', 'cZ4W0HdMlQWfb7kZpUVM0gd3qtvUNJ+LdVWAUoRdYdnYwlH8UmAJbORrmfPSOlUa'),
('admin123', 'C8K9PANt9g2Z/9Y9gdpEQSDwjWaxmUjCexzqaaxr+FdqB4kZtuXm+OgmKXN1CutW');



CREATE TABLE IF NOT EXISTS map_user_role (
    user_id INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    role_id SMALLINT NOT NULL REFERENCES userrole(id),
    PRIMARY KEY (user_id, role_id)
);

INSERT INTO map_user_role (user_id, role_id) VALUES
    (1, 1),
    (2, 2);