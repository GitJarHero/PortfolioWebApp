-- Rollen-Tabelle
CREATE TABLE IF NOT EXISTS userrole (
    id SERIAL PRIMARY KEY UNIQUE NOT NULL,
    name VARCHAR(20) NOT NULL
);

INSERT INTO userrole (name) VALUES
    ('user'), ('admin');


-- Nutzerstatus-Tabelle
CREATE TABLE IF NOT EXISTS state (
    id SMALLSERIAL PRIMARY KEY,
    name VARCHAR(20) NOT NULL UNIQUE
);

INSERT INTO state (name) VALUES
    ('active'), ('inactive'), ('deleted');



CREATE OR REPLACE FUNCTION random_hex_color()
    RETURNS VARCHAR(7) AS $$
DECLARE
    r INT;
    g INT;
    b INT;
    luminance FLOAT;
BEGIN
    LOOP
        r := floor(random() * 256)::int;
        g := floor(random() * 256)::int;
        b := floor(random() * 256)::int;

        -- Luminanz berechnen
        luminance := 0.2126 * r + 0.7152 * g + 0.0722 * b;

        -- Nur Farbn mit mittlerer Helligkeit zulassen
        IF luminance > 64 AND luminance < 192 THEN
            RETURN '#' || lpad(to_hex(r), 2, '0') ||
                   lpad(to_hex(g), 2, '0') ||
                   lpad(to_hex(b), 2, '0');
        END IF;
    END LOOP;
END;
$$ LANGUAGE plpgsql;




CREATE TABLE IF NOT EXISTS appuser (
    id SERIAL PRIMARY KEY,
    username VARCHAR(20) NOT NULL, 
    password VARCHAR(255) NOT NULL,
    profile_color VARCHAR(7) NOT NULL DEFAULT random_hex_color(),
    stateid SMALLINT NOT NULL DEFAULT 1 REFERENCES state(id),
    lastonline TIMESTAMP,
    created TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS idx_appuser_username_lower ON appuser (LOWER(username));

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
    ('RoflRoflLol', '69u3bTSaHAXXHZ69PfUO2U4OnNDPlRS7exDy6OulJWXSySq0ikBZZAHaxyYNwlcc'),
    -- some more example users just so we get a result when we search for users in the app
    ('Alice', 'NmL+Nx1dviOWmnn8+CBS3cjZLIBSlnWNqP09rdCj5XT+uxcrQ6Ut1PMTUzNuWvZN'),
    ('Albert', 'gtRUDGT7vKbiHbcub7wKP3eqr3r/etvWDaGg9Tf7uy/pQztv3R14l3hTBzD7goaT'),
    ('Barbara', 'Vc+Z8CGlE7n1Fh2yPu4hHo3sjD4B+7fq9behuXi2qXnV31kjHryxYuI+sBPCfsv1'),
    ('Ben', 'O0gq6TNmSiRJIjkQezpjEnipxI8l+YTgEMViiNHWiNdaNzbHaR4h0HqHva+hbWdq'),
    ('Clara', '1n6i+Rao2mfg2iha/s2c/PsvDONTEdFQ6RmiG5dwLOb4MbBdTYSJYPjviljY8oFJ'),
    ('Chris', 'lFW2EJhblM/McpQvybp5Biurik0sJ9lPEAHsIrMYwR+j/zU7aQh9wA4BFmccym7u'),
    ('Diana', 'HsobfaYmcAacuybz332lXYEf1dfRCXycKhMM1R2PBjyce6oqxlhO8/WBHGSyPJu/'),
    ('Daniel', 'Y+2dKxE1y/SfwqIwfH7euxLXuRJTy7DREOhm7nIiWlJ3uR1SS0EauorQ+A+/0TnD'),
    ('Eva', 'bvNiJupgadTpjMgencZ92g69ka4dfFx3dEww9yHIkfT0/4TPLYH0nQI4Ee7sam09'),
    ('Edward', 'VgBdOkDw8XpqcASEnsONnYmC7rT7Ly3cbuyuYJa9mFrTQjZlxOQqdUjgV+vUB2Su'),
    ('Frida', 'EHC1aEYzC04giACA/j4SH6VkkKtllrHyeGbIPXuOuGkMGFjUtHppEL1kJqvIElxZ'),
    ('Felix', 'v8WKAfkx/2xEyEgQ/HDP2EiqJ3jQ8gn8eFMJCwedWmFCEfKOYmkjKXUOzT8mprYM'),
    ('Greta', 'KakcCDNBLArm41uSioRMvzaZKb4QvEC9fNnoGlMdkcWCv+2OdvarOAcTEj+9Wm9/'),
    ('George', '0au6y85TKEPq7kKqp7IXbPvOKKtTFPxQSdVs0I86eOjB+UAPGmFbbxXDe4ipEMjo'),
    ('Hanna', 'jKiJzzq3lHwR61cYZUjT2k4Tc/Xj739Af7y26SNt36EJ2jDcGnMx+/B/7i/PgNrN'),
    ('Henry', 'eNaP9dOGQA+AxGaTb7/1cxQjiQOlx5Rll0UaqqcSsODAWUe5lOThXcCQkIXBemXD'),
    ('Ida', 'eS+MqMsaUsTgo626izIyp2IPctQ8kLZZZ9JO3YTrSimTzyH7CzqvLuHPw7SBinog'),
    ('Ian', '+hjhFNKBwYTgVChXgkPblHmiZ5zEi5dbayBfXkw4YIQ5VqDyq7ASQikOsfIBjmc1'),
    ('Julia', '6V6oKRa1aLdtnH04CSke5iXBam5dt08LMbhc11fRlzwS/g8acb8GkDbvne4PJd1H'),
    ('Jonas', 'T/XmgKEzSllyB8oc4dUTH0uEcsi4Jh4MPccQvGz1QEdicntFicUfH9bYGINIbDOU'),
    ('Klara', 'ICQ7WUYnf3u2El6ibX1eNA3aPJBbIuaevAQPR/ZLllSaERuC2q0P9u0q5Bwmhx6S'),
    ('Kevin', 'TTpfFwYxPeodHO4dk/wCvTI8nS7+sSo5g9Q763fMsvjxdS7gxhbq8Yh8teBDRb5D'),
    ('Lena', 'zwuWjRmQuANY2OO1bTUc+5oFw7xVScFBiycSLc1t1UOYmzXQDOx63tjkBa/aieeI'),
    ('Leon', 'esVKANHcFt3HrehB3FulDJFIBs4l7mBboHnlbmn/VGv7PEvmcxuiQ/WMs/JvPCJA'),
    ('Mia', '0ncUCGy2xlmtTbQOpWf4ybA9YVZTyrBw0+QdmHrBFYSXLVhct9vrOZvrBlb1+gaS'),
    ('Max', 'R5N2Tyoz+rfFzOh7FrWxLhYJm/P33XOh53eD9laxo1gujgzTITAg5A6MFj4oI8WI'),
    ('Nora', 'K57UmueD7YguFFPb2OACiClMmjjrjBr8liYCLQBSmyj/yjtISrtg6tZa3v0ohdYn'),
    ('Noah', 'WSZUyxMuqC8jpsWDBKDsDa56LtNX02e3noRMe+wZSMjdkwfXB1yKMNL/Mpjzl4jz'),
    ('Olivia', 'ZDp/+xikXNvBifrFBPAWmz67Sk1a5YfA+muLDXRVaqqBzRfdoFXklxaEMYQIHnmO'),
    ('Oscar', 't8c0xLZS7hQaKrw9wAglHONYpW5Zrtzm+n8MPygCOcA+HDq4S/bpiXhOVes5bbg3'),
    ('Paula', 't01VikuAgxn4GXolcFC0ZgcjqYef5rVSEoP+ETA2v0jSOsIfKIcN/nvk5kboHwLA'),
    ('Phil', 'ra9WOGJ7on2gbY/1DC5JyDb3KbbXQ5XHZz3o7Pu5Kbctv3omd0OZHn4ghGdj6nyf'),
    ('Quinn', 'HoNszA29Al6DlUmLPwKu1bIZJmg3/PhgSscrpDJfDg7dZg3yWGR9tRFK5x8Pfi1/'),
    ('Quentin', 'yfg32CHklder3sli7jxleFT0r4ywZmdmW/zAXzRA9cORy0r66AXrH7ELLgxAfKWC'),
    ('Rosa', 'Vb7fExoaMPmWGjglJT037SbkdjCFpUTgxSqRYas9MVDm3fUuJc6l9tDMGRAsUtFq'),
    ('Robin', 'C4/qOVwsC4DyxN+gFMABVQyf3CvFJMpdGY8YAUs0DJZPhbr6ieXhqYXNvl4bgpJL'),
    ('Sara', 'pmDQti+RLor4bYMbqDxm03gEDn6CJ8VCl/HOSy8RFUbagkw6T9DVLeeya/tFRKZu'),
    ('Simon', 'DsbTuqM60s7EaJ6mzjKzVcZDxLwfzxQvR/MrGK/7azZhYIBUe1FrvOm+alkmYAyU'),
    ('Tina', 'PR/scvnzYEXNS3gqB+TfyeDFaPJq/wiNoSsjWWMAhOYgZ6HfWfRdGGLZAjZeiuA1'),
    ('Tom', 'fqgyHXJv9tka2l/EnBAT3Vtid/zXstwAtD2iiE24FZYpGj6aXxXoK9FDLDPxrx8f'),
    ('Ulla', 'QK/M5Yzk5H/nLB3l3rT05Yc3UcsMYjD/gJ8F5J3xdECS+7gCw6yFxT/PdQ4u2rFo'),
    ('Uwe', 'fduPGM/2Wdv4P/5QOMTJ0eV6ZKGdYeD7IcAKrrdtHFfBoRFT4l36zqt92cAj5c81'),
    ('Vera', 'SgnXDofCHuw1jq9Fttki6D07ZxArvSAiPP1FrA8AWHB423nmknjmkrA+++gdE7tJ'),
    ('Victor', '75ioOC96hzvvtjU5heBWMwEy6peKqDpSFI3p2jitdDgbjWOcxbuNdSXHcsiVtuay'),
    ('Wanda', '0v9QzSl4bdprwjSZHSXLiC+QJcvWhieIkjZ2sH/qjGGjO20/GVp7r3069DcXhZeD'),
    ('Walter', 'OZvKg5LnGBaHspcXMm2tXNqWvjCywfIi1bt1lMhE1A0+qku6Ir2IPxhpyUlML0/w'),
    ('Xenia', 'HcvyLuAePwgJbI/pNYwCEZwXBPfVDah4TsWHt2j1HuCsy28gIIAaC6Q5SSdRKQdF'),
    ('Xaver', 'NeUEm7+RNxwDKNWPD/VP7C18bCYCUEvMAA4+V4DEkb7C5czmmwOy212D6y1yZSXe'),
    ('Yara', 'wV/Wwz5q2afSaopCGGis21LqeZ75L0RdyazUx0YcZ3RM56G0ORVBTW/gNV2imdhe'),
    ('Yannic', 'Klvp+0Kk6ZlS5Sam6BilJLYCgfmZSaXsHEC1HOdnZc/Ng68uU6Z4tab3NHi1PBwH'),
    ('Zoe', 'QhCsEo7Z4oJJ80oSaFXLw8Kci4Q12N8k7rjD0KqfQue20/FeVWgvvM5C5ceJyXkA'),
    ('Zack', 'QBB63HY9ylKmnok7Sl7jfktpGFlYe0xsFgmgz9EYAALdPlADej5DO/eBnMaAHtIw');


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


CREATE TABLE IF NOT EXISTS globalmessage (
    id SERIAL PRIMARY KEY,
    content VARCHAR(255) NOT NULL,
    userid INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    stateid SMALLINT NOT NULL DEFAULT 1 REFERENCES state(id),
    created TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insert 30 sample global messages with logical timestamps
INSERT INTO globalmessage (content, userid, created)
VALUES
    ('Hello everyone!', 1, '2025-05-01 08:12:00'),
    ('What are you working on today?', 2, '2025-05-01 09:45:00'),
    ('I really enjoy using Blazor.', 3, '2025-05-01 11:30:00'),
    ('Anyone here tried SignalR?', 4, '2025-05-01 13:10:00'),
    ('Good vibes only!', 5, '2025-05-01 14:50:00'),
    ('Let’s build something cool.', 6, '2025-05-01 16:05:00'),
    ('Good morning from Germany!', 7, '2025-05-02 08:00:00'),
    ('Looking for coding buddies.', 8, '2025-05-02 09:20:00'),
    ('How do you structure your projects?', 9, '2025-05-02 10:40:00'),
    ('SignalR is magic!', 10, '2025-05-02 12:00:00'),
    ('Nice to meet you all!', 11, '2025-05-02 13:15:00'),
    ('Working on some frontend stuff.', 1, '2025-05-02 14:25:00'),
    ('Blazor Server or WASM?', 2, '2025-05-02 15:45:00'),
    ('Just fixed a nasty bug.', 3, '2025-05-03 09:10:00'),
    ('This app is coming along well.', 4, '2025-05-03 10:30:00'),
    ('Anyone doing mobile dev?', 5, '2025-05-03 11:50:00'),
    ('PostgreSQL is awesome!', 6, '2025-05-03 13:15:00'),
    ('What’s your favorite IDE?', 7, '2025-05-03 14:25:00'),
    ('.NET 8 feels smooth.', 8, '2025-05-03 15:40:00'),
    ('UI is looking clean.', 9, '2025-05-03 17:00:00'),
    ('Learning Entity Framework Core.', 10, '2025-05-04 08:30:00'),
    ('Any Git tips?', 11, '2025-05-04 09:45:00'),
    ('Time for a coffee break ☕', 1, '2025-05-04 11:00:00'),
    ('Chat systems are fun to build.', 2, '2025-05-04 12:10:00'),
    ('C# is powerful and elegant.', 3, '2025-05-04 13:20:00'),
    ('Hello from the other side.', 4, '2025-05-04 14:35:00'),
    ('Debugging is my cardio.', 5, '2025-05-04 15:50:00'),
    ('Everything compiles, yay!', 6, '2025-05-05 08:10:00'),
    ('Creating reusable components.', 7, '2025-05-05 09:25:00'),
    ('Cheers from the dev cave!', 8, '2025-05-05 10:40:00');


CREATE TABLE IF NOT EXISTS friendrequest (
    id          SERIAL PRIMARY KEY,
    from_user   INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    to_user     INT NOT NULL REFERENCES appuser(id) ON DELETE CASCADE,
    created     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Optional: prevent doubling requests
    CONSTRAINT unique_friend_request UNIQUE (from_user, to_user)
);

CREATE OR REPLACE FUNCTION prevent_reverse_friendship()
    RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM friendrequest
        WHERE from_user = NEW.to_user AND to_user = NEW.from_user
    ) THEN
        RAISE EXCEPTION 'Reverse friendship request already exists';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER prevent_reverse_friendship_trigger
    BEFORE INSERT ON friendrequest
    FOR EACH ROW
EXECUTE FUNCTION prevent_reverse_friendship();




CREATE TABLE IF NOT EXISTS directmessage (
    id          SERIAL PRIMARY KEY,
    content     TEXT NOT NULL,
    "from"      INT NOT NULL REFERENCES appuser(id),
    "to"        INT NOT NULL REFERENCES appuser(id),
    created     TIMESTAMP NOT NULL,
    delivered   TIMESTAMP,
    read        TIMESTAMP,
    
    CHECK ("from" <> "to")
);
