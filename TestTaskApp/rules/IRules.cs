using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.sensors;

namespace TestTaskApp.rules
{
    public interface IRules:IComparable<IRules> {
        IActor Actor { get; set; }
        bool ActorState { get; set; }
        TimeSpan? Timer { get; set; }
        uint Id { get; }
        DateTime StartedAt { get; }
        bool IsActive();
        TimeOnly? isTimeRule();
        IEnumerable<ISensor> GetSensors();
    }

    public interface IRuleDoubleOperation: IRules
    {
        IComparable VariableFirst { get; set; }
        string OperationFirst {  get; set; }
        IComparable ComparableFirst { get; set; }
        IComparable VariableSecond { get; set; }
        string OperationSecond { get; set; }
        IComparable ComparableSecond { get; set; }
    }

    public interface IRuleSingleOperation: IRules 
    {
        IComparable Variable { get; set; }
        string Operation { get; set; }
        IComparable Comparable { get; set; }
    }

    public static class RuleIdGenerator
    {
        private static uint globalId = 0;
        private static readonly object _lock = new();

        public static uint generate()
        {
            return Interlocked.Increment(ref globalId);
        }
    }
}
