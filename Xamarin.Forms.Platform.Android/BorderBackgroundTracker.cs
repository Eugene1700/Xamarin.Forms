using System;
using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class BorderBackgroundTracker : IDisposable
	{
		Drawable _defaultDrawable;
		BorderDrawable _backgroundDrawable;
		RippleDrawable _rippleDrawable;
		bool _drawableEnabled;
		bool _disposed;
		IVisualElementRenderer _renderer;
		VisualElement Element => _renderer?.Element;
		AView Control => _renderer?.View;

		public BorderBackgroundTracker(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_renderer.ElementChanged += OnElementChanged;
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				Button.PropertyChanged -= ButtonPropertyChanged;
			}

			if (e.NewElement != null)
			{
				Button = (IBorderController)e.NewElement;
				Button.PropertyChanged += ButtonPropertyChanged;
			}

			Reset();
			UpdateDrawable();
		}


		public IBorderController Button
		{
			get;
			private set;
		}

		public void UpdateDrawable()
		{
			if (Button == null || Control == null)
				return;


			bool cornerRadiusIsDefault = !Button.IsSet(Button.CornerRadiusProperty) || (Button.CornerRadius == (int)Button.CornerRadiusProperty.DefaultValue || Button.CornerRadius == BorderDrawable.DefaultCornerRadius);
			bool backgroundColorIsDefault = !Button.IsSet(VisualElement.BackgroundColorProperty) || Button.BackgroundColor == (Color)VisualElement.BackgroundColorProperty.DefaultValue;
			bool borderColorIsDefault = !Button.IsSet(Button.BorderColorProperty) || Button.BorderColor == (Color)Button.BorderColorProperty.DefaultValue;
			bool borderWidthIsDefault = !Button.IsSet(Button.BorderWidthProperty) || Button.BorderWidth == (double)Button.BorderWidthProperty.DefaultValue;

			if (backgroundColorIsDefault
				&& cornerRadiusIsDefault
				&& borderColorIsDefault
				&& borderWidthIsDefault)
			{
				if (!_drawableEnabled)
					return;

				if (_defaultDrawable != null)
					Control.SetBackground(_defaultDrawable);

				_drawableEnabled = false;
			}
			else
			{
				if (_backgroundDrawable == null)
					_backgroundDrawable = new BorderDrawable(Control.Context.ToPixels, Forms.GetColorButtonNormal(Control.Context));

				_backgroundDrawable.Button = Button;
				_backgroundDrawable.SetPaddingTop(Control.PaddingTop);

				if (_drawableEnabled)
					return;

				if (_defaultDrawable == null)
					_defaultDrawable = Control.Background;

				if (Forms.IsLollipopOrNewer)
				{
					var rippleColor = _backgroundDrawable.PressedBackgroundColor.ToAndroid();

					_rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(rippleColor), _backgroundDrawable, null);
					Control.SetBackground(_rippleDrawable);
				}
				else
				{
					Control.SetBackground(_backgroundDrawable);
				}

				_drawableEnabled = true;
			}

			Control.Invalidate();
		}

		public void Reset()
		{
			if (_drawableEnabled)
			{
				_drawableEnabled = false;
				_backgroundDrawable?.Reset();
				_backgroundDrawable = null;
				_rippleDrawable = null;
			}
		}


		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_backgroundDrawable?.Dispose();
					_backgroundDrawable = null;
					_defaultDrawable?.Dispose();
					_defaultDrawable = null;
					_rippleDrawable?.Dispose();
					_rippleDrawable = null;

					if (Button != null)
					{
						Button.PropertyChanged -= ButtonPropertyChanged;
						Button = null;
					}

					if (_renderer != null)
					{
						_renderer.ElementChanged -= OnElementChanged;
						_renderer = null;
					}

				}
				_disposed = true;
			}
		}

		void ButtonPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals(Button.BorderColorProperty.PropertyName) ||
				e.PropertyName.Equals(Button.BorderWidthProperty.PropertyName) ||
				e.PropertyName.Equals(Button.CornerRadiusProperty.PropertyName) ||
				e.PropertyName.Equals(VisualElement.BackgroundColorProperty.PropertyName))
			{
				Reset();
				UpdateDrawable();
			}
		}

	}
}