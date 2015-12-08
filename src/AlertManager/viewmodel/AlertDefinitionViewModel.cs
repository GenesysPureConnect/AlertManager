using ININ.IceLib.Statistics.Alerts;

namespace AlertManager.viewmodel
{
    public class AlertDefinitionViewModel : ViewModelBase
    {
        #region Private Members

        private static AlertDefinition _alertDefinition;

        #endregion



        #region Public Members

        public string DisplayString => _alertDefinition.DisplayString;
        public string Description => _alertDefinition.Description;
        public int AlertRuleCount => _alertDefinition.AlertRules.Count;
        public string UriString => _alertDefinition.StatisticKey.UriString;

        #endregion



        public AlertDefinitionViewModel(AlertDefinition alertDefinition)
        {
            _alertDefinition = alertDefinition;
        }



        #region Private Methods



        #endregion



        #region Public Methods



        #endregion



        #region Operators

        public static implicit operator AlertDefinitionViewModel(AlertDefinition alertDefinition)
        {
            return new AlertDefinitionViewModel(alertDefinition);
        }

        #endregion
    }
}