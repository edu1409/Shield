﻿namespace Shield.Common.Domain
{
    public class Constants
    {
        public const string SERVICE_STARTED = "Service has been started.";
        public const string SERVICE_MUTEX = "Global\\shmx";

        public const string DISPLAY_WELCOME = "Welcome to SHIELD!";
        public const string DISPLAY_TITLE = "       SHIELD";
        public const string DISPLAY_ERROR_STATE = "Err";
        public const string DISPLAY_FATAL_ERROR = "FATAL ERROR!";
        public const string DISPLAY_DATE_TIME_UPDATED = "Display date and time has been updated.";
        public const string DISPLAY_CLIMATIC_INFO_UPDATED = "Display climatic information has been updated.";

        public const string BACKLIGHT_MANUAL_NOCHANGE = "display backlight has no change.";
        public const string BACKLIGHT_MANUAL_CHANGE = "{0} display backlight turned {1}.";
        public const string BACKLIGHT_ON_SERVICE = "display backlight has been turned on by service.";
        public const string BACKLIGHT_OFF_SERVICE = "display backlight has been turned off by service.";
        public const string BACKLIGHT_BACK_AUTOMATIC = "display backlight has returned to automatic control.";

        public const string CLIMATE_SENSOR_READING = "Climatic parameters read: Temperature: {0:0.#}ºC | Humidity: {1:#.##}%";
        public const string CLIMATE_SENSOR_READING_ERROR = "Climatic sensor reading error.";

        public const string SHARED_MEMORY_FILE = "shmem.map";
        public const string SHARED_MEMORY_INVALID_SOURCE = "Invalid shared memory source.";
        
        public const int IPC_PORT = 10666;
    }
}
