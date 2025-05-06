using UnityEngine;

[CreateAssetMenu(fileName = "ColorMaterialManager", menuName = "Puzzle/Color Material Manager")]
public class ColorMaterialManager : ScriptableObject
{
    [SerializeField] private Material[] colorMaterials;
    
    public Material[] GetColorMaterials()
    {
        return colorMaterials;
    }
} 