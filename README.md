# MailApp API on C# (ASP.NET CORE)

### Тестовое [задание](https://digdes-my.sharepoint.com/:w:/p/stolovaya_m/EUGkQmQpvrtJpRKo8DkV7r8BMqUpyDvZFTaLcd0SPOV93g?rtime=_vJc2mLt2Ug) для компании [Docsvision](https://docsvision.com/).

В качестве базы данных использована PostgreSQL (Npgsql C#).
Была выбрана простая [структура](./MailApp/Migrations/Initial_Up.sql) с одной таблицей 
в БД, однако могут быть созданы дополнительные сущности. Примерная структура 
альтернативной (расширенной) версии отображена в дополнительном файле 
[миграции](./MailApp/Migrations/Alternative_Up.sql). 

Для описания API использован Swagger. При запуске в dev окружении доступен
по адресу `GET /swagger`.