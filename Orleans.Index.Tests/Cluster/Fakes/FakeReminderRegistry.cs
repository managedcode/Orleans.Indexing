using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Services;
using Orleans.Timers;

namespace Orleans.Index.Tests.Cluster.Fakes;

public class FakeReminderRegistry : GrainServiceClient<IReminderService>, IReminderRegistry
{
    private readonly ConcurrentDictionary<GrainReference, ConcurrentDictionary<string, FakeReminder>> reminders =
        new();

    public FakeReminderRegistry(IServiceProvider provider) : base(provider)
    {
    }

    private ConcurrentDictionary<string, FakeReminder> GetRemindersFor(GrainReference reference)
    {
        return reminders.GetOrAdd(reference, _ => new ConcurrentDictionary<string, FakeReminder>());
    }

    #region Test Helpers

    public Task<FakeReminder> GetReminder(GrainReference grainRef, string reminderName)
    {
        GetRemindersFor(grainRef).TryGetValue(reminderName, out var reminder);
        return Task.FromResult(reminder);
    }

    #endregion Test Helpers

    #region Fake Service Calls

    public Task<IGrainReminder> GetReminder(string reminderName)
    {
        GetRemindersFor(CallingGrainReference).TryGetValue(reminderName, out var reminder);
        return Task.FromResult((IGrainReminder) reminder);
    }

    public Task<List<IGrainReminder>> GetReminders()
    {
        return Task.FromResult(GetRemindersFor(CallingGrainReference).Values.Cast<IGrainReminder>().ToList());
    }

    public Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
    {
        var reminder = new FakeReminder(reminderName, dueTime, period);
        GetRemindersFor(CallingGrainReference)[reminderName] = reminder;
        return Task.FromResult((IGrainReminder) reminder);
    }

    public Task UnregisterReminder(IGrainReminder reminder)
    {
        GetRemindersFor(CallingGrainReference).TryRemove(reminder.ReminderName, out _);
        return Task.CompletedTask;
    }

    #endregion Unvalidated Service Calls
}