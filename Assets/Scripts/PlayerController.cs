using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
[Serializable]
public class PlayerController : NetworkBehaviour
{
    public Animator PlayerAnimator;
    public SpriteRenderer PlayerRenderer;
    public BoxCollider2D BodyCollider;
    public CircleCollider2D SwordCollider;
    public GameObject HealtBar;
    private float HealtBarMin = 0.0f;
    private float HealtBarMax = 0.3f;
    private int MaxHealt = 100;
    private BoxCollider2D GameArea;
    private int KeyStatus = 0;
    private float Speed = 2.0f;
    private bool Attack = false;
    private bool Run = false;
    private bool PlayerFlip = false;
    private bool Spawn = false;
    private NetworkVariable<int> PlayerHealt = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameArea = GameObject.FindWithTag("GameArea").GetComponent<BoxCollider2D>();
        if (!GameArea)
        {
            Debug.LogAssertion("GameArea not found in Scene");
        }
        Spawn = true;
        PlayerHealt.OnValueChanged += OnPlayerHealtChanged;
        
    }

    private void OnPlayerHealtChanged(int previousValue, int newValue)
    {
        Debug.Log("Old : " + previousValue + " New : " + newValue);
        float HealtBarScaleXPercentage = Math.Max((float)PlayerHealt.Value / MaxHealt,0.0f);
        float HealtBarScaleX = Math.Max(HealtBarMax * HealtBarScaleXPercentage,0);
        HealtBar.transform.localScale = new Vector3(HealtBarScaleX, HealtBar.transform.localScale.y, HealtBar.transform.localScale.z);
    }

    void Update()
    {
        if (!IsOwner || !Spawn)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            KeyStatus++;
            PlayerFlip = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            KeyStatus++;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            KeyStatus++;
            PlayerFlip = false;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            KeyStatus++;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            KeyStatus--;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            KeyStatus--;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            KeyStatus--;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            KeyStatus--;
        }
        Vector3 Movement = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            Movement.x -= Time.deltaTime * Speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Movement.x += Time.deltaTime * Speed;
        }
        if (Input.GetKey(KeyCode.W))
        {
            Movement.y += Time.deltaTime * Speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Movement.y -= Time.deltaTime * Speed;
        }
        Run = KeyStatus > 0;
        PlayerAnimator.SetBool("Run", Run);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwordAttack();
            Attack = true;
            PlayerAnimator.SetBool("Attack", Attack);
        }
        if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            Attack = false;
            PlayerAnimator.SetBool("Attack", Attack);
        }
        if(!Attack)
        {

            if(PlayerFlip)
            {
                this.transform.rotation = new Quaternion(0,180,0,0);
            }
            else
            {
                this.transform.rotation = new Quaternion(0, 0, 0, 0);
            }
            if(GameArea.OverlapPoint(this.transform.position + Movement))
            {
                this.transform.position += Movement;
            }
 
        }
    }
    private void SwordAttack()
    {
        Debug.Log("Start --> \n");
        ContactFilter2D ContactFilter = new ContactFilter2D();
        List<Collider2D> List = new List<Collider2D>();
        SwordCollider.OverlapCollider(ContactFilter, List);
        foreach (Collider2D c in List)
        {
            if(c != GameArea && c != BodyCollider)
            {
                Debug.Log("Hit --> \n");
                PlayerController Temp = c.GetComponent<PlayerController>();
                DamageServerRpc(Temp.OwnerClientId, 10);
            }
        }
    }
    [ServerRpc]
    private void DamageServerRpc(ulong OwnerClientId, int Damage)
    {
        PlayerController[] Players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController p in Players)
        {
            if(p.OwnerClientId == OwnerClientId)
            {
                p.PlayerHealt.Value -= Damage;
            }
        }
    }
}
