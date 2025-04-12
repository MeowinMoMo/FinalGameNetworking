using UnityEngine;
using Unity.Netcode;


public class Food : NetworkBehaviour
{
    public GameObject prefab;
    private void OnTriggerEnter(Collider col)
    {
        
        if (!col.CompareTag("Player")) return;
       
        if (!NetworkManager.Singleton.IsServer) return;

        if (col.TryGetComponent(out PlayerLength playerLength))
        {
            playerLength.AddLength();
        }
        else if (col.TryGetComponent(out Tail tail))
        {
            tail.networkedOwner.GetComponent<PlayerLength>().AddLength();
        }
        else
        {
            Debug.Log("I did not see shit");
        }
        Debug.Log("Despawning food...");
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);

        NetworkObject.Despawn();
    }
}
