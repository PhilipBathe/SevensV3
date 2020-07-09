using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public bool IsActive = false;

    public bool HasSevenOfDiamonds()
    {
        //TODO: find a less clunky way to find this?
        var sevenOfDiamonds = transform.Find("Hand/Diamond07(Clone)");
        return sevenOfDiamonds != null;
    }
}
