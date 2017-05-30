namespace ImageButton.Abstractions
{
    public interface IImageButtonController
    {
        void SendReleased();
        void SendPressed();
        void SendClicked();
        void OnSelectedChanged(bool selected);
    }
}