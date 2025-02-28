using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    // Controls how much the camera follows the player and how much it follows the mouse
    [SerializeField] Vector2 maxDistance;
    [SerializeField] Vector2 distanceDivider;

    Transform _player;

    void Start()
    {
        _player = PlayerEntityManager.Singleton.gameObject.transform;
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetDist = (mousePos - _player.position) / distanceDivider;

        Vector3 targetPos = new Vector3(Mathf.Clamp(targetDist.x, -maxDistance.x, maxDistance.x),
                                        Mathf.Clamp(targetDist.y, -maxDistance.y, maxDistance.y), 0)
                                        + _player.position;

        transform.position = targetPos;
    }
}
