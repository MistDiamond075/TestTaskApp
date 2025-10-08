using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TestTaskApp.actors;
using TestTaskApp.rules;
using TestTaskApp.sensors;
using TestTaskApp.services;
using TestTaskApp.utils;
using TestTaskApp.Views;
using static TestTaskApp.utils.FilePickerRequests;

namespace TestTaskApp.ViewModels
{
    public class RulesViewModel:ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment => "rules";
        public ObservableCollection<RuleViewModel> Rules { get; } = [];
        public IScreen HostScreen { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> BackCommand { get; }
        public Interaction<OpenFileRequest, string?> OpenFileInteraction { get; } = new();
        public Interaction<SaveFileRequest, string?> SaveFileInteraction { get; } = new();
        public ReactiveCommand<Unit, Unit> ImportCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ICommand SaveCommand { get; }
        private ServiceUserRules srvUserRules;
        private ServiceRulesScheduler srvRulesScheduler;
        private ServiceActors srvActors;
        private ServiceSensors srvSensors;

        public RulesViewModel(IScreen screen,ServiceUserRules serviceUserRules,ServiceActors serviceActors,ServiceSensors serviceSensors,ServiceRulesScheduler serviceRulesScheduler)
        {
            HostScreen = screen;
            BackCommand = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.NavigateBack.Execute(Unit.Default)
            );
            SaveCommand = new RelayCommand(SaveRules);
            ImportCommand = ReactiveCommand.CreateFromTask(ImportRules);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportRules);
            srvUserRules =serviceUserRules;
            srvActors=serviceActors;
            srvSensors=serviceSensors;
            srvRulesScheduler=serviceRulesScheduler;
            foreach(var rule in serviceUserRules.GetRules())
            {
                Rules.Add(new RuleViewModel(rule,serviceActors,serviceSensors,this,srvUserRules));
            }
        }

        private async Task ImportRules()
        {
            var path = await OpenFileInteraction.Handle(new OpenFileRequest("Импорт правил", new[] { "*.json" }));
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                var dto = await Task.Run(() => ServiceRulesExportImport.ImportFromJson(path));

                Rules.Clear();

                foreach (var s in dto.SingleRules)
                {
                    var variable = ParseValue(s.VariableName);
                    var comparable = ParseValue(s.ComparableName);
                    var actor = ParseActor(s.ActorName);

                    var model = new UserRuleSingleOperation(variable, s.Operation, comparable, actor, s.Timer, s.ActorState);
                    srvUserRules.AddRule(model);
                    srvRulesScheduler.AddRule(model);
                    Rules.Add(new RuleViewModel(model, srvActors, srvSensors, this, srvUserRules));
                }

                foreach (var d in dto.DoubleRules)
                {
                    var variableFirst = ParseValue(d.VariableFirstName);
                    var comparableFirst = ParseValue(d.ComparableFirstName);
                    var variableSecond = ParseValue(d.VariableSecondName);
                    var comparableSecond = ParseValue(d.ComparableSecondName);
                    var actor = ParseActor(d.ActorName);

                    var model = new UserRuleDoubleOperation(
                        variableFirst, d.OperationFirst, comparableFirst,
                        variableSecond, d.OperationSecond, comparableSecond,
                        actor, d.ActorState, d.Timer
                    );

                    srvUserRules.AddRule(model);
                    srvRulesScheduler.AddRule(model);
                    Rules.Add(new RuleViewModel(model, srvActors, srvSensors, this, srvUserRules));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("import failed");
            }
        }

        private async Task ExportRules()
        {
            try
            {
                var dto = new DTORules();

                foreach (var vm in Rules)
                {
                    if (vm.rule is UserRuleDoubleOperation)
                    {
                        var r = (UserRuleDoubleOperation)vm.rule;
                        dto.DoubleRules.Add(new DTORuleDouble
                        {
                            VariableFirstName = GetTypedName(r.VariableFirst),
                            OperationFirst = r.OperationFirst,
                            ComparableFirstName = r.ComparableFirst.ToString(),
                            VariableSecondName = GetTypedName(r.VariableSecond),
                            OperationSecond = r.OperationSecond,
                            ComparableSecondName = r.ComparableSecond.ToString(),
                            ActorName = GetTypedName(r.Actor),
                            ActorState = r.ActorState,
                            Timer = r.Timer
                        });
                    }
                    else if (vm.rule is UserRuleSingleOperation s)
                    {
                        dto.SingleRules.Add(new DTORuleSingle
                        {
                            VariableName = GetTypedName(s.Variable),

                            Operation = s.Operation,

                            ComparableName = s.Comparable.ToString(),

                            ActorName = GetTypedName(s.Actor),

                            ActorState = s.ActorState,
                            Timer = s.Timer,
                        });
                    }
                }

                ServiceRulesExportImport.ExportToJson(dto);

                Console.WriteLine("exported");
            }
            catch (Exception ex)
            {
                Console.WriteLine("export failed");
                Console.WriteLine(ex.ToString());
            }
        }

        private IComparable ParseValue(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;
            if (name.StartsWith("Sensor", StringComparison.OrdinalIgnoreCase))
            {
                var parts = name.Split(':');
                var type = parts[0];
                uint? id = parts.Length > 1 && int.TryParse(parts[1], out var parsedId) ? (uint)parsedId : null;

                var list =srvSensors.GetSensorByType(type, id);
                if (list != null)
                    return list[0];
            }

            if (name.Equals("TimeOnly", StringComparison.OrdinalIgnoreCase))
                return new TimeOnly();
            if (TimeOnly.TryParse(name, out var t))
                return t;
            if (int.TryParse(name, out var i))
                return i;
            if (double.TryParse(name, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;

            return name;
        }

        private IActor ParseActor(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Actor name cannot be null or empty");

            var parts = name.Split(':');
            var type = parts[0];
            uint? id = parts.Length > 1 && int.TryParse(parts[1], out var parsedId) ? (uint)parsedId : null;

            var actor = srvActors.GetActorByType(type, id);
            if (actor != null)
                return actor[0];

            throw new InvalidOperationException("Actor "+name+" not found");
        }


        public void RemoveRule(RuleViewModel rule)
        {
            Rules.Remove(rule);
            if (rule.rule != null)
            {
                srvUserRules.RemoveRule(rule.rule.Id);
                srvRulesScheduler.RemoveRules(rule.rule);
            }
        }

        public void CreateRule()
        {
            var vm = new RuleViewModel(null, srvActors, srvSensors, this,srvUserRules);
            Rules.Add(vm);
        }

        public void SaveRules()
        {
            var allVms = Rules.ToList();

            foreach (var vm in allVms)
            {
                try
                {
                    var newModel = vm.BuildModel();
                    bool hasErrors = false;

                    foreach (var rule in Rules)
                    {
                        rule.Validate(nameof(rule.VariableValue),true);
                        rule.Validate(nameof(rule.Operation), true);
                        rule.Validate(nameof(rule.ComparableValue), true);
                        rule.Validate(nameof(rule.Actor), true);
                        rule.Validate(nameof(rule.ActorsState), true);
                        rule.Validate(nameof(rule.OperationSecond), true);
                        rule.Validate(nameof(rule.ComparableSecondValue), true);
                        if (rule.HasErrors)
                            hasErrors = true;
                    }
                    Console.WriteLine(hasErrors);
                    if (hasErrors)
                    {
                        return;
                    }
                    if (vm.rule == null || vm.rule.Id == 0)
                    {
                        if (newModel is UserRuleSingleOperation single)
                        {
                            srvUserRules.AddRule(single);
                            srvRulesScheduler.AddRule(single);
                            vm.UpdateLinkedRule(single);
                        }
                        else if (newModel is UserRuleDoubleOperation dbl)
                        {
                            srvUserRules.AddRule(dbl);
                            srvRulesScheduler.AddRule(dbl);
                            vm.UpdateLinkedRule(dbl);
                        }

                        continue;
                    }

                    bool typeChanged =
                        (vm.rule is UserRuleSingleOperation && newModel is UserRuleDoubleOperation) ||
                        (vm.rule is UserRuleDoubleOperation && newModel is UserRuleSingleOperation);

                    if (typeChanged)
                    {
                        srvUserRules.RemoveRule(vm.rule.Id);
                        srvRulesScheduler.RemoveRules(vm.rule);

                        if (newModel is UserRuleSingleOperation single)
                        {
                            srvUserRules.AddRule(single);
                            srvRulesScheduler.AddRule(single);
                            vm.UpdateLinkedRule(single);
                        }
                        else if (newModel is UserRuleDoubleOperation dbl)
                        {
                            srvUserRules.AddRule(dbl);
                            srvRulesScheduler.AddRule(dbl);
                            vm.UpdateLinkedRule(dbl);
                        }
                    }
                    else
                    {
                        if (newModel is UserRuleSingleOperation s)
                        {
                            srvUserRules.UpdateRule(vm.rule.Id, s.Variable, s.Operation, s.Comparable, s.Actor, s.ActorState, s.Timer);
                            srvRulesScheduler.RemoveRules(vm.rule);
                            srvRulesScheduler.AddRule(vm.rule); 
                            vm.UpdateLinkedRule(vm.rule); 
                        }
                        else if (newModel is UserRuleDoubleOperation d)
                        {
                            srvUserRules.UpdateRule(vm.rule.Id,
                                d.VariableFirst, d.OperationFirst, d.ComparableFirst,
                                d.VariableSecond, d.OperationSecond, d.ComparableSecond,
                                d.Actor, d.ActorState, d.Timer);

                            srvRulesScheduler.RemoveRules(vm.rule);
                            srvRulesScheduler.AddRule(vm.rule);
                            vm.UpdateLinkedRule(vm.rule);
                        }
                    }
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
        }
        private static string? GetTypedName(object? obj)
        {
            if (obj == null)
                return null;

            var typeName = obj.GetType().Name;

            try
            {
                var idProp = obj.GetType().GetProperty("Id");
                if (idProp != null)
                {
                    var idValue = idProp.GetValue(obj);
                    if (idValue != null)
                        return $"{typeName}:{idValue}";
                }
            }
            catch
            {
            }

            return typeName;
        }
    }
}
