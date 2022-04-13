using System;
using Orleans.Runtime;

namespace Orleans.Index.Tests.Cluster.Fakes;

public class FakeReminder : IGrainReminder
{
    public FakeReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
    {
        ReminderName = reminderName;
        DueTime = dueTime;
        Period = period;
    }

    public TimeSpan DueTime { get; }
    public TimeSpan Period { get; }

    public string ReminderName { get; }
}