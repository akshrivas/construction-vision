using UnityEngine;

public class RoomInfo : MonoBehaviour, IInteractable
{
    public string title = "Bedroom";
    public string detail = "Approx. 12 × 14 ft";

    public void Interact(Interactor interactor)
    {
        interactor.ShowRoom(title, detail);
    }
}
