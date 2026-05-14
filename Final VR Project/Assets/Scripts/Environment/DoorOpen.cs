using System.Collections;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public GameObject player;
    public GameObject lDoor, rDoor;
    public string destroyableTag = "DestroyableWeapon";
    private GameObject ammoSpawner;
    [SerializeField] private GameObject Spawner;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ammoSpawner = GameObject.Find("AmmoSpawner");
        Spawner = GameObject.Find("Spawner");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            StartCoroutine(DoorCloseAndOpen());
            Spawner.GetComponent<SpawnManager>().StartNextWave();
        }
    }

    private IEnumerator DoorCloseAndOpen()
    {
        lDoor.transform.Rotate(0, -90, 0);
        rDoor.transform.Rotate(0, 90, 0);
        DestroyWeapons();


        yield return new WaitForSeconds(5);

        Debug.Log("Doors closing");

        lDoor.transform.Rotate(0, 90, 0);
        rDoor.transform.Rotate(0, -90, 0);
        ammoSpawner.GetComponent<AmmoSpawn>().SpawnAmmo();
    }

    private void DestroyWeapons()
    {
        GameObject[] destroyableObjects = GameObject.FindGameObjectsWithTag(destroyableTag);
        foreach (GameObject obj in destroyableObjects)
        {
            Destroy(obj);
        }
    }
}
