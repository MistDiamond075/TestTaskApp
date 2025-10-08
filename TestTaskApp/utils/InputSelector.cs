using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.sensors;

namespace TestTaskApp.utils
{
    public class InputSelector : IDataTemplate
    {
        public IDataTemplate? SensorTemplate { get; set; }
        public IDataTemplate? TimeTemplate { get; set; }
        public IDataTemplate? DefaultTemplate { get; set; }

        public bool Match(object? data)
        {
            return true;
        }

        public Control? Build(object? param)
        {
            if (param is ISensor && SensorTemplate != null)
                return SensorTemplate.Build(param);

            if (param is TimeOnly && TimeTemplate != null)
                return TimeTemplate.Build(param);

            return DefaultTemplate?.Build(param);
        }
    }
}
