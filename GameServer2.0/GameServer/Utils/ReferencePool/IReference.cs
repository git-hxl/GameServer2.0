namespace Utils
{
    public interface IReference
    {
        void OnAcquire();
        void OnRelease();
    }
}