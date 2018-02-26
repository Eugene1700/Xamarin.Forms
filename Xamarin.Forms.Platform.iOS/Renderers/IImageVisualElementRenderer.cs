using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IImageVisualElementRenderer : IVisualNativeElementRenderer
	{
		void SetImage(UIImage image);
		bool IsDisposed { get; }
	}
}