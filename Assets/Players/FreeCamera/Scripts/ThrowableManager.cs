using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThrowableManager : MonoBehaviour
{
    // Basic factory class that supports managing a selected
    // index that the user can CycleUp and CycleDown through.

    private Dictionary<int, GameObject> objectDict;
    private Dictionary<int, string> nameDict;
    private int nextID = 0;
    private int selectedIndex = 0;

    public ThrowableManager()
    {
        objectDict = new Dictionary<int, GameObject>();
        nameDict = new Dictionary<int, string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Must be called here NOT in ctor to ensure that the
        // unity editor fields are actually populated!
        SetupManager();
    }

    // User should register objects in their derived implementation
    // of this method
    protected abstract void SetupManager();

    // Return number of selection options
    public int Size()
    {
        return objectDict.Count;
    }

    private int GetNewID()
    {
        int idVal = nextID;
        nextID++;
        return idVal;
    }

    // Registers the specific object name pair. Returns the integer ID associated
    // with the registered object.
    protected int RegisterObject(string name, GameObject obj)
    {
        int newId = GetNewID();

        objectDict[newId] = obj;
        nameDict[newId] = name;

        return newId;
    }

    // Returns the name associated with the provided index
    public string GetName(int index)
    {
        if (index < Size())
        {
            return nameDict[index];    
        }
        else
        {
            return "";
        }
    }

    public string CycleUp()
    {
        selectedIndex = (Size() > 0) ? (selectedIndex + 1) % Size() : 0;
        return GetName(selectedIndex);
    }

    public string CycleDown()
    {
        if (Size() > 0)
        {
            selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : Size() - 1;
        }
        return GetName(selectedIndex);
    }

    public GameObject CreateObject()
    {
        if (Size() > 0)
        {
            return CreateObject(selectedIndex);
        }
        else
        {
            return null;
        }
    }

    // Creates an object of the specified index
    private GameObject CreateObject(int index)
    {
        GameObject newObj = Instantiate(objectDict[index]);
        return newObj;
    }
}
