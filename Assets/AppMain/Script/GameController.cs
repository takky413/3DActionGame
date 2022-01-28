using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    
    // ゲームオーバーオブジェクト.
    [SerializeField] GameObject gameOver = null;
    // プレイヤー.
    [SerializeField] PlayerController player = null;
    // 敵リスト.
    [SerializeField] List<EnemyBase> enemys = new List<EnemyBase>();

    [SerializeField] GameObject Enemy1 = null; //
    [SerializeField] GameObject Enemy2 = null;
    [SerializeField] GameObject Enemy3 = null;
    [SerializeField] GameObject Enemy4 = null;

    public static int EnemyCounter = 0; //何体目の敵が出ているかをカウントする変数 //EnemyBaseのOnDie()、Retry()関数でも使う
    public static int EnemyCounterAtDie = 0; //味方キャラクターがやられたときに表示していた敵を覚えておく

    // 敵の移動ターゲットリスト.
    //[SerializeField] List<Transform> enemyTargets = new List<Transform>();

    void Start()
    {
        EnemyCounter = 0; //ここでも初期化しておかないと、タイトルに戻って入り直した時に敵が二体出てしまう。

        player.GameOverEvent.AddListener(OnGameOver);

        gameOver.SetActive(false);

        if (TitleScene.isSelectCollaborateMode) {
            Enemy1.SetActive(true); //まず最初の敵を登場させる
            EnemyCounter += 1;
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))//コンティニューする時
        {
            if (PlayerController.isDeath == true)
            {
                OnRetryButtonClicked();

            }
        }

        if (EnemyCounter == 1) //Retryしたときに必要
        {
            Enemy1.gameObject.SetActive(true);
        }
        if (EnemyCounter == 2)
        {
            Enemy2.gameObject.SetActive(true);
        }
        if (EnemyCounter == 3)
        {
            Enemy3.gameObject.SetActive(true);
        }
        if (EnemyCounter == 4)
        {
            Enemy4.gameObject.SetActive(true);
        }

        //Debug.Log(EnemyCounter);
    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// ゲームオーバー時にプレイヤーから呼ばれる.
    /// </summary>
    // ---------------------------------------------------------------------
    void OnGameOver()
    {
        // ゲームオーバーを表示.
        //gameOver.SetActive(true);
        // プレイヤーを非表示.
        //player.gameObject.SetActive(false);
        // 敵の攻撃フラグを解除.
        foreach (EnemyBase enemy in enemys) enemy.isBattle = false;

    }

    // ---------------------------------------------------------------------
    /// <summary>
    /// リトライボタンクリックコールバック.
    /// </summary>
    // ---------------------------------------------------------------------
    public void OnRetryButtonClicked()
    {
        // プレイヤーリトライ処理.
        player.Retry();
        // 敵のリトライ処理.
        foreach (EnemyBase enemy in enemys) enemy.OnRetry();
        // プレイヤーを表示.
        player.gameObject.SetActive(true);
        // ゲームオーバーを非表示.
        gameOver.SetActive(false);

        EnemyCounter = EnemyCounterAtDie; //死ぬ前に表示していた敵を表示する
    }

    /*
    // ---------------------------------------------------------------------
    /// <summary>
    /// リストからランダムにターゲットを取得.
    /// </summary>
    /// <returns> ターゲット. </returns>
    // ---------------------------------------------------------------------
    Transform GetEnemyMoveTarget()
    {
        if (enemyTargets == null || enemyTargets.Count == 0) return null;
        else if (enemyTargets.Count == 1) return enemyTargets[0];

        int num = Random.Range(0, enemyTargets.Count);
        return enemyTargets[num];
    }


    // ---------------------------------------------------------------------
    /// <summary>
    /// 敵に次の目的地を設定.
    /// </summary>
    /// <param name="enemy"> 敵. </param>
    // ---------------------------------------------------------------------
    void EnemyMove(EnemyBase enemy)
    {
        var target = GetEnemyMoveTarget();
        if (target != null) enemy.SetNextTarget(target);
    }
    */

}
