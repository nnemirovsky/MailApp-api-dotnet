DROP TABLE IF EXISTS letters;

CREATE TABLE letters
(
    id         SERIAL         NOT NULL PRIMARY KEY,
    subject    text           NOT NULL,
    created_at timestamp      NOT NULL DEFAULT now(),
    recipients VARCHAR(255)[] NOT NULL,
    sender     VARCHAR(255)   NOT NULL,
    tags       varchar(255)[] NOT NULL,
    body       text           NOT NULL
);
