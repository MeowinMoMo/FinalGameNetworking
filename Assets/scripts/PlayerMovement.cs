using UnityEngine;
using Unity.Netcode;
using JetBrains.Annotations;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;

    [CanBeNull] public static event System.Action GameOverEvent;

    private Camera _mainCamera;
    private Vector3 _mouseInput = Vector3.zero;
    private PlayerLength _playerLength;

    private readonly ulong[] _targetClientsArray = new ulong[1];

    private void Initialize()
    {
        _mainCamera = Camera.main;
        _playerLength = GetComponent<PlayerLength>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void Update()
    {
        if (!IsOwner || !Application.isFocused) return;

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Y = 0 plane

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldCoordinates = ray.GetPoint(distance);

            // Move toward mouse position
            transform.position = Vector3.MoveTowards(transform.position, mouseWorldCoordinates, speed * Time.deltaTime);

            // Rotate toward mouse position
            Vector3 targetDirection = mouseWorldCoordinates - transform.position;
            targetDirection.y = 0f;

            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

    }

    [ServerRpc]
    private void DetermineCollisionWinnerServerRPC(PlayerData player1, PlayerData player2)
    {
        if (player1.length > player2.length)
        {
            WinInformationServerRpc(player1.id, player2.id);
        }
        else
        {
            WinInformationServerRpc(player2.id, player1.id);
        }
    }

    [ServerRpc]
    private void WinInformationServerRpc(ulong winner, ulong loser)
    {
        _targetClientsArray[0] = winner;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _targetClientsArray
            }
        };
        AtePlayerClientRpc(clientRpcParams);

        _targetClientsArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = _targetClientsArray;
        GameOverClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You ate a Player!");
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You Lose");
        GameOverEvent?.Invoke();
        NetworkManager.Singleton.Shutdown();
    }


    private void OnCollisionEnter(Collision col)
    {
        Debug.Log("player Collision");
        if (!col.gameObject.CompareTag("Player")) return;
  
        if (!IsOwner) return;

        //Head Collision
        if (col.gameObject.TryGetComponent(out PlayerLength playerLength))
        {
            var player1 = new PlayerData()
            {
                id = OwnerClientId,
                length = _playerLength.length.Value
            };
            var player2 = new PlayerData()
            {
                id = playerLength.OwnerClientId,
                length = playerLength.length.Value
            };
            DetermineCollisionWinnerServerRPC(player1, player2);
        }

        //Tail Collision
        else if (col.gameObject.TryGetComponent(out Tail tail))
        {
            Debug.Log("Tail Collided");
            WinInformationServerRpc(tail.networkedOwner.GetComponent<PlayerMovement>().OwnerClientId, OwnerClientId); 
        }



    }

    struct PlayerData : INetworkSerializable
    {
        public ulong id;
        public ushort length;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref length);
        }
    }


}
