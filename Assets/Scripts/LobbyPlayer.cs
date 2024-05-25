using Unity.Netcode;

namespace nickmaltbie.ProjectCoda
{
    public class LobbyPlayer : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            LobbyScreen.Instance?.AddPlayer(OwnerClientId);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            LobbyScreen.Instance?.RemovePlayer(OwnerClientId);
        }
    }
}
