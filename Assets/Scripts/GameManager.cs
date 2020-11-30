using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //Game Management
    [Space(10)]
    [Header("Game Variables")]
    public int gameLength = 30;
    public int[] defeatCounter = new int[4] { 0, 0, 0, 0 };

    [SerializeField]
    public Country myCountry;

    [Space(15)]
    public Text warningText;

    [Header("Canvases")]
    public Canvas mainMenu;
    public Canvas createCountry;
    public Canvas gameScreen;
    public Canvas pauseScreen;
    public Canvas eventScreen;
    public Canvas creditCanvas;
    [Space(5)]
    public Canvas defeatScreen;
    public Canvas victoryScreen;


    public Text defeatReasonText;




    //Save Management
    private FileSave saveFile = new FileSave(FileFormat.Json);

    [Space(10)]
    [Header("New Game Screen")]
    //New Game Creation
    public InputField countryNameInput;

    [Space(10)]
    [Header("Game Play Screen")]
    //Game Screen



    public Text countryNameText;
    public Text moneyText;
    public Text foodText;
    public Text sidesText;
    public Text armyPowerText;
    public Text populationText;
    public Text happinessText;
    public Text dayCounterText;

    public Slider relationsSlider;

    public Text taxText;
    public Slider taxSlider;

    /*
     * Buildings
     */
    public Text farmCount;
    public Text baracksCount;
    public Text embassyACount;
    public Text embassyBCount;

    //Pause Control
    private bool eventState;

    //indexes: 0 food 1 money 2 revolution 3 low army
    [SerializeField]
    private bool[] isReasonTriggered = new bool[4] { false, false, false, false };


    [Space(10)]
    [Header("Time Management")]
    //Time Management
    private float timer = 0.0f;
    public int DayLength;
    private float defaultTimeScale;

    [Space(10)]
    [Header("Events")]
    //Event Management
    public Event firstDayEvent;
    public List<Event> myEvents = new List<Event>();
    public List<Event> lowMoneyTriggered = new List<Event>();
    public List<Event> lowFoodTriggered = new List<Event>();
    public List<Event> lowArmyTriggered = new List<Event>();
    public List<Event> highArmyTriggered = new List<Event>();
    private Event currentEvent;

    public Text eventText;
    public Text selection1Text;
    public Text selection2Text;



    [Space(10)]
    [Header("Modifiers")]
    //ModifierManagement
    public int[] buildingModifier = new int[8];


    #region Start-Update Functions

    // Start is called before the first frame update
    void Start()
    {
        warningText.text = "";
        defaultTimeScale = Time.timeScale;
        mainMenu.enabled = true;
        creditCanvas.enabled = false;
        createCountry.enabled = false;
        gameScreen.enabled = false;
        eventScreen.enabled = false;
        pauseScreen.enabled = false;
        victoryScreen.enabled = false;
        defeatScreen.enabled = false;
        Event[] events = Resources.LoadAll("Events", typeof(Event)).Cast<Event>().ToArray();

        foreach (Event e in events)
        {
            if (e.type == Event.EventType.Normal || e.type == Event.EventType.Chain)
            {
                myEvents.Add(e);
            }
            else if (e.type == Event.EventType.Triggered)
            {
                switch (e.triggerReason)
                {
                    case Event.TriggerReasons.LowFood:
                        lowFoodTriggered.Add(e);
                        break;
                    case Event.TriggerReasons.LowMoney:
                        lowMoneyTriggered.Add(e);
                        break;
                    case Event.TriggerReasons.ArmyRevolution:
                        highArmyTriggered.Add(e);
                        break;
                    case Event.TriggerReasons.LowArmyPower:
                        lowArmyTriggered.Add(e);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameScreen.enabled == true)
        {
            GameScreenWriter();
            TimeCounter();
            CheckLoseConditions();
            try
            {
                if (myCountry.day > 30)
                {
                    WinGame();
                }
            }
            catch (System.Exception)
            {

            }
        }
        if (mainMenu.enabled == true)
        {
            Time.timeScale = defaultTimeScale;
        }
    }


    #endregion

    #region UIHandler

    #region Main Menu Canvas
    public void OnNewGameButtonPressed()
    {
        mainMenu.enabled = false;
        createCountry.enabled = true;
    }

    public void OnCreateCountryButtonPressed()
    {
        if (countryNameInput.text == "")
        {
            myCountry = new Country("Inday", gameLength);
            gameScreen.enabled = true;
            float timerT = 0;
            int seconds = 0;
            while (seconds < 2)
            {
                timerT += Time.deltaTime;
                seconds = (int)(timerT % 60);
            }
            EventCreator();
            EventSelector(myCountry.day);
        }
        else
        {
            myCountry = new Country(countryNameInput.text, gameLength);
            createCountry.enabled = false;
            gameScreen.enabled = true;
            float timerT = 0;
            int seconds = 0;
            while (seconds < 2)
            {
                timerT += Time.deltaTime;
                seconds = (int)(timerT % 60);
            }
            EventCreator();
            EventSelector(myCountry.day);
        }

    }

    public void OnLoadButtonPressed()
    {
        if (File.Exists(Application.persistentDataPath + "/saves/save.json"))
        {
            print("Loaded gane");
            myCountry = saveFile.ReadFromFile<Country>(Application.persistentDataPath + "/saves/save.json");
            mainMenu.enabled = false;
            createCountry.enabled = false;
            taxText.text = "Tax: " + myCountry.tax.ToString();
            gameScreen.enabled = true;
            float timerT = 0;
            int seconds = 0;
            while (seconds < 2)
            {
                timerT += Time.deltaTime;
                seconds = (int)(timerT % 60);
            }
            EventSelector(myCountry.day);
        }
        else
        {
            StartCoroutine(WarningMessager("No Load File Found", 5));
        }
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }
    #endregion

    #region Game Screen Canvas
    public void GameScreenWriter()
    {
        countryNameText.text = myCountry.countryName;
        moneyText.text = "Money: " + myCountry.money.ToString();
        foodText.text = "Food: " + myCountry.food.ToString();
        sidesText.text = "Support Ratio: " + myCountry.sides.ToString();
        armyPowerText.text = "Army Power: " + myCountry.armyPower.ToString();
        populationText.text = "Population: " + myCountry.population.ToString() + "K";
        happinessText.text = "Happiness:  " + myCountry.happiness.ToString();
        dayCounterText.text = "Month " + myCountry.day.ToString();
        taxText.text = "Tax: " + myCountry.tax.ToString();
        taxSlider.value = myCountry.tax;

        relationsSlider.value = myCountry.relationWithCountryA;
    }

    public void OnPauseButtonPressed()
    {
        Time.timeScale = 0;
        eventState = eventScreen.enabled;
        eventScreen.enabled = false;
        pauseScreen.enabled = true;
    }
    #endregion

    #region Pause Canvas
    public void OnResumeButtonPressed()
    {
        Time.timeScale = defaultTimeScale;
        pauseScreen.enabled = false;
        eventScreen.enabled = eventState;
    }

    public void OnMainMenuButtonPressed()
    {
        SaveManagement();
        mainMenu.enabled = true;
        createCountry.enabled = false;
        gameScreen.enabled = false;
        eventScreen.enabled = false;
        pauseScreen.enabled = false;
        victoryScreen.enabled = false;
        defeatScreen.enabled = false;
        creditCanvas.enabled = false;
        
        warningText.text = "";
    }
    public void OnSaveButtonPressed()
    {
        SaveManagement();
    }
    #endregion

    #region Selection Canvas
    public void OnSelection1ButtonPressed()
    {
        if (myCountry.events[myCountry.day - 1].type != Event.EventType.Triggered)
        {
            eventScreen.enabled = false;
            myCountry.money += currentEvent.effect1[0];
            myCountry.food += currentEvent.effect1[1];
            myCountry.sides += currentEvent.effect1[2];
            myCountry.armyPower += currentEvent.effect1[3];
            myCountry.population += currentEvent.effect1[4];
            myCountry.happiness += currentEvent.effect1[5];
            myCountry.relationWithCountryA += currentEvent.effect1[6];
            myCountry.relationWithCountryB += currentEvent.effect1[7];
            Time.timeScale = defaultTimeScale;
            print("Non Triggered");
        }
        else
        {
            eventScreen.enabled = false;
            myCountry.money += currentEvent.effect1[0];
            myCountry.food += currentEvent.effect1[1];
            myCountry.sides += currentEvent.effect1[2];
            myCountry.armyPower += currentEvent.effect1[3];
            myCountry.population += currentEvent.effect1[4];
            myCountry.happiness += currentEvent.effect1[5];
            myCountry.relationWithCountryA += currentEvent.effect1[6];
            myCountry.relationWithCountryB += currentEvent.effect1[7];
            /*
            switch (myCountry.events[myCountry.day - 1].triggerReason)
            {
                case Event.TriggerReasons.LowFood:
                    isReasonTriggered[0] = false;
                    break;
                case Event.TriggerReasons.LowMoney:
                    isReasonTriggered[1] = false;
                    break;
                case Event.TriggerReasons.ArmyRevolution:
                    isReasonTriggered[2] = false;
                    break;
                case Event.TriggerReasons.LowArmyPower:
                    isReasonTriggered[3] = false;
                    break;
                default:
                    break;
            }
            */
            print("Triggered: " + myCountry.events[myCountry.day - 1].triggerReason.ToString());
            Time.timeScale = defaultTimeScale;
        }
    }

    public void OnSelection2ButtonPressed()
    {
        if (myCountry.events[myCountry.day - 1].type != Event.EventType.Triggered)
        {
            eventScreen.enabled = false;
            myCountry.money += currentEvent.effect2[0];
            myCountry.food += currentEvent.effect2[1];
            myCountry.sides += currentEvent.effect2[2];
            myCountry.armyPower += currentEvent.effect2[3];
            myCountry.population += currentEvent.effect2[4];
            myCountry.happiness += currentEvent.effect2[5];
            myCountry.relationWithCountryA += currentEvent.effect2[6];
            myCountry.relationWithCountryB += currentEvent.effect2[7];
            Time.timeScale = defaultTimeScale;
            print("Non Triggered");
        }
        else
        {
            eventScreen.enabled = false;
            myCountry.money += currentEvent.effect2[0];
            myCountry.food += currentEvent.effect2[1];
            myCountry.sides += currentEvent.effect2[2];
            myCountry.armyPower += currentEvent.effect2[3];
            myCountry.population += currentEvent.effect2[4];
            myCountry.happiness += currentEvent.effect2[5];
            myCountry.relationWithCountryA += currentEvent.effect2[6];
            myCountry.relationWithCountryB += currentEvent.effect2[7];
            /*
            switch (myCountry.events[myCountry.day - 1].triggerReason)
            {
                case Event.TriggerReasons.LowFood:
                    isReasonTriggered[0] = false;
                    break;
                case Event.TriggerReasons.LowMoney:
                    isReasonTriggered[0] = false;
                    break;
                case Event.TriggerReasons.ArmyRevolution:
                    isReasonTriggered[0] = false;
                    break;
                case Event.TriggerReasons.LowArmyPower:
                    isReasonTriggered[0] = false;
                    break;
                default:
                    break;
            }
            */
            print("Triggered: " + myCountry.events[myCountry.day - 1].triggerReason.ToString());
            Time.timeScale = defaultTimeScale;
        }
    }
    #endregion

    #region Credits

    public void OnCreditButtonPressed()
    {
        mainMenu.enabled = false;
        creditCanvas.enabled = true;
    }

    public void OnCreditExitButtonPressed()
    {
        creditCanvas.enabled = false;
        mainMenu.enabled = true;
    }

    #endregion


    public IEnumerator WarningMessager(string warning, int waitTime)
    {
        warningText.text = warning;
        yield return new WaitForSeconds(waitTime);
        warningText.text = "";
    }

    #endregion

    #region GamePlay Methods

    #region Stat Management
    private void Modifiers()
    {
        BuildingModifierCalculation();
        myCountry.money += (int)(myCountry.population * myCountry.tax *0.25 - buildingModifier[0] + myCountry.decisionModifiers[0]);
        myCountry.food += (int)(-1* myCountry.population * 0.5 + buildingModifier[1] + myCountry.decisionModifiers[1]);
        myCountry.sides += myCountry.decisionModifiers[2] + buildingModifier[2] + (int)(myCountry.relationWithCountryA * .1) - (int)(myCountry.relationWithCountryB * .1);
        myCountry.armyPower += myCountry.decisionModifiers[3] + buildingModifier[3] - (int)(myCountry.armyPower*.1);
        myCountry.population += myCountry.decisionModifiers[4] + buildingModifier[4] + (int)(myCountry.population * .1);
        myCountry.happiness += HappinessCalculation() + myCountry.decisionModifiers[5] + buildingModifier[5];
        myCountry.relationWithCountryA += myCountry.decisionModifiers[6] + buildingModifier[6];
        myCountry.relationWithCountryB += myCountry.decisionModifiers[7] + buildingModifier[7];

        if (myCountry.happiness > 100)
        {
            myCountry.happiness = 100;
        }
        if (myCountry.armyPower > 100)
        {
            myCountry.armyPower = 100;
        }
    }

    private int HappinessCalculation()
    {
        int happinessModifier = 0;
        if (myCountry.food / myCountry.population * 0.05 < 10)
        {
            happinessModifier -= 5;
        }
        else if (myCountry.food / myCountry.population * 0.05 > 20)
        {
            happinessModifier += 1;
        }
        if (myCountry.armyPower < 50)
        {
            happinessModifier -= 3;
        }
        else if (myCountry.armyPower > 70)
        {
            happinessModifier += 1;
        }
        happinessModifier = happinessModifier + (2 - myCountry.tax);
        return happinessModifier;
    }

    public void BuildingModifierCalculation()
    {
        buildingModifier = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        foreach (var build in myCountry.myBuildings)
        {
            for (int i = 0; i < buildingModifier.Length; i++)
            {
                buildingModifier[i] += build.buildingModifier[i];
            }
        }
    }

    public void TaxCalculation()
    {
        myCountry.tax = (int)taxSlider.value;
        taxText.text = "Tax: " + myCountry.tax.ToString();
    }
    #endregion


    private void CheckLoseConditions()
    {
        if (myCountry.money >= 10 * myCountry.population)
        {
            LoseGame("Because of your wealth you became a target and attacked.");
        }
        if (myCountry.food >= 15 * myCountry.population)
        {
            LoseGame("Because of your wealth in food you became a target and attacked.");
        }
        if (myCountry.sides>75)
        {
            LoseGame("Your people wanted to join war against Country B.");
        }
        if (myCountry.sides < 25)
        {
            LoseGame("Your people wanted to join war against Country A.");
        }
        if (myCountry.happiness < 15)
        {
            LoseGame("Your people was sad. They overthrow you.");
        }
        if (myCountry.relationWithCountryA > 80)
        {
            LoseGame("You were too good with Country A. You got attacked by Country B");
        }
        if (myCountry.relationWithCountryB > 80)
        {
            LoseGame("You were too good with Country B. You got attacked by Country A");
        }
        int i = 0;
        foreach (var item in defeatCounter)
        {
            if (item >= 3)
            {
                switch (i)
                {
                    case 0:
                        LoseGame("Your food was not enough. Your people overthrow you.");
                        break;
                    case 1:
                        LoseGame("Your money was not enough. Your people overthrow you.");
                        break;
                    case 2:
                        LoseGame("Your army was too weak. You got attacked.");
                        break;
                    case 3:
                        LoseGame("Your army was too strong. They made a revolution.");
                        break;
                    default:
                        break;
                }
            }
            i++;
        }
    }

    private void LoseGame(string reason)
    {
        print("Lost");
        defeatReasonText.text = reason;
        mainMenu.enabled = false;
        createCountry.enabled = false;
        gameScreen.enabled = false;
        eventScreen.enabled = false;
        pauseScreen.enabled = false;
        victoryScreen.enabled = false;
        defeatScreen.enabled = true;
        myCountry = null;
        for (int i = 0; i < defeatCounter.Length; i++)
        {
            isReasonTriggered[0] = false;
            defeatCounter[0] = 0;
        }
        warningText.text = "";
    }
    private void WinGame()
    {
        print("Win");
        mainMenu.enabled = false;
        createCountry.enabled = false;
        gameScreen.enabled = false;
        eventScreen.enabled = false;
        pauseScreen.enabled = false;
        victoryScreen.enabled = true;
        defeatScreen.enabled = false;
        myCountry = null;
        for (int i = 0; i < defeatCounter.Length; i++)
        {
            isReasonTriggered[0] = false;
            defeatCounter[0] = 0;
        }
        warningText.text = "";
    }


    private void ProblemChecker()
    {
        if (myCountry.food < myCountry.population * 0.05)
        {
            if (isReasonTriggered[0] == false)
            {
                defeatCounter[0] = 0;
                EventTrigger(Event.TriggerReasons.LowFood);
                isReasonTriggered[0] = true;
            }
            else
            {
                defeatCounter[0]++;
            }
            return;
        }
        if (myCountry.money < 0)
        {
            if (isReasonTriggered[1] == false)
            {
                defeatCounter[1] = 0;
                EventTrigger(Event.TriggerReasons.LowMoney);
                isReasonTriggered[1] = true;
            }
            else
            {
                defeatCounter[1]++;
            }
                
            return;
        }
        if (myCountry.armyPower < 30)
        {
            if (isReasonTriggered[2] == false)
            {
                defeatCounter[2] = 0;
                EventTrigger(Event.TriggerReasons.LowArmyPower);
                isReasonTriggered[2] = true;
            }
            else
            {
                defeatCounter[2]++;
            }
            return;
        }
        if (myCountry.armyPower > 90)
        {
            if (isReasonTriggered[3] == false)
            {
                defeatCounter[3] = 0;
                EventTrigger(Event.TriggerReasons.ArmyRevolution);
                isReasonTriggered[3] = true;
            }
            else
            {
                defeatCounter[3]++;
            }
            return;
        }
    }


    #endregion

    public void TimeCounter()
    {
        timer += Time.deltaTime;
        int seconds = (int)(timer % 60);
        if (seconds >= DayLength)
        {
            SaveManagement();
            myCountry.day++;
            timer = 0.0f;
            Modifiers();
            EventSelector(myCountry.day);
            ProblemChecker();
        }
    }

    #region Event Handler

    public void EventSelector(int day)
    {
        Time.timeScale = 0;
        currentEvent = myCountry.events[day - 1];
        eventText.text = currentEvent.eventDescription;
        selection1Text.text = "   " + currentEvent.selection1Description + "   ";
        selection2Text.text = "   " + currentEvent.selection2Description + "   ";
        eventScreen.enabled = true;
    }

    private void EventCreator()
    {
        myCountry.events[0] = firstDayEvent;
        for (int i = 1; i < gameLength; i++)
        {
            if (myCountry.events[i] == null)
            {
                Event tempEvent = myEvents[Random.Range(0, myEvents.Count)];
                switch (tempEvent.type)
                {
                    case Event.EventType.Normal:
                        myCountry.events[i] = tempEvent;
                        break;
                    case Event.EventType.Chain:
                        i = ChainEventAdder(i, tempEvent);
                        break;
                    case Event.EventType.Triggered:
                        i--;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                print("Day" + i.ToString() + "'s event is " + myCountry.events[i].eventName);
            }
        }

    }

    private int ChainEventAdder(int i, Event tempEvent)
    {
        foreach (Event item in myCountry.events)
        {
            try
            {
                if (tempEvent.chainID == item.chainID)
                {
                    i--;
                    return i;
                }
            }
            catch (System.Exception)
            {
                break;
            }
        }
        if (tempEvent.chainOrder == 1)
        {
            myCountry.events[i] = tempEvent;
            for (int k = 0; k < tempEvent.eventGap.Length; k++)
            {
                int index = 1;
                foreach (Event e in myEvents)
                {
                    if (e.chainID == tempEvent.chainID && e.chainOrder == (index + 1))
                    {
                        print("index " + index.ToString());
                        print("Event Chain Order: " + e.chainOrder.ToString());
                        myCountry.events[i + tempEvent.eventGap[k]] = e;
                        index++;
                        break;
                    }
                }
            }

        }
        else
        {
            foreach (Event eve in myEvents)
            {
                if (eve.chainID == tempEvent.chainID && eve.chainOrder == 1)
                {
                    myCountry.events[i] = eve;
                    int index = 1;
                    for (int k = 0; k < eve.eventGap.Length; k++)
                    {
                        foreach (Event e in myEvents)
                        {
                            if (e.chainID == eve.chainID && e.chainOrder == (index + 1))
                            {
                                print("index " + index.ToString());
                                print("Event Chain Order: " + e.chainOrder.ToString());
                                myCountry.events[i + eve.eventGap[k]] = e;
                                index++;
                                break;
                            }
                        }
                    }
                }
            }
        }
        return i;
    }

    private void EventTrigger(Event.TriggerReasons reason)
    {
        print("Trigger Sent: " + reason.ToString());
        int index = 0;
        switch (reason)
        {
            case Event.TriggerReasons.LowFood:
                while (myCountry.events[myCountry.day + index].type == Event.EventType.Chain)
                {
                    print("Chain Event. Indexer increased to" + index.ToString());
                    index++;
                }
                myCountry.events[myCountry.day + index] = lowFoodTriggered[Random.Range(0,lowFoodTriggered.Count)];
                break;
            case Event.TriggerReasons.LowMoney:
                while (myCountry.events[myCountry.day + index].type == Event.EventType.Chain)
                {
                    print("Chain Event. Indexer increased to" + index.ToString());
                    index++;
                }
                myCountry.events[myCountry.day + index] = lowFoodTriggered[Random.Range(0, lowMoneyTriggered.Count)];
                break;
            case Event.TriggerReasons.ArmyRevolution:
                while (myCountry.events[myCountry.day + index].type == Event.EventType.Chain)
                {
                    print("Chain Event. Indexer increased to" + index.ToString());
                    index++;
                }
                myCountry.events[myCountry.day + index] = lowFoodTriggered[Random.Range(0, highArmyTriggered.Count)];
                break;
            case Event.TriggerReasons.LowArmyPower:
                while (myCountry.events[myCountry.day + index].type == Event.EventType.Chain)
                {
                    print("Chain Event. Indexer increased to" + index.ToString());
                    index++;
                }
                myCountry.events[myCountry.day + index] = lowFoodTriggered[Random.Range(0, lowArmyTriggered.Count)];
                break;
            default:
                break;
        }
    }

    #endregion








    private void SaveManagement()
    {
        var path = Application.persistentDataPath + "/saves";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            print("DirectoryCreated");
        }
        print("Saved game to " + Application.persistentDataPath);
        saveFile.WriteToFile(Application.persistentDataPath + "/saves/save.json", myCountry);
    }


}
