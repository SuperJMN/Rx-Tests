namespace WpfApplication6
{
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using Annotations;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string message;
        private Random r = new Random((int) DateTimeOffset.Now.Ticks);

        public MainWindow()
        {
            InitializeComponent();

            var query =
                from n in Observable.Interval(TimeSpan.FromSeconds(2.0))
                from ds in Observable.Start(CallService).Amb(
                    Observable
                        .Timer(TimeSpan.FromSeconds(5.0))
                        .Select(_ => "Timeout"))
                select ds;

            query.Subscribe(s => Message = s);

            DataContext = this;
        }

        public string Message
        {
            get { return message; }
            set
            {
                if (value == message)
                {
                    return;
                }
                message = value;
                OnPropertyChanged();
            }
        }

        public string Result { get; set; }

        private string CallService()
        {
            var sleepingFor = r.Next(500, 8000);
            Console.WriteLine($"Sleeping for: {sleepingFor}");
            Thread.Sleep(sleepingFor);
            return "OK";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}