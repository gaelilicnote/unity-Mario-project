using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;

    public int Ammo;
    public int Coin;
    public int Health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxhasGrenades;


    float hAxis;
    float vAxis;

    bool wDown; // Walk
    bool jDown; // Jump
    bool dDown; // Dodge
    bool fDown; // Attack
    bool tDown;  // throw
    bool rDown; // Reload
    bool iDown; // Interation
    bool sDown1; // Swap
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isBorder;

    Vector3 moveVec;
    Vector3 dodgeVec;


    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Trun();
        Jump();
        Attack();
        Grenade();
        Reload();
        Dodge();
        Swap();
        Interation();
    }


    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        tDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        dDown = Input.GetButtonDown("Dodge");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    //이동
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || (!isFireReady && !isJump))
            moveVec = Vector3.zero;

        if(!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

    }

    void Trun()
    {
        // 1 키보드 회전
        transform.LookAt(transform.position + moveVec);
        
        // 2 마우스 회전
        if (fDown) {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            // out : return처럼 반환값을 주어진 변수에 저장하는 키워드
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                // nextVec.y = 0; 벽 클릭시에도 y축방향 보지않기
                transform.LookAt(transform.position + nextVec);
            }
        }
    }


    //점프
    void Jump()
    {
        if(jDown && !isJump && !isDodge && !isSwap && !isReload)
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }


    //공격
    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isReload)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0)
            return;

        if(tDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 11;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }


    //장전
    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (Ammo == 0)
            return;
        if (equipWeapon.maxAmmo <= equipWeapon.curAmmo)
            return;

        if(rDown && !isDodge && !isSwap && isFireReady && !isReload)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 1.5f);
        }
    }

    void ReloadOut()
    {
        // 장전시 총알이 남아있어도 소비창에 45개면 45개 모두사라지는 버그 발생
        // int reAmmo = Ammo < equipWeapon.maxAmmo ? Ammo : equipWeapon.maxAmmo;
        // equipWeapon.curAmmo = reAmmo;

        int reAmmo = equipWeapon.maxAmmo - equipWeapon.curAmmo;
        if (reAmmo < Ammo)
        {
            Ammo -= reAmmo;
            equipWeapon.curAmmo += reAmmo;
        }
        else if (reAmmo > Ammo)
        {
            equipWeapon.curAmmo += Ammo;
            Ammo -= Ammo;
        }
        isReload = false;
    }


    // 대쉬
    void Dodge()
    {
        if (dDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }


    // 무기교체
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }


    // 무기줍기
    void Interation()
    {
        if(iDown && nearObject != null && !isJump && !isDodge)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }


    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }


    // 벽인식하기
    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 3, Color.green);
        isBorder = Physics.Raycast(transform.position, moveVec, 3, LayerMask.GetMask("Wall"));
    }    

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }


    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    Ammo += item.value;
                    if (Ammo > maxAmmo)
                        Ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    Coin += item.value;
                    if (Coin > maxCoin)
                        Coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    Health += item.value;
                    if (Health > maxHealth)
                        Health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxhasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    break;
            }
            Destroy(other.gameObject);
        }   
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }

}
