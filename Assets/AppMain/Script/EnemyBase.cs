using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.AI;

using System.Collections;
using System.Collections.Generic;

using SoftGear.Strix.Client.Core.Auth.Message;
using SoftGear.Strix.Client.Core.Error;
using SoftGear.Strix.Client.Core.Model.Manager.Filter;
using SoftGear.Strix.Client.Core;
using SoftGear.Strix.Unity.Runtime;
using SoftGear.Strix.Net.Logging;
using SoftGear.Strix.Unity.Runtime.Event;

public class EnemyBase : StrixBehaviour
{
    // ----------------------------------------------------------
    /// <summary>
    /// ステータス.
    /// </summary>
    // ----------------------------------------------------------
    [System.Serializable]
    public class Status
    {
        // HP.
        [StrixSyncField] public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }

    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status();

    // アニメーター.
    Animator animator = null;

    // 周辺レーダーコライダーコール.
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;
    // 攻撃判定用コライダーコール.
    [SerializeField] ColliderCallReceiver attackHitColliderCall = null;

    // 攻撃間隔.
    [SerializeField] float attackInterval = 3f;
    // 攻撃状態フラグ.
    public bool isBattle = false;
    // 攻撃時間計測用.
    float attackTimer = 0f;

    //! HPバーのスライダー.
    [SerializeField] Slider hpBar = null;

    // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();

    //キャラクター追跡用
    [SerializeField] [Tooltip("追いかける対象1")] private GameObject player1; //Ekardを入れる
    [SerializeField] [Tooltip("追いかける対象2")] private GameObject player2; //Avelynを入れる
    private NavMeshAgent navMeshAgent;

    [SerializeField] public ParticleSystem hitEffect; //ヒットエフェクト

    void Start()
    {
        // 最初に現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;

        // Animatorを取得し保管.
        animator = GetComponent<Animator>();

        // 周辺コライダーイベント登録.
        aroundColliderCall.TriggerEnterEvent.AddListener(OnAroundTriggerEnter);
        aroundColliderCall.TriggerStayEvent.AddListener(OnAroundTriggerStay);
        aroundColliderCall.TriggerExitEvent.AddListener(OnAroundTriggerExit);

        // 攻撃コライダーイベント登録.
        attackHitColliderCall.TriggerEnterEvent.AddListener(OnAttackTriggerEnter);
        attackHitColliderCall.gameObject.SetActive(false);

        // スライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;

        // 開始時の位置回転を保管.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;

        navMeshAgent = GetComponent<NavMeshAgent>();
    }


    void Update()
    {
        if (!isLocal)
        {
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Appear")) //登場アニメーション中は動かさない
        {
            navMeshAgent.destination = this.transform.position;
            return;
        }

        if (PlayerController.isPause == true)　//ポーズしている時は敵を動かさない
        {
            animator.SetBool("isRun", false);
            //RpcToAll("RpcSetBool", "isRun", false);
            navMeshAgent.destination = this.transform.position; 
            return;
        }

        // 攻撃できる状態の時.
        if (isBattle == true)
        {
            animator.SetBool("isRun", false);
            //RpcToAll("RpcSetBool", "isRun", false);
            navMeshAgent.destination = this.transform.position;

            attackTimer += Time.deltaTime;

            if (attackTimer >= 3f)
            {
                animator.SetTrigger("isAttack");
                //RpcToAll("RpcSetTrigger", "isAttack");
                attackTimer = 0;
            }
        }
        else
        {
            attackTimer = 0;

            // プレイヤーを目指して進む
            if (TitleScene.isSelectEkard == true)
            {
                navMeshAgent.destination = player1.transform.position;
                animator.SetBool("isRun", true);
                //RpcToAll("RpcSetBool", "isRun", true);
            }
            else if (TitleScene.isSelectAvelyn == true)
            {
                navMeshAgent.destination = player2.transform.position;
                animator.SetBool("isRun", true);
                //RpcToAll("RpcSetBool", "isRun", true);
            }

        }

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


    // ------------------------------------------------------------
    // 攻撃コライダーエンターイベントコール
    // ------------------------------------------------------------
    //攻撃を当てた時に呼ばれる
    void OnAttackTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("プレイヤーに敵の攻撃がヒット！" + CurrentStatus.Power + "の力で攻撃！");
            var player = other.GetComponent<PlayerController>();
            player?.OnEnemyAttackHit(CurrentStatus.Power/*, this.transform.position*/);
            attackHitColliderCall.gameObject.SetActive(false);
        }
    }


    // ----------------------------------------------------------
    // 攻撃ヒット時コール
    // ----------------------------------------------------------
    //攻撃を食らった時に呼ばれる
    public void OnAttackHit(int damage/*, Vector3 attackPosition*/)
    {
        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;

        // ヒットエフェクトの表示
        Vector3 offsetTransform = new Vector3(0.0f, 0.5f, 0.5f);
        Quaternion offsetRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Instantiate<ParticleSystem>(hitEffect, this.transform.position + this.transform.rotation * offsetTransform, this.transform.rotation * offsetRotation);

        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            animator.SetTrigger("isHit");
        }
    }

    // ----------------------------------------------------------
    // 必殺技ヒット時コール
    // ----------------------------------------------------------
    //必殺技を食らった時に呼ばれる
    public void OnChargeAttackHit(int damage/*, Vector3 attackPosition*/)
    {
        CurrentStatus.Hp -= damage * 5;
        hpBar.value = CurrentStatus.Hp;

        if (CurrentStatus.Hp <= 0)
        {
            OnDie();
        }
        else
        {
            animator.SetTrigger("isChargeAttackHit");
        }
    }

    // ----------------------------------------------------------
    // 死亡時コール
    // ----------------------------------------------------------
    void OnDie()
    {
        Debug.Log("死亡");
        this.gameObject.SetActive(false);
        animator.SetBool("isDie", true);

        GameController.EnemyCounter += 1;
    }


    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダーエンターイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isBattle = true;
        }
    }

    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダーステイイベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var _dir = (other.gameObject.transform.position - this.transform.position).normalized;
            _dir.y = this.transform.position.y;
            this.transform.forward = _dir;
        }
    }


    // ------------------------------------------------------------
    /// <summary>
    /// 周辺レーダーコライダー終了イベントコール.
    /// </summary>
    /// <param name="other"> 接近コライダー. </param>
    // ------------------------------------------------------------
    void OnAroundTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isBattle = false;
        }
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 死亡アニメーション終了時コール.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_DieEnd()
    {
        this.gameObject.SetActive(false);
    }


    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃Hitアニメーションコール.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive(true);
    }

    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃アニメーション終了時コール.
    /// </summary>
    // ----------------------------------------------------------
    void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive(false);
    }

    
    // ----------------------------------------------------------
    /// <summary>
    /// プレイヤーリトライ時の処理.
    /// </summary>
    // ----------------------------------------------------------
    public void OnRetry()
    {
        // 現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // 開始時の位置回転を保管.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;

        //敵を再度表示
        //this.gameObject.SetActive(true);

        //現在の敵を記憶し、敵のカウントをリセット
        //GameController.EnemyCounterAtDie = GameController.EnemyCounter;
        //GameController.EnemyCounter = 0;
    }

}
