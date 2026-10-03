using UnityEngine;

public class MoveBlock2D : MonoBehaviour
{
    [Header("移動の設定")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float minX = -5.0f;
    [SerializeField] private float maxX = 5.0f;
    [SerializeField] private float minY = -3.5f;
    [SerializeField] private float maxY = 3.5f;
    [SerializeField] private Move circleScript;

    [Header("回転の設定")]
    [SerializeField] private float rotateSpeed = 180.0f; // 1秒間に回転する角度

    void Update()
    {
        // --- 1. 移動の処理 ---
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0);
        Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        transform.position = targetPosition;


        // --- 2. 回転の処理（キーの追加） ---
        float rotationAmount = 0f;

        // Eキーが押されている間は、右回転（時計回り = マイナス方向）
        if (Input.GetKey(KeyCode.E))
        {
            rotationAmount -= rotateSpeed * Time.deltaTime;
        }

        // Qキーが押されている間は、左回転（反時計回り = プラス方向）
        if (Input.GetKey(KeyCode.Q))
        {
            rotationAmount += rotateSpeed * Time.deltaTime;
        }

        // Z軸（2Dの回転軸）を中心に回転を適用
        transform.Rotate(0, 0, rotationAmount);
        if (circleScript != null)
        {
            Debug.Log("au");
            circleScript.RotateCircle(rotationAmount);
        }
    }
}
