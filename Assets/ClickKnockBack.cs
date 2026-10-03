using UnityEngine;

/// <summary>
/// ボールの近くをクリックすると、クリックした位置から遠ざかる方向へ
/// 軽く吹き飛ぶ（ノックバックする）ようにするスクリプト。
/// Move スクリプトとは別コンポーネントとして、同じ GameObject にアタッチして使う。
/// </summary>
[RequireComponent(typeof(Move))]
public class ClickKnockback : MonoBehaviour
{
    [Header("検出設定")]
    [Tooltip("この半径内をクリックすると反応する（ボールの見た目より少し大きめが押しやすい）")]
    public float detectionRadius = 1.5f;

    [Header("吹き飛ばし設定")]
    [Tooltip("加える力の強さ")]
    public float impulseStrength = 6f;

    [Tooltip("クリック位置がボールに近いほど強く吹き飛ぶようにする")]
    public bool scaleByDistance = true;

    private Move move;
    private Camera cam;

    void Start()
    {
        move = GetComponent<Move>();
        cam = Camera.main;
    }

    void Update()
    {
        // 左クリックされた瞬間だけ判定する
        if (!Input.GetMouseButtonDown(0)) return;
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 ballPos = transform.position;
        Vector2 clickPos = mouseWorld;

        float distance = Vector2.Distance(ballPos, clickPos);
        if (distance > detectionRadius) return;

        // クリックした場所から遠ざかる方向に飛ばす
        // 例）ボールの下でクリック → 上に飛ぶ
        //     右下でクリック → 左上に飛ぶ
        //     右上でクリック → 左下に飛ぶ
        Vector2 direction = (ballPos - clickPos);
        if (direction.sqrMagnitude < 0.0001f)
        {
            // ほぼ真上でクリックした場合（方向が定まらない）は適当に上向きにする
            direction = Vector2.up;
        }
        direction.Normalize();

        float power = impulseStrength;
        if (scaleByDistance)
        {
            // 近くでクリックするほど強く、detectionRadius付近ではほぼ効果なし
            float t = 1f - Mathf.Clamp01(distance / detectionRadius);
            power *= t;
        }

        // Move側の速度に直接加算することで「軽く吹き飛ぶ」動きを作る
        move.velocity += direction * power;
    }

    // シーンビューで検出範囲を確認しやすくする
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}