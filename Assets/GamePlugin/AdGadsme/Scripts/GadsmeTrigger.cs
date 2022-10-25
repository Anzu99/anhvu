using UnityEngine;
using mygame.sdk;
#if ENABLE_Gadsme

#endif

public class GadsmeTrigger : MonoBehaviour
{
#if ENABLE_Gadsme
    
#endif
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"mysdk: gadsme OnTriggerEnter={other.name}");
#if ENABLE_Gadsme
        
#endif
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log($"mysdk: gadsme OnTriggerExit={other.name}");
#if ENABLE_Gadsme
        
#endif
    }
}