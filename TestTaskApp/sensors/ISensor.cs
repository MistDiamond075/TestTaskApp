using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskApp.sensors
{
    public interface ISensor: INotifyPropertyChanged,IComparable
    {
        int Value {  get; set; }
        int Trend {  get; set; }
        DateTime LastUpdated { get; }
    }
}
