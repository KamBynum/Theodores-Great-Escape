// Boilerplate from CS4455_M1 project

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioEventManager : MonoBehaviour
{
    private static bool _amplificationDone = false;

    public EventSound3D eventSound3DPrefab;

    public float ambientMinDist = 1f;
    public float ambientMaxDist = 10f;

    ////////////////////////////////////////////////////////////////////////////
    /// Player Audio
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip playerLandsAudio;
    public AudioClip playerStepAudio;
    public AudioClip deathAudio;
    public AudioClip jumpAudio;
    public AudioClip punchAudio;
    public AudioClip squeak1;
    public AudioClip squeak2;
    public AudioClip squeak3;
    public AudioClip squeak4;
    public AudioClip squeak5;
    public AudioClip squeak6;
    public AudioClip grunt1;
    public AudioClip grunt2;
    public AudioClip grunt3;
    public AudioClip grunt4;
    public AudioClip squeakHighImpact;
    public AudioClip superPunchGained;
    public AudioClip superPunchLost;

    private UnityAction<Vector3, float> playerSqueaksEventListener;
    private UnityAction<Vector3, float> playerStepEventListener;
    private UnityAction<Vector3, float> playerGruntsEventListener;
    private UnityAction<Vector3, float> playerFallDamageEventListener;
    private UnityAction<Vector3, float, float> playerLandsEventListener;
    private UnityAction<Vector3> jumpEventListener;
    private UnityAction<GameObject> deathEventListener;
    private UnityAction<Vector3> superPunchGainedEventListener;
    private UnityAction<Vector3> superPunchLostEventListener;


    ////////////////////////////////////////////////////////////////////////////
    /// Game and Misc Audio
    ////////////////////////////////////////////////////////////////////////////

    // Clicks
    public AudioClip click1;
    public AudioClip click2;
    public AudioClip click3;
    public AudioClip click4;
    public AudioClip click5;
    public AudioClip click6;
    public AudioClip click7;
    public AudioClip click8;
    public AudioClip click9;
    public AudioClip click10;
    public AudioClip click11;
    public AudioClip click12;
    public AudioClip click13;
    public AudioClip click14;
    public AudioClip click15;
    public AudioClip click16;

    // Clanks
    public AudioClip metallicClank1;
    public AudioClip metallicClank2;
    public AudioClip metallicClank3;
    public AudioClip metallicClank4;
    public AudioClip metallicClank5;
    
    // Pops
    public AudioClip pop1;
    public AudioClip pop2;
    public AudioClip pop3;
    public AudioClip pop4;

    // Gamestate Sounds
    public AudioClip lossSound1;
    public AudioClip lossSound2;
    public AudioClip victorySound;
    public AudioClip allHoneyCollectedSound;

    // Music
    public AudioClip music1;
    public AudioClip music2;

    // Ambient effects
    public AudioClip[] rooster;
    public AudioClip cow;
    public AudioClip[] bird;
    public AudioClip flies;
    public AudioClip crickets;
    public AudioClip goat;
    public AudioClip hawk;
    public AudioClip[] dirtFalling;



    ////////////////////////////////////////////////////////////////////////////
    /// Game and UI handling
    ////////////////////////////////////////////////////////////////////////////
    private UnityAction<Vector3> clickEventListener;
    private UnityAction<Vector3> gameOverEventListener;
    private UnityAction<Vector3> victoryEventListener;
    private UnityAction<Vector3> allHoneyCollectedEventListener;
    private UnityAction<Vector3> musicEventListener;
    
    ////////////////////////////////////////////////////////////////////////////
    /// BearTrap handling
    ////////////////////////////////////////////////////////////////////////////
    private UnityAction<Vector3> bearTrapTriggerEventListener;
    private UnityAction<Vector3> bearTrapCloseEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// PillBug handling
    ////////////////////////////////////////////////////////////////////////////
    public float PillbugAmplification   = 3f;
    public float PillbugSoundMinRange   = 1f;
    public float PillbugSoundMaxRange   = 10f;
    public float PillbugSoundMaxVolume = 0.9f;
    public AudioClip pillbugClean;
    public AudioClip pillbugPunch;
    public AudioClip pillbugStep;
    public AudioClip pillbugRoll1A;
    public AudioClip pillbugRoll1B;
    public AudioClip pillbugSquash;
    public AudioClip pillbugSqueal;
    
    private UnityAction<Vector3> pillbugCleanEventListener;
    private UnityAction<Vector3> pillbugPunchEventListener;
    private UnityAction<Vector3> pillbugPunchedByPlayerEventListener;
    private UnityAction<Vector3> pillbugRollAEventListener;
    private UnityAction<Vector3> pillbugRollBEventListener;
    private UnityAction<Vector3> pillbugStepEventListener;
    private UnityAction<Vector3> pillbugDeathEventListener;
    private UnityAction<Vector3> pillbugStruggleEventListener;
    
    // FIXME KLUDGE for temporal filtering of step events
    public const int PILLBUG_STEP_PERIOD = 5;
    private int _pillbugStepCounter = 0;

    ////////////////////////////////////////////////////////////////////////////
    /// DizzyBirds handling
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip tweet1;
    public AudioClip tweet2;
    public AudioClip tweet3;

    private UnityAction<Vector3> dizzyBirdTweetEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Pickup handling
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip collectHoney;

    private UnityAction<Vector3> collectHoneyEventListener;
    public AudioClip collectSuperHoney;

    private UnityAction<Vector3> collectSuperHoneyEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Breakable Wall handling
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip[] breakableWallSound = null;

    private UnityAction<Vector3, float> breakableWallEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Spider handling
    ////////////////////////////////////////////////////////////////////////////
    public float SpiderAmplification = 3f;
    public float SpiderSoundMinRange = 1f;
    public float SpiderSoundMaxRange = 30f;
    public float SpiderSoundMaxVolume = 0.9f;
    public AudioClip spiderPunch;
    public AudioClip spiderStep;
    public AudioClip spiderDeath;
    public AudioClip spiderLeap;
    public AudioClip spiderWebShot;


    private UnityAction<Vector3> spiderLeapEventListener;
    private UnityAction<Vector3> spiderPunchEventListener;
    private UnityAction<Vector3> spiderPunchedByPlayerEventListener;
    private UnityAction<Vector3> spiderStepEventListener;
    private UnityAction<Vector3> spiderDeathEventListener;
    private UnityAction<Vector3> spiderWebShotEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Collapsing Platform Handling
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip collapse;

    private UnityAction<Vector3> platformCollapseEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Cloud Handling
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip cloudBounce;

    private UnityAction<Vector3> cloudHitEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Hay Bale Handling
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip hayBaleHit;

    private UnityAction<Vector3> hayBaleHitEventListener;

    ////////////////////////////////////////////////////////////////////////////
    /// Misc
    ////////////////////////////////////////////////////////////////////////////
    public AudioClip[] woodHit;

    private UnityAction<Vector3, float> woodHitEventListener;

    private UnityAction<Vector3> birdCallEventListener;
    private UnityAction<Vector3> cricketEventListener;
    private UnityAction<Vector3> dirtFallEventListener;
    private UnityAction<Vector3> flyBuzzEventListener;
    private UnityAction<Vector3> goatEventListener;
    private UnityAction<Vector3> hawkEventListener;
    private UnityAction<Vector3> mooEventListener;
    private UnityAction<Vector3> roosterEventListener;


    void Awake()
    {

        playerGruntsEventListener       = new UnityAction<Vector3, float>(playerGruntsEventHandler);
        playerStepEventListener         = new UnityAction<Vector3, float>(playerStepEventHandler);
        playerSqueaksEventListener      = new UnityAction<Vector3, float>(playerSqueaksEventHandler);
        playerFallDamageEventListener   = new UnityAction<Vector3, float>(playerFallDamageEventHandler);
        playerLandsEventListener        = new UnityAction<Vector3, float, float>(playerLandsEventHandler);
        jumpEventListener               = new UnityAction<Vector3>(jumpEventHandler);
        deathEventListener              = new UnityAction<GameObject>(deathEventHandler);
        superPunchGainedEventListener   = new UnityAction<Vector3>(superPunchGainedEventHandler);
        superPunchLostEventListener     = new UnityAction<Vector3>(superPunchLostEventHandler);


        ////////////////////////////////////////////////////////////////////////////
        /// Game and UI handling
        ////////////////////////////////////////////////////////////////////////////
        clickEventListener                  = new UnityAction<Vector3>(clickEventHandler);
        gameOverEventListener               = new UnityAction<Vector3>(gameOverEventHandler);
        victoryEventListener                = new UnityAction<Vector3>(victoryEventHandler);
        allHoneyCollectedEventListener      = new UnityAction<Vector3>(allHoneyCollectedHandler);
        musicEventListener                  = new UnityAction<Vector3>(musicHandler);

        ////////////////////////////////
        // BearTrap handling
        ////////////////////////////////
        bearTrapTriggerEventListener    = new UnityAction<Vector3>(bearTrapTriggerEventHandler);
        bearTrapCloseEventListener      = new UnityAction<Vector3>(bearTrapCloseEventHandler);

        ////////////////////////////////
        // PillBug handling
        ////////////////////////////////
        pillbugCleanEventListener           = new UnityAction<Vector3>(pillbugCleanEventHandler);
        pillbugPunchEventListener           = new UnityAction<Vector3>(pillbugPunchEventHandler);
        pillbugStepEventListener            = new UnityAction<Vector3>(pillbugStepEventHandler);
        pillbugRollAEventListener           = new UnityAction<Vector3>(pillbugRollAEventHandler);
        pillbugRollBEventListener           = new UnityAction<Vector3>(pillbugRollBEventHandler);
        pillbugPunchedByPlayerEventListener = new UnityAction<Vector3>(pillbugPunchedByPlayerEventHandler);
        pillbugDeathEventListener           = new UnityAction<Vector3>(pillbugDeathEventHandler);
        pillbugStruggleEventListener        = new UnityAction<Vector3>(pillbugStruggleEventHandler);

        ////////////////////////////////
        // DizzyBird handling
        ////////////////////////////////
        dizzyBirdTweetEventListener         = new UnityAction<Vector3>(dizzyBirdTweetEventHandler);

        
        ////////////////////////////////////////////////////////////////////////////
        /// Pickup handling
        ////////////////////////////////////////////////////////////////////////////
        collectHoneyEventListener           = new UnityAction<Vector3>(collectHoneyEventHandler);
        collectSuperHoneyEventListener      = new UnityAction<Vector3>(collectSuperHoneyEventHandler);

        ////////////////////////////////////////////////////////////////////////////
        /// Breakable Wall handling
        ////////////////////////////////////////////////////////////////////////////
        breakableWallEventListener          = new UnityAction<Vector3, float>(breakableWallEventHandler);

        ////////////////////////////////
        // Spider handling
        ////////////////////////////////
        spiderLeapEventListener = new UnityAction<Vector3>(spiderLeapEventHandler);
        spiderPunchEventListener = new UnityAction<Vector3>(spiderPunchEventHandler);
        spiderStepEventListener = new UnityAction<Vector3>(spiderStepEventHandler);
        spiderPunchedByPlayerEventListener = new UnityAction<Vector3>(spiderPunchedByPlayerEventHandler);
        spiderDeathEventListener = new UnityAction<Vector3>(spiderDeathEventHandler);
        spiderWebShotEventListener = new UnityAction<Vector3>(spiderWebShotEventHandler);

        ////////////////////////////////////////////////////////////////////////////
        /// Collapsing Platform Handling
        ////////////////////////////////////////////////////////////////////////////
        platformCollapseEventListener       = new UnityAction<Vector3>(platformCollapseEventHandler);

        ////////////////////////////////////////////////////////////////////////////
        /// Cloud Handling
        ////////////////////////////////////////////////////////////////////////////
        cloudHitEventListener               = new UnityAction<Vector3>(cloudHitEventHandler);

        ////////////////////////////////////////////////////////////////////////////
        /// Hay Bale Handling
        ////////////////////////////////////////////////////////////////////////////
        hayBaleHitEventListener             = new UnityAction<Vector3>(hayBaleHitEventHandler);

        ////////////////////////////////////////////////////////////////////////////
        /// Misc
        ////////////////////////////////////////////////////////////////////////////
        woodHitEventListener                = new UnityAction<Vector3, float>(woodHitEventHandler);

        birdCallEventListener               = new UnityAction<Vector3>(birdCallEventHandler);
        cricketEventListener                = new UnityAction<Vector3>(cricketEventHandler);
        dirtFallEventListener               = new UnityAction<Vector3>(dirtFallEventHandler);
        flyBuzzEventListener                = new UnityAction<Vector3>(flyBuzzEventHandler);
        goatEventListener                   = new UnityAction<Vector3>(goatEventHandler);
        hawkEventListener                   = new UnityAction<Vector3>(hawkEventHandler);
        mooEventListener                    = new UnityAction<Vector3>(mooEventHandler);
        roosterEventListener                = new UnityAction<Vector3>(roosterEventHandler);

        ////////////////////////////////////////////////////////////////////////////
        // KLUDGE Make sure we only do this once per game session!
        if (!_amplificationDone)
        {
            AmplifySounds();
            _amplificationDone = true;
        }
        // Do not put anything after this in the Awake() method. Put things before the AmplifySounds() usage.
    }


    // Use this for initialization
    void Start()
    {
    }

    private void AmplifySounds()
    {
        // Pillbug sound clean up -- I recorded things too quietly...
        AmplifyClip(pillbugClean,   2f*PillbugAmplification);
        AmplifyClip(pillbugPunch,   0.5f*PillbugAmplification);
        AmplifyClip(pillbugStep,    PillbugAmplification);
        AmplifyClip(pillbugRoll1A,  PillbugAmplification);
        AmplifyClip(pillbugRoll1B,  PillbugAmplification);
        AmplifyClip(pillbugSquash,  4f*PillbugAmplification);
        AmplifyClip(pillbugSqueal,  PillbugAmplification);

        // Pop cleanup 
        AmplifyClip(pop1, 3f);
        AmplifyClip(pop2, 3f);
        AmplifyClip(pop3, 3f);
        AmplifyClip(pop4, 3f);

        // Clank cleanup
        AmplifyClip(metallicClank1, 3f);
        AmplifyClip(metallicClank2, 3f);
        AmplifyClip(metallicClank3, 3f);
        AmplifyClip(metallicClank4, 3f);
        AmplifyClip(metallicClank5, 3f);
        
    }

    // This is a hack where we can load the data, mod the data, and put it back. This
    // is really gross, but I don't want to export everything from Audacity again... This is
    // taken from answer here: http://answers.unity.com/answers/1418207/view.html
    void AmplifyClip(AudioClip clip, float amp)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        for (int idx = 0; idx < samples.Length; idx++)
        { 
            samples[idx] = samples[idx] * amp;
        } 
        clip.SetData(samples, 0);
    }


    void OnEnable()
    {
        EventManager.StartListening<PlayerGruntsEvent,      Vector3, float>(playerGruntsEventListener);
        EventManager.StartListening<PlayerSqueaksEvent,     Vector3, float>(playerSqueaksEventListener);
        EventManager.StartListening<PlayerFallDamageEvent,  Vector3, float>(playerFallDamageEventListener);
        EventManager.StartListening<PlayerLandsEvent,       Vector3, float, float>(playerLandsEventListener);
        EventManager.StartListening<JumpEvent,              Vector3>(jumpEventListener);
        EventManager.StartListening<DeathEvent,             GameObject>(deathEventListener);
        EventManager.StartListening<SuperPunchGainedEvent,  Vector3>(superPunchGainedEventListener);
        EventManager.StartListening<SuperPunchLostEvent,    Vector3>(superPunchLostEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Game and UI handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StartListening<ClickEvent,                     Vector3>(clickEventListener);
        EventManager.StartListening<GameOverEvent,                  Vector3>(gameOverEventListener);
        EventManager.StartListening<VictoryEvent,                   Vector3>(victoryEventListener);
        EventManager.StartListening<AllHoneyOnLevelEvent,           Vector3>(allHoneyCollectedEventListener);
        EventManager.StartListening<MusicStartEvent,                Vector3>(musicEventListener);

        ////////////////////////////////
        // BearTrap handling
        ////////////////////////////////
        EventManager.StartListening<BearTrapTriggerEvent,           Vector3>(bearTrapTriggerEventListener);
        EventManager.StartListening<BearTrapCloseEvent,             Vector3>(bearTrapCloseEventListener);

        ////////////////////////////////
        // PillBug Handling
        ////////////////////////////////
        EventManager.StartListening<PillBugCleanEvent,              Vector3>(pillbugCleanEventListener);
        EventManager.StartListening<PillBugPunchEvent,              Vector3>(pillbugPunchEventListener);
        EventManager.StartListening<PillBugPunchedByPlayerEvent,    Vector3>(pillbugPunchedByPlayerEventListener);
        EventManager.StartListening<PillBugRollAEvent,              Vector3>(pillbugRollAEventListener);
        EventManager.StartListening<PillBugRollBEvent,              Vector3>(pillbugRollBEventListener);
        EventManager.StartListening<PillBugStepEvent,               Vector3>(pillbugStepEventListener);
        EventManager.StartListening<PillBugDeathEvent,              Vector3>(pillbugDeathEventListener);
        EventManager.StartListening<PillBugStruggleEvent,           Vector3>(pillbugStruggleEventListener);

        ////////////////////////////////
        // DizzyBird Handling
        ////////////////////////////////
        EventManager.StartListening<DizzyBirdTweetEvent,            Vector3>(dizzyBirdTweetEventListener);

        ////////////////////////////////
        // Pickup Handling
        ////////////////////////////////
        EventManager.StartListening<CollectHoneyEvent,              Vector3>(collectHoneyEventListener);
        EventManager.StartListening<CollectSuperHoneyEvent,         Vector3>(collectSuperHoneyEventListener);

        ////////////////////////////////
        // Breakable Wall Handling
        ////////////////////////////////
        EventManager.StartListening<WallBreakEvent,                 Vector3, float>(breakableWallEventListener);

        ////////////////////////////////
        // Spider Handling
        ////////////////////////////////
        EventManager.StartListening<SpiderLeapEvent, Vector3>(spiderLeapEventListener);
        EventManager.StartListening<SpiderPunchEvent, Vector3>(spiderPunchEventListener);
        EventManager.StartListening<SpiderPunchedByPlayerEvent, Vector3>(spiderPunchedByPlayerEventListener);
        EventManager.StartListening<SpiderStepEvent, Vector3>(spiderStepEventListener);
        EventManager.StartListening<SpiderDeathEvent, Vector3>(spiderDeathEventListener);
        EventManager.StartListening<SpiderWebAttackEvent, Vector3>(spiderWebShotEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Collapsing Platform Handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StartListening<PlatformCollapseEvent, Vector3>(platformCollapseEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Cloud Handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StartListening<CloudHitEvent,  Vector3>(cloudHitEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Hay Bale Handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StartListening<HayBaleHitEvent, Vector3>(hayBaleHitEventListener);

        ////////////////////////////////
        // Misc Handling
        ////////////////////////////////
        EventManager.StartListening<WoodHitEvent,   Vector3, float>(woodHitEventListener);

        EventManager.StartListening<BirdCallEvent,  Vector3>(birdCallEventListener  );
        EventManager.StartListening<CricketEvent,   Vector3>(cricketEventListener   );
        EventManager.StartListening<DirtFallEvent,  Vector3>(dirtFallEventListener  );
        EventManager.StartListening<FlyBuzzEvent,   Vector3>(flyBuzzEventListener   );
        EventManager.StartListening<GoatEvent,      Vector3>(goatEventListener      );
        EventManager.StartListening<HawkEvent,      Vector3>(hawkEventListener      );
        EventManager.StartListening<MooEvent,       Vector3>(mooEventListener       );
        EventManager.StartListening<RoosterEvent,   Vector3>(roosterEventListener   );
    }

    void OnDisable()
    {
        EventManager.StopListening<PlayerGruntsEvent,       Vector3, float>(playerGruntsEventListener);
        EventManager.StopListening<PlayerSqueaksEvent,      Vector3, float>(playerSqueaksEventListener);
        EventManager.StopListening<PlayerFallDamageEvent,   Vector3, float>(playerFallDamageEventListener);
        EventManager.StopListening<PlayerLandsEvent,        Vector3, float, float>(playerLandsEventListener);
        EventManager.StopListening<JumpEvent,               Vector3>(jumpEventListener);
        EventManager.StopListening<DeathEvent,              GameObject>(deathEventListener);
        EventManager.StopListening<SuperPunchGainedEvent,   Vector3>(superPunchGainedEventListener);
        EventManager.StopListening<SuperPunchLostEvent,     Vector3>(superPunchLostEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Game and UI handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StopListening<ClickEvent,                      Vector3>(clickEventListener);
        EventManager.StopListening<GameOverEvent,                   Vector3>(gameOverEventListener);
        EventManager.StopListening<VictoryEvent,                    Vector3>(victoryEventListener);
        EventManager.StopListening<AllHoneyOnLevelEvent,            Vector3>(allHoneyCollectedEventListener);
        EventManager.StopListening<MusicStartEvent,                 Vector3>(musicEventListener);

        ////////////////////////////////
        // BearTrap handling
        ////////////////////////////////
        EventManager.StopListening<BearTrapTriggerEvent,            Vector3>(bearTrapTriggerEventListener);
        EventManager.StopListening<BearTrapCloseEvent,              Vector3>(bearTrapCloseEventListener);

        ////////////////////////////////
        // PillBug Handling
        ////////////////////////////////
        EventManager.StopListening<PillBugCleanEvent,               Vector3>(pillbugCleanEventListener);
        EventManager.StopListening<PillBugPunchEvent,               Vector3>(pillbugPunchEventListener);
        EventManager.StopListening<PillBugPunchedByPlayerEvent,     Vector3>(pillbugPunchedByPlayerEventListener);
        EventManager.StopListening<PillBugRollAEvent,               Vector3>(pillbugRollAEventListener);
        EventManager.StopListening<PillBugRollBEvent,               Vector3>(pillbugRollBEventListener);
        EventManager.StopListening<PillBugStepEvent,                Vector3>(pillbugStepEventListener);
        EventManager.StopListening<PillBugDeathEvent,               Vector3>(pillbugDeathEventListener);
        EventManager.StopListening<PillBugStruggleEvent,            Vector3>(pillbugStruggleEventListener);

        ////////////////////////////////
        // DizzyBird Handling
        ////////////////////////////////
        EventManager.StopListening<DizzyBirdTweetEvent,             Vector3>(dizzyBirdTweetEventListener);

        ////////////////////////////////
        // Pickup Handling
        ////////////////////////////////
        EventManager.StopListening<CollectHoneyEvent,               Vector3>(collectHoneyEventListener);
        EventManager.StopListening<CollectSuperHoneyEvent,          Vector3>(collectSuperHoneyEventListener);

        ////////////////////////////////
        // Breakable Wall Handling
        ////////////////////////////////
        EventManager.StopListening<WallBreakEvent,                  Vector3, float>(breakableWallEventListener);

        ////////////////////////////////
        // Spider Handling
        ////////////////////////////////
        EventManager.StopListening<SpiderLeapEvent, Vector3>(spiderLeapEventListener);
        EventManager.StopListening<SpiderPunchEvent, Vector3>(spiderPunchEventListener);
        EventManager.StopListening<SpiderPunchedByPlayerEvent, Vector3>(spiderPunchedByPlayerEventListener);
        EventManager.StopListening<SpiderStepEvent, Vector3>(spiderStepEventListener);
        EventManager.StopListening<SpiderDeathEvent, Vector3>(spiderDeathEventListener);
        EventManager.StopListening<SpiderWebAttackEvent, Vector3>(spiderWebShotEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Collapsing Platform Handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StopListening<PlatformCollapseEvent, Vector3>(platformCollapseEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Cloud Handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StopListening<CloudHitEvent,   Vector3>(cloudHitEventListener);

        ////////////////////////////////////////////////////////////////////////////
        /// Hay Bale Handling
        ////////////////////////////////////////////////////////////////////////////
        EventManager.StopListening<HayBaleHitEvent, Vector3>(hayBaleHitEventListener);
        
        ////////////////////////////////
        // Misc Handling
        ////////////////////////////////
        EventManager.StopListening<WoodHitEvent,    Vector3, float>(woodHitEventListener);

        EventManager.StopListening<BirdCallEvent,   Vector3>(birdCallEventListener  );
        EventManager.StopListening<CricketEvent,    Vector3>(cricketEventListener   );
        EventManager.StopListening<DirtFallEvent,   Vector3>(dirtFallEventListener  );
        EventManager.StopListening<FlyBuzzEvent,    Vector3>(flyBuzzEventListener   );
        EventManager.StopListening<GoatEvent,       Vector3>(goatEventListener      );
        EventManager.StopListening<HawkEvent,       Vector3>(hawkEventListener      );
        EventManager.StopListening<MooEvent,        Vector3>(mooEventListener       );
        EventManager.StopListening<RoosterEvent,    Vector3>(roosterEventListener   );
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Random AudioClip Helpers
    ////////////////////////////////////////////////////////////////////////////////////////////////
    private AudioClip GetRandomMusic()
    {
        Debug.Log("Play");
        switch (Random.Range(0, 2))
        {
            case 0:
                return music1;
            case 1:
                return music2;
            default:
                Debug.LogError("Unsupported value.");
                return music1;
        }
    }
    private AudioClip GetRandomClick()
    {
        switch (Random.Range(0,16))
        {
            case 0:
                return click1;
            case 1:
                return click2;
            case 2:
                return click3;
            case 3:
                return click4;
            case 4:
                return click5;
            case 5:
                return click6;
            case 6:
                return click7;
            case 7:
                return click8;
            case 8:
                return click9;
            case 9:
                return click10;
            case 10:
                return click11;
            case 11:
                return click12;
            case 12:
                return click13;
            case 13:
                return click14;
            case 14:
                return click15;
            case 15:
                return click16;
            default:
                Debug.LogError("Unsupported value.");
                return click1;
        }
    }
    private AudioClip GetRandomMetallicClank()
    {
        switch (Random.Range(0,5))
        {
            case 0:
                return metallicClank1;
            case 1:
                return metallicClank2;
            case 2:
                return metallicClank3;
            case 3:
                return metallicClank4;
            case 4:
                return metallicClank5;
            default:
                Debug.LogError("Unsupported value.");
                return metallicClank1;
        }
    }
    private AudioClip GetRandomPop()
    {
        switch (Random.Range(0,4))
        {
            case 0:
                return pop1;
            case 1:
                return pop2;
            case 2:
                return pop3;
            case 3:
                return pop4;
            default:
                Debug.LogError("Unsupported value.");
                return pop1;
        }
    }
    
    private AudioClip GetRandomTweet()
    {
        switch (Random.Range(0,3))
        {
            case 0:
                return tweet1;
            case 1:
                return tweet2;
            case 2:
                return tweet3;
            default:
                Debug.LogError("Unsupported value.");
                return tweet1;
        }
    }

    private AudioClip GetRandomSqueak()
    {
        switch (Random.Range(0,6))
        {
            case 0:
                return squeak1;
            case 1:
                return squeak2;
            case 2:
                return squeak3;
            case 3:
                return squeak4;
            case 4:
                return squeak5;
            case 5:
                return squeak6;
            default:
                Debug.LogError("Unsupported value.");
                return squeak1;
        }
    }
    
    private AudioClip GetRandomGrunt()
    {
        switch (Random.Range(0,4))
        {
            case 0:
                return grunt1;
            case 1:
                return grunt2;
            case 2:
                return grunt3;
            case 3:
                return grunt4;
            default:
                Debug.LogError("Unsupported value.");
                return grunt1;
        }
    }

    private AudioClip GetRandomWoodHit()
    {
        return woodHit[Random.Range(0,woodHit.Length)];
    }

    private AudioClip GetRandomDirtFall()
    {
        return dirtFalling[Random.Range(0, dirtFalling.Length)];
    }

    private AudioClip GetRandomBird()
    {
        return bird[Random.Range(0, bird.Length)];
    }

    private AudioClip GetRandomRooster()
    {
        return rooster[Random.Range(0, rooster.Length)];
    }

    ///////////////////////////

    void musicHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, GameManager.Instance.transform.position, Quaternion.identity, null);

            snd.audioSrc.clip = music1;
            snd.audioSrc.volume = 0.7f * 1f/16f;
            snd.audioSrc.minDistance = 1f;
            snd.audioSrc.maxDistance = 100f;
            snd.audioSrc.loop = true;
            snd.transform.parent = GameManager.Instance.transform;
            snd.audioSrc.Play();
        }
    }

    void clickEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
                EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

                snd.audioSrc.clip = GetRandomClick();

                snd.audioSrc.minDistance = 5f;
                snd.audioSrc.maxDistance = 100f;

                snd.audioSrc.Play();
        }
    }
    void gameOverEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip = lossSound1;

            snd.audioSrc.volume      = 0.5f;
            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }
    void victoryEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip = victorySound;
            snd.audioSrc.volume      = 0.5f;
            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }
    void allHoneyCollectedHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip = allHoneyCollectedSound;

            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }

    void playerSqueaksEventHandler(Vector3 worldPos, float scalar)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
            snd.audioSrc.clip = GetRandomSqueak();
            snd.audioSrc.volume = 0.5f;
            snd.audioSrc.pitch = 1.5f - scalar * 0.5f;

            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }

    void playerGruntsEventHandler(Vector3 worldPos, float scalar)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
            snd.audioSrc.clip = GetRandomGrunt();
            snd.audioSrc.volume = 0.5f;
            snd.audioSrc.pitch = 1.8f - scalar * 0.5f;

            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }

    void playerFallDamageEventHandler(Vector3 worldPos, float scalar)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
            snd.audioSrc.clip = squeakHighImpact;
            snd.audioSrc.volume = 0.5f;
            snd.audioSrc.pitch = 1.5f - scalar * 0.5f;

            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }

    void playerLandsEventHandler(Vector3 worldPos, float playerScale, float collisionMagnitude)
    {
        //AudioSource.PlayClipAtPoint(this.explosionAudio, worldPos, 1f);
        if (eventSound3DPrefab)
        {
            // FIXME the denom should be made public or something for class-global tuning across other impact velocity effects too
            const float maxFallSpeed = 20f;
            float volumeCoeff = collisionMagnitude / maxFallSpeed;

            if (collisionMagnitude > 1f)
            {
                EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

                snd.audioSrc.clip = this.playerLandsAudio;
                snd.audioSrc.pitch = 1.8f - playerScale * 0.5f;
                snd.audioSrc.volume = Mathf.Lerp(0.05f, 0.5f, volumeCoeff);
                snd.audioSrc.minDistance = 5f;
                snd.audioSrc.maxDistance = 100f;

                snd.audioSrc.Play();

                float gruntThreshold = 10f;
                if (collisionMagnitude > gruntThreshold)
                {
                    EventSound3D snd2 = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

                    float gruntVolumeCoeff = (collisionMagnitude - gruntThreshold) / (maxFallSpeed - gruntThreshold);
                    gruntVolumeCoeff *= gruntVolumeCoeff; // Scale as quadratic

                    snd2.audioSrc.clip = GetRandomGrunt();
                    snd2.audioSrc.pitch = 1.8f - playerScale * 0.5f;
                    snd2.audioSrc.volume = Mathf.Lerp(0.1f, 1f, gruntVolumeCoeff);
                    snd2.audioSrc.minDistance = 5f;
                    snd2.audioSrc.maxDistance = 100f;

                    snd2.audioSrc.Play();
                }
            }
        }
    }
    
    void playerStepEventHandler(Vector3 worldPos, float playerScale)
    {
        //AudioSource.PlayClipAtPoint(this.explosionAudio, worldPos, 1f);
        if (eventSound3DPrefab)
        {
                float volumeCoeff = playerScale;
            if(playerScale < 0.65f)
            {

            
                EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

                snd.audioSrc.clip = this.playerStepAudio;
                snd.audioSrc.pitch = 1.8f - playerScale * 0.5f;
                snd.audioSrc.volume = Mathf.Lerp(0.05f, 0.5f, volumeCoeff);
                snd.audioSrc.minDistance = 5f;
                snd.audioSrc.maxDistance = 100f;

                snd.audioSrc.Play();
            }
            else
            {
                EventSound3D snd2 = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);


                snd2.audioSrc.clip = this.playerStepAudio;
                snd2.audioSrc.pitch = 1.8f - playerScale * 0.5f;
                snd2.audioSrc.volume = Mathf.Lerp(0.1f, 1f, volumeCoeff);
                snd2.audioSrc.minDistance = 5f;
                snd2.audioSrc.maxDistance = 100f;

                snd2.audioSrc.Play();
            }
        }
    }

    void jumpEventHandler(Vector3 worldPos)
    {
        //AudioSource.PlayClipAtPoint(this.explosionAudio, worldPos, 1f);

        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip = this.jumpAudio;

            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }

    void deathEventHandler(GameObject go)
    {
        //AudioSource.PlayClipAtPoint(this.explosionAudio, worldPos, 1f);

        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, go.transform);

            snd.audioSrc.clip = this.deathAudio;

            snd.audioSrc.minDistance = 5f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }

    void superPunchGainedEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip           = superPunchGained;
            snd.audioSrc.volume         = 0.2f;

            snd.audioSrc.Play();
        }
    }

    void superPunchLostEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.clip           = superPunchLost;
            snd.audioSrc.volume         = 0.2f;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// BEARTRAP HANDLING
    ////////////////////////////////////////////////////////////////////////////
    void bearTrapTriggerEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = GetRandomClick(); // TODO might be better to use a single recognizable click sound for UX
            snd.audioSrc.volume         = 1f;
            snd.audioSrc.minDistance    = 1f;
            snd.audioSrc.maxDistance    = 20f;

            snd.audioSrc.Play();
        }
    }

    void bearTrapCloseEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = GetRandomMetallicClank();
            snd.audioSrc.volume         = 1f;
            snd.audioSrc.minDistance    = 1f;
            snd.audioSrc.maxDistance    = 20f;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// PILLBUG HANDLING
    ////////////////////////////////////////////////////////////////////////////
    void pillbugCleanEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = this.pillbugClean;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void pillbugPunchEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.rolloffMode    = AudioRolloffMode.Logarithmic;
            snd.audioSrc.clip           = this.pillbugPunch;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }
    
    void pillbugRollAEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = this.pillbugRoll1A;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }
    void pillbugRollBEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = this.pillbugRoll1B;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void pillbugStepEventHandler(Vector3 worldPos)
    {
        _pillbugStepCounter = (_pillbugStepCounter+1) % PILLBUG_STEP_PERIOD;

        if (_pillbugStepCounter != 0)
        {
            return;
        }

        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.rolloffMode    = AudioRolloffMode.Logarithmic;
            snd.audioSrc.clip           = this.pillbugStep;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = 3*PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void pillbugPunchedByPlayerEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = GetRandomPop();
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void pillbugDeathEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = this.pillbugSquash;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }
    void pillbugStruggleEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = this.pillbugSqueal;
            snd.audioSrc.volume         = PillbugSoundMaxVolume;
            snd.audioSrc.minDistance    = PillbugSoundMinRange;
            snd.audioSrc.maxDistance    = PillbugSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// DIZZYBIRDS HANDLING
    ////////////////////////////////////////////////////////////////////////////
    void dizzyBirdTweetEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = GetRandomTweet();
            snd.audioSrc.volume         = 1f;
            snd.audioSrc.minDistance    = 1f;
            snd.audioSrc.maxDistance    = 20f;
            snd.audioSrc.pitch          += Random.Range(-0.125f,0.125f);
            snd.audioSrc.Play();
        }
    }
    ////////////////////////////////////////////////////////////////////////////
    /// PICKUP HANDLING
    ////////////////////////////////////////////////////////////////////////////

    void collectHoneyEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            //snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = collectHoney; 
            //snd.audioSrc.volume = 2f;
            snd.audioSrc.minDistance = 1f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }
    
    void collectSuperHoneyEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {

            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            //snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = collectHoney; 
            //snd.audioSrc.volume = 2f;
            snd.audioSrc.minDistance = 1f;
            snd.audioSrc.maxDistance = 100f;

            snd.audioSrc.Play();
        }
    }
    ////////////////////////////////////////////////////////////////////////////
    /// BREAKABLE WALL HANDLING
    ////////////////////////////////////////////////////////////////////////////
    void breakableWallEventHandler(Vector3 worldPos, float impactForce)
    {
        const float halfSpeedRange = 0.2f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.volume = 1f/4f;
        snd.audioSrc.clip = breakableWallSound[Random.Range(0, breakableWallSound.Length)];
        snd.audioSrc.pitch = Random.Range(1f - halfSpeedRange, 1f + halfSpeedRange);

        snd.audioSrc.minDistance = Mathf.Lerp(1f, 8f, impactForce / 200f);
        snd.audioSrc.maxDistance = 100f;

        snd.audioSrc.Play();
    }

    ////////////////////////////////////////////////////////////////////////////
    /// PILLBUG HANDLING
    ////////////////////////////////////////////////////////////////////////////
    void spiderLeapEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = this.spiderLeap;
            snd.audioSrc.volume = 1f; //SpiderSoundMaxVolume;
            snd.audioSrc.minDistance = 2f*SpiderSoundMinRange;
            snd.audioSrc.maxDistance = SpiderSoundMaxRange;

            snd.audioSrc.Play();
        }
    }
    void spiderPunchEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = this.spiderPunch;
            snd.audioSrc.volume = 0.5f*SpiderSoundMaxVolume;
            snd.audioSrc.minDistance = SpiderSoundMinRange;
            snd.audioSrc.maxDistance = SpiderSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void spiderStepEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = this.spiderStep;
            snd.audioSrc.volume = SpiderSoundMaxVolume;
            snd.audioSrc.minDistance = SpiderSoundMinRange;
            snd.audioSrc.maxDistance = SpiderSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void spiderPunchedByPlayerEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = GetRandomPop();
            snd.audioSrc.volume = SpiderSoundMaxVolume;
            snd.audioSrc.minDistance = SpiderSoundMinRange;
            snd.audioSrc.maxDistance = SpiderSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    void spiderDeathEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = this.spiderDeath;
            snd.audioSrc.volume = SpiderSoundMaxVolume;
            snd.audioSrc.minDistance = SpiderSoundMinRange;
            snd.audioSrc.maxDistance = SpiderSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    private void spiderWebShotEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend = 1f;
            snd.audioSrc.clip = this.spiderWebShot;
            snd.audioSrc.volume = 0.75f * SpiderSoundMaxVolume;
            snd.audioSrc.minDistance = 5f;//SpiderSoundMinRange;
            snd.audioSrc.maxDistance = SpiderSoundMaxRange;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collapsing Platform Handling
    ////////////////////////////////////////////////////////////////////////////
    void platformCollapseEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = collapse;
            snd.audioSrc.volume         = 0.75f;
            snd.audioSrc.minDistance    = 2f;
            snd.audioSrc.maxDistance    = 15f;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Cloud Handling
    ////////////////////////////////////////////////////////////////////////////
    void cloudHitEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);

            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = cloudBounce;
            snd.audioSrc.volume         = 0.8f;
            snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
            snd.audioSrc.minDistance    = 2f;
            snd.audioSrc.maxDistance    = 15f;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Hay Bale Handling
    ////////////////////////////////////////////////////////////////////////////
    void hayBaleHitEventHandler(Vector3 worldPos)
    {
        if (eventSound3DPrefab)
        {
            EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
            snd.audioSrc.spatialBlend   = 1f;
            snd.audioSrc.clip           = hayBaleHit;
            snd.audioSrc.volume         = 0.5f;
            snd.audioSrc.minDistance    = 2f;
            snd.audioSrc.maxDistance    = 15f;

            snd.audioSrc.Play();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Misc HANDLING
    ////////////////////////////////////////////////////////////////////////////
    void woodHitEventHandler(Vector3 worldPos, float impactForce)
    {
        const float halfSpeedRange = 0.2f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.clip           = GetRandomWoodHit();
        snd.audioSrc.volume         = 0.25f;
        snd.audioSrc.pitch          = Random.Range(1f - halfSpeedRange, 1f + halfSpeedRange);
        snd.audioSrc.minDistance    = Mathf.Lerp(1f, 8f, impactForce / 200f);
        snd.audioSrc.maxDistance    = 20f;

        snd.audioSrc.Play();
    }

    void birdCallEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.05f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 0.75f;
        snd.audioSrc.clip           = GetRandomBird();
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

    void cricketEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 0.3f;
        snd.audioSrc.clip           = crickets;
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

    void dirtFallEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 0.25f;
        snd.audioSrc.clip           = GetRandomDirtFall();
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = 2f*ambientMaxDist;

        snd.audioSrc.Play();
    }

    void flyBuzzEventHandler(Vector3 worldPos)
    {
        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 1f;
        snd.audioSrc.clip           = flies;
        snd.audioSrc.pitch          = 1f;
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

    void goatEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 0.9f;
        snd.audioSrc.clip           = goat;
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

    void hawkEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 1f;
        snd.audioSrc.clip           = hawk;
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

    void mooEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 0.9f;
        snd.audioSrc.clip           = cow;
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

    void roosterEventHandler(Vector3 worldPos)
    {
        const float pitchRange = 0.1f;

        EventSound3D snd = Instantiate(eventSound3DPrefab, worldPos, Quaternion.identity, null);
        snd.audioSrc.spatialBlend   = 1f;
        snd.audioSrc.volume         = 0.9f;
        snd.audioSrc.clip           = GetRandomRooster();
        snd.audioSrc.pitch          = Random.Range(1f - pitchRange, 1f + pitchRange);
        snd.audioSrc.minDistance    = ambientMinDist;
        snd.audioSrc.maxDistance    = ambientMaxDist;

        snd.audioSrc.Play();
    }

}
