using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PacketSnifferWPF.Helpers;
using PacketSnifferWPF.Models;

namespace PacketSnifferWPF
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<PacketModel> _packets = new();
        private CancellationTokenSource? _cts;
        private int _count;
        private DateTime _startTime;

        public MainWindow()
        {
            InitializeComponent();
            PacketGrid.ItemsSource = _packets;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            _cts = new CancellationTokenSource();
            _packets.Clear();
            _count = 0;
            CountText.Text = "0";
            PpsText.Text = "0";
            _startTime = DateTime.UtcNow;
            Task.Run(() => CaptureLoop(_cts.Token));
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
        }

        private async Task CaptureLoop(CancellationToken ct)
        {
            var rnd = new Random();
            while (!ct.IsCancellationRequested)
            {
                // Simulate packet arrival time between 50ms and 600ms
                await Task.Delay(rnd.Next(50, 600), ct);
                var raw = PacketGenerator.GenerateRandomPacket();
                var model = PacketGenerator.ParseRawPacket(raw);
                model.Index = ++_count;

                Dispatcher.Invoke(() =>
                {
                    _packets.Insert(0, model);
                    CountText.Text = _count.ToString();
                    var elapsed = DateTime.UtcNow - _startTime;
                    var pps = elapsed.TotalSeconds > 0 ? Math.Round(_count / elapsed.TotalSeconds, 2) : 0;
                    PpsText.Text = pps.ToString();
                    if (_packets.Count > 1000) _packets.RemoveAt(_packets.Count - 1);
                });
            }
        }

        private void PacketGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PacketGrid.SelectedItem is PacketModel p)
            {
                DetailsBox.Text = p.FullDetails;
            }
        }
    }
}
