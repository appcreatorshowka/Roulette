using UnityEngine;
using UnityEngine.UI;

public class AngelInfinityMove : MonoBehaviour // AngelInfinityMoveというクラスを定義
{
    RectTransform rect; // 変数rectにRectTransform型のコンポーネントを格納
    Vector2 startPos; // 変数startPosにVector2型の初期位置を格納

    [SerializeField] float width = 100f;   // 横幅
    [SerializeField] float height = 50f;   // 高さ
    [SerializeField] float speed = 1f;     // 移動速度

    [SerializeField] private Image image; 

    [SerializeField] private Sprite angelSprite;

    [SerializeField] private Sprite devilSprite;

    private bool isAngel = true;
    public bool IsAngel => isAngel; // IsAngelは読み取り専用窓口、isAngel

    void Start()
    {
        rect = GetComponent<RectTransform>(); // RectTransformを取得   
        startPos = rect.anchoredPosition; // 初期位置を保存
    }

    void Update()
    {
        float t = Time.time * speed; // 時間に応じて変化する値を計算

        float x = Mathf.Sin(t) * width; // 横方向の移動量を計算
        float y = Mathf.Sin(t * 2f) * height; // 縦方向の移動量を計算

        rect.anchoredPosition =
            startPos + new Vector2(x, y); // 初期位置に移動量を加算して新しい位置を設定
    }


    public void ToggleCharacter() // キャラクターを切り替えるメソッド
    {
        isAngel = !isAngel; // isAngelが trueならfalseに、falseならtrueに切り替える

        if (isAngel) // isAngelがtrueの場合、つまり天使の状態の場合
        {
            image.sprite = angelSprite; // 画像を天使のスプライトに切り替える
        }
        else
        {
            image.sprite = devilSprite; // 画像を悪魔のスプライトに切り替える
        }
    }
}
