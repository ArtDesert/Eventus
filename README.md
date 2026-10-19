# Eventus - сервис для управления мероприятиями.
## Stack: ASP.NET Core, .NET 10.

### Eventus.WebApi
1. [GET] /api/events - возвращает список всех мероприятий. 
2. [GET] /api/events/{id} - возвращает меропритие по его идентификатору.
3. [POST] /api/events - создаёт новое мероприятие.
4. [PUT] /api/events/{id} - обновляет мероприятие.
5. [DELETE] /api/events/{id} - удаляет мероприятие.

При отсутствии данных о мероприятии возврашается код 404 NotFound.

### Запуск сервиса
Из корневой директории выполнить команды:
```
dotnet build
```

```
dotnet run --project ./Eventus.WebApi/Eventus.WebApi.csproj
```

Веб сервер будет доступен по адресу http://localhost:5240

Сваггер http://localhost:5240/swagger/index.html 