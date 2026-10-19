using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjetoFinalMVVM.ViewModels
{
    /// <summary>
    /// Classe base para as ViewModels implementando INotifyPropertyChanged de forma manual e pura.
    /// Não utiliza bibliotecas externas como CommunityToolkit.Mvvm, garantindo aderência
    /// estrita às boas práticas acadêmicas e aos conceitos fundamentais de Data Binding.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Dispara a notificação de alteração de propriedade para atualizar a interface (UI).
        /// </summary>
        /// <param name="propertyName">Nome da propriedade que foi alterada.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Método auxiliar clássico para atribuir o valor e notificar caso haja alteração.
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
