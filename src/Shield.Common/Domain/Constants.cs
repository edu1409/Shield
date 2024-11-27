namespace Shield.Common.Domain
{
    public class Constants
    {
        public const string SERVICE_STARTED = "Service has been started.";
        public const string SERVICE_MUTEX = "Global\\shmx";

        public const string LCD20x4_WELCOME = "Welcome to SHIELD!";
        public const string LCD20x4_TITLE = "       SHIELD";
        public const string LCD16x2_TITLE = "-- SHIELD --";
        public const string LCD16x2_SUBTITLE = "Media Server";
        public const string LCD_ERROR_STATE = "Err";
        public const string LCD_FATAL_ERROR = "FATAL ERROR!";
        public const string LCD_DATE_TIME_UPDATED = "Display date and time has been updated.";
        public const string LCD_CLIMATIC_INFO_UPDATED = "Display climatic information has been updated.";

        public const string LCD_BACKLIGHT_MANUAL_NOCHANGE = "{0} display backlight has no change.";
        public const string LCD_BACKLIGHT_MANUAL_CHANGE = "{0} display backlight turned {1}.";
        public const string LCD_BACKLIGHT_ON_SERVICE = "display backlight has been turned on by service.";
        public const string LCD_BACKLIGHT_OFF_SERVICE = "display backlight has been turned off by service.";
        public const string LCD_BACKLIGHT_BACK_AUTOMATIC = "{0} display backlight has returned to automatic control.";

        public const string FAN_MANUAL_NOCHANGE = "{0} fan has no change.";
        public const string FAN_MANUAL_CHANGE = "{0} fan turned {1}.";
        public const string FAN_BACK_AUTOMATIC = "{0} fan has returned to automatic control.";
        public const string FAN_DUTY_CYCLE_CHANGE = "{0} fan has duty cycle changed to {1:P2}.";


        public const string CLIMATE_SENSOR_READING = "Climatic parameters read: Temperature: {0:0.#}ºC | Humidity: {1:#.##}%";
        public const string CLIMATE_SENSOR_READING_ERROR = "Climatic sensor reading error.";

        public const string SHARED_MEMORY_FILE = "shmem.map";
        public const string SHARED_MEMORY_INVALID_SOURCE = "Invalid shared memory source.";
        
        public const int IPC_PORT = 10666;
    }
}
