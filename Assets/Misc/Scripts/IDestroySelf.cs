using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestroySelf
{
    // Simple interface to add to GameObjects that should support handling deleting themselves (think death animations etc.)
    public void DestroySelf();
}
