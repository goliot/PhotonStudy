using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    int dir;

    private void Start() => Destroy(gameObject, 3.5f);

    private void Update() => transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") pv.RPC("DestroyRPC", RpcTarget.AllBuffered);

        // 맞는 쪽 입장에서 생각
        // 총알이 내꺼가 아니고, 그 총알이 플레이어에 닿았으며, 그 플레이어가 나일 때 호출하는것
        if(!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine) // 느린쪽에 맞춰 Hit 판정
        {
            collision.GetComponent<PlayerController>().Hit();
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
