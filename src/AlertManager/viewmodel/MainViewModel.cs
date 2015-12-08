using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ININ.IceLib.Connection;
using ININ.IceLib.Statistics;
using ININ.IceLib.Statistics.Alerts;

namespace AlertManager.viewmodel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        private Session _session = new Session();
        private AlertCatalog _alertCatalog;

        private BackgroundWorker _cleanupWorker = new BackgroundWorker();
        private bool _cleanupEmptyAlertSets;
        private bool _cleanupDeletedUsers;
        private bool _cleanupInactiveUsers;
        private string _statusText = "";
        private bool _confirmManualDelete = true;
        private bool _isConnected;
        private bool _isConnecting;

        #endregion



        #region Public Members

        public ObservableCollection<AlertSetSummaryViewModel> AlertSets { get; } =
            new ObservableCollection<AlertSetSummaryViewModel>();

        public int AlertSetCount => AlertSets.Count;
        public int EmptyAlertSetCount => AlertSets.Count(set => !set.HasAlerts);

        public bool CleanupEmptyAlertSets
        {
            get { return _cleanupEmptyAlertSets; }
            set
            {
                if (value == _cleanupEmptyAlertSets) return;
                _cleanupEmptyAlertSets = value;
                OnPropertyChanged();
            }
        }

        public bool CleanupDeletedUsers
        {
            get { return _cleanupDeletedUsers; }
            set
            {
                if (value == _cleanupDeletedUsers) return;
                _cleanupDeletedUsers = value;
                OnPropertyChanged();
            }
        }

        public bool CleanupInactiveUsers
        {
            get { return _cleanupInactiveUsers; }
            set
            {
                if (value == _cleanupInactiveUsers) return;
                _cleanupInactiveUsers = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (value == _statusText) return;
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public bool ConfirmManualDelete
        {
            get { return _confirmManualDelete; }
            set
            {
                if (value == _confirmManualDelete) return;
                _confirmManualDelete = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (value == _isConnected) return;
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnecting
        {
            get { return _isConnecting; }
            set
            {
                if (value == _isConnecting) return;
                _isConnecting = value;
                OnPropertyChanged();
            }
        }


        public static MainViewModel Instance { get; private set; }

        #endregion



        public MainViewModel()
        {
            Instance = this;

            _cleanupWorker.DoWork += CleanupWorkerOnDoWork;

            _session.ConnectionStateChanged += SessionOnConnectionStateChanged;
            //Connect("admin.one", "1234", false, "tim-cic.dev2000.com");
        }



        #region Private Methods
        
        private void SessionOnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    IsConnected = _session.ConnectionState == ConnectionState.Up;
                    IsConnecting = _session.ConnectionState == ConnectionState.Attempting;
                }, null);

                switch (_session.ConnectionState)
                {
                    case ConnectionState.None:
                        StatusText = "Welcome";
                        break;
                    case ConnectionState.Up:
                        StatusText = $"Connected to {_session.Endpoint.Host}";
                        break;
                    case ConnectionState.Down:
                        StatusText = "Disconnected";
                        break;
                    case ConnectionState.Attempting:
                        StatusText = "Connecting to CIC...";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (_session.ConnectionState != ConnectionState.Up) return;

                _alertCatalog = new AlertCatalog(StatisticsManager.GetInstance(_session));
                _alertCatalog.AlertCatalogChanged += AlertCatalogOnAlertCatalogChanged;
                _alertCatalog.StartWatchingAsync(AlertSetCategories.All, new[]
                {
                    AlertSet.Property.AccessMode, AlertSet.Property.AlertDefinitions, AlertSet.Property.Created, AlertSet.Property.Description, AlertSet.Property.DisplayString, AlertSet.Property.Id, AlertSet.Property.Modified, AlertSet.Property.ModifiedBy, AlertSet.Property.Owner, AlertSet.Property.OwnerDisplayName, AlertSet.Property.SubscribedByOther, AlertSet.Property.SubscribedByUser, AlertSet.Property.AlertSetSubscribers
                }, AlertSetWatchCompleted, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AlertCatalogOnAlertCatalogChanged(object sender, AlertCatalogChangedEventArgs<AlertSet, AlertSet.Property> e)
        {
            try
            {
#if DEBUG
                // For debugging
                //LogAlertSet(e.Added, "Added");
                //LogAlertSet(e.Changed, "Changed");
                //LogAlertSet(e.Removed, "Removed");
#endif

                // Add new alert sets
                foreach (var alertSet in e.Added)
                {
                    Context.Send(s => AlertSets.Add(alertSet), null);
                }

                // Update existing alert sets
                foreach (var alertSet in e.Changed)
                {
                    Context.Send(s =>
                    {
                        try
                        {
                            var existing = AlertSets.FirstOrDefault(set => set.Id.Equals(alertSet.Id, StringComparison.InvariantCultureIgnoreCase));

                            if (existing != null)
                            {
                                if (!existing.Id.Equals(alertSet.Id, StringComparison.InvariantCultureIgnoreCase))
                                    Console.WriteLine("MISMATCHED IDs!");
                                else if (existing.AlertDefinitionsCount != alertSet.AlertDefinitions.Count)
                                    Console.WriteLine("*** Wrong counts!");
                            }
                            // Update if not null
                            existing?.Update(alertSet);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }, null);
                }

                // Remove alert sets
                foreach (var alertSet in e.Removed)
                {
                    Context.Send(s =>
                    {
                        try
                        {
                            var existing = AlertSets.FirstOrDefault(set => set.Id.Equals(alertSet.Id, StringComparison.InvariantCultureIgnoreCase));

                            // Remove if not null
                            if (existing != null)
                                AlertSets.Remove(existing);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }, null);
                }

                OnPropertyChanged(nameof(AlertSetCount));
                OnPropertyChanged(nameof(EmptyAlertSetCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AlertSetWatchCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogAlertSet(IEnumerable<AlertSet> alerts, string message = null)
        {
            foreach (var alertSet in alerts)
            {
                LogAlertSet(alertSet, message);
            }
        }

        private void LogAlertSet(AlertSet alertSet, string message = "*****")
        {
            Console.WriteLine("===== {0} =====", message);
            Console.WriteLine("ID: {0}", alertSet.Id);
            Console.WriteLine("DisplayString: {0}", alertSet.DisplayString);
            Console.WriteLine("Owner: {0}", alertSet.Owner);
            Console.WriteLine("OwnerDisplayName: {0}", alertSet.OwnerDisplayName);
            Console.WriteLine("AlertDefinitions.Count: {0}", alertSet.AlertDefinitions.Count);
            LogAlert(alertSet.AlertDefinitions);
        }

        private void LogAlert(IEnumerable<AlertDefinition> alertDefinitions)
        {
            var i = 1;
            foreach (var alertDefinition in alertDefinitions)
            {
                LogAlert(alertDefinition, i++.ToString());
            }
        }

        private void LogAlert(AlertDefinition alertDefinition, string message = "")
        {
            Console.WriteLine("  ----- {0} -----", message);
            Console.WriteLine("  DisplayString: {0}", alertDefinition.DisplayString);
            Console.WriteLine("  Description: {0}", alertDefinition.Description);
            Console.WriteLine("  AlertRules.Count: {0}", alertDefinition.AlertRules.Count);
            Console.WriteLine("  StatisticKey.UriString: {0}", alertDefinition.StatisticKey.UriString);
        }

        private void CleanupWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                if (CleanupEmptyAlertSets)
                {
                    SetStatusText("Cleaning up empty Alert Sets...");

                    var deleted = 0;
                    foreach (var alertSetSummaryViewModel in AlertSets.ToArray())
                    {
                        try
                        {
                            if (alertSetSummaryViewModel.AlertDefinitionsCount == 0)
                            {
                                _alertCatalog.RemoveAlertSet(alertSetSummaryViewModel.Id);
                                deleted++;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    SetStatusText($"Cleaned up {deleted} empty Alert Sets");
                }

                if (CleanupDeletedUsers)
                {
                }

                if (CleanupInactiveUsers)
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetStatusText(string message)
        {
            Context.Send(s => StatusText = message, null);
        }

        #endregion

        #region Public Methods

        public void Connect(string username, string password, bool useWindowsAuth, string server)
        {
            try
            {
                if (_session.ConnectionState == ConnectionState.Up)
                {
                    Disconnect();
                    return;
                }

                var authSettings = useWindowsAuth ? new WindowsAuthSettings() as AuthSettings : new ICAuthSettings(username, password);
                _session.ConnectAsync(new SessionSettings(), new HostSettings(new HostEndpoint(server)), authSettings, new StationlessSettings(), null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_alertCatalog.IsWatching())
                    _alertCatalog.StopWatching();

                _session.Disconnect();

                AlertSets.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CleanUp()
        {
            try
            {
                if (!_cleanupWorker.IsBusy) _cleanupWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Delete(AlertSetSummaryViewModel alertSet)
        {
            try
            {
                var result = MessageBoxResult.Yes;
                if (ConfirmManualDelete)
                    result = MessageBox.Show($"Permanently delete Alert set for \"{alertSet.OwnerDisplayString}\" with {alertSet.AlertDefinitionsCount} alerts? " + "This action cannot be undone.", "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    _alertCatalog.RemoveAlertSet(alertSet.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
