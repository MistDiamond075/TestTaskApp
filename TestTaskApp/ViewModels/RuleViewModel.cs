using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TestTaskApp.actors;
using TestTaskApp.rules;
using TestTaskApp.sensors;
using TestTaskApp.services;
using TestTaskApp.utils;

namespace TestTaskApp.ViewModels
{
    public class RuleViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public IRules? rule { get; private set; }
        public ObservableCollection<string> ActorsState { get; } =["Вкл", "Выкл"];
        public ObservableCollection<string> VariableTypes { get; } = ["Время","Температура","Углекислый газ","Влажность","Освещённость"];
        public ObservableCollection<string> AvailableOperations { get; } = ["<", ">","=","<=",">="];
        public ObservableCollection<string> Actors { get; } = [];
        private ServiceActors srvActors;
        private ServiceSensors srvSensors;
        private ServiceUserRules srvUserRules;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand DeleteCommand { get; }

        private readonly RulesViewModel parent;
        private object? _variableValue;            
        private object? _comparableValue;
        private string _operation = "";
        private object? _variableSecondValue;
        private object? _comparableSecondValue;
        private string _operationSecond = "";
        private string _actorName = "";
        private bool _actorStateBool = false;
        private TimeSpan? _timer = null;
        private readonly Dictionary<string, List<string>> _errors = new();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => _errors.Any();
        public IEnumerable GetErrors(string? propertyName)
            => propertyName != null && _errors.ContainsKey(propertyName)
                ? _errors[propertyName]
                : Enumerable.Empty<string>();

        public RuleViewModel(IRules rule, ServiceActors srvActors,ServiceSensors srvSensors, RulesViewModel parent, ServiceUserRules srvUserRules)
        {
            this.rule = rule;
            DeleteCommand = new RelayCommand(Delete);
            this.parent = parent;
            if (rule is INotifyPropertyChanged npc)
                npc.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            this.srvActors = srvActors;
            this.srvSensors = srvSensors;
            foreach (var actor in srvActors.GetActors())
            {
                Actors.Add(Parser.ParseActorName(actor));
            }
            foreach (var sensor in srvSensors.GetSensors())
            {
                if (sensor is SensorSoilMosture ssm)
                {
                    VariableTypes.Add("Влажность почвы " + ssm.Id);
                }
            }
            if (rule != null)
            {
                if (rule is IRuleSingleOperation s)
                {
                    _variableValue = s.Variable is ISensor ? Parser.ParseSensorName((ISensor)s.Variable) :
                                     s.Variable is TimeOnly ? "Время" : s.Variable;
                    _operation = s.Operation ?? "";
                    _comparableValue = s.Comparable is ISensor ? Parser.ParseSensorName((ISensor)s.Comparable) : s.Comparable;
                    _actorName = Parser.ParseActorName(s.Actor);
                    _actorStateBool = s.ActorState;
                    _timer = s.Timer;
                }
                else if (rule is IRuleDoubleOperation d)
                {
                    _variableValue = d.VariableFirst is ISensor ? Parser.ParseSensorName((ISensor)d.VariableFirst) :
                                     d.VariableFirst is TimeOnly ? "Время" : d.VariableFirst;
                    _operation = d.OperationFirst ?? "";
                    _comparableValue = d.ComparableFirst is ISensor ? Parser.ParseSensorName((ISensor)d.ComparableFirst) : d.ComparableFirst;

                    _variableSecondValue = d.VariableSecond is ISensor ? Parser.ParseSensorName((ISensor)d.VariableSecond) :
                                           d.VariableSecond is TimeOnly ? "Время" : d.VariableSecond;
                    _operationSecond = d.OperationSecond ?? "";
                    _comparableSecondValue = d.ComparableSecond is ISensor ? Parser.ParseSensorName((ISensor)d.ComparableSecond) : d.ComparableSecond;

                    _actorName = Parser.ParseActorName(d.Actor);
                    _actorStateBool = d.ActorState;
                    _timer = d.Timer;
                }
            }

            this.srvUserRules = srvUserRules;
        }
        private void AddError(string propertyName, string message)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();
            if (!_errors[propertyName].Contains(message))
            {
                _errors[propertyName].Add(message);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        public string DisplayName => (rule==null || rule.Id==0) ? "Новое правило" : "Правило "+rule.Id;

        public object? VariableValue { get => _variableValue; set { _variableValue = value; OnPropertyChanged();Validate(nameof(VariableValue)); } }
        public object? ComparableValue { get => _comparableValue; set { _comparableValue = value; OnPropertyChanged(); Validate(nameof(ComparableValue)); } }
        public string Operation { get => _operation; set { _operation = value ?? ""; OnPropertyChanged(); Validate(nameof(Operation)); } }

        public object? VariableSecondValue { get => _variableSecondValue; set { _variableSecondValue = value; OnPropertyChanged(); Validate(nameof(VariableSecondValue)); } }
        public object? ComparableSecondValue { get => _comparableSecondValue; set { _comparableSecondValue = value; OnPropertyChanged(); Validate(nameof(ComparableSecondValue)); } }
        public string OperationSecond { get => _operationSecond; set { _operationSecond = value ?? ""; OnPropertyChanged(); Validate(nameof(OperationSecond)); } }


        public string Actor { get => _actorName; set { _actorName = value; OnPropertyChanged(); } }

        public string ActorState { get => _actorStateBool ? "Вкл" : "Выкл"; set { _actorStateBool = value == "Вкл"; OnPropertyChanged(); } }

        public TimeSpan? Timer { get => _timer; set { _timer = value; OnPropertyChanged(); } }
        public IRules BuildModel()
        {
            IComparable conv1 = ConvertValue(_variableValue);
            IComparable comp1 = ConvertValue(_comparableValue);
            string op1 = _operation ?? "";

            var actorModel = srvActors.GetActorByName(Parser.ParseActorType(_actorName));
            if (actorModel == null) throw new ArgumentException("Actor not selected or unknown");

            bool actorState = _actorStateBool;
            TimeSpan? timer = _timer;

            bool hasSecond = !string.IsNullOrWhiteSpace(_operationSecond)
                             || _variableSecondValue != null
                             || _comparableSecondValue != null;

            if (!hasSecond)
            {
                return new UserRuleSingleOperation(conv1, op1, comp1, actorModel, timer, actorState);
            }
            else
            {
                IComparable conv2 = ConvertValue(_variableSecondValue);
                IComparable comp2 = ConvertValue(_comparableSecondValue);
                string op2 = _operationSecond ?? "";

                return new UserRuleDoubleOperation(conv1, op1, comp1, conv2, op2, comp2, actorModel, actorState, timer);
            }
        }

        private IComparable ConvertValue(object? value)
        {
            if (value == null) return string.Empty;

            var s = value.ToString() ?? "";

            var sensor = srvSensors.GetSensorByName(Parser.ParseSensorType(s));
            if (sensor != null) return sensor;
            if (s.Equals("Время", StringComparison.OrdinalIgnoreCase))
                return new TimeOnly();
                if (TimeOnly.TryParse(s, CultureInfo.InvariantCulture, out var t)) return t;
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            return s;
        }

        public void Validate(string propertyName,bool withBuild=false)
        {
            ClearErrors(propertyName);

            switch (propertyName)
            {
                case nameof(VariableValue):
                    if (VariableValue == null)
                        AddError(propertyName, "Не выбрана переменная");
                    break;

                case nameof(Operation):
                    if (string.IsNullOrWhiteSpace(Operation))
                        AddError(propertyName, "Не выбрана операция сравнения");
                    break;

                case nameof(ComparableValue):
                    if (ComparableValue == null)
                        AddError(propertyName, "Не указано значение для сравнения");
                    break;
                case nameof(VariableSecondValue):
                    if (VariableSecondValue == null && (!string.IsNullOrWhiteSpace(OperationSecond) || ComparableSecondValue != null))
                        AddError(propertyName, "Не выбрана переменная");
                    break;
                case nameof(ComparableSecondValue):
                    {
                        if ((VariableSecondValue != null || !string.IsNullOrWhiteSpace(OperationSecond)) && ComparableSecondValue == null)
                        {
                            AddError(propertyName, "Не указано значение для сравнения");
                        }
                        break;
                    }
                case nameof(OperationSecond):
                    {
                        if ((VariableSecondValue != null || ComparableSecondValue != null) && string.IsNullOrWhiteSpace(OperationSecond))
                        {
                            AddError(propertyName, "Не выбрана операция сравнения");
                        }
                        break;
                    }
                case nameof(Actor):
                    if (Actor == null)
                        AddError(propertyName, "Не выбрано устройство");
                    break;
                case nameof(ActorState):
                    if (VariableValue == null)
                        AddError(propertyName, "Не выбрано состояние");
                    break;
            }

            if (withBuild)
            {
                try
                {
                    var model = BuildModel();
                    if (!srvUserRules.IsValid(model))
                    {
                        AddError(string.Empty, "Правило конфликтует с существующими или некорректно по логике");
                    }
                }
                catch
                {
                    AddError(string.Empty, "Не удалось создать модель для проверки");
                }
            }
        }

        public void UpdateLinkedRule(IRules newRule)
        {
            rule = newRule;
            OnPropertyChanged(nameof(DisplayName));
        }

        private void Delete()
        {
            parent.RemoveRule(this);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
