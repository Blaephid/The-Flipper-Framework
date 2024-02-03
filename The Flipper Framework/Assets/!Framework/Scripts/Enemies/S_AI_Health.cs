using UnityEngine;
using System.Collections;

public class S_AI_Health : MonoBehaviour {

    public int MaxHealth = 1;
    int HP;

    public GameObject Explosion;
    public S_Spawn_Enemy_Eternal SpawnReference { get; set; }

    public bool destroy = true;

    void Awake()
    {
        HP = MaxHealth;
    }

    public void DealDamage(int Damage)
    {
        HP -= Damage;
        if(HP <= 0)
        {
            if(SpawnReference != null)
            {
                SpawnReference.ResartSpawner();
            }
            GameObject.Instantiate(Explosion, transform.position,Quaternion.identity);

            if(destroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

}
