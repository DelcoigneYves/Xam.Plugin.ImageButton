using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ImageButton.Abstractions;
using ImageButton.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
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

        public static void Init()
        {
            var temp = DateTime.Now;
        } 

        protected override global::Android.Widget.ImageButton CreateNativeControl()
        {
            var imageButton = new global::Android.Widget.ImageButton(Context);
            return imageButton;
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
                    imageButton.SetScaleType(ImageView.ScaleType.FitCenter);
                    imageButton.SetAdjustViewBounds(true);
                    imageButton.SetPadding(0, 0, 0, 0);
                    imageButton.SetBackgroundColor(Color.Transparent);

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
                e.PropertyName == Abstractions.ImageButton.PressedSourceProperty.PropertyName ||
                e.PropertyName == Abstractions.ImageButton.SelectedSourceProperty.PropertyName)
            {
                UpdateBitmap();
            }
        }

        protected virtual StateListDrawable MakeSelector(Bitmap bitmap, Bitmap pressedBitmap, Bitmap selectedBitmap)
        {
            BitmapDrawable image = null;
            BitmapDrawable pressedImage = null;
            BitmapDrawable selectedImage = null;

            if (bitmap != null)
            {
                image = new BitmapDrawable(bitmap);
            }
            if (pressedBitmap != null)
            {
                pressedImage = new BitmapDrawable(pressedBitmap);
            }
            if (selectedBitmap != null)
            {
                selectedImage = new BitmapDrawable(selectedBitmap);
            }

            var res = new StateListDrawable();

            if (selectedImage != null)
            {
                res.AddState(new[] { global::Android.Resource.Attribute.StateSelected }, selectedImage);
            }
            if (pressedImage != null)
            {
                res.AddState(new[] {global::Android.Resource.Attribute.StatePressed}, pressedImage);
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
            var pressedImage = await GetPressedBitmap();
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
                    Control.SetImageDrawable(MakeSelector(defaultImage, pressedImage, selectedImage));
                }

                defaultImage?.Dispose();
                pressedImage?.Dispose();

                ((IVisualElementController) Element).NativeSizeChanged();
            }
        }

        protected virtual Task<Bitmap> GetDefaultBitmap()
        {
            var elementImage = Element.Source;

            return GetBitmap(elementImage);
        }

        protected virtual Task<Bitmap> GetPressedBitmap()
        {
            var elementPressedImage = Element.PressedSource;

            return GetBitmap(elementPressedImage);
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

            if (Element == null)
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
                        v.Selected = !v.Selected;
                        buttonController?.OnSelectedChanged(v.Selected);
                        buttonController?.SendReleased();
                    }
                }
                return false;
            }
        }
    }
}