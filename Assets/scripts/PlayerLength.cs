using JetBrains.Annotations;
using Unity.Netcode; 
using UnityEngine;
using System.Collections.Generic;


public class PlayerLength : NetworkBehaviour
{
    [SerializeField] private GameObject _tailPrefab;
    public NetworkVariable<ushort> length = new( 1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [CanBeNull] public static event System.Action<ushort> ChangeLengthEvent;

    private List<GameObject> _tails;
    private Transform _lastTail;
    private SphereCollider _spherecollider;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _tails = new List<GameObject>();
        _lastTail = transform;
        _spherecollider = GetComponent<SphereCollider>();
        if (!IsServer) length.OnValueChanged += LengthChangeEvent;
    }

    [ContextMenu(itemName:"Add Length")]
    public void AddLength()
    {
        length.Value += 1;
        LengthChanged();
    }

    private void LengthChanged()
    {
        InstantiateTail();

        if (!IsOwner) return;
        ChangeLengthEvent?.Invoke(length.Value);
    }
    private void LengthChangeEvent(ushort previousValue, ushort newValue)
    {
        Debug.Log("LengthChange Callback");
        LengthChanged();
        
    }

    private void InstantiateTail()
    {
        GameObject tailGameObject = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
       //tailGameObject.GetComponent<MeshRenderer>().sortingOrder = -length.Value;
        if (tailGameObject.TryGetComponent(out Tail tail))
        {
            tail.networkedOwner = transform;
            tail.followTransform = _lastTail;
            _lastTail = tailGameObject.transform;
            Physics.IgnoreCollision(tailGameObject.GetComponent<SphereCollider>(), _spherecollider);
        }
        _tails.Add(tailGameObject);
    }

}
