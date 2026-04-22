using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private Renderer[] renderers;
    private Color[] originalColors;

    public Color highlightColor = Color.yellow;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    public void SetHighlightAmount(float amount)
    {
        amount = Mathf.Clamp01(amount);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = Color.Lerp(originalColors[i], highlightColor, amount);
            }
        }
    }

    public void Highlight()
    {
        SetHighlightAmount(1f);
    }

    public void Unhighlight()
    {
        SetHighlightAmount(0f);
    }
}