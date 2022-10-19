using UnityEngine;

public class Keyword : MonoBehaviour
{
    public Material material;
    public float nm;
    private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");

    void Start()
    {
        CheckShaderKeywordState();
    }

    public void CheckShaderKeywordState()
    {
        // Get the instance of the Shader class that the material uses
        var shader = material.shader;

        // Get all the local keywords that affect the Shader
        var keywordSpace = shader.keywordSpace;
        // Iterate over the local keywords
        foreach (var localKeyword in keywordSpace.keywords)
        {
            // If the local keyword is overridable (i.e., it was declared with a global scope),
            // and a global keyword with the same name exists and is enabled,
            // then Unity uses the global keyword state
            if (localKeyword.isOverridable && Shader.IsKeywordEnabled(localKeyword.name))
            {
                //Debug.Log("Local keyword with name of " + localKeyword.name + " is overridden by a global keyword, and is enabled");
                if (localKeyword.name == "_Smoothness")
                {
                    material.SetFloat(Smoothness, nm);
                    print(material.GetFloat(Smoothness));
                }
            }
            // Otherwise, Unity uses the local keyword state
            else
            {
                //var state = material.IsKeywordEnabled(localKeyword) ? "enabled" : "disabled";
                if (material.HasProperty("_Smoothness"));
                {
                    material.SetFloat(Smoothness, nm);
                    print(material.GetFloat(Smoothness));
                }
                //Debug.Log("Local keyword with name of " + localKeyword.name + " is " + state);
            }            
        }
    }
}
