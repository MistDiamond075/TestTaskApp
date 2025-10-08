Не успел: график, журнал действий, лог выполнения правил, тёмная тема, тесты


Как запустить:

dotnet restore

dotnet build

dotnet run --project TestTaskApp.Desktop


Архитектура:

===СЛОЙ MODEL==

Actors (ActorPump, ActorLamp, ActorVentilation, IActor) — устройства

Sensors (SensorTemperature, SensorSoilMosture, SensorIlluminance, SensorCA (Углекислый газ), SensorHumidify, ISensor) — датчики

Rules (UserRuleSingleOperation, UserRuleDoubleOperation, IRules) — модели пользовательских правил

DTORules — объект передачи данных для экспорта/импорта

Services — сервисы для управления:

ServiceActors — хранение и поиск акторов;

ServiceSensors — управление сенсорами;

ServiceSensorSimulation — симуляция данных с датчиков

ServiceUserRules — хранение пользовательских правил;

ServiceRulesScheduler — выполнение и планирование правил;

ServiceRulesExportImport — экспорт/импорт правил  в JSON.

Utils - утилитарные классы


===СЛОЙ VIEWMODEL===

MainViewModel - Корневая модель, управляющая навигацией между экранами

IndexViewModel - Главная страница, отображающая сенсоры и акторы, с кнопкой перехода к “Правилам”

RulesViewModel - Управление списком правил, их добавление, удаление, импорт/экспорт, сохранение

RuleViewModel - Представление одного конкретного правила

ActorViewModel - Представление одного актора в UI

SensorViewModel - Представление одного сенсора в UI


===СЛОЙ VIEW===

MainWindow - Главное окно приложения

IndexView - Главный экран (список датчиков и устройств)

RulesView - Экран управления пользовательскими правилами

