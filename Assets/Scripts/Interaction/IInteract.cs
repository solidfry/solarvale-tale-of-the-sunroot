namespace Interaction
{
    public interface IInteract
    {
        public string Name { get; }
        public string InteractMessage { get; }
        void Interact();
    }
}