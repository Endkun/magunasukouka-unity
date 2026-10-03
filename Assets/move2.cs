using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("速度・物理設定（秒単位）")]
    public Vector2 velocity = new Vector2(12f, 3f);
    public float ay = -9.81f; // 重力加速度
    public float ka = 0f; // 回転力・マグヌス効果の係数（正でトップスピン、負でバックスピン）
    public float kt = 0.05f;  // 空気抵抗の係数
    public float ko = 0.1f;   // 空気の密度
    public float angle = 0f;  // 回転の角度（度数法）
    public Transform Square;

    [Header("跳ね返り設定")]
    [Range(0f, 1f)] public float bounceFactor = 0.8f; // 跳ね返り係数
    public float minBounceVelocity = 0.5f;           // 接地判定にする速度しきい値

    [Header("接地中の摩擦")]
    [Range(0f, 1f)] public float groundFriction = 0.9f; // 1に近いほど減速しにくい

    private Rigidbody2D rb;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            // このスクリプトの当たり判定コールバックは Rigidbody2D が無いと基本的に発火しないため注意
            Debug.LogWarning("Move: Rigidbody2D がアタッチされていません。衝突コールバックが正しく動作しない可能性があります。");
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.useFullKinematicContacts = true;
        }

        transform.position = new Vector3(-8f, 0f, 0f);
    }
    public void RotateCircle(float amount)
    {
        ka *= amount;
    }
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (isGrounded)
        {
            velocity.y = 0f;
            // 接地中の摩擦（横方向を徐々に減速）
            velocity.x *= groundFriction;
        }
        else
        {
            // 1. 空気抵抗：速度方向に対して逆向きに働く力（velocity自体が向きを持つため、
            //    velocity * |velocity| は速度と同方向・速度の2乗に比例した大きさになり、
            //    それを velocity から引くことで正しく減速する）
            Vector2 drag = 0.5f * ko * kt * velocity * velocity.magnitude;
            velocity -= drag * dt;

            // 2. 重力
            velocity.y += ay * dt;

            // 3. マグヌス効果（スピンによる力）
            // 進行方向（velocity）に対して90度回転した向きに、スピン量（ka）に応じた加速度を与える
            if (velocity.magnitude > 0.001f)
            {
                Vector2 perpendicular = new Vector2(-velocity.y, velocity.x).normalized;
                Vector2 magnusAcceleration = perpendicular * ka * velocity.magnitude;
                velocity += magnusAcceleration * dt;
            }
        }

        // 移動処理
        Vector3 moveStep = (Vector3)velocity * dt;

        if (rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)moveStep);
        }
        else
        {
            transform.position += moveStep;
        }

        // 回転の計算（角度が無限に増え続けて精度が落ちないよう 0〜360 に丸める）
        angle = Mathf.Repeat(angle + ka * 3000f * dt, 360f);
        float rad = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        if (Square != null)
        {
            Square.localPosition = new Vector3(x / 3f, y / 3f, -1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    // Kinematicの場合、接触し続けている間はStayが呼ばれるため、ここで接地を維持する
    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void HandleCollision(Collision2D collision)
    {
        if (collision.contactCount == 0) return;

        // 複数接触点がある場合、最もめり込みが深い（separationが最小=最も負）接触点を採用する
        ContactPoint2D contact = collision.contacts[0];
        for (int i = 1; i < collision.contactCount; i++)
        {
            if (collision.contacts[i].separation < contact.separation)
            {
                contact = collision.contacts[i];
            }
        }

        Vector2 normal = contact.normal;

        // 床（上向きの面）に当たったかを判定
        bool hitFloor = normal.y > 0.7f;

        // めり込みを押し戻す補正処理：実際にめり込んでいる（separation < 0）場合のみ行う。
        // separation >= 0（接触はしているが離れている/ちょうど接している）場合に押し出すと
        // 接地中に外側へじわじわドリフトし、Exit/Enterが誤発火してガタつく原因になる。
        if (contact.separation < 0f)
        {
            float correction = -contact.separation; // separationが負なので正の値になる
            if (rb != null)
                rb.position += normal * correction;
            else
                transform.position += (Vector3)(normal * correction);
        }

        // 接地判定時の静止処理
        if (hitFloor && Mathf.Abs(velocity.y) < minBounceVelocity)
        {
            isGrounded = true;
            velocity.y = 0f;
            return;
        }

        // 通常の跳ね返り処理（衝突した瞬間のフレームのみで速度を反射させる）
        // Stay時にも毎回反射させるとガタつくため、法線方向への速度が衝突向きの時だけ計算
        if (Vector2.Dot(velocity, normal) < 0)
        {
            Vector2 reflectedVelocity = Vector2.Reflect(velocity, normal);
            velocity = reflectedVelocity * bounceFactor;

            if (velocity.y > minBounceVelocity)
            {
                isGrounded = false;
            }
        }
    }
}