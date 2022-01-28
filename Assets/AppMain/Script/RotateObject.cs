using UnityEngine;

public class RotateObject : MonoBehaviour
{
    /// <summary>
    /// X軸を基点に回転する速さ
    /// </summary>
    [SerializeField]
    private float rotAngleX = 0.0f;

    /// <summary>
    /// Y軸を基点に回転する速さ
    /// </summary>
    [SerializeField]
    private float rotAngleY = 1.5f;

    /// <summary>
    /// Z軸を基点に回転する速さ
    /// </summary>
    [SerializeField]
    private float rotAngleZ = 0.0f;

    // Update is called once per frame
    void Update()
    {
        // フレームごとに回転させる
        transform.Rotate(rotAngleX, rotAngleY, rotAngleZ);
    }
}