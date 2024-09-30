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

        // �´� �� ���忡�� ����
        // �Ѿ��� ������ �ƴϰ�, �� �Ѿ��� �÷��̾ �������, �� �÷��̾ ���� �� ȣ���ϴ°�
        if(!pv.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine) // �����ʿ� ���� Hit ����
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
