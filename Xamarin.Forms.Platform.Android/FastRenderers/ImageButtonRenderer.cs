using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using AImageButton = Android.Widget.ImageButton;
using AView = Android.Views.View;
using Android.Views;
using Xamarin.Forms.Internals;
using AMotionEventActions = Android.Views.MotionEventActions;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal sealed class ImageButtonRenderer : 
		AppCompatImageButton, 
		IVisualElementRenderer, 
		IImageRendererController, 
		AView.IOnFocusChangeListener, 
		AView.IOnClickListener, 
		AView.IOnTouchListener
	{
		bool _inputTransparent;
		bool _disposed; 
		bool _skipInvalidate;
		int? _defaultLabelFor;
		VisualElementTracker _tracker;
		VisualElementRenderer _visualElementRenderer;
		private BorderBackgroundTracker _backgroundTracker;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;



		void IVisualElementRenderer.UpdateLayout() => _tracker?.UpdateLayout();

		VisualElement IVisualElementRenderer.Element => Button;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		ImageButton Button { get; set; }

		void IImageRendererController.SkipInvalidate() => _skipInvalidate = true;
		bool IImageRendererController.IsDisposed => _disposed;

		AppCompatImageButton Control => this;

		public ImageButtonRenderer(Context context) : base(context)
		{
			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			OnFocusChangeListener = this;
			Tag = this; 
		}


		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{

				ImageElementManager.Dispose(this);

				_tracker?.Dispose();
				_tracker = null;

				_backgroundTracker?.Dispose();
				_backgroundTracker = null;

				if (Button != null)
				{
					Button.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Button) == this)
					{
						Button.ClearValue(Platform.RendererProperty);
					}

					Button = null;
				}
			}

			base.Dispose(disposing);
		}

		public override void Invalidate()
		{
			if (_skipInvalidate)
			{
				_skipInvalidate = false;
				return;
			}

			base.Invalidate();
		}

		Size MinimumSize()
		{
			return new Size();
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (_disposed)
			{
				return new SizeRequest();
			}

			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), MinimumSize());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!(element is ImageButton image))
			{
				throw new ArgumentException("Element is not of type " + typeof(ImageButton), nameof(element));
			}

			ImageButton oldElement = Button;
			Button = image;

			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (_backgroundTracker == null)
			{
				_backgroundTracker = new BorderBackgroundTracker(this);
			}

			element.PropertyChanged += OnElementPropertyChanged;

			if (_tracker == null)
			{
				_tracker = new VisualElementTracker(this);
				ImageElementManager.Init(this);
			}

			if (_visualElementRenderer == null)
			{
				_visualElementRenderer = new VisualElementRenderer(this);
			}

			Performance.Stop(reference);
			this.EnsureId();
			 
			
			UpdateInputTransparent();


			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Button));
			Button?.SendViewInitialized(Control);
		}

		
		// Automation properties
		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = LabelFor;

			LabelFor = (int)(id ?? _defaultLabelFor);
		}
		// Automation properties
		

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (!Enabled || (_inputTransparent && Enabled))
				return false;

			return base.OnTouchEvent(e);
		}

		void UpdateInputTransparent()
		{
			if (Button == null || _disposed)
			{
				return;
			}

			_inputTransparent = Button.InputTransparent;
		}

		// Image related
		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent();
			}
			// General update blocks

			ElementPropertyChanged?.Invoke(this, e);
		}


		// general state related
		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			((IElementController)Button).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}
		// general state related


		// Button related
		void IOnClickListener.OnClick(AView v)=>
			ButtonElementManager.OnClick(Button, Button, v);

		bool IOnTouchListener.OnTouch(AView v, MotionEvent e) =>
			ButtonElementManager.OnTouch(Button, Button, v, e);
		// Button related
	}
}
