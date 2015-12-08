using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ININ.IceLib.Statistics.Alerts;

namespace AlertManager.viewmodel
{
    public class AlertSetSummaryViewModel : ViewModelBase
    {
        #region Private Members

        private AlertSet _alertSet;

        #endregion



        #region Public Members

        public string Id => _alertSet.Id;
        public string DisplayString => _alertSet.DisplayString;
        public string Owner => _alertSet.Owner;
        public string OwnerDisplayName => _alertSet.OwnerDisplayName;
        public string OwnerDisplayString => string.IsNullOrEmpty(OwnerDisplayName) ? Owner : OwnerDisplayName;
        public AlertSetAccessMode AccessMode => _alertSet.AccessMode;
        public int AlertDefinitionsCount => _alertSet.AlertDefinitions.Count;
        public bool HasAlerts => AlertDefinitionsCount > 0;

        public string TooltipText
            =>
                (string.IsNullOrEmpty(OwnerDisplayName) ? Owner : OwnerDisplayName + $" ({Owner})") +
                $"  ID: {_alertSet.Id}";

        #endregion



        public AlertSetSummaryViewModel(AlertSet alertSet)
        {
            _alertSet = alertSet;
            Update(alertSet);
        }



        #region Private Methods



        #endregion



        #region Public Methods

        public void Update(AlertSet alertSet)
        {
            _alertSet = alertSet;

            //Context.Send(s =>
            //{
                try
                {
                    // Trigger property updates
                    OnPropertyChanged(nameof(Id));
                    OnPropertyChanged(nameof(DisplayString));
                    OnPropertyChanged(nameof(Owner));
                    OnPropertyChanged(nameof(OwnerDisplayName));
                    OnPropertyChanged(nameof(OwnerDisplayString));
                    OnPropertyChanged(nameof(AccessMode));
                    OnPropertyChanged(nameof(AlertDefinitionsCount));
                    OnPropertyChanged(nameof(HasAlerts));
                    OnPropertyChanged(nameof(TooltipText));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            //}, null);
        }

        #endregion



        #region Operators

        public static implicit operator AlertSetSummaryViewModel(AlertSet alertSet)
        {
            return new AlertSetSummaryViewModel(alertSet);
        }

        #endregion
    }
}
