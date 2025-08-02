using UnityEngine;

public class TooltipCursorTrigger : MonoBehaviour
{
    [SerializeField] LayerMask TooltipSourceLayer;
    ITooltipSource currSource;
    private void Update()
    {
        var hit = Physics2D.Raycast(CursorManager.Singleton.transform.position, Vector2.zero, 0, TooltipSourceLayer);
        if (hit.collider != null)
        {
            ITooltipSource tHit = hit.collider.GetComponent<ITooltipSource>();
            if (currSource != tHit)
            {
                currSource?.OnHoverExit();
                currSource = tHit;
                currSource.OnHoverEnter();
            }
        }
        else
        {
            if (currSource != null)
            {
                currSource.OnHoverExit();
                currSource = null;
            }
        }
    }

    public bool IsCursorHoveringOnTooltip(Collider2D collider2D)
    {
        var hits = Physics2D.RaycastAll(CursorManager.Singleton.transform.position, Vector2.zero, 0, TooltipSourceLayer);
        foreach (var hit in hits)
        {
            if (hit.collider == collider2D) return true;
        }
        return false;
    }
}