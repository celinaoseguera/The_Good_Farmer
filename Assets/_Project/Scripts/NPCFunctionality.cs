using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputPublisher;
using static PlayerInventory;


public class NPCFunctionality : MonoBehaviour
{
    //NPC requesters
    [SerializeField] InputPublisher inputPublisher;
    private Vector2 npcPos;
    private Vector2 npcPosOffsetY;
    private Vector2 npcPosOffsetXY;

    public event EventHandler OnCropGiven;

    //NPC states
    [SerializeField] GameObject requestToSpawn;
    [SerializeField] GameObject failureToSpawn;
    [SerializeField] GameObject completeToSpawn;
    private bool requestRaised = false;
    private bool requestCompleted = false;
    private int failedNum = 0;
    private GameObject requestSpawned;
    private GameObject failureSpawned;
    private GameObject completeSpawned;
    private int spawnCountFlag = 0;
    private float timer = 0;
    private float requestFailStart = 30;
    private float requestFailEnd = 31;
    private float randomDelay = 0f;
    private float failureTransparency = 0f;
    private float failureTransCount = 0;
    private bool failureStarted = false;


    public event EventHandler OnFailure;
    public event EventHandler OnCompleted;


    //Player
    [SerializeField] PlayerInventory playerInventory;
    private const string PLAYER_TAG = "Player";

    // audio
    [SerializeField] private AudioClip moneySoundClip;
    [SerializeField] private AudioClip failureSoundClip;
    [SerializeField] private AudioClip requestSoundClip;

    // Start is called before the first frame update
    void Start()
    {
        npcPos = this.transform.position;
        npcPosOffsetY = new Vector2 (npcPos.x, npcPos.y + 1.3f);
        npcPosOffsetXY = new Vector2(npcPos.x + .9f, npcPos.y + 1.3f);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(PLAYER_TAG))
        {
            inputPublisher.OnEPressed += GiveCrop;
        }

        return;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(PLAYER_TAG))
        {
            inputPublisher.OnEPressed -= GiveCrop;
        }

        return;
    }

    private void GiveCrop(object sender, EventArgs e)
    {
        if (requestRaised == true && playerInventory.wheatAmount >= 1)
        {
            completeSpawned = Instantiate(completeToSpawn, npcPosOffsetXY, Quaternion.identity);
            StartCoroutine(DelayRequestFade(2f));
            StartCoroutine(DelayRequestCompletedFade(2f));
            requestCompleted = true;
            requestRaised = false;
            Destroy(failureSpawned);
            failureStarted = false;
            failureTransCount = 0f;
            timer = 0;
            spawnCountFlag = 0;
            requestFailStart--;
            requestFailEnd--;
            Debug.Log(requestFailEnd + " for " + gameObject.name);
            SoundFXManager.instance.PlaySoundFXClip(moneySoundClip, transform, 1f);
            OnCompleted?.Invoke(this, EventArgs.Empty);
            OnCropGiven?.Invoke(this, EventArgs.Empty);
        }

    }

    void FailedRequest()

    {
        failedNum++;
        failureSpawned = Instantiate(failureToSpawn, npcPosOffsetXY, Quaternion.identity);
        requestRaised = false;
        failureStarted = false;
        OnFailure?.Invoke(this, EventArgs.Empty);
        SoundFXManager.instance.PlaySoundFXClip(failureSoundClip, transform, 1f);
        StartCoroutine(DelayRequestFailedFade(2f));
        StartCoroutine(DelayRequestFade(2f));
    }

    void RaiseRequest()
    {
        requestSpawned = Instantiate(requestToSpawn, npcPosOffsetY, Quaternion.identity);
        SoundFXManager.instance.PlaySoundFXClip(requestSoundClip, transform, 1f);
        requestCompleted = false;
        requestRaised = true;

    }

    private IEnumerator DelayRequestFade(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(requestSpawned);
    }

    private IEnumerator DelayRequestFailedFade(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(failureSpawned);
    }

    private IEnumerator DelayRequestCompletedFade(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(completeSpawned);
    }

    private IEnumerator DelayForRandTime()
    {
        spawnCountFlag++;
        if (spawnCountFlag < 2)
        {
            // to offset the failed request time to accommodate for the delay
            randomDelay = UnityEngine.Random.Range(3f, 10f);
            timer -= randomDelay;
                
            yield return new WaitForSeconds(randomDelay);

            RaiseRequest();

        }
    }

    void Update()
    {

        timer += Time.deltaTime;
        

            if (timer > 10 && timer < 11)
            {
              StartCoroutine(DelayForRandTime());
            }

            if (timer > requestFailEnd - 7 && timer < requestFailEnd)
            {
                failureTransCount += .00017f;
                if (failureStarted == false)
                {
                    failureSpawned = Instantiate(failureToSpawn, npcPosOffsetXY, Quaternion.identity);
                    failureStarted = true;
                }
                failureSpawned.GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, failureTransparency + failureTransCount);
               }

        if (timer > requestFailStart && timer < requestFailEnd && requestCompleted == false && requestRaised == true)
            {
            // delete old spawn
                Destroy(failureSpawned);
                FailedRequest();
                timer = 0;
                spawnCountFlag = 0;
                failureTransCount = 0f;
            }



    }

}
    
