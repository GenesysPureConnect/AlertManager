using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlertManager.Annotations;

namespace AlertManager.viewmodel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Private Members

        protected SynchronizationContext Context { get; }

        #endregion



        #region Public Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion



        public ViewModelBase()
        {
            Context = SynchronizationContext.Current;
            if (Context == null)
                throw new ThreadStateException("Synchronization context was null in the constructor for " + GetType());
        }



        #region Private Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



        #region Public Methods



        #endregion
    }
}
