using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceEntryUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI valueText;
    [HideInInspector] public ResourceDefinition def;

    public void Bind(ResourceDefinition d, string initial, Sprite overrideIcon = null)
    {
        def = d;
        if (icon) icon.sprite = overrideIcon ? overrideIcon : (d ? d.icon : null);
        if (valueText) valueText.text = initial ?? "0";
    }

    public void SetValue(int v, string format = "n0")
    {
        if (valueText) valueText.text = v.ToString(format);
    }
}
