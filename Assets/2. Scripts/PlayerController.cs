using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public PhotonView pv;
    public Text nicknameText;
    public Image healthImage;

    bool isGround;
    Vector3 curPos;

    private void Awake()
    {
        nicknameText.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nicknameText.color = pv.IsMine ? Color.green : Color.red;

        if(pv.IsMine)
        {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.Follow = transform;
            cm.LookAt = transform;
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            // 이동
            float axis = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * axis, rb.velocity.y);

            if (axis != 0)
            {
                anim.SetBool("walk", true);
                pv.RPC("FlipXRPC", RpcTarget.AllBuffered, axis); // 재접속시 flipX를 동기화해주기 위한 AllBuffered
            }
            else
            {
                anim.SetBool("walk", false);
            }

            //점프, 바닥 체크
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            anim.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.W) && isGround)
                pv.RPC("JumpRPC", RpcTarget.All);

            // 공격
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(spriteRenderer.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity)
                    .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, spriteRenderer.flipX ? -1 : 1);
                anim.SetTrigger("shot");
            }
        }
        // 내꺼가 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    [PunRPC]
    void FlipXRPC(float axis) => spriteRenderer.flipX = axis == -1;

    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 700);
    }

    public void Hit()
    {
        healthImage.fillAmount -= 0.1f;
        if(healthImage.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            pv.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 위치, 체력 변수 동기화
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
