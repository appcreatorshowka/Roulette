using UnityEngine;
using TMPro;

public class NumberDataManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private TMP_InputField inputField;

    // ルーレット上の文字
    [SerializeField] private TMP_Text[] rouletteTexts; // ルーレット上の文字を表示するTextMeshProUGUIの配列

    // 各色の保存データ
    private string[] data = new string[6]; // dataという変数に6色分の文字を保存する配列（保存箱）を作る

    void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownChanged); // ドロップダウンの選択が変わったら、OnDropdownChangedを実行
        inputField.onEndEdit.AddListener(OnInputChanged); // InputFieldの入力が終了したら、OnInputChangedを実行

        ShowData(dropdown.value); // 初期表示として、ドロップダウンの現在の選択内容をInputFieldに表示
    }

    // ドロップダウン変更時
    void OnDropdownChanged(int index) // ドロップダウンの選択が変更されたときに呼ばれるメソッド
    {
        ShowData(index); // 選択内容をInputFieldへ表示
    }

    // 入力確定時
    void OnInputChanged(string value) // InputFieldの入力が確定されたときに呼ばれるメソッド
    {
        int index = dropdown.value; //  現在のドロップダウンの選択インデックスを取得

        data[index] = value; // 入力内容を保存（dataという配列の、index番目の場所に、valueという値を代入する）

        // ルーレット文字更新
        if (index < rouletteTexts.Length) // 配列の範囲内であれば、ルーレット上の文字を更新
        {
            rouletteTexts[index].text = value; // ルーレット上の文字を更新
            // rouletteTextsは配列、indexは何番目の文字を更新するか、textはTextMeshProUGUIの表示文字、valueはInputFieldに入力された文字
        }
    }

    // 選択内容をInputFieldへ表示
    void ShowData(int index) // ドロップダウンの選択インデックスに応じて、InputFieldに保存データを表示するメソッド
    {
        inputField.text = data[index]; // 保存データをInputFieldに表示
    }
}