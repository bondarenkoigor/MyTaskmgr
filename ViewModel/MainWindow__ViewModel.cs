using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using MyTaskmgr.Model;
using System.Threading;

namespace MyTaskmgr.ViewModel
{
    public class MainWindow__ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Process__Model> Processes { get; set; }

        private Process__Model selectedProcess;

        public Process__Model SelectedProcess
        {
            get { return selectedProcess; }
            set { selectedProcess = value; OnPropertyChanged(nameof(SelectedProcess)); }
        }

        private PerformanceCounter totalCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private PerformanceCounter totalRAMCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", null);

        public int TotalCPU
        {
            get { return (int)totalCPUCounter.NextValue(); }
            private set {  }
        }

        public int TotalRAM
        {
            get { return (int)totalRAMCounter.NextValue(); }
            private set { }
        }

        public MainWindow__ViewModel()
        {
            Thread updateProcesses = new Thread(new ThreadStart(UpdateProcesses));
            updateProcesses.Start();
            updateProcesses.IsBackground = true;

        }

        private void UpdateProcesses()
        {
            while (true)
            {
                var processList = Process.GetProcesses().OrderBy(proc => proc.ProcessName).ToList();
                var modelList = new ObservableCollection<Process__Model>();
                foreach (var process in processList)
                    modelList.Add(new Process__Model(process));

                Processes = modelList;
                try { this.SelectedProcess = modelList[modelList.IndexOf(selectedProcess)]; } catch { }
                Thread.Sleep(1000);
                OnPropertyChanged(nameof(Processes));
                OnPropertyChanged(nameof(TotalCPU));
                OnPropertyChanged(nameof(TotalRAM));
            }
        }

        RelayCommand endTaskCommand;

        public RelayCommand EndTaskCommand
        {
            get
            {
                return endTaskCommand ?? (endTaskCommand = new RelayCommand(() =>
                {
                    try
                    {
                        SelectedProcess.MyProcess.Kill();
                        SelectedProcess.cpuUsageUpdate.Abort();
                        this.Processes.Remove(this.SelectedProcess);
                    }
                    catch { }
                }));
            }
            set { endTaskCommand = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
