using Unity.Netcode;
using UnityEngine;


public class Player : NetworkBehaviour {


    public NetworkVariable<Vector3> PositionChange = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> RotationChange = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);
    public NetworkVariable<int> Score = new NetworkVariable<int>(50);

    public TMPro.TMP_Text txtScoreDisplay;

    private GameManager _gameMgr;
    private Camera _camera;
    public float movementSpeed = .5f;
    private float rotationSpeed = 4f;
    private BulletSpawner _bulletSpawner;
    
//------------------------------------
// Behaviour
//----------------------------------------
    private void Start() {
        ApplyPlayerColor();
        PlayerColor.OnValueChanged += OnPlayerColorChanged;
    }

    public override void OnNetworkSpawn() {
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        Score.OnValueChanged += ClientOnScoreChanged;
        _bulletSpawner = transform.Find("RArm").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();
        if (IsHost) {
            _bulletSpawner.BulletDamage.Value = 1;
        }
        DisplayScore();
    }

    void Update() {
        if (IsOwner) {
            Vector3[] results = CalcMovement();
            RequestPositionForMovementServerRpc(results[0], results[1]);
            if (Input.GetButtonDown("Fire1")) {
                _bulletSpawner.FireServerRpc();
            }
        }

        if(!IsOwner || IsHost){
            transform.Translate(PositionChange.Value);
            transform.Rotate(RotationChange.Value);
        }
    }

//---------------------------------
//Private
//------------------------------
    // horiz changes y rotation or x movement if shift down, vertical moves forward and back.
    private Vector3[] CalcMovement() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");
        float y_rot = 0.0f;

        if (isShiftKeyDown) {
            x_move = Input.GetAxis("Horizontal");
        } else {
            y_rot = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed;

        Vector3 rotVect = new Vector3(0, y_rot, 0);
        rotVect *= rotationSpeed;

        return new[] { moveVect, rotVect };
    }

    private void HostHandleBulletCollision(GameObject bullet) {
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        Score.Value -= bulletScript.Damage.Value;

        ulong ownerClientId = bullet.GetComponent<NetworkObject>().OwnerClientId;
        Player otherPlayer = NetworkManager.Singleton.ConnectedClients[ownerClientId].PlayerObject.GetComponent<Player>();
        otherPlayer.Score.Value += bulletScript.Damage.Value;

        Destroy(bullet);
    }

    private void HostHandleDamageBoostPickup(Collider other) {
        if (!_bulletSpawner.IsAtMaxDamage()){
            _bulletSpawner.IncreaseDamage();
            Score.Value += 1;
        
            other.GetComponent<NetworkObject>().Despawn();
        }
    }

// ---------------------
// Events
// ---------------------
    private void ClientOnScoreChanged(int previous, int current){
        DisplayScore();
    }

    private void OnPlayerColorChanged(Color previous, Color current) {
        ApplyPlayerColor();
    }
    public void OnCollisionEnter(Collision collision) {
        if (IsHost) {
            if (collision.gameObject.CompareTag("Bullet")) {
                if(Score.Value <= 0){
                    NetworkManager.SceneManager.LoadScene("DIE", UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
                HostHandleBulletCollision(collision.gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider other){
        if(IsHost){
            if(other.gameObject.CompareTag("DamageBoost")) {
                HostHandleDamageBoostPickup(other);
            }
        }
    }
//------------------------
// RPC
//------------------------
    [ServerRpc]
    void RequestPositionForMovementServerRpc(Vector3 posChange, Vector3 rotChange) {
        if (!IsServer && !IsHost) return;

        PositionChange.Value = posChange;
        RotationChange.Value = rotChange;
    }

    [ServerRpc]
    public void RequestSetScoreServerRpc(int value) {
        Score.Value = value;
    }
//---------------------------
//publuc
//------------------------------
    public void ApplyPlayerColor() {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        transform.Find("TankTireL").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        transform.Find("TankTireR").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        transform.Find("body").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        transform.Find("RArm").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;

    }
        public void DisplayScore() {
        txtScoreDisplay.text = Score.Value.ToString();
    }
}