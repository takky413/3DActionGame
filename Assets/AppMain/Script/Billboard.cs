using UnityEngine;

public class Billboard : MonoBehaviour
{
    //! ターゲットカメラ.
    [SerializeField] GameObject lookCamera = null;
    //! Y軸のみの回転にするフラグ.
    [SerializeField] bool isY = false;


    void Start()
    {
        //if (lookCamera == null) lookCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if (lookCamera == null) return;

        // Y軸回転のみ.
        if (isY == true)
        {
            var cameraPos = lookCamera.transform.position;
            //var cameraRot = lookCamera.transform.rotation;
            cameraPos.y = this.transform.position.y;
            //this.transform.rotation = Quaternion.Euler(0, 180, 0); //向きを反転
            var look = this.transform.position - cameraPos;

            this.transform.forward = -look;
        }
        // 完全に正面をカメラに向ける.
        else
        {
            var cameraPos = lookCamera.transform.position;
            var look = this.transform.position - cameraPos;

            this.transform.forward = look;
        }
    }
}