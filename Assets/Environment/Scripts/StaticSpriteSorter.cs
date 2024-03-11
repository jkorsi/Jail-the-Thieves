using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StaticSpriteSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetSortingOrder();
    }

    public void SetSortingOrder()
    {
            spriteRenderer.sortingOrder =  Mathf.RoundToInt(transform.position.y * -100);
    }
}
