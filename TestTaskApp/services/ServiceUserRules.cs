using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.rules;
using TestTaskApp.sensors;

namespace TestTaskApp.services
{
    public class ServiceUserRules
    {
        private readonly List<IRules> rules=[];
        private readonly ServiceActors srvActors;
        private readonly ServiceSensors srvSensors;
        public ServiceUserRules(ServiceActors serviceActors,ServiceSensors serviceSensors) {
            srvActors = serviceActors;
            srvSensors = serviceSensors;
            /*UserRuleSingleOperation rule = new(new TimeOnly(20, 04, 02), "<=", new TimeOnly(23, 01, 02), srvActors.GetActorByType("ActorPump")[0],
                null, true
               );
            UserRuleDoubleOperation rule2 = new(new TimeOnly(06, 42, 00), "<=", new TimeOnly(13, 38, 11), srvSensors.GetSensors()[5],"=",20,
                srvActors.GetActorByType("ActorVentilation")[0],
                true,null
               );*/
            //Console.WriteLine("&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& "+rule.IsActive());
          //rules.Add(rule);
           // rules.Add(rule2);
        }

        public void AddRule(IRules rule)
        {
            if (!IsValid(rule))
            {
                throw new ArgumentException();
            }
            rules.Add(rule);
        }

        public void AddDoubleOperationRule(IComparable firstVariable,string firstOperation, IComparable firstComparable, IComparable secondVariable,string secondOperation,IComparable secondComparable,IActor actor,bool actorState,TimeSpan? timer)
        {
            rules.Add(
                new UserRuleDoubleOperation(firstVariable,firstOperation,firstComparable,secondVariable,secondOperation,secondComparable,actor,actorState,timer)
                );
        }

        public void AddSingleOperationRule(IComparable variable,string operation, IComparable comparable,IActor actor,bool actorState,TimeSpan? timer)
        {
            rules.Add(
                new UserRuleSingleOperation(variable,operation,comparable,actor,timer, actorState)
                );
        }

        public List<IRules> GetRules()
        {
            return rules;
        }
        public void UpdateRule(uint id, IComparable variable, string operation, IComparable comparable, IActor actor, bool actorState, TimeSpan? timer)
        {
            var ruleFound = (rules.FirstOrDefault(r => r.Id == id));
            if (ruleFound is IRuleSingleOperation rule)
            {
                rule.Variable = variable;
                rule.Operation = operation;
                rule.Comparable = comparable;
                rule.Actor = actor;
                rule.Timer = timer;
                rule.ActorState = actorState;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void UpdateRule(uint id, IComparable variable, string operation, IComparable comparable,IComparable variableSecond,string operationSecond,IComparable comparableSecond, IActor actor, bool actorState, TimeSpan? timer)
        {
            var ruleFound = (rules.FirstOrDefault(r => r.Id == id));
            if (ruleFound is IRuleDoubleOperation rule)
            {
                rule.VariableFirst = variable;
                rule.OperationFirst = operation;
                rule.ComparableFirst = comparable;
                rule.VariableSecond=variableSecond; 
                rule.OperationSecond=operationSecond;
                rule.ComparableSecond=comparableSecond;
                rule.Actor = actor;
                rule.Timer = timer;
                rule.ActorState = actorState;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void RemoveRule(uint id)
        {
            var rule = rules.FirstOrDefault(r => r.Id == id);
            if (rule != null)
            {
                rules.Remove(rule);
            }
        }

        public bool IsValid(IRules newRule)
        {

            return IsNoActorConflicts(newRule); //|| IsNoTimeConflicts(newRule);
        }

        private bool IsNoTimeConflicts(IRules rule)
        {
            var list = new List<IRules>(rules);
            list.Add(rule);
            var timeRules = list
                 .Select(r => new
                 {
                     Rule = r,
                     Range = (r is UserRuleDoubleOperation d) ? d.GetTimeRange()
                             : (r is UserRuleSingleOperation s && s.isTimeRule() is TimeOnly t)
                                 ? (start: t, end: t)
                                 : ((TimeOnly?, TimeOnly?)?)null,
                     r.Actor
                 })
                 .Where(r => r.Range != null)
                 .ToList();

            foreach (var group in timeRules.GroupBy(r => r.Actor))
            {
                var rulesForActor = group.ToList();
                for (int i = 0; i < rulesForActor.Count; i++)
                {
                    for (int j = i + 1; j < rulesForActor.Count; j++)
                    {
                        var (start1, end1) = rulesForActor[i].Range.Value;
                        var (start2, end2) = rulesForActor[j].Range.Value;

                        if (RangesOverlap(start1, end1, start2, end2))
                            return false;
                    }
                }
            }

            return true;

            static bool RangesOverlap(TimeOnly? start1, TimeOnly? end1, TimeOnly? start2, TimeOnly? end2)
            {
                if (start1 == null || start2 == null)
                    return false;

                var s1 = start1.Value;
                var e1 = end1 ?? start1;
                var s2 = start2.Value;
                var e2 = end2 ?? start2;

                return s1 < e2 && s2 < e1;
            }
        }


        private bool IsNoActorConflicts(IRules rule)
        {
            var list = new List<IRules>(rules);
            list.Add(rule);
            var groups = list.GroupBy(r => r.Actor);
            foreach (var group in groups)
            {
                var actorRules = group.ToList();
                for (int i = 0; i < actorRules.Count; i++)
                {
                    for (int j = i + 1; j < actorRules.Count; j++)
                    {
                        var r1 = actorRules[i];
                        var r2 = actorRules[j];
                        if (r1.ActorState == r2.ActorState)
                            continue; 
                        if (ConditionsOverlap(r1, r2))
                        {
                            return IsNoTimeConflicts(rule);
                        }
                    }
                }
            }
            return true;
        }

        private bool ConditionsOverlap(IRules r1, IRules r2)
        {
            if (r1 is IRuleSingleOperation s1 && r2 is IRuleSingleOperation s2)
            {
                if (s1.Variable is not ISensor || s2.Variable is not ISensor)
                    return false;

                if (s1.Variable.GetType() == s2.Variable.GetType() && s1.Variable is IComparable)
                {
                    double min1 = GetLowerBound(s1);
                    double max1 = GetUpperBound(s1);
                    double min2 = GetLowerBound(s2);
                    double max2 = GetUpperBound(s2);

                    return min1 < max2 && min2 < max1;
                }
            }
            return false;
        }

        private double GetLowerBound(IRuleSingleOperation r)
        {
            if (r.Operation is ">" or ">=")
                return Convert.ToDouble(r.Comparable);
            return double.NegativeInfinity;
        }

        private double GetUpperBound(IRuleSingleOperation r)
        {
            if (r.Operation is "<" or "<=")
                return Convert.ToDouble(r.Comparable);
            return double.PositiveInfinity;
        }
    }
}
