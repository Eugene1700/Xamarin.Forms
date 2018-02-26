using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ImageButtonRenderer))]
	public class ImageButton : View, IImageController, IElementConfiguration<ImageButton>, IBorderElement, IButtonController, IBorderController
	{
		const int DefaultBorderRadius = 5;
		const int DefaultCornerRadius = -1;

		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Button), null,
					propertyChanging: OnCommandChanging,
					propertyChanged: OnCommandChanged
				);

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create("CornerRadius", typeof(int), typeof(Button), defaultValue: DefaultCornerRadius);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(ImageButton), null,
			propertyChanged: (bindable, oldvalue, newvalue) => ButtonElementManager.CommandCanExecuteChanged(bindable, EventArgs.Empty));

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(Button), -1d);

		public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageButton), default(ImageSource),
			propertyChanging: OnSourcePropertyChanging, propertyChanged: OnSourcePropertyChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(ImageButton), Aspect.AspectFit);

		public static readonly BindableProperty IsOpaqueProperty = BindableProperty.Create(nameof(IsOpaque), typeof(bool), typeof(ImageButton), false);

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(ImageButton), default(bool));

		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(ImageButton), default(bool));

		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<ImageButton>> _platformConfigurationRegistry;

		public event EventHandler Clicked;
		public event EventHandler Pressed;
		public event EventHandler Released;
		event EventHandler<BindableValueChangedEventArgs> _imageSourceChanged;
		event EventHandler<BindableValueChangedEventArgs> _imageSourceChanging;
		event EventHandler _imageSourcesSourceChanged;


		public ImageButton()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ImageButton>>(() => new PlatformConfigurationRegistry<ImageButton>(this));
			ImageElementManager.Init(this);
			ButtonElementManager.Init(this);
		}

		public Color BorderColor
		{
			get { return (Color)GetValue(BorderElement.BorderColorProperty); }
			set { SetValue(BorderElement.BorderColorProperty, value); }
		}

		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set { SetValue(CornerRadiusProperty, value); }
		}

		public double BorderWidth
		{
			get { return (double)GetValue(BorderWidthProperty); }
			set { SetValue(BorderWidthProperty, value); }
		}

		public Aspect Aspect
		{
			get { return (Aspect)GetValue(AspectProperty); }
			set { SetValue(AspectProperty, value); }
		}

		public bool IsLoading
		{
			get { return (bool)GetValue(IsLoadingProperty); }
		}
		public bool IsPressed
		{
			get { return (bool)GetValue(IsPressedProperty); }
		}

		public bool IsOpaque
		{
			get { return (bool)GetValue(IsOpaqueProperty); }
			set { SetValue(IsOpaqueProperty, value); }
		}
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		bool IButtonController.IsEnabledCore
		{
			set { SetValueCore(IsEnabledProperty, value); }
		}


		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			ImageElementManager.OnBindingContextChanged(this, this);
			base.OnBindingContextChanged();
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			SizeRequest desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
			return ImageElementManager.Measure(this, desiredSize, widthConstraint, heightConstraint);
		}

		event EventHandler<BindableValueChangedEventArgs> IImageController.ImageSourceChanged
		{
			add => _imageSourceChanged += value;
			remove => _imageSourceChanged -= value;
		}

		event EventHandler<BindableValueChangedEventArgs> IImageController.ImageSourceChanging
		{
			add => _imageSourceChanging += value;
			remove => _imageSourceChanging -= value;
		}

		event EventHandler IImageController.ImageSourcesSourceChanged
		{
			add => _imageSourcesSourceChanged += value;
			remove => _imageSourcesSourceChanged -= value;
		}

		private void OnSourceChanged(object sender, EventArgs e) =>
			_imageSourcesSourceChanged?.Invoke(this, EventArgs.Empty);

		static void OnSourcePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var image = ((ImageButton)bindable);

			if (newvalue != null)
			{
				((ImageSource)newvalue).SourceChanged += image.OnSourceChanged;
			}

			image._imageSourceChanged?.Invoke(bindable, new BindableValueChangedEventArgs(bindable, oldvalue, newvalue));
		}

		static void OnSourcePropertyChanging(BindableObject bindable, object oldvalue, object newvalue)
		{
			var image = ((ImageButton)bindable);
			if (oldvalue != null)
			{
				((ImageSource)oldvalue).SourceChanged -= image.OnSourceChanged;
			}

			image._imageSourceChanging?.Invoke(bindable, new BindableValueChangedEventArgs(bindable, oldvalue, newvalue));
		}


		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsLoading(bool isLoading)
		{
			SetValue(IsLoadingPropertyKey, isLoading);
		}

		public IPlatformElementConfiguration<T, ImageButton> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}


		protected override void ChangeVisualState()
		{
			if (IsEnabled && IsPressed)
			{
				VisualStateManager.GoToState(this, ButtonElementManager.PressedVisualState);
			}
			else
			{
				base.ChangeVisualState();
			}
		}

		event EventHandler<BindableValueChangedEventArgs> _commandChanged;
		event EventHandler<BindableValueChangedEventArgs> _commandChanging;
		event EventHandler _commandCanExecuteChanged;

		event EventHandler<BindableValueChangedEventArgs> IButtonController.CommandChanged { add => _commandChanged += value; remove => _commandChanged -= value; }
		event EventHandler<BindableValueChangedEventArgs> IButtonController.CommandChanging { add => _commandChanging += value; remove => _commandChanging -= value; }
		event EventHandler IButtonController.CommandCanExecuteChanged { add => _commandCanExecuteChanged += value; remove => _commandCanExecuteChanged -= value; }

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetIsPressed(bool isPressed) =>
			SetValue(IsPressedPropertyKey, isPressed);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendClicked() =>
			ButtonElementManager.SendClicked(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendPressed() =>
			ButtonElementManager.SendPressed(this, this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendReleased() =>
			ButtonElementManager.SendReleased(this, this);

		public void OnClicked() =>
			Clicked?.Invoke(this, EventArgs.Empty);

		public void OnPressed() =>
			Pressed?.Invoke(this, EventArgs.Empty);

		public void OnReleased() =>
			Released?.Invoke(this, EventArgs.Empty);

		public void RaiseImageSourcePropertyChanged() =>
			OnPropertyChanged(nameof(Source));

		BindableProperty IBorderController.CornerRadiusProperty => ImageButton.CornerRadiusProperty;

		BindableProperty IBorderController.BorderColorProperty => ImageButton.BorderColorProperty;

		BindableProperty IBorderController.BorderWidthProperty => ImageButton.BorderWidthProperty;

		BindableProperty IImageController.SourceProperty => SourceProperty;

		BindableProperty IImageController.AspectProperty => AspectProperty;

		BindableProperty IImageController.IsOpaqueProperty => IsOpaqueProperty;

		private void OnCommandCanExecuteChanged(object sender, EventArgs e) =>
			_commandCanExecuteChanged?.Invoke(this, EventArgs.Empty);

		private static void OnCommandChanged(BindableObject bo, object o, object n)
		{
			var button = (ImageButton)bo;
			if (n != null)
			{
				var newCommand = n as ICommand;
				newCommand.CanExecuteChanged += button.OnCommandCanExecuteChanged;
			}

			button._commandChanged?.Invoke(bo, new BindableValueChangedEventArgs(bo, o, n));
		}

		private static void OnCommandChanging(BindableObject bo, object o, object n)
		{
			var button = (ImageButton)bo;
			if (o != null)
			{
				(o as ICommand).CanExecuteChanged -= button.OnCommandCanExecuteChanged;
			}

			button._commandChanging?.Invoke(bo, new BindableValueChangedEventArgs(bo, o, n));
		}
	}
}