using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Foundation;
using ImageButton.Abstractions;
using ImageButton.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ImageButton.Abstractions.ImageButton), typeof(ImageButtonRenderer))]

namespace ImageButton.iOS
{
    [Preserve(AllMembers = true)]
    public class ImageButtonRenderer : ViewRenderer<Abstractions.ImageButton, UIButton>
    {
        private bool _isDisposed;

        public new static async void Init()
        {
            var temp = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Abstractions.ImageButton> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
            }

            if (e.NewElement != null)
            {
                var button = UIButton.FromType(UIButtonType.Custom);
                button.TintColor = UIColor.Clear;
                SetNativeControl(button);

                Control.TouchUpInside += OnButtonTouchUpInside;
                Control.TouchDown += OnButtonTouchDown;
            }

            UpdateImage();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Abstractions.ImageButton.SourceProperty.PropertyName ||
                e.PropertyName == Abstractions.ImageButton.SelectedSourceProperty.PropertyName)
            {
                UpdateImage();
            }
        }

        protected virtual async void UpdateImage()
        {
            var defaultImage = await GetDefaultImage();
            var selectedImage = await GetSelectedImage();

            if (!_isDisposed)
            {
                if (defaultImage != null)
                {
                    Control.SetImage(defaultImage, UIControlState.Normal);

                    if (selectedImage != null)
                    {
                        Control.SetImage(selectedImage, UIControlState.Highlighted);
                    }
                }

                defaultImage?.Dispose();
                selectedImage?.Dispose();

                ((IVisualElementController) Element).NativeSizeChanged();
            }
        }

        protected virtual Task<UIImage> GetDefaultImage()
        {
            var elementImage = Element.Source;

            return GetImage(elementImage);
        }

        protected virtual Task<UIImage> GetSelectedImage()
        {
            var selectedElementImage = Element.SelectedSource;

            return GetImage(selectedElementImage);
        }

        protected virtual async Task<UIImage> GetImage(ImageSource imageSource)
        {
            if (imageSource == null)
            {
                return null;
            }

            UIImage image = null;

            try
            {
                var handler = new FileImageSourceHandler();
                image = await handler.LoadImageAsync(imageSource);
            }
            catch
            {
                // ignored
            }


            if (Element == null)
            {
                image?.Dispose();
                return null;
            }

            return image;
        }

        protected virtual void OnButtonTouchUpInside(object sender, EventArgs eventArgs)
        {
            ((IImageButtonController) Element)?.SendReleased();
            ((IImageButtonController) Element)?.SendClicked();
        }

        protected virtual void OnButtonTouchDown(object sender, EventArgs eventArgs)
        {
            ((IImageButtonController) Element)?.SendPressed();
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                var normalImage = Control.ImageForState(UIControlState.Normal);
                var selectedImage = Control.ImageForState(UIControlState.Selected);

                normalImage?.Dispose();
                normalImage = null;
                selectedImage?.Dispose();
                selectedImage = null;
            }

            _isDisposed = true;

            base.Dispose(disposing);
        }
    }
}