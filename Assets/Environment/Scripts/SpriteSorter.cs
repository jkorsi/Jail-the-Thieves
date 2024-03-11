using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StaticSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetSortingOrder();
    }

    void SetSortingOrder()
    {
        // Adjust this value based on your game's specific needs
        int sortingOrderBase = 5000;
        spriteRenderer.sortingOrder = sortingOrderBase - Mathf.RoundToInt(transform.position.y * 100);
    }
}