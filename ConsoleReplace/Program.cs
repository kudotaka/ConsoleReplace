using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ConsoleAppFramework;
using Utf8StringInterpolation;
using ZLogger;
using ZLogger.Providers;

var app = ConsoleApp.Create();
app.ConfigureDefaultConfiguration()
    .ConfigureServices((configuration, services) =>
    {
        services.Configure<MyConfig>(configuration);
 
    })
    .ConfigureLogging((configration, logging) =>
    {
        logging.ClearProviders();
//        logging.SetMinimumLevel(LogLevel.Trace);
        logging.SetMinimumLevel(configration.GetValue<LogLevel>("Logging:LogLevel:ConsoleReplaceApp"));
        var jstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        var utcTimeZoneInfo = TimeZoneInfo.Utc;
        logging.AddZLoggerConsole(options =>
        {
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter($"{0:yyyy-MM-dd'T'HH:mm:sszzz}|{1:short}|", (in MessageTemplate template, in LogInfo info) => template.Format(TimeZoneInfo.ConvertTime(info.Timestamp.Utc, jstTimeZoneInfo), info.LogLevel));
                formatter.SetExceptionFormatter((writer, ex) => Utf8String.Format(writer, $"{ex.Message}"));
            });
        });
        logging.AddZLoggerRollingFile(options =>
        {
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter($"{0:yyyy-MM-dd'T'HH:mm:sszzz}|{1:short}|", (in MessageTemplate template, in LogInfo info) => template.Format(TimeZoneInfo.ConvertTime(info.Timestamp.Utc, jstTimeZoneInfo), info.LogLevel));
                formatter.SetExceptionFormatter((writer, ex) => Utf8String.Format(writer, $"{ex.Message}"));
            });

            // File name determined by parameters to be rotated
            options.FilePathSelector = (timestamp, sequenceNumber) => $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:00}.log";

            // The period of time for which you want to rotate files at time intervals.
            options.RollingInterval = RollingInterval.Day;

            // Limit of size if you want to rotate by file size. (KB)
            options.RollingSizeKB = 1024;
        });
    });

app.Add<ConsoleReplaceApp>();
app.Run(args);

public class ConsoleReplaceApp(ILogger<ConsoleReplaceApp> logger, IOptions<MyConfig> config)
{
//    [Command("")]
    public void Replace()
    {
        //== start
        logger.ZLogInformation($"==== tool {getMyFileVersion()} ====");

        logger.ZLogInformation($"config.Header:{config.Value.Header}");

        logger.ZLogTrace($"ZLogTrace");
        logger.ZLogInformation($"ZLogInformation");



        //== finish
        logger.ZLogInformation($"==== tool finish ====");
    }

    private string getMyFileVersion()
    {
        System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
        return ver.InternalName + "(" + ver.FileVersion + ")";
    }
}

//==
public class MyConfig
{
    public string Header { get; set; } = "";
}