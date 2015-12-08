using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ININ.IceLib.Statistics.Alerts;

namespace AlertManager.viewmodel
{
    public class AlertSetViewModel : ViewModelBase
    {
        #region Private Members

        private AlertSet _alertSet;

        #endregion



        #region Public Members

        public string Id => _alertSet.Id;
        public string DisplayString => _alertSet.DisplayString;
        public string Owner => _alertSet.Owner;
        public string OwnerDisplayName => _alertSet.OwnerDisplayName;
        public string OwnerDisplayString => $"{_alertSet.OwnerDisplayName} ({_alertSet.Owner})";
        public int AlertDefinitionsCount => AlertDefinitions.Count;

        public ObservableCollection<AlertDefinitionViewModel> AlertDefinitions { get; } =
            new ObservableCollection<AlertDefinitionViewModel>();

        #endregion



        public AlertSetViewModel(AlertSet alertSet)
        {
            Update(alertSet);
        }



        #region Private Methods



        #endregion



        #region Public Methods

        public void Update(AlertSet alertSet)
        {
            Context.Send(s =>
            {
                try
                {
                    _alertSet = alertSet;

                    // Clear list
                    AlertDefinitions.Clear();

                    // Add definitions
                    foreach (var alertDefinition in alertSet.AlertDefinitions)
                    {
                        AlertDefinitions.Add(alertDefinition);
                    }

                    // Trigger property updates
                    OnPropertyChanged(nameof(Id));
                    OnPropertyChanged(nameof(DisplayString));
                    OnPropertyChanged(nameof(Owner));
                    OnPropertyChanged(nameof(OwnerDisplayName));
                    OnPropertyChanged(nameof(OwnerDisplayString));
                    OnPropertyChanged(nameof(AlertDefinitionsCount));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, null);
        }

        #endregion



        #region Operators

        public static implicit operator AlertSetViewModel(AlertSet alertSet)
        {
            return new AlertSetViewModel(alertSet);
        }

        #endregion
    }
}
