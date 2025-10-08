using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestTaskApp.rules;
using TestTaskApp.sensors;
using TestTaskApp.services;

public class ServiceRulesScheduler : BackgroundService
{
    private readonly SortedDictionary<DateTime, List<IRules>> timeRules = [];
    private readonly Dictionary<ISensor, List<IRules>> otherRules = [];
    private readonly SortedDictionary<DateTime, List<IRules>> rulesToStop = [];
    private readonly AutoResetEvent wakeUpEvent = new(false);

    private readonly ServiceSensors srvSensors;

    public ServiceRulesScheduler(ServiceSensors serviceSensors, ServiceUserRules serviceUserRules)
    {
        srvSensors = serviceSensors;

        foreach (var sensor in srvSensors.GetSensors())
            sensor.PropertyChanged += OnSensorPropertyChanged;

        foreach (var rule in serviceUserRules.GetRules())
            AddRule(rule);
    }

    public void AddRule(IRules rule)
    {
        if (rule.isTimeRule() is TimeOnly t)
        {
            var now = DateTime.Now;
            var trigger = new DateTime(now.Year, now.Month, now.Day, t.Hour, t.Minute, t.Second);

            if (trigger <= now)
                trigger = trigger.AddDays(1);

            if (!timeRules.TryGetValue(trigger, out var list))
                timeRules[trigger] = list = [];

            list.Add(rule);
            wakeUpEvent.Set();

            if (rule.IsActive())
                ApplyRule(rule);
        }
        else
        {
            var sensors = rule.GetSensors().Where(s => s != null).Distinct();
            if (!sensors.Any())
                throw new InvalidOperationException("sensors not found");

            foreach (var sensor in sensors)
            {
                if (!otherRules.TryGetValue(sensor, out var rules))
                    otherRules[sensor] = rules = [];
                rules.Add(rule);
            }

            if (rule.IsActive())
                ApplyRule(rule);
        }
    }

    public void RemoveRules(IRules rule)
    {
        foreach (var kvp in timeRules.ToList())
        {
            if (kvp.Value.Remove(rule) && kvp.Value.Count == 0)
                timeRules.Remove(kvp.Key);
        }

        foreach (var kvp in otherRules.ToList())
        {
            if (kvp.Value.Remove(rule) && kvp.Value.Count == 0)
                otherRules.Remove(kvp.Key);
        }
    }

    private void OnSensorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ISensor sensor || e.PropertyName != nameof(sensor.Value))
            return;

        if (!otherRules.TryGetValue(sensor, out var list))
            return;

        foreach (var rule in list)
        {
            bool isActive = rule.IsActive();
            if (isActive)
                ApplyRule(rule);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            CheckRulesToStop();

            if (timeRules.Count == 0)
            {
                await Task.Run(() => wakeUpEvent.WaitOne(1000), token);
                continue;
            }

            var next = timeRules.First();
            var delay = next.Key - DateTime.Now;
            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            var delayTask = Task.Delay(delay, token);
            var wakeTask = Task.Run(() => wakeUpEvent.WaitOne(), token);
            var completed = await Task.WhenAny(delayTask, wakeTask);

            if (completed == wakeTask)
                continue;

            var triggeredRules = next.Value;
            timeRules.Remove(next.Key);

            foreach (var rule in triggeredRules)
                ApplyRule(rule);

            // Переносим на следующий день
            var nextDay = next.Key.AddDays(1);
            timeRules[nextDay] = triggeredRules;
        }
    }

    private void ApplyRule(IRules rule)
    {
        rule.Actor.State = rule.ActorState;
       // Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Rule applied → {rule.Actor} = {(rule.ActorState ? "ON" : "OFF")}");

        if (rule.Timer is not null)
            ScheduleStop(rule);
    }

    private void ScheduleStop(IRules rule)
    {
        DateTime stopTime = DateTime.Now.Add(rule.Timer.Value);
        if (!rulesToStop.TryGetValue(stopTime, out var list))
            rulesToStop[stopTime] = list = [];
        list.Add(rule);
    }

    private void CheckRulesToStop()
    {
        var now = DateTime.Now;
        var expired = rulesToStop.Where(x => x.Key <= now).ToList();
        foreach (var kvp in expired)
        {
            foreach (var rule in kvp.Value)
            {
                rule.Actor.State = !rule.ActorState;
             //   Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Timer expired → {rule.Actor} reverted");
            }
            rulesToStop.Remove(kvp.Key);
        }
    }
}
