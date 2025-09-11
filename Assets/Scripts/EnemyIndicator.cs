using UnityEngine;

public class EnemyIndicator : MonoBehaviour
{
    public RectTransform canvas;   
    public RectTransform arrowPrefab;
    public float radius = 200f;

    private RectTransform screenCenter;

    void Start()
    {
        screenCenter = canvas.GetComponent<RectTransform>();
    }

    public void ShowIndicator(Transform enemy)
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.position); //

        Vector2 dir = (Vector2)screenPos - center;
        dir.Normalize();

        Vector2 circlePos = center + dir * radius;

        RectTransform arrow = Instantiate(arrowPrefab, canvas);
        arrow.position = circlePos;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}
