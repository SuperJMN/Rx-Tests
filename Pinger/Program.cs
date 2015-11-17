using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinger
{
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
           var pinger = new SystemPinger("wordpress.com");
            pinger.PingSource.Subscribe(OnNext);
            Console.Read();
        }

        private static void OnNext(bool b)
        {
            Console.WriteLine(b ? "Reachable" : "Unreachable");
        }

        public class Pinger
        {
            public SystemPinger SystemWatcher { get; set; }
        }

        public class SystemPinger
        {
            private const int PingTimeout = 3000;
            private const int PingFrequencySeconds = 30;

            public SystemPinger(string computerName)
            {
                var pollingPeriod = TimeSpan.FromSeconds(PingFrequencySeconds);
                var scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = "DatabasePoller" });

                var query = Observable.Timer(pollingPeriod, scheduler)
                                .SelectMany(_ => Ping(computerName).ToObservable())
                                .Repeat();  //Loop on success

                PingSource = query;
            }

            public IObservable<bool> PingSource { get; set; }

            private static Task<bool> Ping(string computer)
            {
                return Task.Run(
                    () =>
                    {
                        var ping = new Ping();
                        try
                        {
                            var reply = ping.Send(computer, PingTimeout);
                            return reply.Status == IPStatus.Success;
                        }
                        catch (PingException e)
                        {
                            return false;
                        }
                    });
            }
        }
    }
}
