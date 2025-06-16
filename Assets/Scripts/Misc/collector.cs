using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

public class collector : MonoBehaviour
{

    public int collection_mask = 0b01;
    public float Collection_Radius = 1.0f;

  
    void Update()
    {


        collect_bones();

    }



    private void collect_everything(bool force = false)
    {
        collect_weapons(force);
        collect_bones(force);
    }


    private void collect_bones(bool force = false)
    {
        Bone[] bones = FindObjectsOfType<Bone>();
        foreach (Bone bone in bones)
        {
            Vector3 disposition = bone.transform.position - transform.position;
            if (disposition.magnitude <= Collection_Radius)
            {
                float telly_time = Mathf.Pow((disposition).magnitude, 0.75f);
                bone.Collect(gameObject, telly_time, (x) => Destroy(x.gameObject));
            }
        }
    }


    private void collect_weapons(bool force = false)
    {
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            Vector3 disposition = weapon.transform.position - transform.position;
            if (disposition.magnitude > Collection_Radius || force)
            {
                if (!weapon.Wielder && !weapon.ImpaledObject && weapon != Player.INSTANCE.HostWeapon)
                {
                    float telly_time = Mathf.Pow((disposition).magnitude, 0.75f);
                    weapon.Telecommute(gameObject, telly_time, (x) => Destroy(x.gameObject));
                }
            }
        }
    }


}
