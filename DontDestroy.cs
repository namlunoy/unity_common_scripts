using UnityEngine;
using System.Collections;

public class DontDestroy<T> :  MonoBehaviour where T : MonoBehaviour {
    
    private static T _instance;
    
    void Awake()
    {
        if(_instance == null)
        {
            _instance = this.GetComponent<T>();
            DontDestroyOnLoad(_instance.gameObject);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }

}
