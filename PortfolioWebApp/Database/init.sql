-- Rollen-Tabelle
CREATE TABLE IF NOT EXISTS userrole (
    id SERIAL PRIMARY KEY UNIQUE NOT NULL,
    name VARCHAR(20) NOT NULL
);

INSERT INTO userrole (name) VALUES
    ('user'), ('admin');


-- Nutzerstatus-Tabelle
CREATE TABLE IF NOT EXISTS userstate (
    id SMALLSERIAL PRIMARY KEY,
    name VARCHAR(20) NOT NULL UNIQUE
);

INSERT INTO userstate (name) VALUES
    ('active'), ('inactive'), ('deleted');


-- Benutzer-Tabelle
CREATE TABLE IF NOT EXISTS appuser (
    id SERIAL PRIMARY KEY,
    username VARCHAR(20) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    stateid SMALLINT NOT NULL DEFAULT 1 REFERENCES userstate(id),
    lastonline TIMESTAMP,
    created TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO appuser (username, password) VALUES
    ('test123', 'cZ4W0HdMlQWfb7kZpUVM0gd3qtvUNJ+LdVWAUoRdYdnYwlH8UmAJbORrmfPSOlUa'),
    ('admin123', 'C8K9PANt9g2Z/9Y9gdpEQSDwjWaxmUjCexzqaaxr+FdqB4kZtuXm+OgmKXN1CutW'),
    ('Max123', '4W5sCzmrZQCOUnWmH9t4/iJNOqpx9pzp+ukvmaKRMn+URPI2inB+3KWeFY+CADC0'),
    ('Mr.Blazor', '/c9pCH3RuEpuGdS+jIPTgF7mb0tiRVE+B+KqlUGM8V+2j/WupfiXDngy4qjcKL+e'),
    ('TomTom112', 'j4obk+8YOqF+6zE4vH68cauU9sG6iq24zx9B3UhEGa/qgXMxkC6kbafIyDCpp6pi'),
    ('LarryL', '6NJ7AHH4w6R9yKhnH4YNm3RWDBXUiED6m1qM6/NtTMT43kOjuEWzpVuM66YAQX94'),
    ('BerndTheGiant', 'XgcTJ9nvkUKn5xOPJug//4GeYJPYkXUGlpPpFRuIBIgWPGj5ZeLXGaeMF9LCOTbX'),
    ('SomeUser', '2lRH6jxHY7FVsx4Wx97Q7f02tpw3PNMqNEfMQbYWd+zHQ2ikjyOuIAf44aYKvRlC'),
    ('WhoAmI', 'auhfdZCVtgjPe2S4ZRENmTboP++bIVOpXODMSoAQxkZiMosd4nYk83q19R0qDJEh'),
    ('BliBlaBlubb', 'hSp1AXwPEG2TdXenMinWJIguPo9ttVq8w91Mf4YXzqydti6jPgv14TVS8KxSY1Wt'),
    ('RoflRoflLol', '69u3bTSaHAXXHZ69PfUO2U4OnNDPlRS7exDy6OulJWXSySq0ikBZZAHaxyYNwlcc');



-- Benutzer-Rollen-Zuordnung
CREATE TABLE IF NOT EXISTS map_user_role (
    user_id INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    role_id SMALLINT NOT NULL REFERENCES userrole(id),
    PRIMARY KEY (user_id, role_id)
);

INSERT INTO map_user_role (user_id, role_id) VALUES
    (1, 1),
    (2, 2);


CREATE TABLE IF NOT EXISTS friendship (
    id SERIAL PRIMARY KEY,
    user1 INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    user2 INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    created TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user1, user2)
);

-- Make user 'test123' friends with everybody
INSERT INTO friendship (user1, user2)
SELECT 1 AS user1, id AS user2
FROM appuser
WHERE id != 1;
