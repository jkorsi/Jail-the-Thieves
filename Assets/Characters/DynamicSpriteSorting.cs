using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicSpriteSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private bool enableDynamicSorting = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (enableDynamicSorting)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
        }    
    }

    public void DisableDynamicSorting()
    {
        enableDynamicSorting = false;
    }

    public void EnableDynamicSorting()
    {
        enableDynamicSorting = true;
    }

}
