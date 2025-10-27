using NUnit;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;
    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public bool dead;
    public string deathObj;
    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;
    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;
    public HeaderInfo headerInfo;

    public static PlayerController me;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;
        Move();
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
            Attack();
        float mouseX = (Screen.width/2)-Input.mousePosition.x;
        if (mouseX > 0)
        {
            weaponAnim.transform.parent.localScale = new Vector3(-4, 4, 4);
            headerInfo.transform.localScale = new Vector3((float) -0.0025, (float) 0.0025, (float)0.0025);
        }
        else
        {
            weaponAnim.transform.parent.localScale = new Vector3(4, 4, 4);
            headerInfo.transform.localScale = new Vector3((float)0.0025, (float)0.0025, (float)0.0025);
        }
    }

    void Move()
    {
        // get the horizontal and vertical inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        // apply that to our velocity
        rig.linearVelocity = new Vector2(x, y) * moveSpeed;
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        // calculate the direction
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;
        // shoot a raycast in the direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);
        // did we hit an enemy?
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit");
            // get the enemy and damage them
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
        }
        // play attack animation
        weaponAnim.SetTrigger("Attack");
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;
        headerInfo.GetComponent<PhotonView>().RPC("UpdateHealthBar", RpcTarget.All, curHp);
        if (curHp < 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFlash());
            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
    }

    void Die()
    {
        dead = true;
        rig.isKinematic = true;
        GameObject goldPile = PhotonNetwork.Instantiate(deathObj, transform.position, Quaternion.identity);
        goldPile.GetComponent<PhotonView>().RPC("SetGold", RpcTarget.All, gold);
        gold = 0;
        GameUI.instance.UpdateGoldText(gold);
        transform.position = new Vector3(0, 99, 0);
        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;
        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    IEnumerator Spawn(Vector3 spawnPos,float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);
        dead = false;
        transform.position = spawnPos;
        curHp = maxHp;
        rig.isKinematic = false;
        headerInfo.GetComponent<PhotonView>().RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    [PunRPC]
    public void Initialize(Player player)
    {
        GameManager.instance.players[id] = this;
        id = player.ActorNumber;
        photonPlayer = player;
        // initialize the health bar
        if (player.IsLocal)
            me = this;
        else
            rig.isKinematic = true;
        headerInfo.Initialize(player.NickName, maxHp);
    }

    [PunRPC]
    void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);
        headerInfo.GetComponent<PhotonView>().RPC("UpdateHealthBar", RpcTarget.All, curHp);
        // update the health bar
    }

    [PunRPC]
    void GiveGold(int goldToGive)
    {
        gold += goldToGive;
        GameUI.instance.UpdateGoldText(gold);
        // update the ui
    }
}
