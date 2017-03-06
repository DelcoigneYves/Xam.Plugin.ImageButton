namespace ImageButton.Abstractions
{
    public interface IImageButtonController
    {
        void SendReleased();
        void SendPressed();
        void SendClicked();
    }
}