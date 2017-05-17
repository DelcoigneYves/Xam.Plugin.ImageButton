using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace ImageButton.Abstractions
{
    public class ImageButton : View, IImageButtonController
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source), typeof(ImageSource), typeof(ImageButton));

        public static readonly BindableProperty PressedSourceProperty = BindableProperty.Create(
            nameof(PressedSource), typeof(ImageSource), typeof(ImageButton));

        public static readonly BindableProperty SelectedSourceProperty = BindableProperty.Create(
            nameof(SelectedSource), typeof(ImageSource), typeof(ImageButton));

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command), typeof(ICommand), typeof(ImageButton));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public ImageSource PressedSource
        {
            get { return (ImageSource)GetValue(PressedSourceProperty); }
            set { SetValue(PressedSourceProperty, value); }
        }

        public ImageSource SelectedSource
        {
            get { return (ImageSource)GetValue(SelectedSourceProperty); }
            set { SetValue(SelectedSourceProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public void SendReleased()
        {
            Released?.Invoke(this, EventArgs.Empty);
        }

        public void SendPressed()
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        public void SendClicked()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Clicked;

        public event EventHandler Pressed;

        public event EventHandler Released;
    }
}