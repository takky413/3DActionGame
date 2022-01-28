using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using UnityEngine.XR;

using UnityEngine.SceneManagement;

using SoftGear.Strix.Client.Core.Auth.Message;
using SoftGear.Strix.Client.Core.Error;
using SoftGear.Strix.Client.Core.Model.Manager.Filter;
using SoftGear.Strix.Client.Core;
using SoftGear.Strix.Unity.Runtime;
using SoftGear.Strix.Net.Logging;
using SoftGear.Strix.Unity.Runtime.Event;


public class PlayerController : StrixBehaviour
{
    // 攻撃判定用オブジェクト.
    [SerializeField] GameObject attackHit = null;
    // 必殺技定用オブジェクト.
    [SerializeField] GameObject chargeAttackHit = null;
    // 設置判定用ColliderCall.
    [SerializeField] ColliderCallReceiver footColliderCall = null;
    // ジャンプ力.
    [SerializeField] float jumpPower = 20f;
    // タッチマーカー.
    [SerializeField] GameObject touchMarker = null;
    // アニメーター.
    Animator animator = null;
    // リジッドボディ.
    Rigidbody rigid = null;
    // ゲームオーバー時に表示するキャンバス
    [SerializeField] GameObject gameOver = null; //追加
    // 攻撃アニメーション中フラグ
    bool isAttack = false;
    // 接地フラグ.
    bool isGround = false;
    // 防御中フラグ
    bool isGuard = false;
    // チャージ中フラグ
    bool isCharging = false;
    // 死亡フラグ GameControllerで使うためにpublicにしておく
    public static bool isDeath = false;
    // 必殺技エフェクト表示中フラグ、当たり判定のコラーダーを動かすタイミング制御に用いる
    bool isChargeAttackEffect = false;
    // 回避中フラグ
    bool isAvoid = false;
    // Oculusのボタンで操作する際に、ポーズパネルを表示しているかどうかを判定するフラグ
    // また、他のスクリプトでポーズしている時は動きを止めるために使う
    public static bool isPause = false;

    //キャラクターの正面方向を保持しておく変数
    Vector3 PlayerfForwardDirection = new Vector3(0.0f, 0.0f, 0.0f); //初期化しておかないと使えない

    // PCキー横方向入力.
    float horizontalKeyInput = 0;
    // PCキー縦方向入力.
    float verticalKeyInput = 0;

    // ボタンの長押しに関する変数
    float time = 0.0f; //カウント用
    bool isButtonDown = false; //長押ししているかどうかのフラグ

    // カメラコントローラー.
    [SerializeField] PlayerCameraController cameraController = null;

    // -------------------------------------------------------
    /// ステータス
    // -------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // 体力.
        [StrixSyncField] public int Hp = 10; //同期させる
        // 攻撃力.
        public int Power = 1;
    }

    // 攻撃HitオブジェクトのColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;
    // 必殺技HitオブジェクトのColliderCall.
    [SerializeField] ColliderCallReceiver chargeAttackHitCall = null;
    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status(); //同期させる

    //! HPバーのスライダー.
    [SerializeField] Slider hpBar = null;


    //! ゲームオーバー時イベント.
    public UnityEvent GameOverEvent = new UnityEvent();

    // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();

    //味方キャラクターが死んだ時に、敵を非アクティブにするために設定する
    [SerializeField] GameObject Enemy1 = null;
    [SerializeField] GameObject Enemy2 = null;
    [SerializeField] GameObject Enemy3 = null;
    [SerializeField] GameObject Enemy4 = null;

    // パーティクル関係
    [SerializeField] public ParticleSystem slashEffect; //斬撃エフェクト
    [SerializeField] public ParticleSystem slashDoubleEffect; //三段目の攻撃の斬撃エフェクト
    [SerializeField] public ParticleSystem barrierEffect; //バリアエフェクト
    [SerializeField] public ParticleSystem chargeEffect; //チャージエフェクト
    [SerializeField] public ParticleSystem chargeAttackEffect; //必殺技エフェクト
    [SerializeField] public ParticleSystem hitEffect; //ヒットエフェクト

    //ポーズ画面のCanvas
    [SerializeField] public GameObject PauseCanvas;

    
    //スマホタッチのフラグ
    bool isTouch = false;
    // 左半分タッチスタート位置.
    Vector2 leftStartTouch = new Vector2();
    // 左半分タッチ入力.
    Vector2 leftTouchInput = new Vector2();

    //効果音
    private AudioSource audioSource;
    public AudioClip swordAttackSound;
    public AudioClip swordAttackSound2;

    //Oculusコントローラ
    OVRInput.Controller RightCon = OVRInput.Controller.RTouch;
    OVRInput.Controller LeftCon = OVRInput.Controller.LTouch;

    //タイトルに戻る時にルームとの接続を切るためのオブジェクト
    [SerializeField] public GameObject StrixDisconnectRoom;

    // Start is called before the first frame update
    void Start()
    {
        // Animatorを取得し保管.
        animator = GetComponent<Animator>();
        // Rigidbodyの取得.
        rigid = GetComponent<Rigidbody>();
        // 攻撃判定用オブジェクトを非表示に.
        attackHit.SetActive(false);

        // FootSphereのイベント登録.
        footColliderCall.TriggerEnterEvent.AddListener(OnFootTriggerEnter);
        footColliderCall.TriggerExitEvent.AddListener(OnFootTriggerExit);

        // 攻撃判定用コライダーイベント登録.
        attackHitCall.TriggerEnterEvent.AddListener(OnAttackHitTriggerEnter);
        // 必殺技判定用コライダーイベント登録.
        chargeAttackHitCall.TriggerEnterEvent.AddListener(OnChargeAttackHitTriggerEnter);

        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;

        // スライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;

        
        // 開始時の位置回転を保管.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        //Rpc呼び出しにすると、なぜかゲーム開始直後に浮くため追加
        animator.SetBool("isGround", true);

        // AudioSourceを取得
        audioSource = GetComponent<AudioSource>();

    }


    // Update is called once per frame
    void Update()
    {
        
        // Strixで自分のキャラクターだけを動かすための記述
        if (!isLocal)
        {
            return;
        }

        if (isPause) //ポーズ中は何もしない
        {
            if (OVRInput.GetDown(OVRInput.Button.Three)) //ポーズパネル表示中にメニューボタンでゲームに戻る
            {
                OnReturnGameButtonClicked();
                //isPause = false; //上の関数内でfalseにする
            }
            if (isPause == true && OVRInput.GetDown(OVRInput.Button.Four)) //ポーズパネル表示中にXボタンでタイトルに戻る
            {
                OnGoTitleButtonClicked();
                //isPause = false; //上の関数内でfalseにする
            }
            return;
        }


        // カメラをプレイヤーに向ける. 
        cameraController.UpdateCameraLook(this.transform);
        //視点の変更
        cameraController.UpdateRightJoyStick(); //追加

        //スマホによる操作の場合
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // スマホタッチ操作.
            // タッチしている指の数が０より多い.
            if (Input.touchCount > 0)
            {
                isTouch = true;
                // タッチ情報をすべて取得.
                Touch[] touches = Input.touches;
                // 全部のタッチを繰り返して判定.
                foreach (var touch in touches)
                {
                    bool isLeftTouch = false;
                    bool isRightTouch = false;
                    // タッチ位置のX軸方向がスクリーンの左側.
                    if (touch.position.x > 0 && touch.position.x < Screen.width / 2)
                    {
                        isLeftTouch = true;
                    }
                    // タッチ位置のX軸方向がスクリーンの右側.
                    else if (touch.position.x > Screen.width / 2 && touch.position.x < Screen.width)
                    {
                        isRightTouch = true; ;
                    }

                    // 左タッチ.
                    if (isLeftTouch == true)
                    {
                        // タッチ開始.
                        if (touch.phase == TouchPhase.Began)
                        {
                            // 開始位置を保管.
                            leftStartTouch = touch.position;
                            // 開始位置にマーカーを表示.
                            touchMarker.SetActive(true);
                            Vector3 touchPosition = touch.position;
                            touchPosition.z = 1f;
                            Vector3 markerPosition = Camera.main.ScreenToWorldPoint(touchPosition);
                            touchMarker.transform.position = markerPosition;
                        }
                        // タッチ中.
                        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            // 現在の位置を随時保管.
                            Vector2 position = touch.position;
                            // 移動用の方向を保管.
                            leftTouchInput = position - leftStartTouch;
                        }
                        // タッチ終了.
                        else if (touch.phase == TouchPhase.Ended)
                        {
                            leftTouchInput = Vector2.zero;
                            // マーカーを非表示.
                            touchMarker.gameObject.SetActive(false);
                        }
                    }

                    // 右タッチ.
                    if (isRightTouch == true)
                    {
                        cameraController.UpdateRightTouch(touch);
                    }
                }
            }
            else
            {
                isTouch = false;
            }
        }
        else
        {
            // PCキー入力取得.
            horizontalKeyInput = -OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x + Input.GetAxis("Horizontal");
            verticalKeyInput = -OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y + Input.GetAxis("Vertical");

            //Oculusからの入力を-1から1に変換
            if (horizontalKeyInput > 1) { horizontalKeyInput = 1; }
            else if (horizontalKeyInput < -1) { horizontalKeyInput = -1; }
            if (verticalKeyInput > 1) { verticalKeyInput = 1; }
            else if (verticalKeyInput < -1) { verticalKeyInput = -1; }

        }

        // プレイヤーの向きを調整.
        //bool isKeyInput = (horizontalKeyInput != 0 || verticalKeyInput != 0);
        bool isKeyInput = (horizontalKeyInput != 0 || verticalKeyInput != 0 || leftTouchInput != Vector2.zero);
        if (isKeyInput == true && isAttack == false && isGuard == false && isCharging == false)
        {

            bool currentIsRun = animator.GetBool("isRun");
            if (currentIsRun == false) RpcToAll("RpcSetBool","isRun", true); //animator.SetBool("isRun", true);
            Vector3 dir = rigid.velocity.normalized;
            dir.y = 0;
            this.transform.forward = dir;
        }
        else
        {
            bool currentIsRun = animator.GetBool("isRun");
            if (currentIsRun == true) RpcToAll("RpcSetBool","isRun", false); //animator.SetBool("isRun", false);
        }


        //Oculusコントローラによるジャンプや攻撃の処理
        if (OVRInput.GetDown(OVRInput.Button.One)) //Aボタンで攻撃
        {
            OnAttackButtonClicked();
        }
        if (OVRInput.GetDown(OVRInput.Button.Two)) //Bボタンで必殺技
        {
            OnLongAttackButtonDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.Two)) //Bボタンでチャージ解除
        {
            OnLongAttackButtonUp();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) //右手の人差し指ボタンでジャンプ
        {
            OnJumpButtonClicked();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)) //左手の人差し指ボタンでガード
        {
            OnGuardButtonDown();
        }
        if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger)) //左手の人差し指ボタンでガード解除
        {
            OnGuardButtonUp();
        }
        if (isPause == false){
            if (OVRInput.GetDown(OVRInput.Button.Three)) //左手のメニューボタンでポーズ画面表示//オン・オフ不安定なので注意
            {
                OVRInput.SetControllerVibration(0.1f, 0.1f, LeftCon); //左コントローラを振動 //デバック
                OnPauseButtonClicked();
                //isPause = true; //上の関数内でtrueにする
            }
        }
        /*
        if(isPause == true){
            if (OVRInput.GetDown(OVRInput.Button.Three)) //ポーズパネル表示中にメニューボタンでゲームに戻る
            {
                OnReturnGameButtonClicked();
                //isPause = false; //上の関数内でfalseにする
            }
            if (isPause == true && OVRInput.GetDown(OVRInput.Button.Four)) //ポーズパネル表示中にXボタンでタイトルに戻る
            {
                OnGoTitleButtonClicked();
                //isPause = false; //上の関数内でfalseにする
            }
        }
        */

    }


    void FixedUpdate()
    {
        // Strixで自分のキャラクターだけを動かすための記述
        if (!isLocal)
        {
            return;
        }

        if (isPause) //ポーズ中は何もしない
        {
            return;
        }


        // カメラの位置をプレイヤーに合わせる.
        cameraController.FixedUpdateCameraPosition(this.transform);

        if (isAttack == false && isGuard == false && isCharging == false)
        {
            Vector3 input = new Vector3();
            Vector3 move = new Vector3();
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            //Androidも入れてしまうと、Oculusでコントロールできなくなってしまう。
            {
                input = new Vector3(leftTouchInput.x, 0, leftTouchInput.y);
                move = input.normalized * 2f;
            }
            else
            {
                input = new Vector3(horizontalKeyInput, 0, verticalKeyInput);
                move = input.normalized * 2f;
            }


            Vector3 cameraMove = Camera.main.gameObject.transform.rotation * move;
            cameraMove.y = 0;
            Vector3 currentRigidVelocity = rigid.velocity;
            currentRigidVelocity.y = 0;

            rigid.AddForce(cameraMove - currentRigidVelocity, ForceMode.VelocityChange);
        }

        //必殺技のエフェクトに合わせて、当たり判定のコライダーを動かす
        if (isChargeAttackEffect == false)
        {
            //必殺技の当たり判定コライダは独立しているため、ここで親の位置に合わせる
            //プレイヤーと重なっていると、必殺技を打った時に自分も喰らってしまうため、少し前にずらす
            chargeAttackHit.transform.position = this.transform.position + this.transform.forward.normalized * 1.2f;
            chargeAttackHit.transform.rotation = this.transform.rotation;
            PlayerfForwardDirection = this.transform.forward.normalized; //正面方向を保持しておく
        }
        else
        {
            chargeAttackHit.transform.position += PlayerfForwardDirection * 0.13f;

            //当たり判定のコライダーが十分進んだら=プレイヤーから十分離れたら、非表示にして位置をリセットする（エフェクトの終了を読むのは難しいため）
            if (chargeAttackHit.transform.position.magnitude > this.transform.position.magnitude + 13)
            {
                chargeAttackHit.SetActive(false); // 必殺技あたり判定用オブジェクトを非表示に
                chargeAttackHit.transform.position = this.transform.position; //位置をリセット
                isChargeAttackEffect = false; //エフェクトも終了したとみなす
            }
        }


        //Oculusコントローラ加速度による処理
        float accRight = OVRInput.GetLocalControllerAcceleration(RightCon).magnitude; //右コントローラ加速度で攻撃
        if (accRight > 5)
        {
            OnAttackButtonClicked();
        }
        float accLeft = OVRInput.GetLocalControllerAcceleration(LeftCon).magnitude; //左コントローラ加速度で回避
        if (accLeft > 10)
        {
            OnAvoidButtonClicked();
        }
        /*
        if (OVRInput.GetDown(OVRInput.Button.Two)) //Bボタンで必殺技
        {
            OnLongAttackButtonDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.Two)) //Bボタンでチャージ解除
        {
            OnLongAttackButtonUp();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) //右手の人差し指ボタンでジャンプ
        {
            OnJumpButtonClicked();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)) //左手の人差し指ボタンでガード
        {
            OnGuardButtonDown();
        }
        if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger)) //
        {
            OnGuardButtonUp();
        }*/
    }

    //---------------------------------------------------------------------
    //SetTriggerをRpc呼び出しするための関数
    //---------------------------------------------------------------------
    [StrixRpc]
    void RpcSetTrigger(string _string)
    {
        animator.SetTrigger(_string);
    }

    //---------------------------------------------------------------------
    //SetBoolをRpc呼び出しするための関数
    //---------------------------------------------------------------------
    [StrixRpc]
    void RpcSetBool(string _string, bool _bool)
    {
        animator.SetBool(_string, _bool);
    }

    //---------------------------------------------------------------------
    //audioSource.PlayOneShotをRpc呼び出しするための関数
    //---------------------------------------------------------------------
    [StrixRpc]
    void RpcPlayOneShot(AudioClip _AudioClip)
    {
        audioSource.PlayOneShot(_AudioClip);
    }


    //---------------------------------------------------------------------
    //Destroy関数をRpc呼び出しするための関数
    //---------------------------------------------------------------------
    [StrixRpc]
    void RpcDestroy(string _string)
    {
        Destroy(GameObject.Find(_string));
    }


    //---------------------------------------------------------------------
    //bool値の設定ををRpc呼び出しするための関数
    //---------------------------------------------------------------------
    [StrixRpc]
    void RpcisChargingFaulse()
    {
        isCharging = false;
    }
    [StrixRpc]
    void RpcisGuardFaulse()
    {
        isGuard = false;
    }


    // ---------------------------------------------------------------------
    // 攻撃ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnAttackButtonClicked()
    {
        if (isAttack == false)
        {
            // AnimationのisAttackトリガーを起動.
            RpcToAll("RpcSetTrigger","isAttack");
            // 攻撃開始.
            isAttack = true;
            // 攻撃判定用の球を有効化
            //attackHit.SetActive(true); //Anim_AttackHit側で呼び出すことにしてあるため不要
        }
        //isAttack = false; //自分で追加 //Anim_AttackHitなどのアニメーションイベントが外れていたため追加したが、修正済み

        Debug.Log("攻撃ボタンが押されました");
    }


    // ---------------------------------------------------------------------
    // 攻撃ボタン長押しコールバック
    // ---------------------------------------------------------------------
    //ボタンが押された時
    public void OnLongAttackButtonDown()
    {
        isButtonDown = true; //長押し検知
        time = 0; //カウントをリセット

        StartCoroutine(LongCount()); //カウント開始

        //isCharging = true; //エフェクトを一回だけ呼ぶために、アニメーターイベント内で変更する
        RpcToAll("RpcSetBool", "isCharging", true); //チャージアニメーション開始

        // チャージエフェクトプレハブをインスタンス化
        /* //アニメーターイベントから呼ぶことにする
        Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Instantiate<ParticleSystem>(chargeEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);
        */
    }

    //ボタンから離れた時
    public void OnLongAttackButtonUp()
    {
        //isCharging = false;
        RpcToAll("RpcisChargingFaulse");
        isButtonDown = false;
        RpcToAll("RpcSetBool", "isCharging", false); //チャージ中断
        //Destroy(GameObject.Find("eff_pfb_charge_001(Clone)")); //Ekardに設定したチャージエフェクトを破壊
        RpcToAll("RpcDestroy","eff_pfb_charge_001(Clone)");
        //Destroy(GameObject.Find("eff_pfb_charge_Avelyn_001(Clone)")); //Avelynに設定したチャージエフェクトを破壊
        RpcToAll("RpcDestroy", "eff_pfb_charge_Avelyn_001(Clone)");

    }

    //カウント用を行うコルーチン
    private IEnumerator LongCount()
    {
        yield return new WaitForSeconds(0.1f);

        if (isButtonDown == true) //ボタンを押している時はカウント数を増やす
        {
            time += 0.1f;

            if (time > 2.0f) //3秒たったら、必殺技を発動する
            {
                Destroy(GameObject.Find("eff_pfb_charge_001(Clone)")); //Ekardに設定したチャージエフェクトを破壊
                Destroy(GameObject.Find("eff_pfb_charge_Avelyn_001(Clone)")); //Avelynに設定したチャージエフェクトを破壊

                RpcToAll("RpcSetTrigger", "isCharged"); //必殺技アニメーション開始
                //RpcToAll("RpcSetBool", "isCharging", false); //チャージ完了　//なぜかいらない

               //エフェクトの呼び出しはアニメーターイベントで行う
                yield break;
            }
            else //まだ3秒たっていなければ処理を繰り返す
            {
                StartCoroutine(LongCount());
                yield break;
            }
        }
    }


    // ---------------------------------------------------------------------
    // ガードボタンクリックコールバック
    // ---------------------------------------------------------------------
    //押している間
    public void OnGuardButtonDown()
    {
        if (isAttack == true || isGround == false) //攻撃中やジャンプ中はガードできない
        {
            return;
        }

        //isGuard = true; //アニメーターイベントでいじるように変更
        RpcToAll("RpcSetBool", "isGuard", true);

        // バリアエフェクトプレハブをインスタンス化
        /*
        Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Instantiate<ParticleSystem>(barrierEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);
        */
    }

    //離した時
    public void OnGuardButtonUp()
    {
        //isGuard = false;
        RpcToAll("RpcisGuardFaulse");
        RpcToAll("RpcSetBool", "isGuard", false);

        //Destroy(GameObject.Find("eff_pfb_barrier_001(Clone)")); //Ekardに設定したバリアエフェクト
        RpcToAll("RpcDestroy", "eff_pfb_barrier_001(Clone)");
        //Destroy(GameObject.Find("eff_pfb_barrier_green_001(Clone)")); //Avelynに設定したバリアエフェクト
        RpcToAll("RpcDestroy", "eff_pfb_barrier_green_001(Clone)");

    }


    // ---------------------------------------------------------------------
    // ジャンプボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnJumpButtonClicked()
    {
        if (isGround == true)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }


    // ---------------------------------------------------------------------
    // 回避ボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnAvoidButtonClicked()
    {
        RpcToAll("RpcSetTrigger", "isAvoid");
        isAvoid = true;
    }

    
    // ---------------------------------------------------------------------
    // ポーズボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnPauseButtonClicked()
    {
        PauseCanvas.gameObject.SetActive(true);
        isPause = true;
        Debug.Log("PauseButtonClicked");
    }


    // ---------------------------------------------------------------------
    // ゲームへ戻るボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnReturnGameButtonClicked()
    {
        PauseCanvas.gameObject.SetActive(false);
        isPause = false;
    }


    // ---------------------------------------------------------------------
    // タイトルへ戻るボタンクリックコールバック
    // ---------------------------------------------------------------------
    public void OnGoTitleButtonClicked()
    {
        StrixDisconnectRoom.gameObject.SetActive(true);
        isPause = false;
        SceneManager.LoadScene("Title");
    }


    // ---------------------------------------------------------------------
    // FootSphereトリガーエンターコール
    // ---------------------------------------------------------------------
    void OnFootTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Ground")
        {
            isGround = true;
            //animator.SetBool("isGround",true);
            RpcToAll("RpcSetBool","isGround", true);
        }
    }


    // ---------------------------------------------------------------------
    // FootSphereトリガーイグジットコール
    // ---------------------------------------------------------------------
    void OnFootTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Ground")
        {
            isGround = false;
            //animator.SetBool("isGround", false);
            RpcToAll("RpcSetBool", "isGround", false);
        }
    }


    // ---------------------------------------------------------------------
    // 攻撃判定トリガーエンターイベントコール
    // ---------------------------------------------------------------------
    //攻撃を当てた時に呼ばれる
    [StrixRpc]
    void OnAttackHitTriggerEnter(Collider col)
    {
        if (!isLocal)
        {
            return;
        }

        if (col.gameObject.tag == "Enemy")
        {
            var enemy = col.gameObject.GetComponent<EnemyBase>();
            enemy?.OnAttackHit(CurrentStatus.Power/*, this.transform.position*/);
            attackHit.SetActive(false);
        }


        //対戦でプレイヤー同士の場合
        if (TitleScene.isSelectBattleMode == true) //バトルモードの時だけ
        {
            if (col.gameObject.tag == "Player")
            {
                var player = col.GetComponent<PlayerController>();
                player?.RpcToAll("OnEnemyAttackHit", CurrentStatus.Power/*, this.transform.position*/);
                attackHit.SetActive(false);
            }
        }

    }


    // ---------------------------------------------------------------------
    // 敵の攻撃がヒットしたときの処理
    // ---------------------------------------------------------------------
    //攻撃を当てられた時に呼ばれる
    [StrixRpc]
    public void OnEnemyAttackHit(int damage/*, Vector3 attackPosition*/)
    {
        /*
        if (!isLocal) //ここが原因？ifの中をisLocalだけにしてみよう→自分のhpが減らなくなる
        {
            return;
        }*///if文をなくしたらできた！

        if (isGuard == true || isAvoid == true) //ガード中、回避中だったら食らわない
        {
            return;
        }

        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;

        OVRInput.SetControllerVibration(0.1f, 0.1f, RightCon); //右コントローラを振動
        OVRInput.SetControllerVibration(0.1f, 0.1f, LeftCon); //左コントローラを振動

        // ヒットエフェクトの表示
        Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Instantiate<ParticleSystem>(hitEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            RpcToAll("RpcSetTrigger", "isHit"); //Hitアニメーション
            Debug.Log(damage + "のダメージを食らった!!残りHP" + CurrentStatus.Hp);
        }
    }


    // ---------------------------------------------------------------------
    // 必殺技判定トリガーエンターイベントコール
    // ---------------------------------------------------------------------
    //必殺技を当てた時に呼ばれる
    [StrixRpc]
    void OnChargeAttackHitTriggerEnter(Collider col)
    {
        if (!isLocal)
        {
            return;
        }

        if (col.gameObject.tag == "Enemy")
        {
            var enemy = col.gameObject.GetComponent<EnemyBase>();
            enemy?.OnChargeAttackHit(CurrentStatus.Power);
            chargeAttackHit.SetActive(false);
        }

        //対戦でプレイヤー同士の場合
        if (col.gameObject.tag == "Player")
        {
            var player = col.GetComponent<PlayerController>();
            player?.RpcToAll("OnChargeAttackHit", CurrentStatus.Power);
            attackHit.SetActive(false);
        }

    }


    // ---------------------------------------------------------------------
    // 必殺技が当てられたときの処理
    // ---------------------------------------------------------------------
    [StrixRpc]
    public void OnChargeAttackHit(int damage/*, Vector3 attackPosition*/)
    {
        if (isAvoid == true) //回避中だったら食らわない //ガードは貫通
        {
            return;
        }

        CurrentStatus.Hp -= damage * 5;
        hpBar.value = CurrentStatus.Hp;

        OVRInput.SetControllerVibration(0.2f, 0.1f, RightCon); //右コントローラを振動
        OVRInput.SetControllerVibration(0.2f, 0.1f, LeftCon); //左コントローラを振動

        // ヒットエフェクトの表示
        Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Instantiate<ParticleSystem>(hitEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            RpcToAll("RpcSetTrigger", "isChargeAttackHit"); //必殺技Hitアニメーション
        }
    }

    // ---------------------------------------------------------------------
    // 攻撃アニメーションHitイベントコール
    // ---------------------------------------------------------------------
    void Anim_AttackHit()
    {
        if (!isLocal)
        {
            return;
        }

        // 攻撃判定用オブジェクトを表示.
        attackHit.SetActive(true);
    }


    // ---------------------------------------------------------------------
    // 攻撃アニメーション終了イベントコール
    // ---------------------------------------------------------------------
    void Anim_AttackEnd()
    {
        if (!isLocal)
        {
            return;
        }

        Debug.Log("End");
        // 攻撃判定用オブジェクトを非表示に.
        attackHit.SetActive(false);
        // 攻撃終了.
        isAttack = false;
    }


    // ---------------------------------------------------------------------
    // 剣で攻撃したときの効果音を再生
    // 同時に、斬撃エフェクトも表示
    // ---------------------------------------------------------------------
    void Anim_PlaySwordSound()
    {
        audioSource.PlayOneShot(swordAttackSound);

        // 斬撃エフェクトプレハブをインスタンス化。キャラクターによって出現位置を変更
        Vector3 offsetTransform = new Vector3(0.0f, 0.0f ,0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        if (TitleScene.isSelectEkard)
        {
            offsetTransform = new Vector3(0.0f, 0.5f, 0.8f);
            offsetRotation = Quaternion.Euler(new Vector3(0, 0, 45));
        }
        else if (TitleScene.isSelectAvelyn)
        {
            offsetTransform = new Vector3(0.0f, 0.7f, 0.5f);
            offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        Instantiate<ParticleSystem>(slashEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

    }

    void Anim_PlaySwordSound2()
    {
        audioSource.PlayOneShot(swordAttackSound);
        Destroy(GameObject.Find("myeff_pfb_slash_001(Clone)"));
        Destroy(GameObject.Find("eff_pfb_slash_blue_001(Clone)"));

        // 斬撃エフェクトプレハブをインスタンス化。キャラクターによって出現位置を変更
        Vector3 offsetTransform = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        if (TitleScene.isSelectEkard)
        {
            offsetTransform = new Vector3(0.0f, 0.7f, 0.5f);
            offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (TitleScene.isSelectAvelyn)
        {
            offsetTransform = new Vector3(0.0f, 0.5f, 0.8f);
            offsetRotation = Quaternion.Euler(new Vector3(0, 0, 45));
        }
        Instantiate<ParticleSystem>(slashEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

    }


    void Anim_PlaySwordSound3()
    {

        audioSource.PlayOneShot(swordAttackSound2);
        Destroy(GameObject.Find("myeff_pfb_slash_001(Clone)"));
        Destroy(GameObject.Find("eff_pfb_slash_blue_001(Clone)"));

        // 斬撃エフェクトプレハブをインスタンス化。キャラクターによって出現位置を変更
        Vector3 offsetTransform = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        if (TitleScene.isSelectEkard)
        {
            offsetTransform = new Vector3(0.0f, 0.7f, 0.8f);
            offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (TitleScene.isSelectAvelyn)
        {
            offsetTransform = new Vector3(0.0f, 0.7f, 0.8f);
            offsetRotation = Quaternion.Euler(new Vector3(0, 0, 45));
        }
        Instantiate<ParticleSystem>(slashDoubleEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);
    }


    // ---------------------------------------------------------------------
    // ガードアニメーションエフェクト生成イベントコール
    // ---------------------------------------------------------------------
    void Anim_CreateGuardEffect()
    {
        if (isGuard == false)
        {
            Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.0f);
            Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            Instantiate<ParticleSystem>(barrierEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);
        }
        isGuard = true;
    }


    // ---------------------------------------------------------------------
    // チャージアニメーションエフェクト生成イベントコール
    // ---------------------------------------------------------------------
    void Anim_CreateChargeEffect()
    {
        if (isCharging == false)
        {
            Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.0f);
            Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            Instantiate<ParticleSystem>(chargeEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

            isCharging = true;
        }
    }


    // ---------------------------------------------------------------------
    // 必殺技アニメーションHitイベントコール
    // ---------------------------------------------------------------------
    void Anim_ChargeAttackHit()
    {
        if (!isLocal)
        {
            return;
        }

        // 必殺技あたり判定用オブジェクトを表示.
         chargeAttackHit.SetActive(true);
    }

    // ---------------------------------------------------------------------
    // 必殺技アニメーション終了イベントコール
    // ---------------------------------------------------------------------
    void Anim_ChargeAttackEnd()
    {
        //Vector3 offsetTransform = new Vector3(-1.5f, 4.5f, 2.5f);
        //Quaternion offsetRotation = Quaternion.Euler(new Vector3(-90, 0, 0)); //slash_002を使った場合
        Vector3 offsetTransform = new Vector3(0.0f, 0.7f, 0.0f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Instantiate<ParticleSystem>(chargeAttackEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

        //必殺技当たり判定オブジェクトを動かす
        isChargeAttackEffect = true;

    }


    // ---------------------------------------------------------------------
    // 回避アニメーション終了イベントコール
    // ---------------------------------------------------------------------
    void Anim_AvoidEnd()
    {
        isAvoid = false;
    }

    // ---------------------------------------------------------------------
    // 死亡アニメーション終了イベントコール
    // ---------------------------------------------------------------------
    void Anim_DeathEnd()
    {
        if (!isLocal) { return; }

        // プレイヤーを非表示.
        this.gameObject.SetActive(false);
        // 敵を非表示
        Enemy1.gameObject.SetActive(false);
        Enemy2.gameObject.SetActive(false);
        Enemy3.gameObject.SetActive(false);
        Enemy4.gameObject.SetActive(false);
        // 敵のカウンターをリセット
        //現在の敵を記憶し、敵のカウントをリセット
        GameController.EnemyCounterAtDie = GameController.EnemyCounter;
        GameController.EnemyCounter = 0;
        // ゲームオーバーを表示.
        gameOver.SetActive(true);

    }



    // ---------------------------------------------------------------------
    /// <summary>
    /// 死亡時処理.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnDie()
    {
        if (!isLocal)
        {
            return;
        }

        RpcToAll("RpcSetBool", "isDeath", true); //Deathアニメーション
        

        Debug.Log("死亡しました。");
        isDeath = true;
        GameOverEvent?.Invoke();

        /*
        StopAllCoroutines();
        if (particleObjectList.Count > 0)
        {
            foreach (var obj in particleObjectList) Destroy(obj);
            particleObjectList.Clear();
        }*/
    }


    
    // ---------------------------------------------------------------------
    /// <summary>
    /// リトライ処理.
    /// </summary>
    // ---------------------------------------------------------------------
    public void Retry()
    {
        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // 位置回転を初期位置に戻す.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;

        // HPスライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;

        //攻撃処理の途中でやられた時用
        isAttack = false;
        //死亡アニメーションのリセット
        RpcToAll("RpcSetBool", "isDeath", false); //Deathアニメーション
    }


}