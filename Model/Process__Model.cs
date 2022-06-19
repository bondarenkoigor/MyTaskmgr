using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace MyTaskmgr.Model
{
    public class Process__Model : INotifyPropertyChanged
    {
        public Process MyProcess { get; set; }
        private PerformanceCounter CPUCounter { get; set; }

        public Thread cpuUsageUpdate;

        public Process__Model(Process process)
        {
            MyProcess = process;

            CPUCounter = new PerformanceCounter("Process", "% Processor Time", MyProcess.ProcessName);
            CPUCounter.NextValue();

            cpuUsageUpdate = new Thread(new ThreadStart(UpdateCPU));
            cpuUsageUpdate.IsBackground = true;
            cpuUsageUpdate.Start();

        }

        private void UpdateCPU()
        {
            this.CPUUsage = this.CPUCounter.NextValue();
        }

        public string ProcessName { get 
            {
                if(MyProcess.MainWindowTitle == String.Empty) return MyProcess.ProcessName;
                return MyProcess.MainWindowTitle;
            } set { } }
        public double RAMUsage { get { return MyProcess.PrivateMemorySize64 / 1000000.0; } set { } }


        private float _CPUUsage;

        public float CPUUsage
        {
            get { return _CPUUsage / Environment.ProcessorCount; }
            set { _CPUUsage = value; OnPropertyChanged("CPUUsage"); }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
