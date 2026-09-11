namespace Duskline {
public interface IInteractable {
    string Prompt {get;}
    bool CanInteract(PlayerMotor player);
    void Interact(PlayerMotor player);
}
}
