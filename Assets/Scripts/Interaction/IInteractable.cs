namespace Interaction
{
    public interface IInteractable
    {
        public string Name { get; }
        public string InteractMessage { get; }
        void Interact();
    }
}