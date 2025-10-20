using UnityEditor;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public enum methods
    {
        destroy,    // Will destroy the target object
        deactivate, // Will deactivate the target object
        custom,     // Will call a custom method (user needs to implement in SkillActive.cs under DoDamage())
        none        // Will do nothing to the target object
    };
    
    public static methods damageMethod = methods.destroy; // Set how damage works here!
}
