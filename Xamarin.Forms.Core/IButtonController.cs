using System;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public interface IButtonController : IViewController
	{
		void SendClicked();
		void SendPressed();
		void SendReleased();
		object CommandParameter { get; set; }
		ICommand Command { get; set; }
		event EventHandler<BindableValueChangedEventArgs> CommandChanged;
		event EventHandler<BindableValueChangedEventArgs> CommandChanging;
		event EventHandler CommandCanExecuteChanged;

		bool IsEnabledCore { set; }


		void OnClicked();
		void OnPressed();
		void OnReleased();
		bool IsPressed { get; }
		void SetIsPressed(bool isPressed);

	}
}