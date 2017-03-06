using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using ImageButton.Abstractions;
using ImageButton.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Object = Java.Lang.Object;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(ImageButton.Abstractions.ImageButton), typeof(ImageButtonRenderer))]

namespace ImageButton.Android
{
    [Preserve(AllMembers = true)]
    public class ImageButtonRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer
        <Abstractions.ImageButton, global::Android.Widget.ImageButton>
    {
        private bool _isDisposed;

        protected override global::Android.Widget.ImageButton CreateNativeControl()
        {
            return new global::Android.Widget.ImageButton(Context);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Abstractions.ImageButton> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var imageButton = CreateNativeControl();

                    imageButton.SetOnClickListener(ButtonClickListener.Instance.Value);
                    imageButton.SetOnTouchListener(ButtonTouchListener.Instance.Value);
                    imageButton.Tag = this;

                    SetNativeControl(imageButton);
                }

                UpdateBitmap();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Abstractions.ImageButton.SourceProperty.PropertyName ||
                e.PropertyName == Abstractions.ImageButton.SelectedSourceProperty.PropertyName)
            {
                UpdateBitmap();
            }
        }

        protected virtual StateListDrawable MakeSelector(Bitmap bitmap, Bitmap selectedBitmap)
        {
            BitmapDrawable image = null;
            BitmapDrawable selectedImage = null;

            if (bitmap != null)
            {
                image = new BitmapDrawable(bitmap);
            }
            if (selectedBitmap != null)
            {
                selectedImage = new BitmapDrawable(selectedBitmap);
            }

            var res = new StateListDrawable();
            if (selectedImage != null)
            {
                res.AddState(new[] {global::Android.Resource.Attribute.StatePressed}, selectedImage);
            }
            if (image != null)
            {
                res.AddState(new int[] {}, image);
            }
            return res;
        }

        protected virtual async void UpdateBitmap()
        {
            var defaultImage = await GetDefaultBitmap();
            var selectedImage = await GetSelectedBitmap();

            if (!_isDisposed)
            {
                if (defaultImage == null)
                {
                    // Try to fetch the drawable another way
                    var source = Element.Source as FileImageSource;
                    if (source != null)
                    {
                        Control.SetImageResource(ResourceManager.GetDrawableByName(source.File));
                    }
                }
                else
                {
                    Control.SetImageDrawable(MakeSelector(defaultImage, selectedImage));
                }

                defaultImage?.Dispose();
                selectedImage?.Dispose();

                ((IVisualElementController) Element).NativeSizeChanged();
            }
        }

        protected virtual Task<Bitmap> GetDefaultBitmap()
        {
            var elementImage = Element.Source;

            return GetBitmap(elementImage);
        }

        protected virtual Task<Bitmap> GetSelectedBitmap()
        {
            var elementSelectedImage = Element.SelectedSource;

            return GetBitmap(elementSelectedImage);
        }

        protected virtual async Task<Bitmap> GetBitmap(ImageSource imageSource)
        {
            if (imageSource == null)
            {
                return null;
            }

            Bitmap bitmap = null;

            try
            {
                var handler = new FileImageSourceHandler();
                bitmap = await handler.LoadImageAsync(imageSource, Context);
            }
            catch
            {
                // ignored
            }

            if (Element == null || !Equals(Element.Source, imageSource))
            {
                bitmap?.Dispose();
                return null;
            }

            return bitmap;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing)
            {
                if (Control != null)
                {
                    Control.SetOnClickListener(null);
                    Control.SetOnTouchListener(null);
                    Control.Tag = null;
                }
            }

            base.Dispose(disposing);
        }

        private class ButtonClickListener : Object, IOnClickListener
        {
            #region Statics

            public static readonly Lazy<ButtonClickListener> Instance =
                new Lazy<ButtonClickListener>(() => new ButtonClickListener());

            #endregion

            public void OnClick(View v)
            {
                var renderer = v.Tag as ImageButtonRenderer;
                ((IImageButtonController) renderer?.Element)?.SendClicked();
            }
        }

        private class ButtonTouchListener : Object, IOnTouchListener
        {
            public static readonly Lazy<ButtonTouchListener> Instance =
                new Lazy<ButtonTouchListener>(() => new ButtonTouchListener());

            public bool OnTouch(View v, MotionEvent e)
            {
                var renderer = v.Tag as ImageButtonRenderer;
                if (renderer != null)
                {
                    var buttonController = renderer.Element as IImageButtonController;
                    if (e.Action == MotionEventActions.Down)
                    {
                        buttonController?.SendPressed();
                    }
                    else if (e.Action == MotionEventActions.Up)
                    {
                        buttonController?.SendReleased();
                    }
                }
                return false;
            }
        }
    }
}