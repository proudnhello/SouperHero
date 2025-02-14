using UnityEngine;

public class PlayerInputAndAttackManager : MonoBehaviour
{
    public static PlayerInputAndAttackManager Singleton { get; private set; }

    public PlayerInputActions input;
    public Transform playerAttackPoint;
    public LayerMask collisionLayer;
    public LayerMask interactableLayer;
    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        input = new();
        input.Enable();
    }

}