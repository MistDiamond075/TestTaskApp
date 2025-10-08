using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskApp.rules
{
    public class DTORules
    {
        public List<DTORuleSingle> SingleRules { get; set; } = new();
        public List<DTORuleDouble> DoubleRules { get; set; } = new();
        public int Version { get; set; } = 1;
    }

    public class DTORuleSingle
    {
        public string? VariableName { get; set; }
        public string? Operation { get; set; }
        public string? ComparableName { get; set; }
        public string? ActorName { get; set; }
        public bool ActorState { get; set; }
        public TimeSpan? Timer { get; set; }
    }

    public class DTORuleDouble
    {
        public string? VariableFirstName { get; set; }
        public string? OperationFirst { get; set; }
        public string? ComparableFirstName { get; set; }

        public string? VariableSecondName { get; set; }
        public string? OperationSecond { get; set; }
        public string? ComparableSecondName { get; set; }

        public string? ActorName { get; set; }
        public bool ActorState { get; set; }
        public TimeSpan? Timer { get; set; }
    }
}
