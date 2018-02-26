using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ImageViewExtensions
	{
		// TODO hartez 2017/04/07 09:33:03 Review this again, not sure it's handling the transition from previousImage to 'null' newImage correctly
		public static async Task UpdateBitmap(
			this AImageView imageView,
			IImageController newView,
			IImageController previousView = null)
		{
			IImageController imageController = null;
			ImageSource newImageSource = newView?.Source;
			ImageSource previousImageSource = previousView?.Source;
			try
			{
				if (imageView == null || imageView.IsDisposed())
					return;

				if (Device.IsInvokeRequired)
					throw new InvalidOperationException("Image Bitmap must not be updated from background thread");

				if (previousView != null && Equals(previousImageSource, newImageSource))
					return;

				imageController = newView as IImageController;
				imageController?.SetIsLoading(true);

				(imageView as IImageRendererController)?.SkipInvalidate();


				imageView.SetImageResource(global::Android.Resource.Color.Transparent);

				Bitmap bitmap = null;
				IImageSourceHandler handler;

				if (newImageSource != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(newImageSource)) != null)
				{
					try
					{
						bitmap = await handler.LoadImageAsync(newImageSource, imageView.Context);
					}
					catch (TaskCanceledException)
					{
						imageController?.SetIsLoading(false);
					}
				}

				if (newView == null)
				{
					bitmap?.Dispose();
					return;
				}

				if (!imageView.IsDisposed())
				{
					if (bitmap == null && newImageSource is FileImageSource fileImageSource)
					{
						imageView.SetImageResource(ResourceManager.GetDrawableByName((fileImageSource).File));
					}
					else
					{
						imageView.SetImageBitmap(bitmap);
					}
				}

				bitmap?.Dispose();
				imageController?.SetIsLoading(false);
				((IVisualElementController)newView).NativeSizeChanged();

			}
			catch (Exception ex)
			{
				Internals.Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
			}
			finally
			{
				imageController?.SetIsLoading(false);
			}
		}
	}
}
