
using UnityEngine;

public class AngelInfinityMove : MonoBehaviour
{
    RectTransform rect;
    Vector2 startPos;

    [SerializeField] float width = 100f;
    [SerializeField] float height = 50f;
    [SerializeField] float speed = 1f;

    [SerializeField] private Animator animator;

    [SerializeField] private AnimationClip angelClip;
    [SerializeField] private AnimationClip devilClip;

    private bool isAngel = true;

    public bool IsAngel => isAngel;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;

        // 最初は天使のアニメーション
        animator.Play(angelClip.name);
    }

    void Update()
    {
        float t = Time.time * speed;

        float x = Mathf.Sin(t) * width;
        float y = Mathf.Sin(t * 2f) * height;

        rect.anchoredPosition =
            startPos + new Vector2(x, y);
    }

    public void ToggleCharacter()
    {
        isAngel = !isAngel;

        if (isAngel)
        {
            animator.Play(angelClip.name);
        }
        else
        {
            animator.Play(devilClip.name);
        }
    }
}
