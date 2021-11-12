using UnityEngine;
using Mirror;
using System.Collections;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour
{
    [SyncVar]
    private bool _isDead = false;
    // accessor -> detect the changes in isDead
    public bool isDead { get { return _isDead; } protected set { _isDead = value; } }
    
    [SerializeField]
    private int maxHealth = 100;

    // SyncVar syncs the player current health to all the clients automatically
    [SyncVar]
    private int currentHealth;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    [SerializeField]
    private GameObject deathEffect;

    [SerializeField]
    private GameObject spawnEffect;

    private bool firstSetup = true;

    // Setup called by the local player
    public void SetupPlayer()
    {

        if (isLocalPlayer)
        {
            // Swtich camera
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }

        CmdBroadcastNewPlayerSetup();
    }

    // Let the server know a local player is being setup
    [Command]
    private void CmdBroadcastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    // Let the server update on all clients
    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];

            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }

            firstSetup = false;
        }
        
        SetDefaults();
    }

/*    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(9999);
        }
    }*/

    public void SetDefaults()
    {
        isDead = false;
        currentHealth = maxHealth;

        // Enable componenets
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        // Enable GameObjects
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
        }

        // Enable the collider alongside the components
        Collider col = GetComponent<Collider>();
        if(col != null)
        {
            col.enabled = true;
        }


        // Create spawn effect
        GameObject gfxInstance = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(gfxInstance, 3f);
    }

    // The server runs this method on all connected clients
    [ClientRpc]
    public void RpcTakeDamage(int amount)
    {
        if(isDead)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log(transform.name + " now has " + currentHealth + " health");

        if(currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;

        // Disable components
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        // Disable GameObjects
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        // Disable the collider alongside the components
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        Debug.Log(transform.name + " is DEAD!");

        // Spawn a death effect
        GameObject gfxInstance = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gfxInstance, 3f);

        // Swtich camera
        if(isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        // Respawn if dead
        StartCoroutine(Respawn());
    }

    // Wait a few seconds, then respawn at spawn point
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        SetupPlayer();
        
        Debug.Log(transform.name + " .respawned");
    }
}
