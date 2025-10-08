using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.sensors;
using TestTaskApp.utils;

namespace TestTaskApp.rules
{
    public class UserRuleDoubleOperation : IRuleDoubleOperation, INotifyPropertyChanged
    {
        private IComparable variableFirst;
        private IComparable variableSecond;
        private string operationFirst;
        private string operationSecond;
        private IComparable comparableFirst;
        private IComparable comparableSecond;
        private IActor actor;
        private bool actorState;
        private TimeSpan? timer;
        private DateTime startedAt=DateTime.MinValue;
        private uint id;
        public event PropertyChangedEventHandler? PropertyChanged;

        public UserRuleDoubleOperation()
        {
            id = 0;
        }

        public UserRuleDoubleOperation(UserRuleDoubleOperation other)
        {
            variableFirst = other.variableFirst;
            variableSecond = other.variableSecond;
            this.operationFirst = other.operationFirst;
            this.operationSecond = other.operationSecond;
            comparableFirst = other.comparableFirst;
            comparableSecond = other.comparableSecond;
            actor = other.actor;
            actorState = other.actorState;
            timer = other.timer;
            startedAt = other.startedAt;
            id=other.id==0 ? RuleIdGenerator.generate() : other.id;
        }

        public UserRuleDoubleOperation(IComparable var_first, string operation_first, IComparable compare_first_to, IComparable var_second, string operation_second, IComparable compare_second_to, IActor actor,bool actorState, TimeSpan? timer)
        {
            variableFirst = var_first;
            variableSecond = var_second;
            operationFirst = operation_first;
            operationSecond = operation_second;
            comparableFirst = compare_first_to;
            comparableSecond = compare_second_to;
            this.actor = actor;
            this.actorState = actorState;
            this.timer = timer;
            startedAt = DateTime.MinValue;
            id = RuleIdGenerator.generate();
        }

        public IComparable VariableFirst { get => variableFirst; set {
                if (!Equals(variableFirst, value))
                {
                    variableFirst = value;
                    OnPropertyChanged();
                }
            } }
        public string OperationFirst { get => operationFirst; set {
                if (!Equals(operationFirst, value))
                {
                    operationFirst = value;
                    OnPropertyChanged();
                }
            } }
        public IComparable ComparableFirst { get => comparableFirst; set {
                if (!Equals(comparableFirst, value))
                {
                    comparableFirst = value;
                    OnPropertyChanged();
                }
            } }
        public IComparable VariableSecond { get => variableSecond; set {
                if (!Equals(variableSecond, value))
                {
                    variableSecond = value;
                    OnPropertyChanged();
                }
            } }
        public string OperationSecond { get => operationSecond; set {
                if (!Equals(operationSecond, value))
                {
                    operationSecond = value;
                    OnPropertyChanged();
                }
            } }
        public IComparable ComparableSecond { get => comparableSecond; set {
                if (!Equals(comparableSecond, value))
                {
                    comparableSecond = value;
                    OnPropertyChanged();
                }
            } }
        public IActor Actor { get => actor; set {
                if (!Equals(actor, value))
                {
                    actor = value;
                    OnPropertyChanged();
                }
            } }
        public bool ActorState { get => actorState; set {
                if (!Equals(actorState, value))
                {
                    actorState = value;
                    OnPropertyChanged();
                }
            } }
        public TimeSpan? Timer { get => timer; set {
                if (!Equals(timer, value))
                {
                    timer = value;
                    OnPropertyChanged();
                }
            } }
        public DateTime StartedAt { get => startedAt; private set; }
        public uint Id { get => id; }
        public bool IsActive()
        {
            //Console.Write(variableFirst.ToString() + " " + operationFirst + " " + comparableFirst.ToString()+'\t');
            //Console.WriteLine(variableSecond.ToString() + " " + operationSecond + " " + comparableSecond.ToString());
            int compare_result_first = variableFirst is ISensor ? variableFirst.CompareTo(comparableFirst)  : TimeOnly.FromDateTime(DateTime.Now).CompareTo(comparableFirst); ;
            int compare_result_second= variableSecond is ISensor ? variableSecond.CompareTo(comparableSecond) : TimeOnly.FromDateTime(DateTime.Now).CompareTo(comparableSecond); ;
            return Parser.ParseTwoExpressionResult(operationFirst, compare_result_first, operationSecond, compare_result_second);
        }

        public int CompareTo(IRules? other)
        {
            if (other == null) return 1;
            return StartedAt.CompareTo(other.StartedAt);
        }

        public TimeOnly? isTimeRule()
        {
            if((comparableFirst is TimeOnly) && (comparableSecond is TimeOnly))
            {
                return (TimeOnly)(comparableFirst.CompareTo(comparableSecond)>0 ? comparableFirst : comparableSecond);
            }
            return null;
        }

        public (TimeOnly? start, TimeOnly? end)? GetTimeRange()
        {
            TimeOnly? start = null;
            TimeOnly? end = null;
            if (VariableFirst is TimeOnly t1)
                start = t1;
            if (ComparableFirst is TimeOnly c1)
                end = c1;
            if (VariableSecond is TimeOnly t2)
            {
                if (start == null) start = t2;
                else if (end == null) end = t2;
            }
            if (ComparableSecond is TimeOnly c2)
            {
                if (start == null) start = c2;
                else if (end == null) end = c2;
            }
            if (start == null && end == null)
                return null;
            if (start != null && end != null && start > end)
                (start, end) = (end, start);

            return (start, end);
        }


        public IEnumerable<ISensor> GetSensors()
        {
            if (VariableFirst is ISensor s1) yield return s1;
            if (ComparableFirst is ISensor s2) yield return s2;
            if (VariableSecond is ISensor s3) yield return s3;
            if (ComparableSecond is ISensor s4) yield return s4;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class UserRuleSingleOperation : IRuleSingleOperation, INotifyPropertyChanged
    {
        private IComparable variable;
        private string operation;
        private IComparable comparable;
        private IActor actor;
        private bool actorState;
        private TimeSpan? timer;
        private DateTime startedAt=DateTime.MinValue;
        private uint id;
        public event PropertyChangedEventHandler? PropertyChanged;

        public UserRuleSingleOperation()
        {
            id = 0;
        }

        public UserRuleSingleOperation(UserRuleSingleOperation other)
        {
            variable = other.variable;
            operation = other.operation;
            comparable = other.comparable;
            actor = other.actor;
            actorState = other.actorState;
            timer = other.timer;
            startedAt = other.StartedAt;
            id = other.id == 0 ? RuleIdGenerator.generate() : other.id ;
        }

        public UserRuleSingleOperation(IComparable var,string op,IComparable compare_to,IActor actor,TimeSpan? timer, bool actorState)
        {
            if(var==null)
            {
                throw new ArgumentNullException(nameof(var));
            }else if (compare_to == null)
            {
                throw new ArgumentNullException(nameof(compare_to));
            }else ArgumentNullException.ThrowIfNull(actor);
            variable = var;
            operation = op;
            comparable = compare_to;
            this.actor = actor;
            this.actorState = actorState;
            this.timer= timer;
            startedAt = DateTime.MinValue;
            id = RuleIdGenerator.generate();
        }

        public IComparable Variable { get => variable; set {
                if (!Equals(variable, value))
                {
                    variable = value;
                    OnPropertyChanged();
                }
            } }
        public string Operation { get => operation; set {
                if (!Equals(operation, value))
                {
                    operation = value;
                    OnPropertyChanged();
                }
            } }
        public IComparable Comparable { get => comparable; set {
                if (!Equals(comparable, value))
                {
                    comparable = value;
                    OnPropertyChanged();
                }
            } }
        public IActor Actor { get => actor; set {
                if (!Equals(actor, value))
                {
                    actor = value;
                    OnPropertyChanged();
                }
            } }
        public bool ActorState { get => actorState; set {
                if (!Equals(actorState, value))
                {
                    actorState = value;
                    OnPropertyChanged();
                }
            } }
        public TimeSpan? Timer { get => timer; set {
                if (!Equals(timer, value))
                {
                    timer = value;
                    OnPropertyChanged();
                }
            } }
        public DateTime StartedAt { get => startedAt; private set; }
        public uint Id { get => id; }
        public bool IsActive()
        {
            Console.WriteLine(variable.ToString() + " " + operation + " " + comparable.ToString());
            int compare_result = variable is ISensor ? variable.CompareTo(comparable) : TimeOnly.FromDateTime(DateTime.Now).CompareTo(comparable);
            return Parser.ParseExpressionResult(operation, compare_result);
        }

        public int CompareTo(IRules? other)
        {
            if (other == null) return 1;
            return StartedAt.CompareTo(other.StartedAt);
        }

        public TimeOnly? isTimeRule()
        {
            return variable is TimeOnly ? (TimeOnly)(object)comparable : null;
        }

        public IEnumerable<ISensor> GetSensors()
        {
            if (Variable is ISensor s1) yield return s1;
            if (Comparable is ISensor s2) yield return s2;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
