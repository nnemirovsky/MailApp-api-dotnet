DROP TABLE IF EXISTS letters;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS letters_recipients;
DROP TABLE IF EXISTS tags;
DROP TABLE IF EXISTS letters_tags;

CREATE TABLE letters
(
    id         SERIAL                    NOT NULL PRIMARY KEY,
    subject    VARCHAR(255)              NOT NULL,
    created_at timestamp                 NOT NULL DEFAULT now(),
    sender_id  int references users (id) NOT NULL,
    body       text                      NOT NULL
);

CREATE TABLE users
(
    id    SERIAL       NOT NULL PRIMARY KEY,
    name  VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL
);

CREATE TABLE letters_recipients
(
    letter_id    int references letters (id) NOT NULL,
    recipient_id int references users (id)   NOT NULL
);

CREATE TABLE tags
(
    id   SERIAL       NOT NULL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

CREATE TABLE letters_tags
(
    letter_id int references letters (id) NOT NULL,
    tag_id    int references tags (id)    NOT NULL
);
