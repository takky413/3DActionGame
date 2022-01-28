using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    //Ekardでプレイする場合のオブジェクト
    [SerializeField] private GameObject Ekard = null;
    [SerializeField] private GameObject EkardCanvas = null;
    [SerializeField] private GameObject EkardGameOverCanvas = null;
    //[SerializeField] private GameObject EkardCanvasManager = null;

    //Avelynでプレイする場合のオブジェクト
    [SerializeField] private GameObject Avelyn = null;
    [SerializeField] private GameObject AvelynCanvas = null;
    [SerializeField] private GameObject AvelynGameOverCanvas = null;
    //[SerializeField] private GameObject AvelynCanvasManager = null;


    
    // Start is called before the first frame update
    void Start()
    {
        /*
        //Ekard関係のゲームオブジェクトを取得
        Ekard = GameObject.Find("Ekard");
        EkardCanvas = GameObject.Find("EkardCanvas");
        EkardGameOverCanvas = GameObject.Find("EkardGameOverCanvas");
        EkardCanvasManager = GameObject.Find("EkardCanvasManager");
        */ //ここで取得しようとしたけどうまくいかないため、inspecterから設定することにした

        if (TitleScene.isSelectEkard == true)
        {
            Ekard.SetActive(true);
            EkardCanvas.SetActive(true);
            EkardGameOverCanvas.SetActive(true);
            //EkardCanvasManager.SetActive(true);
        }
        else if (TitleScene.isSelectAvelyn == true)
        {
            Avelyn.SetActive(true);
            AvelynCanvas.SetActive(true);
            AvelynGameOverCanvas.SetActive(true);
            //AvelynCanvasManager.SetActive(true);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
