
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    public float gunDistance = 0.6f;

    void Update()
    {
        // Stop aiming when the game is paused or over
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (Mouse.current == null || Camera.main == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            mouseWorld - transform.parent.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg - 90f;

        transform.rotation =
            Quaternion.Euler(0, 0, angle);

        transform.localPosition =
            direction.normalized * gunDistance;
    }
}
