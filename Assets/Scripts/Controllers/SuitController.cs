using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class SuitController : PlayerController
{
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 1.9f;

    private bool isGrounded;
    public bool isGravityActive = true;
    private Vector3 input;

    float verticalLookRotation;
    public Transform groundChecker;
    public float mouseSensitivityX = 2;
    public float mouseSensitivityY = 2;

    public override void Start()
    {
        _rigidbody.freezeRotation = true;
        canvasController.hud01.gameObject.SetActive(false);
        canvasController.hud02.gameObject.SetActive(false);
        canvasController.currentSpeed.gameObject.SetActive(false);
        this.canvasController.crosshair.sprite = Resources.Load<Sprite>("Textures/UI/center_crosshair02");
        isGravityActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Camera camera = cameraManager.GetMainCamera();
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
        camera.transform.localEulerAngles = Vector3.left * verticalLookRotation;
        float moveVertical = 0f;

        if (!isGravityActive)
        {
            if (Input.GetKey(KeyCode.Space))
                moveVertical = 1f;
            else if (Input.GetKey(KeyCode.LeftShift))
                moveVertical = -1f;
        }

        input = new Vector3(Input.GetAxisRaw("Horizontal"), moveVertical, Input.GetAxisRaw("Vertical")).normalized;
        if (isGravityActive)
        {
            // Проверка на землю
            isGrounded = Physics.CheckSphere(groundChecker.position, groundCheckRadius, groundMask);
        }

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded && isGravityActive)
        {
            _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, jumpForce, _rigidbody.linearVelocity.z);
        }
    }

    public override void FixedUpdate()
    {
        if (isGravityActive)
        {
            Vector3 move = transform.TransformDirection(new Vector3(input.x, 0, input.z)) * moveSpeed;
            _rigidbody.linearVelocity = new Vector3(move.x, _rigidbody.linearVelocity.y, move.z);
        }
        else
        {
            Vector3 move = transform.TransformDirection(input) * moveSpeed;
            _rigidbody.linearVelocity = move;
        }
    }
}
