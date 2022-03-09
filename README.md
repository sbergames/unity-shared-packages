# Общие пакеты, используемые несколькими студиями
      
+ **DataPlatformAnalytics** - платформа аналитики
+ **UnityDataPlatformDemo** - демо-приложение для тестрования и отладки платформы аналитики
+ **ru.sbergames.monsterstone.system-libraries** - [см. описание](\ru.sbergames.monsterstone.system-libraries\Readme.md)
+ NullableAttributes
+ Utils
+ Utils.Tests

## Платформа аналитики - DataPlatformAnalytics

- Модуль предназначен для использования в среде Unity. 
- Версия Юнити - 2020 и выше. 
- Можно пользоваться в IOS/Android и Editor-режимах. 
- Для установки в проект нужен **API_KEY**
- Примеры использования лежат в папке **unity-shared-packages/DataPlatformAnalytics/Example** основого репозитория

#### Задачи (выполняемые функции) модуля

- Отправка событий игровой аналитики на сервер data-platform
- Сохранение событий в файл до отправки на сервер
- Формирование строки запроса.
- Батчинг нескольких сообщений (до 20) в одно.

#### Описание параметров - смотри
[https://sbergames.atlassian.net/wiki/spaces/GA/pages/95093003/Event+Starter+pack+v.+0.30](https://sbergames.atlassian.net/wiki/spaces/GA/pages/95093003/Event+Starter+pack+v.+0.30)

#### Инструкция по взаимодействию с Web API
[https://sbergames.atlassian.net/wiki/spaces/GA/pages/119799835/data-platform](https://sbergames.atlassian.net/wiki/spaces/GA/pages/119799835/data-platform)

#### Пример пустого проекта
[https://disk.yandex.ru/d/d1BEYS4W3SiyQg](https://disk.yandex.ru/d/d1BEYS4W3SiyQg)

## Демо приложение для тестрования и отладки - UnityDataPlatformDemo

Для работы с дата-платформой необходимо минимально два файла - файл обработки события и отправки уведомления - например ButtonBehaviour.cs
```C#
public class ButtonBehaviour : MonoBehaviour
{
    public void ClickedEvent()
    {
        DataPlatformAnalyticsWrapper.Instance.SendButtonClickedEvent();
    }
}
```
И файл варппер-класса, например DataPlatformAnalyticsWrapper. В котором будут указаны apiKey и baseUri а также приведена инициализация основного класса-исполнителя.
```C#
public class DataPlatformAnalyticsWrapper
{
    private string apiKey = "UdVvIOmJuv-oY9OXwfXhHVLq1Hv8YwWoOkIbwAPQMSDgF1KjChPx1n63981zoWL6";
    private string baseUri = "https://api.sbergames.network/api/events/batch?api_key={API_KEY}";

    private static DataPlatformAnalyticsWrapper _instance = null;

    private DataPlatformAnalytics dataPlatformAnalytics = null;

    public static DataPlatformAnalyticsWrapper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataPlatformAnalyticsWrapper();
                _instance.Initialize();
            }
            return _instance;
        }
    }

    private void Initialize()
    {
        dataPlatformAnalytics = new DataPlatformAnalytics();
        dataPlatformAnalytics.Initialize(apiKey, baseUri);
        dataPlatformAnalytics.SetUserProperty("user_id", "example_user");
    }

    public void SendButtonClickedEvent()
    {
        EventData eventData = new EventData("button_clicked").AddData("some_param", "some_value");
        dataPlatformAnalytics.SendEvent(eventData);
    }
}
```

## Unity Assets DataPlatformAnalytics

#### Unity Cross-Platform версия от Александра Башкирева (Tipping Point)
Фреймворк включает основной интерфейс IDataPlatformAnalytics и его реализацию.

Namespace SberGames.DataPlatform и SberGames.DataPlatform.Core

**Unit-tests** см. в папке unity-shared-packages/DataPlatformAnalytics/Tests.
Тестов два - FileEventCacheTests и RuntimeEventCacheTest
