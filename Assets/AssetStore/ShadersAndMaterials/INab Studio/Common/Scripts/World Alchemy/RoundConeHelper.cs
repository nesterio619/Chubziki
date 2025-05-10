using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldAlchemy
{
    /// <summary>
    /// Assists with round cone features for a Mask.
    /// </summary>
    public class RoundConeHelper : MonoBehaviour
    {
        // Reference to associated Mask.
        public Mask mask;
        /// <summary>
        /// Initializes helper with a Mask.
        /// </summary>
        /// <param name="mask">Mask to link.</param>
        public void Setup(Mask mask)
        {
            this.mask = mask;
        }

    }
}