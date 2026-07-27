namespace Desktop.App.Models.Alarms;

public enum AlarmSourceType
{
    Bit = 0,
    CodeWord = 1,
    System = 2,
}

public enum AlarmType
{
    Alarm = 0,
    Error = 1,
    System = 2,
}

public enum AlarmSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2,
}

public enum AlarmRecordStatus
{
    Active = 0,
    Resolved = 1,
}
