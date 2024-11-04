using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
   public void Throw(){
    throw new System.Exception("This is an exception");
   }
}
