using Photon.Pun;
using Photon.Realtime;
using System.Diagnostics;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);
    [SerializeField] private float cameraSmoothSpeed = 10f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Canvas playerCanvas;

    [Header("Visual")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Color[] playerColors;

    // Componentes
    private CharacterController characterController;
    private PhotonView photonView;
    private Animator animator;

    // Estado do jogador
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    // Sincronização
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // Camera
    private float mouseX;
    private float mouseY;

    #region Inicialização

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    private void Start()
    {
        SetupPlayer();

        if (photonView.IsMine)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }
    }

    private void SetupPlayer()
    {
        if (playerNameText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
        }

        if (playerRenderer != null && playerColors.Length > 0)
        {
            int colorIndex = photonView.Owner.ActorNumber % playerColors.Length;
            playerRenderer.material.color = playerColors[colorIndex];
        }

        if (playerCanvas != null)
        {
            playerCanvas.transform.SetParent(null);
        }

        UnityEngine.Debug.Log($"Jogador configurado: {photonView.Owner.NickName}");
    }

    private void SetupLocalPlayer()
    {
        if (cameraTransform == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            cameraTransform = camObj.transform;

            Camera cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        cameraTransform.position = transform.position + cameraOffset;
        cameraTransform.LookAt(transform);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UnityEngine.Debug.Log($"Jogador local criado: {photonView.Owner.NickName}");
    }

    private void SetupRemotePlayer()
    {
        UnityEngine.Debug.Log($"Jogador remoto criado: {photonView.Owner.NickName}");
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        if (photonView.IsMine)
        {
            CheckGround();
            HandleMovement();
            HandleJump();
            UpdateCamera();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }

        UpdateNameCanvas();
    }

    #endregion

    #region Movimento

    private void CheckGround()
    {
        /*if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            UnityEngine.Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance, isGrounded ? Color.green : Color.red);
        }
        else
        {
            isGrounded = characterController.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }*/
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        UnityEngine.Debug.Log($"🔍 GroundCheck Y: {groundCheck.position.y:F2} | " +
                  $"Player Y: {transform.position.y:F2} | " +
                  $"Grounded: {isGrounded}");

        // Desenha um raio para visualizar
        UnityEngine.Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance,
                      isGrounded ? Color.green : Color.red);

    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = Vector3.zero;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * vertical + right * horizontal).normalized;
        }
        else
        {
            moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : moveSpeed;

        velocity.y += gravity * Time.deltaTime;

        //MOVIMENTO
        Vector3 finalMove = moveDirection * currentSpeed * Time.deltaTime;
        finalMove.y = velocity.y * Time.deltaTime;
        characterController.Move(finalMove);

        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude * currentSpeed);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            UnityEngine.Debug.Log($"Saltar! Velocidade Y: {velocity.y}");

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    #endregion

    #region Camera

    private void UpdateCamera()
    {
        if (cameraTransform == null) return;

        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        mouseY = Mathf.Clamp(mouseY, -35f, 60f);

        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);
        Vector3 targetPosition = transform.position - (rotation * Vector3.forward * Mathf.Abs(cameraOffset.z));
        targetPosition.y = transform.position.y + cameraOffset.y;

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraSmoothSpeed * Time.deltaTime);
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    private void UpdateNameCanvas()
    {
        if (playerCanvas != null && Camera.main != null)
        {
            playerCanvas.transform.position = transform.position + Vector3.up * 2.5f;
            playerCanvas.transform.LookAt(Camera.main.transform);
            playerCanvas.transform.Rotate(0, 180, 0);
        }
    }

    #endregion

    #region Sincronização Photon

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(velocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
        }
    }

    #endregion

    #region Métodos Públicos

    public void SetSpawnPosition(Vector3 position)
    {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;

        networkPosition = position;
        velocity = Vector3.zero;

        UnityEngine.Debug.Log($"Spawn em: {position}");
    }

    public bool IsLocalPlayer => photonView.IsMine;
    public string PlayerName => photonView.Owner.NickName;
    public int PlayerID => photonView.Owner.ActorNumber;

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    #endregion
}
