using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicAnimatedSorting : MonoBehaviour
{
    public bool enableSorting = true;
    public GameObject sortingPivotGameObject; // Reference to the foot GameObject

    // Allow to change the sorting layer from the Inspector
    public string sortingLayer = "Default";

    private SpriteRenderer[] spriteRenderers;
    private int[] originalSortingOrders;
    private Transform footTransform; // Transform of the foot GameObject

    void Start()
    {
        // Get all SpriteRenderers in this GameObject and its children
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Select only the SpriteRenderers in given sorting layer
        spriteRenderers = System.Array.FindAll(spriteRenderers, sr => sr.sortingLayerName == sortingLayer);

        originalSortingOrders = new int[spriteRenderers.Length];

        // Store the original sorting order of each SpriteRenderer
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalSortingOrders[i] = spriteRenderers[i].sortingOrder;
        }

        if (sortingPivotGameObject != null)
        {
            footTransform = sortingPivotGameObject.transform;
        }
        else
        {
            Debug.LogError("No GameObject with the tag 'foot' found. Please ensure the foot GameObject is tagged correctly.");
        }
    }

    void Update()
    {
        if (enableSorting && footTransform != null)
        {
            SortToSameOrderWithOffset();
        }
    }

    // Function to sort the sprite renderers based on the foot's Y-position with an offset from the original sorting order
    public void SortToSameOrderWithOffset()
    {
        // Use the Y-position of the foot for sorting
        int baseSortingOrder = Mathf.RoundToInt(footTransform.position.y * -100);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingOrder = baseSortingOrder + originalSortingOrders[i];
        }
    }

    public void SetCustomSortingORderToAllChildren(int CustomSortingOrder)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingOrder = CustomSortingOrder;
        }
    }

    public void DisableDynamicSorting()
    {
        enableSorting = false;
    }

    public void EnableDynamicSorting()
    {
        enableSorting = true;
    }
}