using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST_ThrowableManager : ThrowableManager
{
    public List<GameObject> objectsToThrow = new List<GameObject>();

    protected override void SetupManager()
    {
        foreach (var v in objectsToThrow)
        {
            RegisterObject(v.name, v);
        }
    }
}
