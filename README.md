# ✈️ Airline Ticket System - Професионална система за управление на авиобилети

## 📋 Съдържание
- [1. Въведение](#1-въведение)
- [2. Системни изисквания](#2-системни-изисквания)
- [3. Инсталация и настройка](#3-инсталация-и-настройка)
- [4. Архитектура на системата](#4-архитектура-на-системата)
- [5. Потребителски роли и права](#5-потребителски-роли-и-права)
- [6. Основни функционалности](#6-основни-функционалности)
- [7. Разширени възможности](#7-разширени-възможности)
- [8. Администрация](#8-администрация)
- [9. Техническа документация](#9-техническа-документация)
- [10. Поддръжка и поддръжка](#10-поддръжка-и-поддръжка)

------------------------------------------------------------------------

## 1. Въведение

**Airline Ticket System** е модерна, професионална уеб базирана информационна система за пълното управление на авиокомпания - от резервация на билети до финансови отчети и административно управление.

### 🌟 Ключови предимства
- **Професионален дизайн** с авиационна стилистика
- **Пълно управление на полети** с разписание и статуси
- **Разширена система за резервации** с PNR кодове и плащания
- **Мощни търсачки** с множество критерии и сортиране
- **Детайлни отчети** за анализ на бизнеса
- **Автоматизирани имейл уведомления**
- **Професионално ниво на сигурност**

### 🏗️ Технологичен стек
- **Backend**: .NET Core ASP.NET Core MVC (не REST API)
- **Database**: Entity Framework Core с SQL Server (DefaultConnection)
- **Frontend**: Razor Views с Bootstrap и професионална авиационна стилистика
- **Authentication**: ASP.NET Core Identity с cookie authentication и ролево-базиран достъп
- **Email**: Razor template-based система с SMTP (SmtpClient)
- **Background Services**: Hosted Services за автоматични reminder
- **Logging**: ILogger интеграция
- **Testing**: AirlineTicketSystem.Tests проект

------------------------------------------------------------------------

## 2. Системни изисквания

### 📋 Минимални изисквания
- **Операционна система**: Windows 10/11, macOS 11+, Linux (Ubuntu 20.04+)
- **.NET Runtime**: .NET 10 SDK
- **База данни**: SQL Server 2019+, SQL Server Express, LocalDB
- **Браузър**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **RAM**: Минимум 4GB, препоръчително 8GB+
- **Диск**: Минимум 2GB свободно пространство

### 🔧 Препоръчителни изисквания за production
- **CPU**: 4+ ядра
- **RAM**: 16GB+
- **База данни**: SQL Server Standard/Enterprise
- **SSL сертификат** за HTTPS
- **Backup система** за база данни

------------------------------------------------------------------------

## 3. Инсталация и настройка

### 🚀 Бърз старт

1. **Клониране на проекта**
```bash
git clone [repository-url]
cd airline_tickets
```

2. **Инсталиране на зависимости**
```bash
dotnet restore
```

3. **Настройка на базата данни**
```bash
# Създаване на миграцията (ако е необходимо)
dotnet ef migrations add InitialCreate

# Прилагане на миграцията
dotnet ef database update
```

4. **Стартиране на приложението**
```bash
dotnet run --project AirlineTicketSystem
```

5. **Достъп до системата**
- Отворете браузър на: `https://localhost:5001`
- Първоначален админ акаунт се създава автоматично

### ⚙️ Подробна конфигурация

#### 3.1 База данни
Редактирайте `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AirlineTicketSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

#### 3.2 Администраторски акаунт
```json
{
  "AdminUser": {
    "Email": "admin@airlinetickets.bg",
    "Password": "Admin@123456",
    "Name": "System Administrator"
  }
}
```

#### 3.3 Имейл система (препоръчително за production)

**Gmail SMTP настройка:**
1. **Създайте Gmail акаунт** специално за системата
2. **Активирайте 2-Factor Authentication** 
3. **Генерирайте App Password** (16 символа)
4. **Конфигурирайте appsettings.json:**

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "notifications@yourairline.bg",
    "SenderName": "Airline Ticket System",
    "Username": "notifications@yourairline.bg", 
    "AppPassword": "abcd-efgh-ijkl-mnop",
    "EnableSsl": true,
    "MaxEmailsPerHour": 100
  }
}
```

**Development режим:**
- Ако SMTP настройките са празни, системата работи в development режим
- Имейлите се логират в конзолата вместо да се изпращат
- Идеално за локално тестване и разработка

------------------------------------------------------------------------

## 4. Архитектура на системата

### 🏛️ Слоеста архитектура

#### 4.1 Presentation Layer (Презентационен слой)
- **Controllers**: Обработват HTTP заявки и връщат отговори
- **Views**: Razor страници за потребителския интерфейс
- **ViewModels**: Модели за пренос на данни между контролери и изгледи
- **Middleware**: Обработка на заявки и сигурност

#### 4.2 Business Logic Layer (Бизнес логика)
- **Services**: Основната бизнес логика (FlightService, BookingService, ReportService)
- **Interfaces**: Абстракции за услугите
- **Domain Models**: Бизнес обекти и правила
- **Validators**: Валидация на бизнес правила

#### 4.3 Data Access Layer (Достъп до данни)
- **Repositories**: Абстракция на достъпа до данни
- **DbContext**: Entity Framework контекст
- **Entities**: Модели на базата данни
- **Migrations**: Промени в схемата на базата данни

#### 4.4 Infrastructure Layer (Инфраструктура)
- **Email Services**: Изпращане на имейли с Razor templates
- **Logging**: ILogger структурирано логиране
- **Configuration**: appsettings.json и IOptions pattern
- **Background Services**: FlightReminderBackgroundService (hosted service)

### 🔄 Модел на данните

```
ApplicationUser (Потребители) - extends IdentityUser
├── FirstName, FamilyName, Email (от Identity)
├── IsActive (bool активен/неактивен)
└── AspNetUserRoles → AspNetRoles (Admin, Operator, User)

Flight (Полети)
├── FlightNumber (номер на полет)
├── DepartureCity, ArrivalCity
├── DepartureDateTime, ArrivalDateTime 
├── Duration (минути), Price (decimal)
├── Capacity (int капацитет)
├── Status (string статус на полета)
├── Gate (nullable string)
└── RowVersion (за concurrency control)

FlightPassenger (Резервации) - many-to-many junction
├── PNR (уникален 6-char код)
├── BookingStatus ("Confirmed"/"Cancelled")
├── PaymentAmount, PaymentStatus ("Captured"/"Refunded"/"Forfeited")
├── RefundAmount, CancelledAt
├── CreatedAt, CreatedByUserId
└── FlightId, PassengerId (foreign keys)

Passenger (Пасажери)
├── FirstName, FamilyName
├── Email (nullable, добавен в миграция)
└── FlightPassengers navigation
```

------------------------------------------------------------------------

## 5. Потребителски роли и права

### 👑 Администратор
**Пълен достъп до системата**
- ✅ Създаване, редактиране и изтриване на полети
- ✅ Управление на потребители (без други админи)
- ✅ Активиране/деактивиране на акаунти (с изключение на себе си)
- ✅ Създаване на оператори
- ✅ Достъп до всички отчети (дневни, статистики, финансови)
- ✅ Преглед на всички резервации по PNR
- ✅ Правене на резервации за клиенти
- ✅ Автоматични имейл уведомления при промени в полети
- ❌ Не може да деактивира себе си или други админи

### 🧑‍💼 Оператор
**Ограничен административен достъп**
- ✅ Преглед на всички полети и детайли (анонимно)
- ✅ Правене на резервации за клиенти
- ✅ Търсене на резервации по PNR
- ✅ Достъп до дневни отчети и статистики за резервации
- ❌ Не може да създава/редактира/изтрива полети
- ❌ Не може да управлява потребители
- ❌ Няма достъп до финансови отчети

### 👤 Потребител
**Самообслужване и лични резервации**
- ✅ Търсене и филтриране на полети (анонимно)
- ✅ Преглед на детайли за полети (анонимно)
- ✅ Правене на резервации (предимно за себе си)
- ✅ Преглед на собствените резервации
- ✅ Отмяна на резервации (24+ часа = пълно възстановяване, по-малко = 0)
- ✅ Редактиране на личен профил (име, фамилия, имейл)
- ✅ Търсене на собствени резервации по PNR
- ❌ Достъп само до собствените резервации
- ❌ Няма административни права

------------------------------------------------------------------------

## 6. Основни функционалности

### ✈️ Управление на полети

#### За администратори:
- **Създаване на нови полети** с пълна информация
  - Номер на полет (автоматично ToUpperInvariant), маршрут, разписание
  - Продължителност в минути, капацитет, цена, опционален гейт
  - Статус се задава автоматично като "Scheduled"
  - ArrivalDateTime = DepartureDateTime + Duration
- **Редактиране на съществуващи полети**
  - Ако няма потвърдени резервации: пълно редактиране
  - С потвърдени резервации: само разписание/статус/гейт (не капацитет/цена/маршрут)
  - Автоматични уведомления до CreatedByUser.Email при значителни промени
- **Изтриване на полети** (GET заявка - само при липса на потвърдени резервации)

### 🎫 Система за резервации

#### Реализирани възможности:
- **Уникални PNR кодове** (6 символа) за всяка резервация
- **Вътрешно отчитане на плащания** (без реален PSP) - статуси "Captured", "Refunded", "Forfeited"
- **Автоматично управление на капацитета** на полетите с проверка за наличност
- **Защита от двойни резервации** (unique constraint за потвърдени резервации)
- **Система за отмени** с автоматично изчисляване: ≥24ч = пълно възстановяване, <24ч = 0
- **Трансакционна сигурност** със serializable isolation level

#### Процес на резервация:
1. **Търсене** на подходящ полет (с филтри и сортиране)
2. **Избор** на полет и въвеждане на пасажерски данни
3. **Валидация** на капацитет и уникалност на резервацията
4. **Генериране на PNR** и задаване на PaymentAmount = цена на полета
5. **Автоматично имейл** уведомление (ако SMTP е конфигуриран)
6. **Възможност за отмяна** с изчисляване на възстановяване

### 🔍 Търсачка на полети

#### Реализирани критерии за търсене:
- **Основни**: DepartureCity и ArrivalCity (точно съвпадение)
- **Дати**: FromDate и ToDate (период)
- **Цени**: MinPrice и MaxPrice (граници)
- **Статус**: филтриране по статус на полета
- **Пагинация**: Page и PageSize (по подразбиране 15 на страница)

#### Сортиране по (SortBy parameter):
- "departure" - време на заминаване (по подразбиране)
- "price" - цена
- "capacity" - капацитет
- Други критерии според FlightSearchCriteria

#### Реализирани функции:
- **Пагинация** с TotalCount за големи резултати
- **POST форма с Reset** за изчистване на търсенето
- **Анонимен достъп** - не изисква логин за търсене
- Резултатите включват само полети със Status и налични места

------------------------------------------------------------------------

## 7. Разширени възможности

### 📊 Отчети и анализи

#### 7.1 Дневни отчети за полети [Admin,Operator]
- Списък с всички полети за избрана дата (по подразбиране днес)
- Статус на всеки полет и load factor (заети/общо места)
- Обобщена статистика за деня
- Достъпно за Admin и Operator роли

#### 7.2 Статистики за резервации [Admin,Operator]
- **Общо резервации**: потвърдени vs отменени
- **Популярни маршрути**: най-търсените дестинации (топ маршрути)
- **Заетост на полетите**: процент запълнени места
- Агрегирана статистика без дата филтри

#### 7.3 Финансови отчети [Admin само]
- **Общи приходи** за период (опционални from/to дата филтри)
- **Gross приходи** vs **Възстановявания** (refunds)
- **Нетни приходи** (gross - refunds)
- Достъпно само за Admin роля

### 📧 Професионална система за имейл уведомления

#### Архитектура на имейл системата:
- **Template-базирана система** с Razor Views
- **SMTP интеграция** чрез MailKit (Gmail поддръжка)
- **Автоматизиран Background Service** за напомняния
- **Rate limiting** - защита от спам (100 имейла/час)
- **Development режим** - логиране вместо изпращане за тестване
- **Error resilience** - неуспешни имейли не блокират операции

#### Пълен набор от имейл шаблони:

**🔐 Управление на акаунти:**
- **Добре дошли** (`WelcomeRegistration.cshtml`) - при регистрация
- **Създаден оператор** (`OperatorAccountCreated.cshtml`) - за нови оператори
- **Промяна статус** (`AccountStatusChanged.cshtml`) - активиране/деактивиране

**🎫 Управление на резервации:**
- **Потвърждение резервация** (`BookingConfirmation.cshtml`) - с PNR и детайли
- **Отмяна резервация** (`BookingCancelled.cshtml`) - с възстановяване
- **Напомняне за полет** (`FlightReminder.cshtml`) - 24 часа преди заминаване

**✈️ Управление на полети:**
- **Промяна разписание** (`FlightScheduleChanged.cshtml`) - уведомява всички пасажери

#### Професионален дизайн и функции:
- **Авиационна стилистика** с синя градиентна тема
- **Responsive дизайн** за всички устройства
- **Българска локализация** с професионален тон
- **HTML + Plain Text** версии за съвместимост
- **Персонализирано съдържание** с данни от системата
- **Брандинг** с лого и контактна информация
- **Директни линкове** към релевантни страници в системата

#### Автоматизация и интеграция:
- **Автоматични уведомления** при всички важни действия
- **Background service** за flight reminders (стартира всеки час)
- **Контролерна интеграция** в AccountController, BookingController, FlightController
- **Проследяване на грешки** с подробно логиране
- **Rate limiting** за предотвратяване на злоупотреби

#### SMTP конфигурация:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-airline@gmail.com", 
    "SenderName": "Airline Ticket System",
    "Username": "your-airline@gmail.com",
    "AppPassword": "your-16-char-app-password",
    "EnableSsl": true,
    "MaxEmailsPerHour": 100
  }
}
```

### 🔐 Сигурност и одитиране

#### Мерки за сигурност:
- **Ролево-базиран достъп** с детайлни права
- **Валидация** на всички входни данни
- **Защита от CSRF** атаки
- **Secure headers** за HTTPS
- **Логиране на действия** за одит

#### Одитиране:
- Проследяване на всички промени в данните
- Логиране на неоторизиран достъп
- Мониторинг на системните грешки
- Автоматично архивиране на логове

------------------------------------------------------------------------

## 8. Администрация

### 🛠️ Административен панел

#### Управление на потребители:
- **Преглед на всички потребители** с роли и статус
- **Активиране/деактивиране** на акаунти
- **Създаване на оператори** с автоматичен имейл
- **Промяна на роли** и права
- **Проследяване на активността** на потребителите

#### Управление на системата:
- **Основен CRUD** за потребители и полети
- **ILogger логиране** в конзолата и файлове
- **appsettings.json** конфигурация
- **Entity Framework миграции** за база данни
- **Manual deployment** и обновяване

### 📊 Налични отчети и статистики

#### Реализирани отчети:
- **Дневни полети** (/Reports/DailyFlights) - списък за дата с load factor
- **Статистики за резервации** (/Reports/BookingStatistics) - общо/отменени + топ маршрути  
- **Финансови отчети** (/Reports/Financial) - приходи/възстановявания за период
- **Лични резервации** (/Booking/MyBooked) - за всеки потребител

#### Логиране и мониторинг:
- **ILogger интеграция** в всички ключови операции
- **Email операции** - успешни/неуспешни изпращания
- **Background service** - Flight reminder execution
- **Database операции** - booking conflicts, concurrency
- **Authentication** - login attempts, account status changes

------------------------------------------------------------------------

## 9. Техническа документация

### 🔧 MVC Контролери и действия

#### Flight Management (FlightController)
```
GET    /Flight                    - Списък полети с търсене и пагинация (анонимно)
GET    /Flight/Details/{id}       - Детайли за полет (анонимно)
GET    /Flight/Create             - Форма за създаване на полет [Admin]
POST   /Flight/Create             - Създаване на полет [Admin]
GET    /Flight/Edit/{id}          - Форма за редактиране [Admin]
POST   /Flight/Edit/{id}          - Редактиране на полет [Admin] + Email уведомления
GET    /Flight/Delete/{id}        - Изтриване на полет [Admin] (само без резервации)
POST   /Flight/Reset              - Нулиране на търсенето
GET    /Flight/BookSeat/{id}      - Пренасочване към резервация [Authorize]
```

#### Booking Management (BookingController) - [Authorize на целия контролер]
```
GET    /Booking/Create?id={flightId}  - Форма за резервация
POST   /Booking/Create                - Нова резервация + Confirmation email
GET    /Booking/MyBooked              - Лични резервации на потребителя
POST   /Booking/Cancel/{id}           - Отмяна на резервация + Cancellation email
GET    /Booking/ByPnr?pnr={code}      - Търсене по PNR код (собственик/Admin/Operator)
```

#### Reports & Analytics (ReportsController) - [Authorize]
```
GET    /Reports/DailyFlights?day={date}     - Дневен отчет [Admin,Operator]
GET    /Reports/BookingStatistics           - Статистики за резервации [Admin,Operator]  
GET    /Reports/Financial?from={date}&to={date} - Финансов отчет [Admin]
```

#### User Management (AccountController)
```
GET    /Account/Register          - Форма за регистрация (анонимно)
POST   /Account/Register          - Регистрация потребител + Welcome email [AllowAnonymous]
GET    /Account/Login             - Форма за вход (анонимно)
POST   /Account/Login             - Вход в системата + проверка IsActive
POST   /Account/Logout            - Изход от системата
GET    /Account/RegisterOperator  - Форма за оператор (няма [Authorize]!)
POST   /Account/RegisterOperator  - Създаване оператор [Admin] + Welcome email
GET    /Account/EditProfile       - Редактиране на профил [User]
POST   /Account/EditProfile       - Обновяване на профил [User]
GET    /Account/Users             - Списък потребители (не Admin) [Admin]
POST   /Account/ToggleUserStatus  - Активиране/деактивиране [Admin] + Status email
GET    /Account/AccessDenied      - Страница за отказан достъп
```

### 📧 Email Service API

#### IEmailService Interface
```csharp
Task SendWelcomeAsync(string toEmail, string displayName);
Task SendOperatorCreatedAsync(string toEmail, string displayName);
Task SendAccountActiveChangedAsync(string toEmail, string displayName, bool isActive);
Task SendBookingConfirmationAsync(string toEmail, string pnr, Flight flight, Passenger passenger);
Task SendBookingCancelledAsync(string toEmail, string pnr, decimal? refundAmount);
Task SendFlightScheduleChangedAsync(string toEmail, Flight flight);
Task SendFlightReminderAsync(string toEmail, string pnr, Flight flight, Passenger passenger);
```

#### Email Template Structure (Проверена структура)
```
Views/EmailTemplates/
├── Shared/_EmailLayout.cshtml     - Основен layout с airline брандинг
├── Account/                       - Акаунт управление
│   ├── WelcomeRegistration.cshtml        - При регистрация на потребител
│   ├── OperatorAccountCreated.cshtml     - При създаване на оператор
│   └── AccountStatusChanged.cshtml       - При активиране/деактивиране
├── Booking/                       - Резервации  
│   ├── BookingConfirmation.cshtml        - Потвърждение на резервация с PNR
│   ├── BookingCancelled.cshtml           - Отмяна с възстановяване
│   └── FlightReminder.cshtml             - 24ч напомняне (background service)
└── Flight/                        - Полети
    └── FlightScheduleChanged.cshtml      - Промяна в разписание/статус/гейт
```

#### Background Services
```
FlightReminderBackgroundService:
- Изпълнява се: Всеки час (hosted service)
- Цел: Намира полети със статус "Scheduled", заминаващи в 24-25 часа  
- Действие: Изпраща flight reminder имейли на CreatedByUser.Email (не Passenger.Email)
- Ограничение: Няма флаг за изпратено напомняне (може да се повтори)
- Логиране: Подробно проследяване на всички операции
- Placeholder линк: /Booking/Checkin/{pnr} (не съществува в BookingController)
```

### 🗄️ База данни

#### Основни таблици:
- **AspNetUsers** - Потребители с Identity
- **Flights** - Полети с пълна информация
- **Passengers** - Пасажерски данни
- **FlightPassengers** - Резервации (many-to-many)

#### Индекси за производителност:
- `IX_Flights_DepartureCity` - търсене по град на заминаване
- `IX_Flights_ArrivalCity` - търсене по град на пристигане  
- `IX_Flights_DepartureDateTime` - търсене по дата
- `IX_FlightPassengers_PNR_Unique` - уникален PNR код
- `IX_FlightPassengers_CreatedAt` - сортиране по дата

### 🧪 Тестване

#### Unit Tests
- **Service Layer Tests**: Бизнес логика
- **Repository Tests**: Достъп до данни
- **Controller Tests**: HTTP заявки и отговори
- **Model Tests**: Валидация на данни

#### Integration Tests  
- **End-to-End тестове**: Цели потребителски сценарии
- **Database тестове**: Работа с реална база данни
- **Authentication тестове**: Сигурност и роли
- **Email тестове**: Изпращане на уведомления и template rendering

#### Изпълнение на тестове:
```bash
# Всички тестове
dotnet test

# Само unit тестове
dotnet test --filter Category=Unit

# С покритие на кода
dotnet test --collect:"XPlat Code Coverage"
```

------------------------------------------------------------------------

## 10. Поддръжка и поддръжка

### 🚨 Известни проблеми и ограничения

#### Сигурностни забележки:
**⚠️ ВАЖНО: FlightController.Delete е GET заявка**
```bash
# ПРОБЛЕМ: Delete операцията използва GET вместо POST/DELETE
# РЕШЕНИЕ: Трябва да се добави [HttpPost] attribute
# Текущо поведение: /Flight/Delete/{id} може да се извика с GET
# Риск: CSRF атаки въпреки [Authorize(Roles = "Admin")]
```

**⚠️ RegisterOperator GET не изисква Authorization**
```bash
# ПРОБЛЕМ: GET /Account/RegisterOperator няма [Authorize(Roles = "Admin")]
# POST операцията е защитена, но формата може да се достъпи анонимно
# РЕШЕНИЕ: Добавяне на [Authorize(Roles = "Admin")] на GET action
```

#### Функционални ограничения:
- **Checkin функционалност**: Email templates сочат към /Booking/Checkin/{pnr} но не съществува
- **Background reminders**: Няма флаг за изпратено напомняне (може да се повтори в същия 1-часов прозорец)
- **Payment processing**: Само вътрешно отчитане, няма интеграция с реален PSP
- **ActiveAccountMiddleware**: Документиран като "mock" - само логира, не блокира неактивни потребители на request ниво

### 🚨 Известни проблеми и решения

#### Проблеми с базата данни:
**Проблем**: Грешка при миграция
```bash
# Решение: Изтриване и пресъздаване
dotnet ef database drop
dotnet ef database update
```

**Проблем**: Бавна производителност
```sql
-- Решение: Преиндексиране на таблиците
EXEC sp_recompile 'Flights';
EXEC sp_recompile 'FlightPassengers';
```

#### Проблеми с имейлите:
**Проблем**: Неуспешно изпращане на имейли
```bash
# Решение: Проверка на SMTP настройки
# 1. Проверете appsettings.json EmailSettings секцията
# 2. Уверете се, че Gmail App Password е правилно въведен (16 символа)
# 3. Проверете Gmail 2FA е активиран
# 4. Проверете firewall настройките за port 587
```

**Проблем**: Имейлите не се изпращат в development
```bash
# Решение: Това е нормално поведение
# В development режим имейлите се логират вместо да се изпращат
# Проверете конзолата за "DEVELOPMENT EMAIL" съобщения
# За тестване на истинско изпращане, конфигурирайте SMTP настройките
```

**Проблем**: Gmail Authentication грешка
```bash
# Решение: App Password конфигурация
# 1. Отидете на Google Account Settings
# 2. Security > 2-Step Verification > App passwords  
# 3. Генерирайте нов App Password за "Mail"
# 4. Използвайте 16-символния код в AppPassword полето
```

**Проблем**: Rate limiting - твърде много имейли
```bash
# Решение: Регулиране на честотата
# Системата автоматично ограничава до MaxEmailsPerHour
# Проверете логовете за "Email rate limit exceeded" съобщения
# При нужда увеличете MaxEmailsPerHour в конфигурацията
```

**Проблем**: Email templates не се рендират правилно  
```bash
# Решение: Template грешки
# 1. Проверете че всички .cshtml файлове са в правилните папки
# 2. Проверете Views/EmailTemplates/ структурата
# 3. Прегледайте логовете за "template rendering" грешки
# 4. Уверете се че моделите съдържат необходимите данни
```

#### Проблеми с производителността:
**Проблем**: Бавно зареждане на страниците
- Проверете database connection string
- Мониторирайте системните ресурси
- Разгледайте увеличаване на RAM

### 📞 Техническа поддръжка

#### Как да получите помощ:
1. **Проверете документацията** - този README файл
2. **Прегледайте логовете** в `Logs/` директорията  
3. **Потърсете в Issues** раздела на проекта
4. **Създайте нов Issue** с детайли за проблема

#### При създаване на Issue включете:
- **Версия** на системата и .NET
- **Операционна система** и браузър
- **Стъпки за възпроизвеждане** на проблема
- **Съобщения за грешки** от логовете
- **Екранни снимки** ако е приложимо

### 🔄 Обновления

#### Регулярни обновления:
- **Security patches** - веднага при наличност
- **Feature updates** - месечно или при нужда
- **Database migrations** - автоматично или ръчно

#### Процес на обновяване:
1. **Backup** на данните и настройките
2. **Тестване** в staging среда
3. **Приложение** на обновленията
4. **Верификация** че всичко работи правилно
5. **Мониторинг** за първите няколко дни

------------------------------------------------------------------------

## 📝 Лиценз и авторски права

Този проект е разработен като професионална система за управление на авиобилети. Всички права запазени.

### 🤝 Приноси

Приемаме приноси към проекта! Моля:
1. Fork-нете проекта
2. Създайте feature branch
3. Commit-нете промените
4. Push-нете към branch-а
5. Създайте Pull Request

### 📧 Контакти

За въпроси относно системата или техническа поддръжка:
- **Email**: support@airlinetickets.bg
- **Documentation**: Този README файл
- **Issues**: GitHub Issues секцията

---

*Последна актуализация: Април 2026*
*Версия: 2.1.0 - Accurate Implementation Documentation*

**Забележка**: Този README е обновен да отразява точно реализираните функционалности в кода, а не планираните или предполагаемите възможности. Всички описани функции съответстват на действителната имплементация в проекта.


