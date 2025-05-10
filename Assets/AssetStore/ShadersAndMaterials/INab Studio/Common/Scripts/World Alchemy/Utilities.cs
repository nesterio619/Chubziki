namespace INab.WorldAlchemy
{
    /// <summary>
    /// Represents the types of masks available for world dissolve effects.
    /// Each type corresponds to a different geometric shape or form.
    /// </summary>
    public enum Type
    {
        Plane,       // A flat, 2D surface mask
        Box,         // A cubic or rectangular mask
        Sphere,      // A spherical mask
        Ellipse,     // An elliptical mask
        SolidAngle,  // A mask shaped as a solid angle or cone segment
        RoundCone    // A mask shaped as a round cone
    }

    /// <summary>
    /// Defines the available styles for world dissolve shaders.
    /// Each style provides a unique visual effect for the dissolve transition.
    /// </summary>
    public enum DissolveType
    {
        Burn,            // A burning effect
        Smooth,          // A smooth, gradual dissolve
        DisplacementOnly // A dissolve effect using only displacement
    }

    /// <summary>
    /// Specifies the scope of shader property application.
    /// Global affects all instances globally, while Local affects only individual instances.
    /// </summary>
    public enum ShaderType
    {
        Global = 0, // Global properties affect all instances
        Local = 1   // Local properties affect individual instances
    }

    /// <summary>
    /// Identifiers for different sets of global properties in world dissolve shaders.
    /// Allows for multiple instances of global properties, currently limited to two.
    /// </summary>
    /// <remarks>
    /// The number of instances can be increased to four if the 'UseDisplacement' keyword
    /// is removed from the global dissolve shader.
    /// </remarks>
    public enum GlobalPropertiesID
    {
        _1 = 0, // Identifier for the first set of global properties
        _2 = 1  // Identifier for the second set of global properties
        //_3 = 2, // Potential future expansion
        //_4 = 3  // Potential future expansion
    }
}
