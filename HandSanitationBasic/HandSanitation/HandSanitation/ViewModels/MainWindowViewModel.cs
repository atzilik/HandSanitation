using System.Windows;
using HandSanitation.ArduinoAccess;
using HandSanitation.ArduinoAccess.Events;
using HandSanitation.Core.Events;
using Prism.Events;
using Prism.Mvvm;

namespace HandSanitation.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDataAccess _dataAccess;
        private readonly IEventAggregator _eventAggregator;
        private string _title = "Prism Application";
        private string _employeeId;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public string EmployeeId
        {
            get => _employeeId;
            set
            {
                SetProperty(ref _employeeId, value);
                if(!string.IsNullOrEmpty(value))
                _eventAggregator.GetEvent<NewEmployeeIdEvent>().Publish(int.Parse(value));
            }
        }

        public MainWindowViewModel(IDataAccess dataAccess, IEventAggregator eventAggregator)
        {
            _dataAccess = dataAccess;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<AddedToDbEvent>().Subscribe(() => EmployeeId = string.Empty);
            _dataAccess.ReadFromPort();

        }
    }
}
